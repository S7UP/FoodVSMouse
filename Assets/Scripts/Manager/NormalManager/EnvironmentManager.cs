using System.Collections.Generic;
/// <summary>
/// ����Ч������������ȡ������Ϣ��
/// </summary>
public class EnvironmentManager
{
    private static ExcelManager.CSV _InfoCsv;
    private static Dictionary<int, int> Type_InfoCsvRowMap = new Dictionary<int, int>();
    public static ExcelManager.CSV InfoCsv { get { if (_InfoCsv == null) Load(); return _InfoCsv; } }

    public static void Load()
    {
        if (_InfoCsv == null)
        {
            _InfoCsv = ExcelManager.ReadCSV("EnvironmentInfo", 5);
            // �������Ŷ�������ӳ���
            for (int i = 0; i < _InfoCsv.GetRow(); i++)
            {
                int type;
                if (int.TryParse(_InfoCsv.GetValue(i, 0), out type))
                {
                    if (!Type_InfoCsvRowMap.ContainsKey(type))
                    {
                        Type_InfoCsvRowMap.Add(type, i);
                    }
                }
            }
        }
    }

    public static string GetName(int type)
    {
        int row = Type_InfoCsvRowMap[type];
        return InfoCsv.GetValue(row, 1);
    }

    public static string GetSimpleInfo(int type)
    {
        int row = Type_InfoCsvRowMap[type];
        return InfoCsv.GetValue(row, 2);
    }

    public static string GetDetailedInfo(int type)
    {
        int row = Type_InfoCsvRowMap[type];
        return InfoCsv.GetValue(row, 3);
    }

    public static string GetTips(int type)
    {
        int row = Type_InfoCsvRowMap[type];
        return InfoCsv.GetValue(row, 4);
    }
}
