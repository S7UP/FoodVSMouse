using System.Collections.Generic;
/// <summary>
/// ����й�BOSS�ľ�̬����
/// </summary>
public class BossManager
{
    private static Dictionary<BossNameTypeMap, Dictionary<int, string>> bossNameDict = new Dictionary<BossNameTypeMap, Dictionary<int, string>>() 
    {
        { BossNameTypeMap.DongJun, new Dictionary<int, string>()
            {
                { 0, "����" }
            }
        },
        { BossNameTypeMap.Pharaoh1, new Dictionary<int, string>()
            { { 0, "����"}
            }
        },
        { BossNameTypeMap.IceSlag, new Dictionary<int, string>()
            { { 0, "����"}
            }
        },
        { BossNameTypeMap.Thundered, new Dictionary<int, string>()
            { { 0, "��¡¡"}
            }
        },

        { BossNameTypeMap.RatTrain, new Dictionary<int, string>()
            { { 2, "�г��ռ�"}
            }
        },
        { BossNameTypeMap.CaptainAmerica, new Dictionary<int, string>()
            { { 0, "������"} 
            } 
        },
    };

    public static Dictionary<BossNameTypeMap, Dictionary<int, string>> GetBossNameDict()
    {
        return bossNameDict;
    }
}
