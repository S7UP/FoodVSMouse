using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 音乐管理器（静态存放音乐相关）
/// </summary>
public class SoundsManager
{
    private const string path = "Sounds"; // 文件目录

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
    /// 根据音效引用名获取音效信息
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
