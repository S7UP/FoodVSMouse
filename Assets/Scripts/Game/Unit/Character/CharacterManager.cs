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
        { CharacterNameShapeMap.BabyMouse_F, "baby鼠cos"},
        { CharacterNameShapeMap.WhiteMouse_M, "一种白色的老鼠的cos"}
    };

    /// <summary>
    /// 获取全部套装的名字字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, string> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }
}