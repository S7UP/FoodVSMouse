using System;

using UnityEngine;
/// <summary>
/// 玩家设置管理器
/// </summary>
public class ConfigManager
{
    private const string path = "Config"; // 其对应json文件相对路径
    private int fps = 60; // 游戏帧数
    private static bool isDeveloperMode = true; // 是否为开发者模式
    public Config mConfig;

    [Serializable]
    public class Config
    {
        public bool isPlayBGM; // 是否播放BGM
        public bool isPlaySE; // 是否播放音效
        public float BGMVolume; // BGM音量
        public float SEVolume;  // 音效音量
        public bool isEnableQuickReleaseCard; // 是否开启快捷放卡模式
        public bool isHideCardInfo; // 是否隐藏关内卡片信息
    }

    /// <summary>
    /// 初始化一个Config
    /// </summary>
    private Config GetInitialConfigInstance()
    {
        Config c = new Config();
        c.isPlayBGM = true;
        c.isPlaySE = true;
        c.BGMVolume = 0.5f;
        c.SEVolume = 0.5f;
        c.isEnableQuickReleaseCard = false;
        c.isHideCardInfo = false;
        return c;
    }

    public ConfigManager()
    {
        Application.targetFrameRate = fps; // 设置游戏帧数
        if (!JsonManager.TryLoadFromLocal(path, out mConfig))
        {
            RestoreTheDefaultSettings();
        }
        CheckAndRepairConfig(); // 能读取到的话就检测并尝试修复Config文件丢掉的东西
    }

    public void RestoreTheDefaultSettings()
    {
        // 读取不到时就原地生成一个，然后保存至本地
        mConfig = GetInitialConfigInstance();
        isDeveloperMode = false;
        Save();
    }

    public void Save()
    {
        JsonManager.SaveOnLocal(mConfig, path);
    }

    /// <summary>
    /// 检测并尝试修复Config文件丢掉的东西
    /// </summary>
    private void CheckAndRepairConfig()
    {

    }

    /// <summary>
    /// 是否为开发者模式
    /// </summary>
    /// <returns></returns>
    public static bool IsDeveloperMode()
    {
        return isDeveloperMode;
    }

    public static void SetDeveloperMode(bool enable)
    {
        isDeveloperMode = enable;
    }
}
