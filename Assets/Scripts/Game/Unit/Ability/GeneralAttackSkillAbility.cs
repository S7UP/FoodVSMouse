/// <summary>
/// ��ͨ����
/// </summary>
public class GeneralAttackSkillAbility : SkillAbility
{
    public GeneralAttackSkillAbility(BaseUnit pmaster) :base(pmaster)
    {
        // Ĭ��ƽA������յ�λ�Ĺ���������
        UpdateNeedEnergyByAttackSpeed();
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
    }

    public GeneralAttackSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) :base(pmaster, info)
    {
        // Ĭ��ƽA������յ�λ�Ĺ�����������������info�����
        UpdateNeedEnergyByAttackSpeed();
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
    }

    /// <summary>
    /// ����Ŀ��Ĺ�����������Ҫ����ֵ
    /// </summary>
    private void UpdateNeedEnergyByAttackSpeed()
    {
        if (master.mBaseAttackSpeed > 0)
            needEnergy.SetBase(60 / master.mCurrentAttackSpeed);
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return master.IsMeetGeneralAttackCondition();
    }

    public override void BeforeSpell()
    {
        master.BeforeGeneralAttack();
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        master.OnGeneralAttack();
    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return master.IsMeetEndGeneralAttackCondition();
    }

    public override void AfterSpell()
    {
        master.AfterGeneralAttack();
        UpdateNeedEnergyByAttackSpeed();
    }
}
