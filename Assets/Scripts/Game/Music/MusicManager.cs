using System.Collections.Generic;
/// <summary>
/// 音乐管理器（静态存放音乐相关）
/// </summary>
public class MusicManager
{
    private const string path = "MusicConfig"; // 文件目录

    [System.Serializable]
    /// <summary>
    /// 用于存在本地的json文件，可以设置背景音乐相关的信息
    /// </summary>
    public class MusicConfig {
        public List<MusicInfo> musicInfoList = new List<MusicInfo>();
    }

    public static MusicConfig musicConfig;

    public static void Load()
    {
        if (musicConfig == null)
            JsonManager.TryLoadFromResource(path, out musicConfig);
    }

    /// <summary>
    /// 根据音乐引用名获取音乐信息
    /// </summary>
    /// <param name="refenceName"></param>
    /// <returns></returns>
    public static MusicInfo GetMusicInfo(string refenceName)
    {
        foreach (var info in musicConfig.musicInfoList)
        {
            if (info.refenceName.Equals(refenceName))
                return info;
        }
        return null;
    }

}
