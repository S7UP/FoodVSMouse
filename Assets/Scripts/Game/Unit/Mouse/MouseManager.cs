using System.Collections.Generic;

using UnityEngine;

using static ExcelManager;
/// <summary>
/// 老鼠管理器（静态存储老鼠相关信息）
/// </summary>
public class MouseManager
{
    /// <summary>
    /// 某类型老鼠的信息描述
    /// </summary>
    public struct MouseTypeInfo
    {
        public string name;
        public string descript;
        public string tip;
    }

    /// <summary>
    /// 某个具体老鼠的信息描述
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
    private static List<Vector2> mouseTypeShapeSeqList = new List<Vector2>(); // 读取Excel时老鼠编号的顺序表
    private static Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>> mouseInfoDict = new Dictionary<MouseNameTypeMap, Dictionary<int, MouseInfo>>();

    #region 老鼠信息操作方法
    /// <summary>
    /// 获取某种类型老鼠的信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MouseTypeInfo GetMouseTypeInfo(int type)
    {
        return GetMouseTypeInfo((MouseNameTypeMap)type);
    }

    /// <summary>
    /// 获取某种类型老鼠的信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MouseTypeInfo GetMouseTypeInfo(MouseNameTypeMap type)
    {
        return mouseTypeInfoDict[type];
    }

    /// <summary>
    /// 获取某种具体老鼠的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static MouseInfo GetMouseInfo(int type, int shape)
    {
        return GetMouseInfo((MouseNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取某种具体老鼠的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static MouseInfo GetMouseInfo(MouseNameTypeMap type, int shape)
    {
        return mouseInfoDict[type][shape];
    }


    /// <summary>
    /// 获取情报岛加载的老鼠顺序表
    /// </summary>
    /// <returns></returns>
    public static List<Vector2> GetMouseTypeShapeSeqList()
    {
        return mouseTypeShapeSeqList;
    }
    #endregion

    #region 老鼠属性操作方法
    /// <summary>
    /// 获取所有老鼠属性字典
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

    #region 从本地读取的方法
    /// <summary>
    /// 从本地加载与老鼠相关必要的信息
    /// </summary>
    public static void LoadAll()
    {
        LoadMouseTypeInfo();
        LoadMouseInfo();
        LoadMouseAttribute();
    }

    /// <summary>
    /// 从本地读取老鼠种类信息
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
            // 加入到字典以存储
            MouseNameTypeMap mouseType = (MouseNameTypeMap)type;
            if (!mouseTypeInfoDict.ContainsKey(mouseType))
                mouseTypeInfoDict.Add(mouseType, new MouseTypeInfo() { name = name, descript = descript, tip = tip });
        }
    }

    /// <summary>
    /// 从本地读取老鼠具体信息
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
            // 加入到表中
            mouseTypeShapeSeqList.Add(new Vector2(type, shape));
            // 加入到字典以存储
            MouseNameTypeMap mouseType = (MouseNameTypeMap)type;
            if (!mouseInfoDict.ContainsKey(mouseType))
                mouseInfoDict.Add(mouseType, new Dictionary<int, MouseInfo>());
            mouseInfoDict[mouseType].Add(shape, new MouseInfo() { name = name, descript = descript, tip = tip });
        }
    }

    /// <summary>
    /// 从本地读取老鼠属性信息
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

            // 解析 受伤阶段那行 并转换成List
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

            // 解析 备注 那行，提取出重要参数
            Dictionary<string, float[]> ParamDict = new Dictionary<string, float[]>();
            ParamManager.GetStringParamArray(csv.GetValue(i, 11), out ParamDict);
            // 创建attribute
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

    #region 一些杂项方法
    /// <summary>
    /// 获取一只可能是用来挨炸的工具老鼠
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
    /// 寻找某行某范围内最右的单位
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public static BaseUnit FindTheMostRightTarget(BaseUnit unit, float minX, float maxX, int rowIndex)
    {
        BaseUnit target = null;
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0且大于指定坐标的可选取单位
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(unit, item) && item.transform.position.x >= minX && item.transform.position.x <= maxX)
                list.Add(item);
        }
        // 去找坐标最大的单位
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
    /// 寻找某行某范围内最左的单位
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public static BaseUnit FindTheMostLeftTarget(BaseUnit unit, float minX, float maxX, int rowIndex)
    {
        BaseUnit target = null;
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0且大于指定坐标的可选取单位
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(unit, item) && item.transform.position.x >= minX && item.transform.position.x <= maxX)
                list.Add(item);
        }
        // 去找坐标最大的单位
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
    /// 目标是否是常规的老鼠（一般是能从图鉴里看到的）
    /// </summary>
    /// <returns></returns>
    public static bool IsGeneralMouse(BaseUnit u)
    {
        return u.mType >= 0;
    }
    #endregion
}
