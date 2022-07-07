using System;
/// <summary>
/// �������ٻ�С��
/// </summary>
public class SummonRetinueSkillAbility : SkillAbility
{
    private bool openSummon; // ��ʼ�ٻ�
    private bool canSummon; // �ɷ��ٻ�
    private Action SummnonEvent; // �ٻ��¼�

    public SummonRetinueSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
    }

    public SummonRetinueSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // ��Ҫ����⿪���ٻ�
        return openSummon;
    }

    public override void BeforeSpell()
    {
        // ������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
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
    /// �ڷǼ����ڼ�
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
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
    /// �����ٻ�
    /// </summary>
    public void OpenSummon()
    {
        openSummon = true;
    }

    /// <summary>
    /// ����ٻ�������������
    /// </summary>
    public void SetSummonEnable()
    {
        canSummon = true;
    }

    /// <summary>
    /// ��������ٻ��¼�
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        SummnonEvent = action;
    }
}
