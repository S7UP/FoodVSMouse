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
            MouseUnit m = (MouseUnit)baseUnit;
            if (m.CanDrivenAway())
            {
                m.DrivenAway();
            }
        }
        KillThis();
    }
}
