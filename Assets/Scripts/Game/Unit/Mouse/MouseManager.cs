using System.Collections.Generic;
/// <summary>
/// �������������̬�洢���������Ϣ��
/// </summary>
public class MouseManager
{
    private static Dictionary<MouseNameTypeMap, string> mouseTypeNameDict = new Dictionary<MouseNameTypeMap, string>() {
        { MouseNameTypeMap.NormalMouse, "�������"},
        { MouseNameTypeMap.StraddleMouse, "������" },
        { MouseNameTypeMap.KangarooMouse, "������"},
    //  { MouseNameTypeMap.DoorMouse, "���������"},
        { MouseNameTypeMap.LadderMouse, "������"},
        { MouseNameTypeMap.HealMouse, "��Ѫ��"},
        { MouseNameTypeMap.FlyMouse, "����������"},
        { MouseNameTypeMap.FlyBarrierMouse, "����·����"},
        { MouseNameTypeMap.FlySelfDestructMouse, "�����Ա���"},
        { MouseNameTypeMap.AerialBombardmentMouse, "����Ͷ����"},
        { MouseNameTypeMap.AirTransportMouse, "���к�ĸ" },
        { MouseNameTypeMap.NormalWaterMouse, "����ˮ��"},
        { MouseNameTypeMap.SubmarineMouse, "Ǳˮͧ��"},
        { MouseNameTypeMap.RowboatMouse, "��ͧ��"},
        { MouseNameTypeMap.FrogMouse, "����������"},
        { MouseNameTypeMap.CatapultMouse, "Ͷʯ����"},
        { MouseNameTypeMap.Mole, "������"},
        { MouseNameTypeMap.PenguinMouse, "�����"},
        { MouseNameTypeMap.ArsonMouse,  "�ݻ���"},
        { MouseNameTypeMap.PandaMouse, "��è��"},
        { MouseNameTypeMap.MagicMirrorMouse, "ħ����" },
    //{ MouseNameTypeMap.SecondMasterMouse, "��ү��"},
        { MouseNameTypeMap.NinjaMouse, "����������" },
        { MouseNameTypeMap.NinjaRetinueMouse, "���������"},
        { MouseNameTypeMap.PandaRetinueMouse, "��è�����"},
    //{ MouseNameTypeMap.ParatrooperMouse, "ɡ����"},
        { MouseNameTypeMap.SnailMouse, "��ţ��"},
        { MouseNameTypeMap.NonMainstreamMouse, "������"},
        { MouseNameTypeMap.BeeMouse, "��е����"},
        { MouseNameTypeMap.CanMouse, "��ͷ��"},
        { MouseNameTypeMap.WonderLandNormalMouse, "�澳��ͨ����"},
        { MouseNameTypeMap.WonderLandMole, "�澳���" },
        { MouseNameTypeMap.WonderLandFairy, "�澳������" }
    };

    private static Dictionary<MouseNameTypeMap, Dictionary<int, string>> mouseShapeNameDict = new Dictionary<MouseNameTypeMap, Dictionary<int, string>>() {
        // ��ͨ����
        { MouseNameTypeMap.NormalMouse, new Dictionary<int, string>(){
            { 0, "ƽ����"},
            { 1, "������"},
            { 2, "������"},
            { 3, "�ƹ�ƽ����"},
            { 4, "ƻ��������"},
            { 5, "�嵰������"},
            { 6, "��еƽ����"},
            { 7, "��е������"},
            { 8, "��е������"},
            { 9, "������"},
        }},
        // ������
        { MouseNameTypeMap.StraddleMouse, new Dictionary<int, string>(){
            { 0, "������"},
            { 1, "���ӻ�����"},
            { 2, "��е������"},
        }},
        // ������
        { MouseNameTypeMap.KangarooMouse, new Dictionary<int, string>(){
            { 0, "������"},
            { 1, "�ڿ���"},
            { 2, "������"},
        }},
        // ������
        { MouseNameTypeMap.LadderMouse, new Dictionary<int, string>(){
            { 1, "������"},
        }},
        // ��Ѫ��
        { MouseNameTypeMap.HealMouse, new Dictionary<int, string>(){
            { 0, "ħ����"},
            { 1, "������"},
            { 2, "�̻���"},
            { 3, "��ʬħ����"},
        }},
        // ����������
        { MouseNameTypeMap.FlyMouse, new Dictionary<int, string>(){
            { 0, "������"},
            { 1, "��Ʒ�ɱ���"},
            { 2, "�ձ���"},
            { 3, "��ʬ������"},
            { 4, "��ʬ��Ʒ�ɱ���"},
            { 5, "��ʬ�ձ���"},
            { 6, "��е������"},
        }},
        // ����·����
        { MouseNameTypeMap.FlyBarrierMouse, new Dictionary<int, string>(){
            { 0, "����·����"},
        }},
        // �����Ա���
        { MouseNameTypeMap.FlySelfDestructMouse, new Dictionary<int, string>(){
            { 0, "��绬����"},
        }},
        // ���к�ը��
        { MouseNameTypeMap.AerialBombardmentMouse, new Dictionary<int, string>(){
            { 0, "��еͶ����"},
        }},
        // ����������
        { MouseNameTypeMap.AirTransportMouse, new Dictionary<int, string>(){
            { 0, "���к�ĸ"},
        }},
        // ����ˮ��
        { MouseNameTypeMap.NormalWaterMouse, new Dictionary<int, string>(){
            { 0, "ֽ����"},
            { 1, "Ѽ��������"},
            { 2, "�ȴ���������"},
            { 3, "�ƹ�ֽ����"},
            { 4, "ƻ��Ѽ����"},
            { 5, "�嵰�ȴ�����"},
        }},
        // Ǳˮͧ
        { MouseNameTypeMap.SubmarineMouse, new Dictionary<int, string>(){
            { 0, "Ǳˮͧ��"},
            { 1, "����Ǳˮͧ��"},
            { 2, "ȭ��ȭ����"},
            { 3, "��ʬǱˮͧ��"},
            { 4, "��ʬ����Ǳˮͧ��"},
            { 5, "��ʬȭ��ȭ����"},
        }},
        // ��ͧ��
        { MouseNameTypeMap.RowboatMouse, new Dictionary<int, string>(){
            { 0, "��ͧ��"},
            { 1, "������"},
            { 2, "�콢��"},
            { 3, "��ʬ��ͧ��"},
            { 4, "��ʬ������"},
            { 5, "��ʬ�콢��"},
        }},
        // ����������
        { MouseNameTypeMap.FrogMouse, new Dictionary<int, string>(){
            { 0, "����������"},
            { 1, "��Ƥ������"},
        }},
        // ����
        { MouseNameTypeMap.Mole, new Dictionary<int, string>(){
            { 0, "����"},
            { 1, "��ҽ��"},
            { 2, "�⵶����"},
            { 3, "��ʬ����"},
            { 4, "��ʬ��ҽ��"},
            { 5, "��ʬ�⵶����"},
        }},
        // Ͷʯ����
        { MouseNameTypeMap.CatapultMouse, new Dictionary<int, string>(){
            { 0, "���̳���"},
            { 1, "�������"},
            { 2, "���׳���"},
        }},
        // �����
        { MouseNameTypeMap.PenguinMouse, new Dictionary<int, string>(){
            { 0, "�����"},
            { 1, "��ʬ�����"},
        }},
        // �ݻ���
        { MouseNameTypeMap.ArsonMouse, new Dictionary<int, string>(){
            { 0, "�ݻ���"},
            { 1, "��ʬ�ݻ���"},
        }},
        // ��è��
        { MouseNameTypeMap.PandaMouse, new Dictionary<int, string>(){
            { 0, "��è��"},
            { 1, "ˤ������"},
            { 2, "���ֱ���"},
            { 3, "��ʬ��è��"},
            { 4, "��ʬˤ������"},
            { 5, "��ʬ���ֱ���"},
        }},
        // ħ����
        { MouseNameTypeMap.MagicMirrorMouse, new Dictionary<int, string>(){
            { 0, "ħ����"},
        }},
        // ����������
        { MouseNameTypeMap.NinjaMouse, new Dictionary<int, string>(){
            { 0, "����������"},
            { 1, "��ʿ��"},
            { 2, "��Ӱ������"},
        }},
        // �����������
        { MouseNameTypeMap.NinjaRetinueMouse, new Dictionary<int, string>(){
            { 0, "���������"},
            { 1, "��ʿ�����"},
            { 2, "��Ӱ���������"},
        }},
        // ��è�����
        { MouseNameTypeMap.PandaRetinueMouse, new Dictionary<int, string>(){
            { 0, "��è�����"},
            { 1, "ˤ���������"},
            { 2, "���ֱ������"},
            { 3, "��ʬ��è�����"},
            { 4, "��ʬˤ���������"},
            { 5, "��ʬ���ֱ������"},
        }},
        // ��ţ����
        { MouseNameTypeMap.SnailMouse, new Dictionary<int, string>(){
            { 0, "��ţ����"},
            { 1, "��ʬ��ţ����"},
        }},
        // ��������
        { MouseNameTypeMap.NonMainstreamMouse, new Dictionary<int, string>(){
            { 0, "��������"},
            { 1, "��ʬ��������"},
        }},
        // ��е����
        { MouseNameTypeMap.BeeMouse, new Dictionary<int, string>(){
            { 0, "��е������"},
        }},
        // ��ͷ��
        { MouseNameTypeMap.CanMouse, new Dictionary<int, string>(){
            { 0, "��ͷ��"},
            { 1, "��ʬ��ͷ��"},
        }},
        // �澳��ͨ����
        { MouseNameTypeMap.WonderLandNormalMouse, new Dictionary<int, string>(){
            { 0, "��������"},
            { 1, "����������"},
        }},
        // �澳���
        { MouseNameTypeMap.WonderLandMole, new Dictionary<int, string>(){
            { 0, "�澳���" },
        }},
        // �澳����
        { MouseNameTypeMap.WonderLandFairy, new Dictionary<int, string>(){
            { 0, "�澳����" },
        }}
    };


    /// <summary>
    /// ��ȡ�����������������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<MouseNameTypeMap, string> GetMouseTypeNameDict()
    {
        return mouseTypeNameDict;
    }

    /// <summary>
    /// ��ȡĳ������������б��������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<int, string> GetShapeNameDict(MouseNameTypeMap type)
    {
        return mouseShapeNameDict[type];
    }
}
