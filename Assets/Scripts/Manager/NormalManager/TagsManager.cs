/// <summary>
/// 词条管理器
/// </summary>
public class TagsManager
{
    /// <summary>
    /// 目标是否受到冰冻减速、冻结类状态（用于处理目标变蓝）
    /// </summary>
    public static bool IsUnitFrozen(BaseUnit unit)
    {
        if(unit.NumericBox.GetBoolNumericValue(StringManager.Frozen) || unit.NumericBox.GetBoolNumericValue(StringManager.FrozenSlowDown))
        {
            return true;
        }
        return false;
    }
}
