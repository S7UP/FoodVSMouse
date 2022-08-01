using System.Collections.Generic;
/// <summary>
/// 角色信息管理（封装静态方法）
/// </summary>
public class CharacterManager
{
    private static Dictionary<CharacterNameShapeMap, List<string>> CharacterNameDict = new Dictionary<CharacterNameShapeMap, List<string>>()
    {
        { CharacterNameShapeMap.GeminiDress, new List<string>(){"双子座礼服（女)","双子座礼服（男）" }},
    };

    /// <summary>
    /// 获取全部武器的名字字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, List<string>> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }
}