using System.Collections.Generic;
/// <summary>
/// ���ֹ���������̬���������أ�
/// </summary>
public class MusicManager
{
    private const string path = "MusicConfig"; // �ļ�Ŀ¼

    [System.Serializable]
    /// <summary>
    /// ���ڴ��ڱ��ص�json�ļ����������ñ���������ص���Ϣ
    /// </summary>
    public class MusicConfig {
        public List<MusicInfo> musicInfoList = new List<MusicInfo>();
    }

    public static MusicConfig musicConfig;

    /// <summary>
    /// ��̬���췽������������ʵ���������κξ�̬��Ա֮ǰ����̬���캯�����Զ�ִ�У�����ִֻ��һ�Ρ�
    /// </summary>
    //static MusicManager()
    //{
    //    // �ӱ��ؼ���musicConfig
    //    // musicConfig = JsonManager.Load<MusicConfig>(path);
        
    //    //CheckAndRepairFile();
    //}

    /// <summary>
    /// �����Լ��޸��Ա�֤�ļ���������
    /// </summary>
    //private static void CheckAndRepairFile()
    //{
    //    if(musicConfig == null)
    //    {
    //        musicConfig = new MusicConfig();
    //        SaveMusicConfig();
    //    }
    //}

    //public static void SaveMusicConfig()
    //{
    //    JsonManager.Save<MusicConfig>(musicConfig, path);
    //}

    /// <summary>
    /// ����������������ȡ������Ϣ
    /// </summary>
    /// <param name="refenceName"></param>
    /// <returns></returns>
    public static MusicInfo GetMusicInfo(string refenceName)
    {
        if(musicConfig == null)
            JsonManager.TryLoadFromResource(path, out musicConfig);
        foreach (var info in musicConfig.musicInfoList)
        {
            if (info.refenceName.Equals(refenceName))
                return info;
        }
        return null;
    }

}
