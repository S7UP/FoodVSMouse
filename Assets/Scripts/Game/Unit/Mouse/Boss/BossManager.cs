using System.Collections.Generic;
/// <summary>
/// 存放有关BOSS的静态方法
/// </summary>
public class BossManager
{
    private static Dictionary<BossNameTypeMap, Dictionary<int, string>> bossNameDict = new Dictionary<BossNameTypeMap, Dictionary<int, string>>() 
    {
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
