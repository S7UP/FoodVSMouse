using System;
/// <summary>
/// 安放弹簧
/// </summary>
public class PutLadderSkillAbility : SkillAbility
{
    private bool isSkilled;
    private bool canSkill;
    //private bool canTriggerEvent;
    private bool canEndSkill;
    private Action PutEvent;

    public PutLadderSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public PutLadderSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 能量足够即可
        return canSkill && !isSkilled;
    }

    public override void BeforeSpell()
    {
        master.SetActionState(new CastState(master));
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
        return canEndSkill;
    }

    public override void AfterSpell()
    {
        SetSkilled();
        canEndSkill = false;
        canSkill = false;
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// 触发技能
    /// </summary>
    public void TriggerSkill()
    {
        canSkill = true;
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    public void TriggerEvent()
    {
        if (PutEvent!=null)
            PutEvent();
    }

    /// <summary>
    /// 设置事件
    /// </summary>
    public void SetEvent(Action Event)
    {
        PutEvent = Event;
    }

    /// <summary>
    /// 是否放过技能了
    /// </summary>
    public bool IsSkilled()
    {
        return isSkilled;
    }

    /// <summary>
    /// 设置已放过技能
    /// </summary>
    public void SetSkilled()
    {
        isSkilled = true;
    }

    public void SetEndSkill()
    {
        canEndSkill = true;
    }
}
