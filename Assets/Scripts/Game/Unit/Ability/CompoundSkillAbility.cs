using System.Collections.Generic;
using System;
/// <summary>
/// ���������ܴ洢ʵ��
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
    /// �ڼ����ڼ�ľ���ʵ��
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
    /// ���һ���ڼ����ڼ������
    /// </summary>
    public void AddSpellingFunc(Func<bool> func)
    {
        OnSpellingFuncList.Add(func);
    }
}