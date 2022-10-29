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

        { BossNameTypeMap.RatTrain, new Dictionary<int, string>()
            { { 2, "列车终极"}
            }
        },
        { BossNameTypeMap.CaptainAmerica, new Dictionary<int, string>()
            { { 0, "美队鼠"} 
            } 
        },
    };

    public static Dictionary<BossNameTypeMap, Dictionary<int, string>> GetBossNameDict()
    {
        return bossNameDict;
    }
}
