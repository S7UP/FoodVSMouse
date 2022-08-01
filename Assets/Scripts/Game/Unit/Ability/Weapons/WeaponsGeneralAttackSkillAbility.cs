/// <summary>
/// 武器的普通攻击
/// </summary>
public class WeaponsGeneralAttackSkillAbility : WeaponsSkillAbility
{

    public WeaponsGeneralAttackSkillAbility(BaseWeapons w, SkillAbilityInfo info) :base(w, info)
    {
        noClearEnergyWhenStart = false;
        noClearEnergyWhenEnd = true;
    }

    /// <summary>
    /// 满足释放的条件
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
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        master.OnGeneralAttack();
    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
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
