using S7P.Numeric;

using System;

using UnityEngine;

public class ValiMouse : MouseUnit
{
    private static RuntimeAnimatorController Boom_Run;
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // 免疫炸弹秒杀效果
    private bool isAngry = false;

    public override void Awake()
    {
        if (Boom_Run == null)
            Boom_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/31/Boom");
        base.Awake();
    }

    public override void MInit()
    {
        isAngry = false;
        base.MInit();
        // 初始免疫炸弹秒杀效果
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // 受到灰烬效果后会启动自爆程序
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    if (isAngry)
                    {
                        ExecuteBurn();
                    }
                    else
                    {
                        SetAngry();
                    }
                }
            }
        });
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    private void SetAngry()
    {
        if (!isAngry)
        {
            isAngry = true;
            // 锁1血
            SetGetCurrentHpFunc(delegate {
                return Mathf.Max(1, _mCurrentHp);
            });
            // 不可被选为攻击目标、不可阻挡、不可被击中
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // 同时移除身上所有定身类控制效果
            StatusManager.RemoveAllSettleDownDebuff(this);
            // 添加定身与冻结免疫效果
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
            // 获得500%移动速度加成
            NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier(500));

            SetActionState(new TransitionState(this));

            float start_x = transform.position.x;
            float end_x = Mathf.Max(MapManager.GetColumnX(0), transform.position.x + moveRotate.x * 4 * MapManager.gridWidth);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                if (moveRotate.x < 0)
                    return transform.position.x < end_x;
                else
                    return transform.position.x > end_x;
            });
            task.AddOnExitAction(delegate {
                SetActionState(new CastState(this));
            });
            taskController.AddTask(task);
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("BeforeBoom");
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            CreateCheckArea();
            ExecuteDeath();
        }
    }

    private void CreateCheckArea()
    {
        float bomb_dmg_rate = 1.0f;

        // 如果身上有冰冻损伤debuff
        if (Environment.EnvironmentFacade.GetIceDebuff(this) != null)
        {
            bomb_dmg_rate = 0.5f;
            Action<BaseUnit> action = (u) =>
            {
                Environment.EnvironmentFacade.AddIceDebuff(u, 100);
            };
            // 对3*3所有单位施加100冰冻损伤
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(2.75f * MapManager.gridWidth, 2.75f * MapManager.gridHeight), "BothCollide");
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
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
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
            BaseEffect e = BaseEffect.CreateInstance(Boom_Run, null, "Boom", null, false);
            e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
            e.transform.position = transform.position;
            GameController.Instance.AddEffect(e);
        }
    }

    public override void OnDieStateEnter()
    {
        if (isAngry)
            DeathEvent();
        else
            base.OnDieStateEnter();
    }

    public override void OnMoveStateEnter()
    {
        base.OnMoveStateEnter();
        if (isAngry)
            animatorController.Play("AngryMove", true);
        else
            animatorController.Play("Move", true);
    }
}
