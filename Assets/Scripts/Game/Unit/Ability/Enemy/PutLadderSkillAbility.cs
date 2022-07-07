using System;
/// <summary>
/// ���ŵ���
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
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // �����㹻����
        return canSkill && !isSkilled;
    }

    public override void BeforeSpell()
    {
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {

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
    /// ��������
    /// </summary>
    public void TriggerSkill()
    {
        canSkill = true;
    }

    /// <summary>
    /// �����¼�
    /// </summary>
    public void TriggerEvent()
    {
        if (PutEvent!=null)
            PutEvent();
    }

    /// <summary>
    /// �����¼�
    /// </summary>
    public void SetEvent(Action Event)
    {
        PutEvent = Event;
    }

    /// <summary>
    /// �Ƿ�Ź�������
    /// </summary>
    public bool IsSkilled()
    {
        return isSkilled;
    }

    /// <summary>
    /// �����ѷŹ�����
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
