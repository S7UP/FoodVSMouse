using System.Collections.Generic;
using UnityEngine;
using System;

public class OtherUnlockManager : MonoBehaviour
{
    private const string localPath = "OtherUnlock";

    /// <summary>
    /// ����ڱ��صĹ��ܽ����浵
    /// </summary>
    [System.Serializable]
    public class UnlockSaveInLocal
    {
        // keyΪid  bool Ϊ�������
        public Dictionary<string, bool> dict = new Dictionary<string, bool>();
    }

    // ����Ҫ�������ܵ���Ϣ��
    private static Dictionary<string, int> Id_CsvRowMap = new Dictionary<string, int>(); // �������ܵ�Id��CSV���е�ӳ���ϵ
    private static ExcelManager.CSV _InfoCsv;
    public static ExcelManager.CSV InfoCsv { get { if (_InfoCsv == null) Load(); return _InfoCsv; } }
    // �������������������ֵ�
    private static Dictionary<string, Func<bool>> UnlockConditionFuncDict = new Dictionary<string, Func<bool>>() 
    {
        
        
    };

    // ��ұ��صĽ������
    public static UnlockSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static UnlockSaveInLocal _PlayerSave;

    public static void Load()
    {
        if (_InfoCsv == null)
        {
            _InfoCsv = ExcelManager.ReadCSV("OtherUnlockInfo", 5);
            // �������Ŷ�������ӳ���
            for (int i = 0; i < _InfoCsv.GetRow(); i++)
            {
                string id = _InfoCsv.GetValue(i, 0);
                if (!Id_CsvRowMap.ContainsKey(id))
                {
                    Id_CsvRowMap.Add(id, i);
                }
            }
        }
        // ��ȡ��Ҵ浵
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
    /// ����һ�α��صĴ浵
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
    /// ��ȡ��������Ҫ�ĵȼ�
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static int GetUnlockLevel(string id)
    {
        int row = GetRow(id);
        return int.Parse(InfoCsv.GetValue(row, 4));
    }

    /// <summary>
    /// ĳ�����Ƿ��ѽ���
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
    /// ���Խ���ĳЩ����
    /// </summary>
    public static bool TryUnlock(string id)
    {
        if (!IsUnlock(id))
        {
            // ���������������ȼ����������������ֵ䣩
            if(PlayerData.GetInstance().GetLevel() >= GetUnlockLevel(id) && (!UnlockConditionFuncDict.ContainsKey(id) || UnlockConditionFuncDict[id]()))
            {
                PlayerSave.dict[id] = true;
                Save();
                return true;
            }
        }
        // ����ù������ѽ�������ν�����Ϊʧ��
        return false;
    }
}
