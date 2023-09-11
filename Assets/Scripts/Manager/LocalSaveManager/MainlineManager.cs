using System.Collections.Generic;
/// <summary>
/// 主线关卡管理者
/// </summary>
public class MainlineManager
{
    private const string path = "Stage/Mainline/";
    private const string localPath = "Mainline";
    private static bool isLoad; // 是否加载过了（防止重复加载）

    public static Dictionary<string, float> MainlineFirstPassExpDict { get { if (_MainlineFirstPassExpDict == null) Load(); return _MainlineFirstPassExpDict; } }
    private static Dictionary<string, float> _MainlineFirstPassExpDict;

    /// <summary>
    /// 加载全部静态的主线关卡信息
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // 加载首通时的等级给予经验值映射表
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
    /// 获取首通时的经验值加成,0代表查无此项
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
