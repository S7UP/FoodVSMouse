using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �������������̬�洢���������Ϣ��
/// </summary>
public class MouseManager
{
    /// <summary>
    /// ĳ�����������Ϣ����
    /// </summary>
    public struct MouseTypeInfo
    {
        public string name;
        public string descript;
        public string tip;
    }

    /// <summary>
    /// ĳ�������������Ϣ����
    /// </summary>
    public struct MouseInfo
    {
        public string name;
        public string descript;
        public string tip;
    }

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
        { MouseNameTypeMap.MagicianMouse, "ħ��ʦ��" },
        { MouseNameTypeMap.GhostMouse, "������" },
        { MouseNameTypeMap.BeeMouse, "��е����"},
        { MouseNameTypeMap.CanMouse, "��ͷ��"},
        { MouseNameTypeMap.WonderLandNormalMouse, "�澳��ͨ����"},
        { MouseNameTypeMap.WonderLandMole, "�澳���" },
        { MouseNameTypeMap.WonderLandFairy, "�澳������" },
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
            { 0, "������"},
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
        // ħ��ʦ��
        { MouseNameTypeMap.MagicianMouse, new Dictionary<int, string>(){
            { 0, "ħ��ʦ��" },
        }},
        // ������
        { MouseNameTypeMap.GhostMouse, new Dictionary<int, string>(){
            { 0, "������" },
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
        }},
    };



    private static Dictionary<MouseNameTypeMap, MouseTypeInfo> mouseTypeInfoDict = new Dictionary<MouseNameTypeMap, MouseTypeInfo>();
    private static List<Vector2> mouseTypeShapeSeqList = new List<Vector2>(); // ��ȡExcelʱ�����ŵ�˳���
    private static Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>> mouseInfoDict = new Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>>();

    /// <summary>
    /// ��ȡĳ�������������Ϣ
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MouseTypeInfo GetMouseTypeInfo(int type)
    {
        return GetMouseTypeInfo((MouseNameTypeMap)type);
    }

    /// <summary>
    /// ��ȡĳ�������������Ϣ
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MouseTypeInfo GetMouseTypeInfo(MouseNameTypeMap type)
    {
        return mouseTypeInfoDict[type];
    }


    /// <summary>
    /// ��ȡĳ�־����������Ϣ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static MouseInfo GetMouseInfo(int type, int shape)
    {
        return GetMouseInfo((MouseNameTypeMap)type, shape);
    }

    /// <summary>
    /// ��ȡĳ�־����������Ϣ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static MouseInfo GetMouseInfo(MouseNameTypeMap type, int shape)
    {
        return mouseInfoDict[type][shape];
    }


    /// <summary>
    /// ��ȡ�鱨�����ص�����˳���
    /// </summary>
    /// <returns></returns>
    public static List<Vector2> GetMouseTypeShapeSeqList()
    {
        return mouseTypeShapeSeqList;
    }

    /// <summary>
    /// �ӱ��ؼ�����������ر�Ҫ����Ϣ
    /// </summary>
    public static void LoadAll()
    {
        LoadMouseTypeInfo();
        LoadMouseInfo();
    }

    /// <summary>
    /// �ӱ��ض�ȡ����������Ϣ
    /// </summary>
    private static void LoadMouseTypeInfo()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV("MouseTypeInfo", 4);
        int row = csv.GetRow();

        for (int i = 0; i < row; i++)
        {
            int type;
            string name, descript, tip;
            int.TryParse(csv.GetValue(i, 0).ToString(), out type);
            name = csv.GetValue(i, 1).ToString();
            //descript = csv.GetValue(i, 2).ToString();
            descript = csv.GetValueByReplaceAllParam(i, 2);
            tip = csv.GetValue(i, 3).ToString();
            // ���뵽�ֵ��Դ洢
            MouseNameTypeMap mouseType = (MouseNameTypeMap)type;
            if (!mouseTypeInfoDict.ContainsKey(mouseType))
                mouseTypeInfoDict.Add(mouseType, new MouseTypeInfo() { name = name, descript = descript, tip = tip });
        }
    }

    /// <summary>
    /// �ӱ��ض�ȡ���������Ϣ
    /// </summary>
    private static void LoadMouseInfo()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV("MouseInfo", 5);
        int row = csv.GetRow();

        for (int i = 0; i < row; i++)
        {
            int type, shape;
            string name, descript, tip;
            int.TryParse(csv.GetValue(i, 0).ToString(), out type);
            int.TryParse(csv.GetValue(i, 1).ToString(), out shape);
            name = csv.GetValue(i, 2).ToString();
            descript = csv.GetValue(i, 3).ToString();
            tip = csv.GetValue(i, 4).ToString();
            // ���뵽����
            mouseTypeShapeSeqList.Add(new Vector2(type, shape));
            // ���뵽�ֵ��Դ洢
            MouseNameTypeMap mouseType = (MouseNameTypeMap)type;
            if (!mouseInfoDict.ContainsKey(mouseType))
                mouseInfoDict.Add(mouseType, new Dictionary<int, MouseInfo>());
            mouseInfoDict[mouseType].Add(shape, new MouseInfo() { name = name, descript = descript, tip = tip });
        }
    }

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

    /// <summary>
    /// ��ȡһֻ������������ը�Ĺ�������
    /// </summary>
    /// <returns></returns>
    public static MouseUnit GetBombedToolMouse()
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(0, new BaseEnemyGroup.EnemyInfo { type = 0, shape = 0 }).GetComponent<MouseUnit>();
        m.AddCanHitFunc((unit, bullet) =>
        {
            return false;
        });
        m.AddCanBlockFunc((u1, u2) =>
        {
            return false;
        });
        m.NumericBox.MoveSpeed.SetBase(0);
        m.SetAlpha(0);
        m.NumericBox.Defense.SetBase(100);
        m.HideEffect(true);
        return m;
    }

    /// <summary>
    /// Ѱ��ĳ��ĳ��Χ�����ҵĵ�λ
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public static BaseUnit FindTheMostRightTarget(BaseUnit unit, float minX, float maxX, int rowIndex)
    {
        BaseUnit target = null;
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ҵ���ָ������Ŀ�ѡȡ��λ
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(unit, item) && item.transform.position.x >= minX && item.transform.position.x <= maxX)
                list.Add(item);
        }
        // ȥ���������ĵ�λ
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                if (target == null || item.transform.position.x > target.transform.position.x)
                {
                    target = item;
                }
            }
        }
        return target;
    }

    /// <summary>
    /// Ѱ��ĳ��ĳ��Χ������ĵ�λ
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public static BaseUnit FindTheMostLeftTarget(BaseUnit unit, float minX, float maxX, int rowIndex)
    {
        BaseUnit target = null;
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ҵ���ָ������Ŀ�ѡȡ��λ
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(unit, item) && item.transform.position.x >= minX && item.transform.position.x <= maxX)
                list.Add(item);
        }
        // ȥ���������ĵ�λ
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                if (target == null || item.transform.position.x < target.transform.position.x)
                {
                    target = item;
                }
            }
        }
        return target;
    }
}
