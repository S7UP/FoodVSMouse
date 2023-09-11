[System.Serializable]
/// <summary>
/// 音乐信息
/// </summary>
public class MusicInfo
{
    public string refenceName; // 引用名，相当于主键
    public string displayName; // 显示名，显示在游戏内的音乐名
    public string author; // 作者
    public string resPath; // BGM源文件存放地址（相对路径）
    public float startTime; // 第一次播放起点
    public float loopStartTime; // 循环播放时起始点（秒）
    public float loopEndTime = -1; // 循环播放的断点(-1为未设置断点，默认为结尾处）
    public float volume;
}
