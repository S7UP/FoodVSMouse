using UnityEngine;

public class ConfigManager
{
    public const int fps = 60; // ��Ϸ֡��
    public static bool isEnableQuickReleaseCard; // �Ƿ�����ݷſ�ģʽ

    public ConfigManager()
    {
        isEnableQuickReleaseCard = true;
        Application.targetFrameRate = fps; // ������Ϸ֡��
    }
}
