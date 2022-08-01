/// <summary>
/// 存储武器信息的载体
/// </summary>
public class WeaponsInfo
{
    public int type;
    public int shape;

    private WeaponsInfo()
    {
    }

    public WeaponsInfo(int type, int shape)
    {
        this.type = type;
        this.shape = shape;
    }
}
