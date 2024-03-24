using Environment;
/// <summary>
/// 旋转咖啡喷壶
/// </summary>
public class SpinCoffee : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // 一转第一分支为不会被选为攻击目标和不可被阻挡的效果
        if(mShape == 1)
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // 添加隐匿特效
            EnvironmentFacade.AddFogBuff(this);
        }
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        if (GameController.Instance.isEnableNoTargetAttackMode)
            return true;

        float left = transform.position.x - 1.5f * MapManager.gridWidth;
        float right = transform.position.x + 1.5f * MapManager.gridWidth;
        float bottom = transform.position.y - 1.5f * MapManager.gridHeight;
        float top = transform.position.y + 1.5f * MapManager.gridHeight;
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            if (unit.transform.position.x >= left && unit.transform.position.x <= right && unit.transform.position.y >= bottom && unit.transform.position.y <= top)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
        GameManager.Instance.audioSourceController.PlayEffectMusic("Fume");
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        float dmg = mCurrentAttack;
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u) => {
            float dmg_rate = 1;
            if (u.GetHeight() == 1)
                dmg_rate = 0.25f;
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg*dmg_rate);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
        });
        r.SetInstantaneous();
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
