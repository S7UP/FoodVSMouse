/// <summary>
/// 猫猫弹
/// </summary>
public class CatBullet : BaseBullet
{
    /// <summary>
    /// 击中目标单位时不造成伤害而是惊吓并将目标驱赶到其他行
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        if(baseUnit is MouseUnit)
        {
            // 如果目标已被惊吓或者不能被驱赶，则不会产生什么效果
            MouseUnit m = (MouseUnit)baseUnit;
            if (!m.NumericBox.GetBoolNumericValue(StringManager.BeFrightened) && !m.IsBoss())
            {
                if (m.CanDrivenAway())
                {
                    // 添加一个被惊吓的标签
                    m.NumericBox.AddDecideModifierToBoolDict(StringManager.BeFrightened, new BoolModifier(true));
                    m.DrivenAway();
                    new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, 0.04f * baseUnit.mMaxHp).ApplyAction();
                    KillThis();
                }
            }
        }
        else
        {
            KillThis();
        }
        
    }
}
