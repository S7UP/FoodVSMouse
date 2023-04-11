[System.Serializable]
/// <summary>
/// 音效信息
/// </summary>
public class SoundsInfo
{
    public string refenceName; // 引用名，相当于主键
    public string displayName; // 显示名，显示在游戏内的音效名
    public string author; // 作者
    public string resPath; // SE源文件存放地址（相对路径）
    public float volume;

    public SoundsInfo(string refenceName, string displayName, string author, string resPath, float volume)
    {
        this.refenceName = refenceName;
        this.displayName = displayName;
        this.author = author;
        this.resPath = resPath;
        this.volume = volume;
    }
}
