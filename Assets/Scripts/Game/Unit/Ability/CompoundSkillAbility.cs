using System.Collections.Generic;
using System;
/// <summary>
/// 复合任务技能存储实体
/// </summary>
public class CompoundSkillAbility : CustomizationSkillAbility
{
    private List<Func<bool>> OnSpellingFuncList = new List<Func<bool>>();
    private int index = 0;

    public CompoundSkillAbility(BaseUnit master):base(master)
    {

    }
    public CompoundSkillAbility(BaseUnit master, SkillAbilityInfo info):base(master, info)
    {
    }

    public override void BeforeSpell()
    {
        index = 0;
        base.BeforeSpell();
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (index >= OnSpellingFuncList.Count)
            return;
        if (OnSpellingFuncList[index]())
            index++;
    }

    public override bool IsMeetCloseSpellingCondition()
    {
        return index >= OnSpellingFuncList.Count;
    }

    /// <summary>
    /// 添加一个在技能期间的任务
    /// </summary>
    public void AddSpellingFunc(Func<bool> func)
    {
        OnSpellingFuncList.Add(func);
    }
}