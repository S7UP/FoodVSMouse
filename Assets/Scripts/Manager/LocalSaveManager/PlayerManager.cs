// ��ҵĹ������𱣴��Լ����ظ�������Լ���Ϸ����Ϣ

public class PlayerManager
{
    public const int version = 5; // ��ǰ��Ϸ���Ӧ�õĴ浵�汾�ţ�ÿ��������Ϸʱ�����ҵĴ浵�汾�ţ������ڸ�ֵ����������Ҵ浵���¹��� 
    public const int maxLevel = 39; // ��ǰ���ȼ�
    // �洢����ֵ�ı�
    private static ExcelManager.CSV _ExpCsv;
    public static ExcelManager.CSV ExpCsv { get { if (_ExpCsv == null) Load(); return _ExpCsv; } }

    public static void Load()
    {
        if (_ExpCsv == null)
        {
            _ExpCsv = ExcelManager.ReadCSV("Exp", 2);
        }
    }

    /// <summary>
    /// ��ȡ����ȼ�������һ������ľ���ֵ
    /// </summary>
    /// <param name="currentLevel">��ǰ�ȼ�</param>
    /// <returns>������ȼ��Ƿ��򷵻�-1</returns>
    public static float GetNextLevelExp(int currentLevel)
    {
        // ע���һ����1�������±���0��������ȡ����ʱ��Ҫ-1
        float exp;
        if (currentLevel <= ExpCsv.GetRow() && currentLevel > 0 && float.TryParse(ExpCsv.GetValue(currentLevel - 1, 1), out exp))
            return exp;
        return -1;
    }

}
