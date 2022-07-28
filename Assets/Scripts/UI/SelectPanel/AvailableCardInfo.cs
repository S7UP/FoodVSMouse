/// <summary>
/// 可选择的卡的信息
/// </summary>
[System.Serializable]
public class AvailableCardInfo
{
    public int type; // 卡片种类
    public int maxShape; // 卡片转职上限
    public int maxLevel; // 卡片星级上限

    public AvailableCardInfo(int type, int maxShape, int maxLevel)
    {
        this.type = type;
        this.maxShape = maxShape;
        this.maxLevel = maxLevel;
    }
}
