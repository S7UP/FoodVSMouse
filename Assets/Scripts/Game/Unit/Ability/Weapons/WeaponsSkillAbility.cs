/// <summary>
/// �������ܴ洢ʵ��
/// </summary>
public abstract class WeaponsSkillAbility : SkillAbility
{
    public new BaseWeapons master;
    public WeaponsSkillAbility()
    {

    }

    public WeaponsSkillAbility(BaseWeapons master, SkillAbilityInfo info)
    {
        this.master = master;
        Init(info.name, info.needEnergy, info.startEnergy, info.energyRegeneration, info.skillType, info.canActiveInDeathState, info.priority);
    }

    /// <summary>
    /// ����ܷ�������ʱʹ��
    /// </summary>
    /// <returns></returns>
    public override bool IsActiveInDeath()
    {
        return (master.IsAlive()) || canActiveInDeathState;
    }
}