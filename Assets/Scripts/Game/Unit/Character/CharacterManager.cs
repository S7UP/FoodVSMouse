using System.Collections.Generic;
/// <summary>
/// ��ɫ��Ϣ��������װ��̬������
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
        { CharacterNameShapeMap.YingZhiLianQu_M, "ӣ֮�������У�"},
        { CharacterNameShapeMap.BabyMouse_F, "baby��cos"},
        { CharacterNameShapeMap.WhiteMouse_M, "һ�ְ�ɫ�������cos"},
        { CharacterNameShapeMap.MeiLianHuaYu_F, "�������Ů��" },
        { CharacterNameShapeMap.MeiLianHuaYu_M, "��������У�" },
        { CharacterNameShapeMap.HuaJia_F, "����" },
        { CharacterNameShapeMap.LiFu_M, "���" },
        { CharacterNameShapeMap.Juliet, "����Ҷ" },
        { CharacterNameShapeMap.Romeo, "����ŷ" },
        { CharacterNameShapeMap.MoYingShengYi_M, "ħӰʥ��" },
        { CharacterNameShapeMap.MoYingShengYi_F, "ħӰʥ��" },
        { CharacterNameShapeMap.TiLaMiShu, "��������" },
        { CharacterNameShapeMap.XueQiuTuTu, "ѩ������" },
        { CharacterNameShapeMap.BaiTuXianSheng, "��������" },
        { CharacterNameShapeMap.FuGuLuoLiTa_F, "������������Ů��" },
        { CharacterNameShapeMap.FuGuLuoLiTa_M, "�������������У�" },
        { CharacterNameShapeMap.MaoMao, "����" },
        { CharacterNameShapeMap.JiaQiRuMeng, "��������" },
        { CharacterNameShapeMap.XianNv, "����" },
        { CharacterNameShapeMap.TuTu, "����" },
        { CharacterNameShapeMap.Aahao, "aahao" },
    };

    private static Dictionary<CharacterNameShapeMap, string> AuthorNameDict = new Dictionary<CharacterNameShapeMap, string>()
    {
        { CharacterNameShapeMap.GeminiDress_F, "�߲���"},
        { CharacterNameShapeMap.GeminiDress_M, "�߲���"},
        { CharacterNameShapeMap.BabyMouse_F, "�߲���"},
        { CharacterNameShapeMap.WhiteMouse_M, "���׷�"},
    };

    /// <summary>
    /// ��ȡȫ����װ�������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<CharacterNameShapeMap, string> GetCharacterNameDict()
    {
        return CharacterNameDict;
    }

    /// <summary>
    /// ��ȡԭ��������
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