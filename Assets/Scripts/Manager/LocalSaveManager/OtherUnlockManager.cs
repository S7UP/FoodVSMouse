using System.Collections.Generic;
using UnityEngine;
using System;

public class OtherUnlockManager : MonoBehaviour
{
    private const string localPath = "OtherUnlock";

    /// <summary>
    /// 存放在本地的功能解锁存档
    /// </summary>
    [System.Serializable]
    public class UnlockSaveInLocal
    {
        // key为id  bool 为解锁情况
        public Dictionary<string, bool> dict = new Dictionary<string, bool>();
    }

    // 所有要解锁功能的信息表
    private static Dictionary<string, int> Id_CsvRowMap = new Dictionary<string, int>(); // 解锁功能的Id对CSV表行的映射关系
    private static ExcelManager.CSV _InfoCsv;
    public static ExcelManager.CSV InfoCsv { get { if (_InfoCsv == null) Load(); return _InfoCsv; } }
    // 解锁的其他条件函数字典
    private static Dictionary<string, Func<bool>> UnlockConditionFuncDict = new Dictionary<string, Func<bool>>() 
    {
        
        
    };

    // 玩家本地的解锁情况
    public static UnlockSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static UnlockSaveInLocal _PlayerSave;

    public static void Load()
    {
        if (_InfoCsv == null)
        {
            _InfoCsv = ExcelManager.ReadCSV("OtherUnlockInfo", 5);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _InfoCsv.GetRow(); i++)
            {
                string id = _InfoCsv.GetValue(i, 0);
                if (!Id_CsvRowMap.ContainsKey(id))
                {
                    Id_CsvRowMap.Add(id, i);
                }
            }
        }
        // 读取玩家存档
        if(_PlayerSave == null)
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

    private static int GetRow(string id)
    {
        if (Id_CsvRowMap.ContainsKey(id))
            return Id_CsvRowMap[id];
        else
            return 0;
    }

    /// <summary>
    /// 获取解锁所需要的等级
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static int GetUnlockLevel(string id)
    {
        int row = GetRow(id);
        return int.Parse(InfoCsv.GetValue(row, 4));
    }

    /// <summary>
    /// 某功能是否已解锁
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsUnlock(string id)
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
    public static bool TryUnlock(string id)
    {
        if (!IsUnlock(id))
        {
            // 解锁两个条件：等级与特殊条件（见字典）
            if(PlayerData.GetInstance().GetLevel() >= GetUnlockLevel(id) && (!UnlockConditionFuncDict.ContainsKey(id) || UnlockConditionFuncDict[id]()))
            {
                PlayerSave.dict[id] = true;
                Save();
                return true;
            }
        }
        // 如果该功能早已解锁，这次解锁视为失败
        return false;
    }
}
