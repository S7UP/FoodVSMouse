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
            MouseUnit m = (MouseUnit)baseUnit;
            if (m.CanDrivenAway())
            {
                m.DrivenAway();
            }
        }
        KillThis();
    }
}
