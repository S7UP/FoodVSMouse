using System.Collections.Generic;
/// <summary>
/// 武器管理器（封装静态方法）
/// </summary>
public class WeaponsManager
{
    private const string localPath = "WeaponsUnlock";

    /// <summary>
    /// 当前版本可使用的武器
    /// </summary>
    private static List<WeaponsNameTypeMap> currentVersionWeaponsList = new List<WeaponsNameTypeMap>() { 
        WeaponsNameTypeMap.BunGun, WeaponsNameTypeMap.WaterPipeGun, WeaponsNameTypeMap.ThreeLineGun,
        WeaponsNameTypeMap.IceGun, WeaponsNameTypeMap.CatGun, WeaponsNameTypeMap.IceSpoonCrossbowGun
    };
    private static ExcelManager.CSV _InfoCsv;
    private static Dictionary<WeaponsNameTypeMap, int> WeaponsType_InfoCsvRowMap = new Dictionary<WeaponsNameTypeMap, int>(); // 武器种类对武器信息表行的映射关系
    public static ExcelManager.CSV InfoCsv { get { if (_InfoCsv == null) Load(); return _InfoCsv; } }

    /// <summary>
    /// 存放在本地的功能解锁存档
    /// </summary>
    [System.Serializable]
    public class UnlockSaveInLocal
    {
        // key为武器int  bool 为解锁情况
        public Dictionary<WeaponsNameTypeMap, bool> dict = new Dictionary<WeaponsNameTypeMap, bool>();
    }

    // 玩家本地的解锁情况
    public static UnlockSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static UnlockSaveInLocal _PlayerSave;

    public static void Load()
    {
        if (_InfoCsv == null)
        {
            _InfoCsv = ExcelManager.ReadCSV("WeaponsInfo", 5);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _InfoCsv.GetRow(); i++)
            {
                int _type;
                if (int.TryParse(_InfoCsv.GetValue(i, 0), out _type))
                {
                    WeaponsNameTypeMap type = (WeaponsNameTypeMap)_type;
                    if (!WeaponsType_InfoCsvRowMap.ContainsKey(type))
                    {
                        WeaponsType_InfoCsvRowMap.Add(type, i);
                    }
                }
            }
        }
        // 读取玩家存档
        if (_PlayerSave == null)
        {
            if (!JsonManager.TryLoadFromLocal(localPath, out _PlayerSave))
            {
                _PlayerSave = new UnlockSaveInLocal();
                Save();
            }
        }
    }

    /// <summary>
    /// 保存一次本地的存档
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<UnlockSaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// 某编号武器是否已解锁
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsUnlock(WeaponsNameTypeMap id)
    {
        if (!PlayerSave.dict.ContainsKey(id))
        {
            PlayerSave.dict.Add(id, false);
            Save();
        }
        return PlayerSave.dict[id];
    }

    /// <summary>
    /// 尝试解锁某些功能
    /// </summary>
    public static bool TryUnlock(WeaponsNameTypeMap id)
    {
        if (!IsUnlock(id))
        {
            // 解锁两个条件：等级与特殊条件（见字典）
            if (PlayerData.GetInstance().GetLevel() >= GetUnlockLevel(id))
            {
                PlayerSave.dict[id] = true;
                Save();
                return true;
            }
        }
        // 如果该功能早已解锁，这次解锁视为失败
        return false;
    }

    /// <summary>
    /// 通过传入的武器种类来获取该武器在信息表的行下标
    /// </summary>
    private static int GetFoodTypeInfoCsvRow(WeaponsNameTypeMap type)
    {
        if (WeaponsType_InfoCsvRowMap.ContainsKey(type))
            return WeaponsType_InfoCsvRowMap[type];
        else
            return 0;
    }

    /// <summary>
    /// 获取某个武器的名字
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetName(WeaponsNameTypeMap type)
    {
        int row = GetFoodTypeInfoCsvRow(type);
        return InfoCsv.GetValue(row, 1);
    }

    public static string GetName(int type)
    {
        return GetName((WeaponsNameTypeMap)type);
    }

    /// <summary>
    /// 获取某个武器的描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetInfo(WeaponsNameTypeMap type)
    {
        int row = GetFoodTypeInfoCsvRow(type);
        return InfoCsv.GetValue(row, 2);
    }

    public static string GetInfo(int type)
    {
        return GetInfo((WeaponsNameTypeMap)type);
    }

    /// <summary>
    /// 获取某个武器的基础攻击间隔
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetInterval(WeaponsNameTypeMap type)
    {
        int row = GetFoodTypeInfoCsvRow(type);
        return InfoCsv.GetValue(row, 3);
    }

    public static string GetInterval(int type)
    {
        return GetInterval((WeaponsNameTypeMap)type);
    }

    /// <summary>
    /// 获取解锁等级
    /// </summary>
    public static int GetUnlockLevel(WeaponsNameTypeMap type)
    {
        int row = GetFoodTypeInfoCsvRow(type);
        int level;
        if (int.TryParse(InfoCsv.GetValue(row, 4), out level))
            return level;
        else
            return int.MaxValue;
    }

    public static int GetUnlockLevel(int type)
    {
        return GetUnlockLevel((WeaponsNameTypeMap)type);
    }

    /// <summary>
    /// 获取当前可用的武器表
    /// </summary>
    /// <returns></returns>
    public static List<WeaponsNameTypeMap> GetWeaponsList()
    {
        Load();
        List<WeaponsNameTypeMap> list = new List<WeaponsNameTypeMap>();
        foreach (var keyValuePair in WeaponsType_InfoCsvRowMap)
        {
            list.Add(keyValuePair.Key);
        }
        return list;
    }

    /// <summary>
    /// 检查当前武器编号是否合法
    /// </summary>
    /// <returns></returns>
    public static bool IsWeaponsInValid(int index)
    {
        foreach (var type in currentVersionWeaponsList)
        {
            if (index == (int)type)
                return true;
        }
        return false;
    }
}
