using UnityEngine;
/// <summary>
/// 麻辣烫炸弹
/// </summary>
public class SpicyStringBoom : FoodUnit
{
    private int prepareTime;
    private int totalPrepareTime; // 总准备时间
    private bool isTriggerBoom; // 是否被触爆
    private BoolModifier boolModifier = new BoolModifier(true);

    public override void MInit()
    {
        totalPrepareTime = 60 * 5; // 5s准备时间
        prepareTime = totalPrepareTime;
        isTriggerBoom = false;
        base.MInit();
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
        else
        {
            // 准备好后才会检测本格是否有满足触发条件的敌人
            foreach (var enemy in GetGrid().GetMouseUnitList())
            {
                if (enemy.GetHeight() == 1)
                {
                    isTriggerBoom = true;
                    ExecuteDeath();
                    break;
                }
            }
        }
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreIdle");
        prepareTime = 0;
        // 在准备动画几帧内，进入无敌、免疫灰烬秒杀、免疫冻结效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, boolModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, boolModifier);
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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, boolModifier);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        
        if(isTriggerBoom && IsFinishPrepare())
        {
            // 如果是被触爆 且 已完成准备 则效果转化为强化爆破（强制秒杀以本格为中心半径为0.75格的非BOSS地面敌人，并对3*3范围内的所有敌人造成一次灰烬伤害）
            CreateFortifyBoom();
            // 原地产生一个爆炸特效
            {
                GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/BoomEffect");
                BaseEffect effect = instance.GetComponent<BaseEffect>();
                effect.MInit();
                effect.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
                effect.transform.position = transform.position;
                GameController.Instance.AddEffect(effect);
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
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 900, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(1); // 仅对空
            bombEffect.SetEventMouseAction(BurnNoBossEnemyUnit);
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }

        // 3*3所有空军强制击坠效果
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 0, GetRowIndex(), 3, 3, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(1); // 仅对空
            bombEffect.SetEventMouseAction(ExecuteDropEnemyFlyUnit);
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }

    /// <summary>
    /// 直接秒杀非BOSS敌人单位
    /// </summary>
    private void BurnNoBossEnemyUnit(MouseUnit m)
    {
        if(!m.IsBoss())
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, m, m.mCurrentHp).ApplyAction();
        else
            // 对BOSS造成900点灰烬伤害
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, m, 900).ApplyAction();
    }

    /// <summary>
    /// 强制击坠非BOSS空军单位
    /// </summary>
    private void ExecuteDropEnemyFlyUnit(MouseUnit m)
    {
        // 非BOSS单位 且 是空军单位
        if(!m.IsBoss() && typeof(IFlyUnit).IsAssignableFrom(m.GetType()) && m.IsAlive())
        {
            IFlyUnit flyUnit = (IFlyUnit)m;
            flyUnit.ExecuteDrop();
        }
    }
}
