using System.Collections.Generic;
/// <summary>
/// 宝石管理器（封装静态方法）
/// </summary>
public class JewelManager
{
    private static bool isLoad;
    /// <summary>
    /// 当前版本的可选宝石
    /// </summary>
    private static List<JewelNameTypeMap> currentVersionJewelList = new List<JewelNameTypeMap>()
    {
        JewelNameTypeMap.Ice, JewelNameTypeMap.Sacred, JewelNameTypeMap.Bombing, JewelNameTypeMap.Laser, JewelNameTypeMap.CatEyes,
        JewelNameTypeMap.Attack, JewelNameTypeMap.AttackSpeed,
    };
    private static ExcelManager.CSV _csv;
    public static ExcelManager.CSV csv { get { if (_csv == null) Load(); return _csv; } }

    public static void Load()
    {
        if (!isLoad)
        {
            _csv = ExcelManager.ReadCSV("JewelInfo", 5);
            isLoad = true;
        }
    }

    /// <summary>
    /// 传入的宝石号是否合法（就是当前版本有没有，或者等级够不够解锁这个）
    /// </summary>
    /// <returns></returns>
    public static bool IsJewelValid(int index)
    {
        foreach (var type in currentVersionJewelList)
        {
            if (index == (int)type)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取宝石名字
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetName(int type)
    {
        return csv.GetValue(type, 1);
    }

    public static string GetName(JewelNameTypeMap type)
    {
        return GetName((int)type);
    }

    /// <summary>
    /// 获取宝石信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetInfo(int type)
    {
        return csv.GetValueByReplaceAllParam(type, 2);
    }

    public static string GetInfo(JewelNameTypeMap type)
    {
        return GetInfo((int)type);
    }

    /// <summary>
    /// 获取宝石初始技力
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float GetStartEnergy(int type)
    {
        return float.Parse(csv.GetValue(type, 3));
    }

    public static float GetStartEnergy(JewelNameTypeMap type)
    {
        return GetStartEnergy((int)type);
    }

    /// <summary>
    /// 获取宝石最大技力
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float GetMaxEnergy(int type)
    {
        return float.Parse(csv.GetValue(type, 4));
    }

    /// <summary>
    /// 获取宝石最大技力
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float GetMaxEnergy(JewelNameTypeMap type)
    {
        return GetMaxEnergy((int)type);
    }

    public static Dictionary<string, float[]> GetParamDict(int type)
    {
        return csv.GetParamDict(type, 2);
    }

    /// <summary>
    /// 获取技能的一个实例
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BaseJewelSkill GetSkillInstance(int type)
    {
        if (type > -1)
        {
            float maxEnergy = GetMaxEnergy(type);
            float startEnergy = GetStartEnergy(type);
            float deltaEnergy = 1.0f / 60;
            Dictionary<string, float[]> paramDict = GetParamDict(type);

            switch (type)
            {
                case 0: return new IceJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 1: return new SacredJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 2: return new BombingJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 3: return new LaserJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 4: return new CatEyesJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 5: return new AttackJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                case 6: return new AttackSpeedJewelSkill(maxEnergy, startEnergy, deltaEnergy, paramDict);
                default:
                    break;
            }
        }
        return new BaseJewelSkill(120, 0, 1, null);
    }

    /// <summary>
    /// 获取当前版本全部宝石
    /// </summary>
    /// <returns></returns>
    public static List<JewelNameTypeMap> GetJewelList()
    {
        return currentVersionJewelList;
    }
}
