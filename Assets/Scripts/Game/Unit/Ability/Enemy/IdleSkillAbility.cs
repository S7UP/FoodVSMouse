/// <summary>
/// 单纯用来原地发呆站桩的技能
/// </summary>
public class IdleSkillAbility : SkillAbility
{
    private int waitTime;
    private int waitTimeLeft;

    public IdleSkillAbility(BaseUnit master, int time):base(master)
    {
        waitTime = time;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 只要能量足够就可以释放
        return true;
    }

    public override void BeforeSpell()
    {
        waitTimeLeft = waitTime;
        master.SetActionState(new IdleState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (waitTimeLeft > 0)
            waitTimeLeft--;
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
        return waitTimeLeft <= 0;
    }

    public override void OnMeetCloseSpellingCondition()
    {

    }

    public override void AfterSpell()
    {

    }
}
