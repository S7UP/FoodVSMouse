using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 老鼠夹子
/// </summary>
public class MouseCatcher : FoodUnit
{
    private int prepareTime;
    private int totalPrepareTime; // 总准备时间
    private bool isTriggerBoom; // 是否被触爆
    private BoolModifier boolModifier = new BoolModifier(true);
    private FloatModifier burnMod = new FloatModifier(0);

    public override void MInit()
    {
        totalPrepareTime = 60 * 5; // 5s准备时间
        prepareTime = totalPrepareTime;
        isTriggerBoom = false;
        base.MInit();

        if(mShape >= 2)
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // 添加隐匿特效
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/HiddenEffect"), "Appear", "Idle", "Disappear", true);
            e.SetSpriteRendererSorting("Effect", 2);
            GameController.Instance.AddEffect(e);
            AddEffectToDict("SpinCoffeeHidden", e, new Vector2(0, 0 * 0.5f * MapManager.gridWidth));
        }
    }

    /// <summary>
    /// 炸弹掉落等效于正常死亡
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.65f, 0.65f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        Action<MouseUnit> action = (u) =>
        {
            //if (r.isAlive && UnitManager.CanBeSelectedAsTarget(this, u))
            if (r.isAlive)
            {
                isTriggerBoom = true;
                GameManager.Instance.audioSourceManager.PlayEffectMusic("CatcherTrigger");
                ExecuteDeath();
                r.MDestory();
            }
        };
        r.SetOnEnemyEnterAction(action);
        r.SetOnEnemyStayAction(action);
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if(IsAlive())
                r.transform.position = transform.position;
            else
            {
                r.MDestory();
            }
            return false;
        });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public override void OnIdleStateEnter()
    {
        if (IsFinishPrepare())
        {
            animatorController.Play("Idle");
        }
        else
        {
            animatorController.Play("Prepare");
        }
    }

    public override void OnIdleState()
    {
        if (!IsFinishPrepare())
            prepareTime--;
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreIdle");
        prepareTime = 0;
        // 在准备动画几帧内，进入无敌、免疫灰烬秒杀、免疫冻结效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, boolModifier);
        NumericBox.BurnRate.AddModifier(burnMod);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    public override void OnTransitionStateExit()
    {
        // 移除这些效果
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, boolModifier);
        NumericBox.BurnRate.RemoveModifier(burnMod);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
        // 添加检测效果
        CreateCheckArea();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        
        if(isTriggerBoom && IsFinishPrepare())
        {
            // 如果是被触爆 且 已完成准备 则效果转化为强化爆破（强制秒杀以本格为中心半径为0.75格的非BOSS地面敌人，并对3*3范围内的所有敌人造成一次灰烬伤害）
            CreateFortifyBoom();
        }
        else
        {
            // 否则仅对以本格为中心半径为0.75格的地面敌人造成一次900灰烬伤害
            CreateNormalBoom();
        }
        // 原地产生一个爆炸特效
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
    }

    /// <summary>
    /// 是否准备完成
    /// </summary>
    /// <returns></returns>
    private bool IsFinishPrepare()
    {
        return prepareTime <= 0;
    }

    /// <summary>
    /// 普通爆炸
    /// </summary>
    /// <returns></returns>
    private void CreateNormalBoom()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1.5f, 1, "ItemCollideEnemy");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetOnEnemyEnterAction((u) => {
            BurnManager.BurnDamage(this, u);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 强化爆破
    /// </summary>
    private void CreateFortifyBoom()
    {
        // 对地秒杀非BOSS效果
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1.5f, 1, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetOnEnemyEnterAction((u) => {
                if (!u.IsBoss())
                    new DamageAction(CombatAction.ActionType.BurnDamage, this, u, u.mCurrentHp).ApplyAction();
                else
                    BurnManager.BurnDamage(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 3*3
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
