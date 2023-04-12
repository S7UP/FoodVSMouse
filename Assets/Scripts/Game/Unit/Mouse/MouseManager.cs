using System.Collections.Generic;

using UnityEngine;

using static ExcelManager;
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

    public struct MouseAttribute
    {
        public string name;
        public float hp;
        public float attack;
        public float attackSpeed;
        public float moveSpeed;
        public float burnDefence;
        public float aoeDefence;
        public float attackPercent;
        public List<float> mHertRateList;
        public Dictionary<string, float[]> ParamDict;
    }

    private static Dictionary<MouseNameTypeMap, Dictionary<int, MouseAttribute>> attrDict = new Dictionary<MouseNameTypeMap, Dictionary<int, MouseAttribute>>();

    private static Dictionary<MouseNameTypeMap, MouseTypeInfo> mouseTypeInfoDict = new Dictionary<MouseNameTypeMap, MouseTypeInfo>();
    private static List<Vector2> mouseTypeShapeSeqList = new List<Vector2>(); // ��ȡExcelʱ�����ŵ�˳���
    private static Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>> mouseInfoDict = new Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>>();

    #region ������Ϣ��������
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
    #endregion

    #region �������Բ�������
    /// <summary>
    /// ��ȡ�������������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<MouseNameTypeMap, Dictionary<int, MouseAttribute>> GetAttributeDict()
    {
        return attrDict;
    }

    public static MouseAttribute GetAttribute(MouseNameTypeMap type, int shape)
    {
        return attrDict[type][shape];
    }

    public static MouseAttribute GetAttribute(int type, int shape)
    {
        return GetAttribute((MouseNameTypeMap)type, shape);
    }

    #endregion

    #region �ӱ��ض�ȡ�ķ���
    /// <summary>
    /// �ӱ��ؼ�����������ر�Ҫ����Ϣ
    /// </summary>
    public static void LoadAll()
    {
        LoadMouseTypeInfo();
        LoadMouseInfo();
        LoadMouseAttribute();
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
    /// �ӱ��ض�ȡ����������Ϣ
    /// </summary>
    private static void LoadMouseAttribute()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV("Attribute/Mouse", 12);
        int row = csv.GetRow();

        for (int i = 0; i < row; i++)
        {
            int _type, shape;
            int.TryParse(csv.GetValue(i, 0).ToString(), out _type);
            int.TryParse(csv.GetValue(i, 1).ToString(), out shape);
            MouseNameTypeMap type = (MouseNameTypeMap)_type;

            // ���� ���˽׶����� ��ת����List
            List<float> hertRateList = new List<float>();
            {
                string[] strList = csv.GetValue(i, 10).Split('/');
                foreach (var s in strList)
                {
                    float value;
                    if (float.TryParse(s, out value) && value > 0)
                        hertRateList.Add(value);
                }
            }

            // ���� ��ע ���У���ȡ����Ҫ����
            Dictionary<string, float[]> ParamDict = new Dictionary<string, float[]>();
            ParamManager.GetStringParamArray(csv.GetValue(i, 11), out ParamDict);
            // ����attribute
            MouseAttribute attr = new MouseAttribute()
            {
                name = csv.GetValue(i, 2),
                hp = float.Parse(csv.GetValue(i, 3)),
                attack = float.Parse(csv.GetValue(i, 4)),
                attackSpeed = float.Parse(csv.GetValue(i, 5)),
                moveSpeed = float.Parse(csv.GetValue(i, 6)),
                burnDefence = float.Parse(csv.GetValue(i, 7)),
                aoeDefence = float.Parse(csv.GetValue(i, 8)),
                attackPercent = float.Parse(csv.GetValue(i, 9)),
                mHertRateList = hertRateList,
                ParamDict = ParamDict
            };
            if(!attrDict.ContainsKey(type))
                attrDict.Add(type, new Dictionary<int, MouseAttribute>());
            attrDict[type].Add(shape, attr);
        }
    }
    #endregion

    #region һЩ�����
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

    /// <summary>
    /// Ŀ���Ƿ��ǳ��������һ�����ܴ�ͼ���￴���ģ�
    /// </summary>
    /// <returns></returns>
    public static bool IsGeneralMouse(BaseUnit u)
    {
        return u.mType >= 0;
    }
    #endregion
}
