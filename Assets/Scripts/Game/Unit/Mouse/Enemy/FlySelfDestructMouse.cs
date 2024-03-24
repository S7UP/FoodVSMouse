using UnityEngine;
using S7P.Numeric;
using System;

public class FlySelfDestructMouse : MouseUnit
{
    private static RuntimeAnimatorController BoomEffect;

    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // 免疫炸弹秒杀效果
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // 免疫冻结

    public override void Awake()
    {
        if (BoomEffect == null)
            BoomEffect = GameManager.Instance.GetRuntimeAnimatorController("Mouse/8/BoomEffect");
        base.Awake();
    }

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // 降落列默认为3，即左四列
        base.MInit();
        mHeight = 1;
        // 初始免疫炸弹秒杀效果
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // 在受到伤害结算之后，直接判定为击坠状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat)=> { 
            if(!isDrop)
                ExecuteDestruct();
            else
            {
                // 在坠机状态下受到灰烬效果会直接化灰
                if(combat is DamageAction)
                {
                    DamageAction dmgAction = combat as DamageAction;
                    if(dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    {
                        ExecuteBurn();
                    }
                }
            }
        });
        // 锁1血
        SetGetCurrentHpFunc(delegate {
            return Mathf.Max(1, _mCurrentHp);
        });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
            ExecuteDestruct();
    }

    /// <summary>
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void ExecuteDestruct()
    {
        if (!isDrop)
        {
            // 标记已坠机，此后该实例一些行为会发生变化
            isDrop = true;
            // 设置为不可选取，以及不可击中
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // 移除免疫炸弹秒杀效果
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
            // 添加冻结免疫效果
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
            // 如果自身有冰冻损伤，则再自我添加100点冰冻损伤（以实现能稳定携带冰冻损伤落地）
            if (Environment.EnvironmentFacade.GetIceDebuff(this) != null)
            {
                Environment.EnvironmentFacade.AddIceDebuff(this, 100);
            }
            // 同时移除身上所有定身类控制效果
            StatusManager.RemoveAllSettleDownDebuff(this);
            // 修改基础移速为在200帧内移动3格
            NumericBox.MoveSpeed.SetBase(3 * MapManager.gridWidth / 200);
            // 设为转场状态
            SetActionState(new MoveState(this));

            int timeLeft = 200;
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    return true;
                }
                return false;
            });
            task.AddOnExitAction(delegate {
                CreateCheckArea();
                ExecuteDeath();
            });
            taskController.AddTask(task);
        }
    }

    private void CreateCheckArea()
    {
        float bomb_dmg_rate = 1.0f;

        // 如果身上有冰冻损伤debuff
        if(Environment.EnvironmentFacade.GetIceDebuff(this)!=null)
        {
            bomb_dmg_rate = 0.01f;
            Action<BaseUnit> action = (u) => 
            {
                Environment.EnvironmentFacade.AddIceDebuff(u, 100);
            };
            // 对3*3所有单位施加100冰冻损伤
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f*MapManager.gridWidth, 0.5f * MapManager.gridHeight), "BothCollide");
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetOnFoodEnterAction(action);
            r.SetOnEnemyEnterAction(action);
            r.AddExcludeMouseUnit(this);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 落地爆炸判定(对美食）
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f*MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetTotoalTimeAndTimeLeft(2);
            r.AddExcludeMouseUnit(this);
            r.AddBeforeDestoryAction(delegate
            {
                // 所有美食单位分摊伤害
                int count = r.foodUnitList.Count;
                foreach (var u in r.foodUnitList.ToArray())
                    new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(50, u.mMaxHp / 2) / count * bomb_dmg_rate).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // 对老鼠
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(1f * MapManager.gridWidth, 1f * MapManager.gridHeight), "ItemCollideEnemy");
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            r.SetTotoalTimeAndTimeLeft(2);
            r.AddExcludeMouseUnit(this);
            r.AddBeforeDestoryAction(delegate
            {
                // 所有老鼠单位受到一次灰烬效果
                foreach (var u in r.mouseUnitList.ToArray())
                    BurnManager.BurnDamage(this, u, bomb_dmg_rate);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 爆炸特效
        {
            BaseEffect e = BaseEffect.CreateInstance(BoomEffect, null, "Disappear", null, false);
            e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
            e.transform.position = transform.position;
            GameController.Instance.AddEffect(e);
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isDrop)
            animatorController.Play("Move1");
        else
            animatorController.Play("Move0", true);
    }

    public override bool CanTriggerCat()
    {
        return false;
    }

    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }

    /// <summary>
    /// 没有死亡动画
    /// </summary>
    public override void OnDieStateEnter()
    {
        DeathEvent();
    }

    public override void DuringDeath()
    {
        
    }
}
