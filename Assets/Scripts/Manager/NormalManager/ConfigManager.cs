using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager
{
    public const int fps = 60; // ��Ϸ֡��

    public ConfigManager()
    {
        Application.targetFrameRate = fps; // ������Ϸ֡��
    }
}
