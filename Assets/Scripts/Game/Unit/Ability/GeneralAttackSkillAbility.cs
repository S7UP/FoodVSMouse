/// <summary>
/// 普通攻击
/// </summary>
public class GeneralAttackSkillAbility : SkillAbility
{
    public GeneralAttackSkillAbility(BaseUnit pmaster) :base(pmaster)
    {
        // 默认平A间隔按照单位的攻速属性来
        UpdateNeedEnergyByAttackSpeed();
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
    }

    public GeneralAttackSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) :base(pmaster, info)
    {
        // 默认平A间隔按照单位的攻速属性来，否则按照info里的来
        UpdateNeedEnergyByAttackSpeed();
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
    }

    /// <summary>
    /// 根据目标的攻速来决定需要能量值
    /// </summary>
    private void UpdateNeedEnergyByAttackSpeed()
    {
        if (master.mBaseAttackSpeed > 0)
            needEnergy.SetBase(60 / master.mCurrentAttackSpeed);
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return master.IsMeetGeneralAttackCondition();
    }

    public override void BeforeSpell()
    {
        master.BeforeGeneralAttack();
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        master.OnGeneralAttack();
    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return master.IsMeetEndGeneralAttackCondition();
    }

    public override void AfterSpell()
    {
        master.AfterGeneralAttack();
        UpdateNeedEnergyByAttackSpeed();
    }
}
