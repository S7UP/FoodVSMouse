using System;

/// <summary>
/// һЩ��Ҫ�����༼�ܵ�ǰ�ü��ܣ��������õ��߼��ǣ�
/// �����༼�ܵ�CD��ʾ����ʱ��
/// ǰ�ü���������¼�����༼�ܵ���ʵCD�봥����������ǰ�ü��ܴ���ǰԭ�������ܲ��ظ�����
/// �����������ſ�ԭ�������ܵ������ظ�
/// </summary>
public class PreSkillAbility : SkillAbility
{
    public SkillAbility targetSkillAbility; // ԭ����
    public Func<bool> isMeetSkillCondition; // ���ܵ�ʩ���������ɳ��м��ܵ�����嶨��
    public Action beforeSpell; // ʩ��ǰ

    public PreSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public PreSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// �����ͷŵ�����
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
        targetSkillAbility.FullCurrentEnergy(); // �������ֵ����ʹ֮���Ա�ʩ��
        if (beforeSpell!=null)
            beforeSpell();
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    public override void AfterSpell()
    {
        
    }

    /// <summary>
    /// ����Ŀ�꼼��
    /// </summary>
    /// <param name="skillAbility"></param>
    public void SetTargetSkillAbility(SkillAbility skillAbility)
    {
        targetSkillAbility = skillAbility;
        skillAbility.enableEnergyRegeneration = false;
    }

    /// <summary>
    /// ���ü��ܵ��ͷ�����
    /// </summary>
    /// <param name="isMeetSkillCondition"></param>
    public void SetSkillCondition(Func<bool> isMeetSkillCondition)
    {
        this.isMeetSkillCondition = isMeetSkillCondition;
    }

    /// <summary>
    /// ���ü����ͷ�ǰ���¼�
    /// </summary>
    /// <param name="action"></param>
    public void SetBeforeSkillAction(Action action)
    {
        beforeSpell = action;
    }
}
