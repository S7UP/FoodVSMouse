/// <summary>
/// èè��
/// </summary>
public class CatBullet : BaseBullet
{
    /// <summary>
    /// ����Ŀ�굥λʱ������˺����Ǿ��Ų���Ŀ�����ϵ�������
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        if(baseUnit is MouseUnit)
        {
            // ���Ŀ���ѱ����Ż��߲��ܱ����ϣ��򲻻����ʲôЧ��
            MouseUnit m = (MouseUnit)baseUnit;
            if (!m.NumericBox.GetBoolNumericValue(StringManager.BeFrightened) && !m.IsBoss())
            {
                if (m.CanDrivenAway())
                {
                    // ���һ�������ŵı�ǩ
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
