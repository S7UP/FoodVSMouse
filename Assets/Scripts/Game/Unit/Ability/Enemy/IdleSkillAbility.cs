/// <summary>
/// ��������ԭ�ط���վ׮�ļ���
/// </summary>
public class IdleSkillAbility : SkillAbility
{
    private int waitTime;
    private int waitTimeLeft;

    public IdleSkillAbility(BaseUnit master, int time):base(master)
    {
        waitTime = time;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // ֻҪ�����㹻�Ϳ����ͷ�
        return true;
    }

    public override void BeforeSpell()
    {
        waitTimeLeft = waitTime;
        master.SetActionState(new IdleState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        if (waitTimeLeft > 0)
            waitTimeLeft--;
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
        return waitTimeLeft <= 0;
    }

    public override void OnMeetCloseSpellingCondition()
    {

    }

    public override void AfterSpell()
    {

    }
}
