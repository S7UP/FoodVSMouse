using System.Collections.Generic;
/// <summary>
/// ��������������װ��̬������
/// </summary>
public class WeaponsManager
{
    private const string localPath = "WeaponsUnlock";

    /// <summary>
    /// ��ǰ�汾��ʹ�õ�����
    /// </summary>
    private static List<WeaponsNameTypeMap> currentVersionWeaponsList = new List<WeaponsNameTypeMap>() { 
        WeaponsNameTypeMap.BunGun, WeaponsNameTypeMap.WaterPipeGun, WeaponsNameTypeMap.ThreeLineGun,
        WeaponsNameTypeMap.IceGun, WeaponsNameTypeMap.CatGun, WeaponsNameTypeMap.IceSpoonCrossbowGun
    };
    private static ExcelManager.CSV _InfoCsv;
    private static Dictionary<WeaponsNameTypeMap, int> WeaponsType_InfoCsvRowMap = new Dictionary<WeaponsNameTypeMap, int>(); // ���������������Ϣ���е�ӳ���ϵ
    public static ExcelManager.CSV InfoCsv { get { if (_InfoCsv == null) Load(); return _InfoCsv; } }

    /// <summary>
    /// ����ڱ��صĹ��ܽ����浵
    /// </summary>
    [System.Serializable]
    public class UnlockSaveInLocal
    {
        // keyΪ����int  bool Ϊ�������
        public Dictionary<WeaponsNameTypeMap, bool> dict = new Dictionary<WeaponsNameTypeMap, bool>();
    }

    // ��ұ��صĽ������
    public static UnlockSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static UnlockSaveInLocal _PlayerSave;

    public static void Load()
    {
        if (_InfoCsv == null)
        {
            _InfoCsv = ExcelManager.ReadCSV("WeaponsInfo", 5);
            // �������Ŷ�������ӳ���
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
        // ��ȡ��Ҵ浵
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
    /// ����һ�α��صĴ浵
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<UnlockSaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// ĳ��������Ƿ��ѽ���
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
    /// ���Խ���ĳЩ����
    /// </summary>
    public static bool TryUnlock(WeaponsNameTypeMap id)
    {
        if (!IsUnlock(id))
        {
            // ���������������ȼ����������������ֵ䣩
            if (PlayerData.GetInstance().GetLevel() >= GetUnlockLevel(id))
            {
                PlayerSave.dict[id] = true;
                Save();
                return true;
            }
        }
        // ����ù������ѽ�������ν�����Ϊʧ��
        return false;
    }

    /// <summary>
    /// ͨ�������������������ȡ����������Ϣ������±�
    /// </summary>
    private static int GetFoodTypeInfoCsvRow(WeaponsNameTypeMap type)
    {
        if (WeaponsType_InfoCsvRowMap.ContainsKey(type))
            return WeaponsType_InfoCsvRowMap[type];
        else
            return 0;
    }

    /// <summary>
    /// ��ȡĳ������������
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
    /// ��ȡĳ������������
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
    /// ��ȡĳ�������Ļ����������
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
    /// ��ȡ�����ȼ�
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
    /// ��ȡ��ǰ���õ�������
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
    /// ��鵱ǰ��������Ƿ�Ϸ�
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
