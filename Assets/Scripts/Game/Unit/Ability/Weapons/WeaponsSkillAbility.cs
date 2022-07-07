/// <summary>
/// 武器技能存储实体
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
    /// 检测能否在死亡时使用
    /// </summary>
    /// <returns></returns>
    public override bool IsActiveInDeath()
    {
        return (master.IsAlive()) || canActiveInDeathState;
    }
}