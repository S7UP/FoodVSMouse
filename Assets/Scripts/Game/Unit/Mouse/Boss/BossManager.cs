using System.Collections.Generic;
/// <summary>
/// 存放有关BOSS的静态方法
/// </summary>
public class BossManager
{
    private static Dictionary<BossNameTypeMap, Dictionary<int, string>> bossNameDict = new Dictionary<BossNameTypeMap, Dictionary<int, string>>() 
    {
        { BossNameTypeMap.DongJun, new Dictionary<int, string>()
            {
                { 0, "洞君" }
            }
        },
        { BossNameTypeMap.ANuo, new Dictionary<int, string>()
            {
                { 0, "阿诺" }
            }
        },
        { BossNameTypeMap.Pharaoh1, new Dictionary<int, string>()
            { { 0, "法老"}
            }
        },
        { BossNameTypeMap.IceSlag, new Dictionary<int, string>()
            { { 0, "冰渣"}
            }
        },
        { BossNameTypeMap.Thundered, new Dictionary<int, string>()
            { { 0, "轰隆隆"}
            }
        },
        {
            BossNameTypeMap.PinkPaul, new Dictionary<int, string>()
            { { 0, "粉红保罗"}
            }
        },
        {
            BossNameTypeMap.BlondeMary, new Dictionary<int, string>()
            { { 0, "金发玛丽"}
            }
        },
        {
            BossNameTypeMap.SteelClawPete, new Dictionary<int, string>()
            { { 0, "钢爪皮特"}
            }
        },
        { BossNameTypeMap.RatTrain, new Dictionary<int, string>()
            {
                { 0, "列车初级"},
                { 1, "列车进化"},
                { 2, "列车终极"}
            }
        },
        { BossNameTypeMap.CaptainAmerica, new Dictionary<int, string>()
            { { 0, "美队鼠"} 
            } 
        },
    };

    private static ExcelManager.CSV _BossInfoCsv;
    private static Dictionary<BossNameTypeMap, Dictionary<int, int>> BossTypeShape_BossInfoCsvRowMap = new Dictionary<BossNameTypeMap, Dictionary<int, int>>(); // Boss种类变种对Boss信息表行的映射关系
    public static ExcelManager.CSV BossInfoCsv { get { if (_BossInfoCsv == null) Load(); return _BossInfoCsv; } }

    public static void Load()
    {
        if (_BossInfoCsv == null)
        {
            _BossInfoCsv = ExcelManager.ReadCSV("BossInfo", 7);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _BossInfoCsv.GetRow(); i++)
            {
                int _bossType;
                int bossShape;
                if (int.TryParse(_BossInfoCsv.GetValue(i, 0), out _bossType) && int.TryParse(_BossInfoCsv.GetValue(i, 1), out bossShape))
                {
                    BossNameTypeMap bossType = (BossNameTypeMap)_bossType;
                    if (!BossTypeShape_BossInfoCsvRowMap.ContainsKey(bossType))
                    {
                        BossTypeShape_BossInfoCsvRowMap.Add(bossType, new Dictionary<int, int>());
                    }
                    if (!BossTypeShape_BossInfoCsvRowMap[bossType].ContainsKey(bossShape))
                        BossTypeShape_BossInfoCsvRowMap[bossType].Add(bossShape, i);
                }
            }
        }
    }

    /// <summary>
    /// 获取BOSS信息在CSV里的行数，若找不到则返回-1
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public static int GetRowInBossInfoCsv(BossNameTypeMap type, int shape)
    {
        if (BossTypeShape_BossInfoCsvRowMap.ContainsKey(type) && BossTypeShape_BossInfoCsvRowMap[type].ContainsKey(shape))
            return BossTypeShape_BossInfoCsvRowMap[type][shape];
        else
            return -1;
    }

    public static int GetRowInBossInfoCsv(int type, int shape)
    {
        return GetRowInBossInfoCsv((BossNameTypeMap)type, shape);
    }

    public static string GetBossName(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        if (row > -1)
            return BossInfoCsv.GetValue(row, 2);
        else
            return "未知";
    }

    public static string GetBossName(int type, int shape)
    {
        return GetBossName((BossNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取BOSS的简略介绍
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetSimpleInfo(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        if (row > -1)
            return BossInfoCsv.GetValue(row, 3);
        else
            return "未知";
    }

    public static string GetSimpleInfo(int type, int shape)
    {
        return GetSimpleInfo((BossNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取BOSS的详细介绍
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetDetailedInfo(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        if (row > -1)
            return BossInfoCsv.GetValueByReplaceAllParam(row, 4);
        else
            return "未知";
    }

    public static string GetDetailedInfo(int type, int shape)
    {
        return GetDetailedInfo((BossNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取BOSS的参数字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, float[]> GetParamDict(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        return BossInfoCsv.GetParamDict(row, 4);
    }

    public static Dictionary<string, float[]> GetParamDict(int type, int shape)
    {
        return GetParamDict((BossNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取小贴士
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetTips(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        if (row > -1)
            return BossInfoCsv.GetValue(row, 5);
        else
            return "未知";
    }

    public static string GetTips(int type, int shape)
    {
        return GetTips((BossNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取背景故事
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetBackground(BossNameTypeMap type, int shape)
    {
        int row = GetRowInBossInfoCsv(type, shape);
        if (row > -1)
            return BossInfoCsv.GetValue(row, 6);
        else
            return "未知";
    }

    public static string GetBackground(int type, int shape)
    {
        return GetBackground((BossNameTypeMap)type, shape);
    }

    public static Dictionary<BossNameTypeMap, Dictionary<int, string>> GetBossNameDict()
    {
        return bossNameDict;
    }
}
