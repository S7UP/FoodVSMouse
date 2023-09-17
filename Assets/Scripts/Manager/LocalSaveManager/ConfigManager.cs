using System;

using UnityEngine;
/// <summary>
/// ������ù�����
/// </summary>
public class ConfigManager
{
    private const string path = "Config"; // ���Ӧjson�ļ����·��
    private int fps = 60; // ��Ϸ֡��
    private static bool isDeveloperMode = true; // �Ƿ�Ϊ������ģʽ
    public Config mConfig;

    [Serializable]
    public class Config
    {
        public bool isPlayBGM; // �Ƿ񲥷�BGM
        public bool isPlaySE; // �Ƿ񲥷���Ч
        public float BGMVolume; // BGM����
        public float SEVolume;  // ��Ч����
        public bool isEnableQuickReleaseCard; // �Ƿ�����ݷſ�ģʽ
        public bool isHideCardInfo; // �Ƿ����ع��ڿ�Ƭ��Ϣ
    }

    /// <summary>
    /// ��ʼ��һ��Config
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
        Application.targetFrameRate = fps; // ������Ϸ֡��
        if (!JsonManager.TryLoadFromLocal(path, out mConfig))
        {
            RestoreTheDefaultSettings();
        }
        CheckAndRepairConfig(); // �ܶ�ȡ���Ļ��ͼ�Ⲣ�����޸�Config�ļ������Ķ���
    }

    public void RestoreTheDefaultSettings()
    {
        // ��ȡ����ʱ��ԭ������һ����Ȼ�󱣴�������
        mConfig = GetInitialConfigInstance();
        isDeveloperMode = false;
        Save();
    }

    public void Save()
    {
        JsonManager.SaveOnLocal(mConfig, path);
    }

    /// <summary>
    /// ��Ⲣ�����޸�Config�ļ������Ķ���
    /// </summary>
    private void CheckAndRepairConfig()
    {

    }

    /// <summary>
    /// �Ƿ�Ϊ������ģʽ
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
