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
            foreach (var s in unit.statusAbilityManager.GetAllStatusAbility())
            {
                if (s is FrozenSlowStatusAbility)
                {
                    s.TryEndActivate();
                }
            }
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
        }
    }

    /// <summary>
    /// �Ƴ�����������йص�Ч��
    /// </summary>
    public static void RemoveAllSlowDownDebuff(BaseUnit unit)
    {
        foreach (var s in unit.statusAbilityManager.GetAllStatusAbility())
        {
            if(s is SlowStatusAbility || s is FrozenSlowStatusAbility)
            {
                s.TryEndActivate();
            }
        }
    }
}
