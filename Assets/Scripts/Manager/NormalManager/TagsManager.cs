/// <summary>
/// ����������
/// </summary>
public class TagsManager
{
    /// <summary>
    /// Ŀ���Ƿ��ܵ��������١�������״̬�����ڴ���Ŀ�������
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
