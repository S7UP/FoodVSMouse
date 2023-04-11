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

    public static void Load()
    {
        if (musicConfig == null)
            JsonManager.TryLoadFromResource(path, out musicConfig);
    }

    /// <summary>
    /// ����������������ȡ������Ϣ
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
