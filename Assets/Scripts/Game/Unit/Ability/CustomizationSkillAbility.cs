using System;
using System.Collections.Generic;
/// <summary>
/// 技能存储实体
/// </summary>
public class CustomizationSkillAbility : SkillAbility
{
    public Func<bool> IsMeetSkillConditionFunc;
    public Action BeforeSpellFunc;
    public Action OnSpellingFunc;
    public Action OnNoSpellingFunc;
    public Func<bool> IsMeetCloseSpellingConditionFunc;
    public Action AfterSpellFunc;
    private List<Action<SkillAbility>> AfterSpellActionList = new List<Action<SkillAbility>>();

    public CustomizationSkillAbility(BaseUnit master) : base(master)
    {

    }
    public CustomizationSkillAbility(BaseUnit master, SkillAbilityInfo info):base(master, info)
    {
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        if (IsMeetSkillConditionFunc == null)
            return false;
        return IsMeetSkillConditionFunc();
    }

    public override void BeforeSpell()
    {
        if (BeforeSpellFunc != null)
            BeforeSpellFunc();
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (OnSpellingFunc != null)
            OnSpellingFunc();
    }

    /// <summary>
    /// 在非技能期间
    /// </summary>
    public override void OnNoSpelling()
    {
        if (OnNoSpellingFunc != null)
            OnNoSpellingFunc();
    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        if(IsMeetCloseSpellingConditionFunc==null)
            return true;
        return IsMeetCloseSpellingConditionFunc();
    }

    public override void AfterSpell()
    {
        if (AfterSpellFunc != null)
            AfterSpellFunc();
        foreach (var action in AfterSpellActionList)
        {
            action(this);
        }
    }


    public void AddAfterSpellAction(Action<SkillAbility> action)
    {
        AfterSpellActionList.Add(action);
    }

    public void RemoveAfterSpellAction(Action<SkillAbility> action)
    {
        AfterSpellActionList.Remove(action);
    }
}