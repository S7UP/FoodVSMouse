using System;
/// <summary>
/// 忍者鼠召唤小弟
/// </summary>
public class SummonRetinueSkillAbility : SkillAbility
{
    private bool openSummon; // 开始召唤
    private bool canSummon; // 可否召唤
    private Action SummnonEvent; // 召唤事件

    public SummonRetinueSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
    }

    public SummonRetinueSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 需要外界检测开启召唤
        return openSummon;
    }

    public override void BeforeSpell()
    {
        // 播动画
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (canSummon)
        {
            if (SummnonEvent != null)
                SummnonEvent();
            canSummon = false;
            openSummon = false;
        }
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
        return false;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// 开启召唤
    /// </summary>
    public void OpenSummon()
    {
        openSummon = true;
    }

    /// <summary>
    /// 外界召唤出来东西方法
    /// </summary>
    public void SetSummonEnable()
    {
        canSummon = true;
    }

    /// <summary>
    /// 外界设置召唤事件
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        SummnonEvent = action;
    }
}
