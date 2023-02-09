using System.Collections.Generic;

public class TipsManager
{
    private const string localPath = "TipsRecorder";

    /// <summary>
    /// 存放在本地的功能解锁存档
    /// </summary>
    [System.Serializable]
    public class SaveInLocal
    {
        // key为id   int 为已提示次数，若为-1则为未解锁
        public Dictionary<string, int> EnterDict = new Dictionary<string, int>();
        public Dictionary<string, int> ExitDict = new Dictionary<string, int>();
    }

    private static Dictionary<string, int> Id_EnterInfoCsvRowMap = new Dictionary<string, int>();
    private static ExcelManager.CSV _EnterInfoCsv;
    public static ExcelManager.CSV EnterInfoCsv { get { if (_EnterInfoCsv == null) Load(); return _EnterInfoCsv; } }

    private static Dictionary<string, int> Id_ExitInfoCsvRowMap = new Dictionary<string, int>();
    private static ExcelManager.CSV _ExitInfoCsv;
    public static ExcelManager.CSV ExitInfoCsv { get { if (_ExitInfoCsv == null) Load(); return _ExitInfoCsv; } }

    // 玩家本地的解锁情况
    public static SaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static SaveInLocal _PlayerSave;

    public static void Load()
    {
        if (_EnterInfoCsv == null)
        {
            _EnterInfoCsv = ExcelManager.ReadCSV("EnterCombatSceneTips", 4);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _EnterInfoCsv.GetRow(); i++)
            {
                string id = _EnterInfoCsv.GetValue(i, 0);
                if (!Id_EnterInfoCsvRowMap.ContainsKey(id))
                {
                    Id_EnterInfoCsvRowMap.Add(id, i);
                }
            }
        }

        if (_ExitInfoCsv == null)
        {
            _ExitInfoCsv = ExcelManager.ReadCSV("ExitCombatSceneTips", 4);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _ExitInfoCsv.GetRow(); i++)
            {
                string id = _ExitInfoCsv.GetValue(i, 0);
                if (!Id_ExitInfoCsvRowMap.ContainsKey(id))
                {
                    Id_ExitInfoCsvRowMap.Add(id, i);
                }
            }
        }
        // 读取玩家存档
        if (_PlayerSave == null)
        {
            if (!JsonManager.TryLoadFromLocal(localPath, out _PlayerSave))
            {
                _PlayerSave = new SaveInLocal();
                Save();
            }
        }
    }

    /// <summary>
    /// 保存一次本地的存档
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<SaveInLocal>(PlayerSave, localPath);
    }

    private static int GetEnterRow(string id)
    {
        return Id_EnterInfoCsvRowMap[id];
    }

    public static string GetEnterContent(string id)
    {
        int row = GetEnterRow(id);
        return EnterInfoCsv.GetValue(row, 1);
    }

    public static int GetEnterLevel(string id)
    {
        int row = GetEnterRow(id);
        int level;
        if (int.TryParse(EnterInfoCsv.GetValue(row, 2), out level))
            return level;
        return int.MaxValue;
    }

    private static int GetEnterVal(string id)
    {
        int row = GetEnterRow(id);
        int val;
        if (int.TryParse(EnterInfoCsv.GetValue(row, 3), out val))
            return val;
        return int.MaxValue;
    }

    public static void AddEnterVal(string id)
    {
        if (IsEnterTipsUnlock(id))
        {
            PlayerSave.EnterDict[id] += GetEnterVal(id);
            Save();
        }
    }

    /// <summary>
    /// 进入战斗场景前，通过一定算法获取一个随机的TIPS的id
    /// </summary>
    /// <returns>随机Tips的Id</returns>
    public static string GetRandomEnterTipsId()
    {
        int min = int.MaxValue;
        List<string> list = new List<string>();
        foreach (var keyValuePair in Id_EnterInfoCsvRowMap)
        {
            string id = keyValuePair.Key;
            if(IsEnterTipsUnlock(id) || TryUnlockEnterTip(id))
            {
                if(PlayerSave.EnterDict[id] < min)
                {
                    list.Clear();
                    list.Add(id);
                    min = PlayerSave.EnterDict[id];
                }else if(PlayerSave.EnterDict[id] == min)
                {
                    list.Add(id);
                }
            }
        }
        // 随机取一个吧
        System.Random rd = new System.Random();
        return list[rd.Next(0, list.Count)];
    }

    public static bool IsEnterTipsUnlock(string id)
    {
        if (!PlayerSave.EnterDict.ContainsKey(id))
        {
            PlayerSave.EnterDict.Add(id, -1);
            Save();
        }
        return PlayerSave.EnterDict[id] >= 0;
    }

    public static bool TryUnlockEnterTip(string id)
    {
        if (!IsEnterTipsUnlock(id))
        {
            if(PlayerData.GetInstance().GetLevel() >= GetEnterLevel(id))
            {
                PlayerSave.EnterDict[id] = 0;
                Save();
                return true;
            }
        }
        return false;
    }

    ////////////////////////////////////////////////////以下是Exit的，与上面几乎一致

    private static int GetExitRow(string id)
    {
        return Id_ExitInfoCsvRowMap[id];
    }

    public static string GetExitContent(string id)
    {
        int row = GetExitRow(id);
        return ExitInfoCsv.GetValue(row, 1);
    }

    public static int GetExitLevel(string id)
    {
        int row = GetExitRow(id);
        int level;
        if (int.TryParse(ExitInfoCsv.GetValue(row, 2), out level))
            return level;
        return int.MaxValue;
    }

    private static int GetExitVal(string id)
    {
        int row = GetExitRow(id);
        int val;
        if (int.TryParse(ExitInfoCsv.GetValue(row, 3), out val))
            return val;
        return int.MaxValue;
    }

    public static void AddExitVal(string id)
    {
        if (IsExitTipsUnlock(id))
        {
            PlayerSave.ExitDict[id] += GetExitVal(id);
            Save();
        }
    }

    /// <summary>
    /// 离开战斗场景后，通过一定算法获取一个随机的TIPS的id
    /// </summary>
    /// <returns>随机Tips的Id</returns>
    public static string GetRandomExitTipsId()
    {
        int min = int.MaxValue;
        List<string> list = new List<string>();
        foreach (var keyValuePair in Id_ExitInfoCsvRowMap)
        {
            string id = keyValuePair.Key;
            if (IsExitTipsUnlock(id) || TryUnlockExitTip(id))
            {
                if (PlayerSave.ExitDict[id] < min)
                {
                    list.Clear();
                    list.Add(id);
                    min = PlayerSave.ExitDict[id];
                }
                else if (PlayerSave.ExitDict[id] == min)
                {
                    list.Add(id);
                }
            }
        }
        // 随机取一个吧
        System.Random rd = new System.Random();
        return list[rd.Next(0, list.Count)];
    }

    public static bool IsExitTipsUnlock(string id)
    {
        if (!PlayerSave.ExitDict.ContainsKey(id))
        {
            PlayerSave.ExitDict.Add(id, -1);
            Save();
        }
        return PlayerSave.ExitDict[id] >= 0;
    }

    public static bool TryUnlockExitTip(string id)
    {
        if (!IsExitTipsUnlock(id))
        {
            if (PlayerData.GetInstance().GetLevel() >= GetExitLevel(id))
            {
                PlayerSave.ExitDict[id] = 0;
                Save();
                return true;
            }
        }
        return false;
    }
}
