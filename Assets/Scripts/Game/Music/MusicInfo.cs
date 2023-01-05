using System;
[Serializable]
/// <summary>
/// 音乐信息
/// </summary>
public class MusicInfo
{
    public string refenceName; // 引用名，相当于主键
    public string displayName; // 显示名，显示在游戏内的音乐名
    public string author; // 作者
    public string resPath; // BGM源文件存放地址（相对路径）
    public float loopStartTime; // 循环播放时起始点（秒）
}
