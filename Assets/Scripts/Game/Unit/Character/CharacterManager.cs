using System.Collections.Generic;
/// <summary>
/// ��ɫ��Ϣ������װ��̬������
/// </summary>
public class CharacterManager
{
    private static Dictionary<CharacterNameShapeMap, List<string>> CharacterNameDict = new Dictionary<CharacterNameShapeMap, List<string>>()
    {
        { CharacterNameShapeMap.GeminiDress, new List<string>(){"˫���������Ů)","˫����������У�" }},
    };

    /// <summary>
    /// ��ȡȫ�������������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, List<string>> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }
}