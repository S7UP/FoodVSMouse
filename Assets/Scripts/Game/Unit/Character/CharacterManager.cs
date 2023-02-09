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
        { CharacterNameShapeMap.MeiLianHuaYu_M, "梦恋花语（男）" }
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