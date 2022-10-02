/// <summary>
/// BUFF���������ṩ��̬������
/// </summary>
public class StatusManager
{
    /// <summary>
    /// �Ƴ����ж��������Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveAllSettleDownDebuff(BaseUnit unit)
    {
        if (unit.IsAlive())
        {
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Stun);
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
        }  
    }

    /// <summary>
    /// �Ƴ�����������йص�Ч��
    /// </summary>
    public static void RemoveAllFrozenDebuff(BaseUnit unit)
    {
        if (unit.IsAlive())
        {
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.FrozenSlowDown);
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
        }
    }
}
