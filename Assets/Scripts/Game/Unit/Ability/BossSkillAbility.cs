/// <summary>
/// ��BOSSʹ�õļ��ܴ洢ʵ��
/// </summary>
public abstract class BossSkillAbility : SkillAbility
{
    public BossUnit bossMaster;

    public BossSkillAbility(BossUnit bossMaster, SkillAbilityInfo info) : base(bossMaster, info)
    {
        this.bossMaster = bossMaster;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return false;
    }

    public override void BeforeSpell()
    {

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
        return true;
    }

    public override void AfterSpell()
    {

    }
}