/// <summary>
/// BUFF管理器（提供静态方法）
/// </summary>
public class StatusManager
{
    /// <summary>
    /// 移除所有定身类控制效果
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveAllSettleDownDebuff(BaseUnit unit)
    {
        if(unit.IsAlive())
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
    }
}
