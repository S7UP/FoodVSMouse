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
        if(unit.IsAlive())
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
    }
}
