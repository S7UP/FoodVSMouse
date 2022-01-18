using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager
{
    public const int fps = 60; // 游戏帧数

    public ConfigManager()
    {
        Application.targetFrameRate = fps; // 设置游戏帧数
    }
}
