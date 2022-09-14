using System;
/// <summary>
/// ���ܴ洢ʵ��
/// </summary>
public class CustomizationSkillAbility : SkillAbility
{
    public Func<bool> IsMeetSkillConditionFunc;
    public Action BeforeSpellFunc;
    public Action OnSpellingFunc;
    public Action OnNoSpellingFunc;
    public Func<bool> IsMeetCloseSpellingConditionFunc;
    public Action AfterSpellFunc;

    public CustomizationSkillAbility(BaseUnit master) : base(master)
    {

    }
    public CustomizationSkillAbility(BaseUnit master, SkillAbilityInfo info):base(master, info)
    {
    }

    /// <summary>
    /// �����ͷŵ�����
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
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        if (OnSpellingFunc != null)
            OnSpellingFunc();
    }

    /// <summary>
    /// �ڷǼ����ڼ�
    /// </summary>
    public override void OnNoSpelling()
    {
        if (OnNoSpellingFunc != null)
            OnNoSpellingFunc();
    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
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
    }
}