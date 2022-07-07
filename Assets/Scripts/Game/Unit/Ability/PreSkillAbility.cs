using System;

/// <summary>
/// 一些需要引导类技能的前置技能，这里运用的逻辑是：
/// 引导类技能的CD表示引导时间
/// 前置技能用来记录引导类技能的真实CD与触发条件，且前置技能触发前原引导技能不回复能量
/// 触发后立即放开原引导技能的能量回复
/// </summary>
public class PreSkillAbility : SkillAbility
{
    public SkillAbility targetSkillAbility; // 原技能
    public Func<bool> isMeetSkillCondition; // 技能的施放条件，由持有技能的类具体定义
    public Action beforeSpell; // 施放前

    public PreSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public PreSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        if(isMeetSkillCondition!=null)
            return isMeetSkillCondition();
        return true;
    }

    public override void BeforeSpell()
    {
        // targetSkillAbility.enableEnergyRegeneration = true;
        targetSkillAbility.FullCurrentEnergy(); // 能量填充值满，使之可以被施放
        if (beforeSpell!=null)
            beforeSpell();
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
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

    /// <summary>
    /// 设置目标技能
    /// </summary>
    /// <param name="skillAbility"></param>
    public void SetTargetSkillAbility(SkillAbility skillAbility)
    {
        targetSkillAbility = skillAbility;
        skillAbility.enableEnergyRegeneration = false;
    }

    /// <summary>
    /// 设置技能的释放条件
    /// </summary>
    /// <param name="isMeetSkillCondition"></param>
    public void SetSkillCondition(Func<bool> isMeetSkillCondition)
    {
        this.isMeetSkillCondition = isMeetSkillCondition;
    }

    /// <summary>
    /// 设置技能释放前的事件
    /// </summary>
    /// <param name="action"></param>
    public void SetBeforeSkillAction(Action action)
    {
        beforeSpell = action;
    }
}
