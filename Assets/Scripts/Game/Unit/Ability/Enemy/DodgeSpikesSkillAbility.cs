
using UnityEngine;
using System;

/// <summary>
/// 闪避突进
/// </summary>
public class DodgeSpikesSkillAbility : SkillAbility
{
    private bool isMeetSkillCondition; // 由外界控制满足技能触发条件
    private bool isMeetCloseSpellingCondition; // 由外界控制满足技能结束条件
    private Action EndEvent; // 结束事件

    public DodgeSpikesSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
        noClearEnergyWhenEnd = true;
    }

    public DodgeSpikesSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
        noClearEnergyWhenEnd = true;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return isMeetSkillCondition;
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
        return isMeetCloseSpellingCondition;
    }

    public override void AfterSpell()
    {
        // 重置变量，然后实现位移效果，位移效果基于目标标准移动速度
        // 在默认1.0标准移动速度的基础下，每完成一次技能位移1/12格
        // 但由于这个怪一直有200%的移速加成，即3.0标准移速，因此正常情况下一次技能应当位移1/4格
        master.transform.position += (Vector3)master.moveRotate * TransManager.TranToStandardVelocity(master.GetMoveSpeed()) * MapManager.gridWidth / 12;
        master.SetActionState(new MoveState(master));
        // 技力回满，以便下次马上使用
        FullCurrentEnergy();
        isMeetSkillCondition = false;
        isMeetCloseSpellingCondition = false;
        if (EndEvent != null)
            EndEvent();
    }

    public void SetSkillEnable()
    {
        isMeetSkillCondition = true;
    }

    public void EndSkill()
    {
        isMeetCloseSpellingCondition = true;
    }

    public void SetEndEvent(Action action)
    {
        this.EndEvent = action;
    }
}
