/// <summary>
/// ��������ͨ����
/// </summary>
public class WeaponsGeneralAttackSkillAbility : WeaponsSkillAbility
{

    public WeaponsGeneralAttackSkillAbility(BaseWeapons w, SkillAbilityInfo info) :base(w, info)
    {
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
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
    }
}
