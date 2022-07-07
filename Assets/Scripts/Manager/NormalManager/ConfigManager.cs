using UnityEngine;

public class ConfigManager
{
    public const int fps = 60; // 游戏帧数
    public static bool isEnableQuickReleaseCard; // 是否开启快捷放卡模式

    public ConfigManager()
    {
        isEnableQuickReleaseCard = true;
        Application.targetFrameRate = fps; // 设置游戏帧数
    }
}
