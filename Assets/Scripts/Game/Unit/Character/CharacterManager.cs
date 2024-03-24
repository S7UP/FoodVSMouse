using System.Collections.Generic;
/// <summary>
/// 角色信息管理（封装静态方法）
/// </summary>
public class CharacterManager
{
    private static Dictionary<CharacterNameShapeMap, string> CharacterNameDict = new Dictionary<CharacterNameShapeMap, string>()
    {
        { CharacterNameShapeMap.GeminiDress_F, "双子座礼服（女)"},
        { CharacterNameShapeMap.GeminiDress_M, "双子座礼服（男)"},
        { CharacterNameShapeMap.SummerFireflies_F, "夏日萤火（女）"},
        { CharacterNameShapeMap.SummerFireflies_M, "夏日萤火（男）"},
        { CharacterNameShapeMap.ForestPrincess_F, "森林公主"},
        { CharacterNameShapeMap.YingZhiLianQu_F, "樱之恋曲（女）"},
        { CharacterNameShapeMap.YingZhiLianQu_M, "樱之恋曲（男）"},
        { CharacterNameShapeMap.BabyMouse_F, "baby鼠cos"},
        { CharacterNameShapeMap.WhiteMouse_M, "一种白色的老鼠的cos"},
        { CharacterNameShapeMap.MeiLianHuaYu_F, "梦恋花语（女）" },
        { CharacterNameShapeMap.MeiLianHuaYu_M, "梦恋花语（男）" },
        { CharacterNameShapeMap.HuaJia_F, "花嫁" },
        { CharacterNameShapeMap.LiFu_M, "礼服" },
        { CharacterNameShapeMap.Juliet, "朱丽叶" },
        { CharacterNameShapeMap.Romeo, "罗密欧" },
        { CharacterNameShapeMap.MoYingShengYi_M, "魔影圣衣" },
        { CharacterNameShapeMap.MoYingShengYi_F, "魔影圣衣" },
        { CharacterNameShapeMap.TiLaMiShu, "提拉米苏" },
        { CharacterNameShapeMap.XueQiuTuTu, "雪球兔兔" },
        { CharacterNameShapeMap.BaiTuXianSheng, "白兔先生" },
        { CharacterNameShapeMap.FuGuLuoLiTa_F, "复古洛丽塔（女）" },
        { CharacterNameShapeMap.FuGuLuoLiTa_M, "复古洛丽塔（男）" },
        { CharacterNameShapeMap.MaoMao, "？？" },
        { CharacterNameShapeMap.JiaQiRuMeng, "佳期如梦" },
        { CharacterNameShapeMap.XianNv, "？？" },
        { CharacterNameShapeMap.TuTu, "？？" },
        { CharacterNameShapeMap.Aahao, "aahao" },
    };

    private static Dictionary<CharacterNameShapeMap, string> AuthorNameDict = new Dictionary<CharacterNameShapeMap, string>()
    {
        { CharacterNameShapeMap.GeminiDress_F, "芜不想"},
        { CharacterNameShapeMap.GeminiDress_M, "芜不想"},
        { CharacterNameShapeMap.BabyMouse_F, "芜不想"},
        { CharacterNameShapeMap.WhiteMouse_M, "白米饭"},
    };

    /// <summary>
    /// 获取全部套装的名字字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, string> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }

    /// <summary>
    /// 获取原作者名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetAuthorName(CharacterNameShapeMap type, out string name)
    {
        if (AuthorNameDict.ContainsKey(type))
        {
            name = AuthorNameDict[type];
            return true;
        }
        name = "unknow";
        return false;
    }
}