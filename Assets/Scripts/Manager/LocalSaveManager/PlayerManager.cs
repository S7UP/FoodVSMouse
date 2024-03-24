// 玩家的管理，负责保存以及加载各种玩家以及游戏的信息

public class PlayerManager
{
    public const int version = 5; // 当前游戏玩家应该的存档版本号，每次启动游戏时检测玩家的存档版本号，若低于该值，则启用玩家存档更新功能 
    public const int maxLevel = 39; // 当前最大等级
    // 存储经验值的表
    private static ExcelManager.CSV _ExpCsv;
    public static ExcelManager.CSV ExpCsv { get { if (_ExpCsv == null) Load(); return _ExpCsv; } }

    public static void Load()
    {
        if (_ExpCsv == null)
        {
            _ExpCsv = ExcelManager.ReadCSV("Exp", 2);
        }
    }

    /// <summary>
    /// 获取传入等级升到下一级所需的经验值
    /// </summary>
    /// <param name="currentLevel">当前等级</param>
    /// <returns>若传入等级非法则返回-1</returns>
    public static float GetNextLevelExp(int currentLevel)
    {
        // 注意第一行是1级，但下标是0，所以在取数组时需要-1
        float exp;
        if (currentLevel <= ExpCsv.GetRow() && currentLevel > 0 && float.TryParse(ExpCsv.GetValue(currentLevel - 1, 1), out exp))
            return exp;
        return -1;
    }

}
