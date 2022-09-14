/// <summary>
/// 给BOSS使用的技能存储实体
/// </summary>
public abstract class BossSkillAbility : SkillAbility
{
    public BossUnit bossMaster;

    public BossSkillAbility(BossUnit bossMaster, SkillAbilityInfo info) : base(bossMaster, info)
    {
        this.bossMaster = bossMaster;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return false;
    }

    public override void BeforeSpell()
    {

    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {

    }

    /// <summary>
    /// 在非技能期间
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    public override void AfterSpell()
    {

    }
}