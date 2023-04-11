using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ���ֹ���������̬���������أ�
/// </summary>
public class SoundsManager
{
    private const string path = "Sounds"; // �ļ�Ŀ¼

    private static List<SoundsInfo> soundsInfoList = new List<SoundsInfo>();

    public static IEnumerator Load()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV(path, 5);
        for (int i = 0; i < csv.GetRow(); i++)
        {
            SoundsInfo info = new SoundsInfo(csv.GetValue(i, 0), csv.GetValue(i, 1), csv.GetValue(i, 2), csv.GetValue(i, 3), float.Parse(csv.GetValue(i, 4)));
            soundsInfoList.Add(info);
            yield return AudioSourceManager.AsyncLoadEffectMusic(info.refenceName);
        }
    }

    /// <summary>
    /// ������Ч��������ȡ��Ч��Ϣ
    /// </summary>
    /// <param name="refenceName"></param>
    /// <returns></returns>
    public static SoundsInfo GetSoundsInfo(string refenceName)
    {
        foreach (var info in soundsInfoList)
        {
            if (info.refenceName.Equals(refenceName))
                return info;
        }
        return null;
    }

}
