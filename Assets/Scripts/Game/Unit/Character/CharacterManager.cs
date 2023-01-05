using System.Collections.Generic;
/// <summary>
/// ��ɫ��Ϣ������װ��̬������
/// </summary>
public class CharacterManager
{
    private static Dictionary<CharacterNameShapeMap, string> CharacterNameDict = new Dictionary<CharacterNameShapeMap, string>()
    {
        { CharacterNameShapeMap.GeminiDress_F, "˫���������Ů)"},
        { CharacterNameShapeMap.GeminiDress_M, "˫�����������)"},
        { CharacterNameShapeMap.SummerFireflies_F, "����ө��Ů��"},
        { CharacterNameShapeMap.SummerFireflies_M, "����ө���У�"},
        { CharacterNameShapeMap.ForestPrincess_F, "ɭ�ֹ���"},
        { CharacterNameShapeMap.YingZhiLianQu_F, "ӣ֮������Ů��"},
        { CharacterNameShapeMap.BabyMouse_F, "baby��cos"},
        { CharacterNameShapeMap.WhiteMouse_M, "һ�ְ�ɫ�������cos"}
    };

    /// <summary>
    /// ��ȡȫ����װ�������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, string> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }
}