/// <summary>
/// ��ѡ��Ŀ�����Ϣ
/// </summary>
[System.Serializable]
public class AvailableCardInfo
{
    public int type; // ��Ƭ����
    public int maxShape; // ��Ƭתְ����
    public int maxLevel; // ��Ƭ�Ǽ�����

    public AvailableCardInfo(int type, int maxShape, int maxLevel)
    {
        this.type = type;
        this.maxShape = maxShape;
        this.maxLevel = maxLevel;
    }
}
