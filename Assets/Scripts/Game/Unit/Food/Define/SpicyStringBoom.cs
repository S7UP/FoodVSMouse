using System;

using S7P.Numeric;
/// <summary>
/// 麻辣烫炸弹
/// </summary>
public class SpicyStringBoom : FoodUnit
{
    private int prepareTime;
    private int totalPrepareTime; // 总准备时间
    private bool isTriggerBoom; // 是否被触爆
    private BoolModifier boolModifier = new BoolModifier(true);
    private FloatModifier burnMod = new FloatModifier(0);
    private static Func<BaseUnit, BaseUnit, bool> noSelectedFunc = delegate { return false; };

    public override void MInit()
    {
        totalPrepareTime = 60 * 4; // 4s准备时间
        prepareTime = totalPrepareTime;
        isTriggerBoom = false;
        base.MInit();
        // 不被空中单位作为攻击目标
        AddCanBeSelectedAsTargetFunc((u1, u2) => {
            return u2!=null && u2.mHeight != 1;
        });
        // 不被所有单位作为攻击目标
        AddCanBeSelectedAsTargetFunc(noSelectedFunc);
    }

    /// <summary>
    /// 炸弹掉落等效于正常死亡
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    public override void OnIdleStateEnter()
    {
        if (IsFinishPrepare())
        {
            animatorController.Play("Idle", true);
        }
        else
        {
            animatorController.Play("Prepare", true);
        }
    }

    public override void OnIdleState()
    {
        if (!IsFinishPrepare())
            prepareTime--;
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.65f, 0.65f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(1);
        Action<MouseUnit> action = (u) =>
        {
            if (r.isAlive)
            {
                isTriggerBoom = true;
                ExecuteDeath();
                GameManager.Instance.audioSourceController.PlayEffectMusic("CatcherTrigger");
                r.MDestory();
            }
        };
        r.SetOnEnemyEnterAction(action);
        r.SetOnEnemyStayAction(action);
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (IsAlive())
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

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreIdle");
        prepareTime = 0;
        // 在准备动画几帧内，进入无敌、免疫灰烬秒杀、免疫冻结效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, boolModifier); 
        NumericBox.BurnRate.AddModifier(burnMod);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
        // 移除不被选为攻击目标效果
        RemoveCanBeSelectedAsTargetFunc(noSelectedFunc);
        // 添加检测范围
        CreateCheckArea();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        
        if(isTriggerBoom && IsFinishPrepare())
        {
            // 如果是被触爆 且 已完成准备 则效果转化为强化爆破（强制秒杀以本格为中心半径为0.75格的非BOSS空中敌人，并对3*3范围内的所有敌人造成一次灰烬伤害）
            CreateFortifyBoom();
            // 原地产生一个爆炸特效
            {
                BaseEffect e = BaseEffect.GetInstance("BoomEffect");
                e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
                e.transform.position = transform.position;
                e.MInit();
                GameController.Instance.AddEffect(e);
            }
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
    /// 强化爆破
    /// </summary>
    private void CreateFortifyBoom()
    {
        // 单行1.5格内对空秒杀非BOSS效果
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1.5f, 1, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(1);
            r.SetOnEnemyEnterAction((u) => {
                if (!u.IsBoss())
                    new DamageAction(CombatAction.ActionType.BurnDamage, this, u, u.mCurrentHp).ApplyAction();
                else
                    BurnManager.BurnDamage(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 5*3所有空军强制击坠外加破盾
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 5, 3, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(1);
            r.SetOnEnemyEnterAction((u) => {
                if (typeof(IFlyUnit).IsAssignableFrom(u.GetType()) && u.IsAlive())
                {
                    IFlyUnit flyUnit = (IFlyUnit)u;
                    flyUnit.OnShootDown();
                }
                else
                    BurnManager.BurnDamage(this, u);
                // 移除护盾
                u.NumericBox.DamageShield(u.NumericBox.Shield.Value);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
