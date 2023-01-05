using System;
using System.Collections.Generic;
/// <summary>
/// 音乐管理器（静态存放音乐相关）
/// </summary>
public class MusicManager
{
    private const string path = "MusicConfig"; // 文件目录

    [Serializable]
    /// <summary>
    /// 用于存在本地的json文件，可以设置背景音乐相关的信息
    /// </summary>
    public class MusicConfig {
        public List<MusicInfo> musicInfoList = new List<MusicInfo>();

        public MusicConfig()
        {
            
        }
    }

    public static MusicConfig musicConfig;

    /// <summary>
    /// 静态构造方法，当创建类实例或引用任何静态成员之前，静态构造函数被自动执行，并且只执行一次。
    /// </summary>
    static MusicManager()
    {
        // 从本地加载musicConfig
        musicConfig = JsonManager.Load<MusicConfig>(path);
        CheckAndRepairFile();
    }

    /// <summary>
    /// 检验以及修复以保证文件的完整性
    /// </summary>
    private static void CheckAndRepairFile()
    {
        if(musicConfig == null)
        {
            musicConfig = new MusicConfig();
            SaveMusicConfig();
        }
    }

    public static void SaveMusicConfig()
    {
        JsonManager.Save<MusicConfig>(musicConfig, path);
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
