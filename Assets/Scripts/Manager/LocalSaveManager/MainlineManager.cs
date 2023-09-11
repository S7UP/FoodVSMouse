using System.Collections.Generic;
/// <summary>
/// ���߹ؿ�������
/// </summary>
public class MainlineManager
{
    private const string path = "Stage/Mainline/";
    private const string localPath = "Mainline";
    private static bool isLoad; // �Ƿ���ع��ˣ���ֹ�ظ����أ�

    public static Dictionary<string, float> MainlineFirstPassExpDict { get { if (_MainlineFirstPassExpDict == null) Load(); return _MainlineFirstPassExpDict; } }
    private static Dictionary<string, float> _MainlineFirstPassExpDict;

    /// <summary>
    /// ����ȫ����̬�����߹ؿ���Ϣ
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // ������ͨʱ�ĵȼ����辭��ֵӳ���
            {
                _MainlineFirstPassExpDict = new Dictionary<string, float>();
                ExcelManager.CSV csv = ExcelManager.ReadCSV("MainlineFirstPassExpMap", 2);
                for (int i = 0; i < csv.GetRow(); i++)
                {
                    _MainlineFirstPassExpDict.Add(csv.GetValue(i, 0), PlayerManager.GetNextLevelExp(int.Parse(csv.GetValue(i, 1))));
                }
            }
            isLoad = true;
        }
    }

    /// <summary>
    /// ��ȡ��ͨʱ�ľ���ֵ�ӳ�,0������޴���
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static float GetFirstPassExp(string id)
    {
        if (MainlineFirstPassExpDict.ContainsKey(id))
            return MainlineFirstPassExpDict[id];
        else
            return 0;
    }
}
