using System.Collections.Generic;
/// <summary>
/// ��������������װ��̬������
/// </summary>
public class WeaponsManager
{
    private static Dictionary<WeaponsNameTypeMap, string> WeaponsNameDict = new Dictionary<WeaponsNameTypeMap, string>() 
    {
        { WeaponsNameTypeMap.IceSpoonCrossbowGun, "������ǹ"},
        { WeaponsNameTypeMap.CatGun, "èèǹ"},
        //{ WeaponsNameTypeMap.V5Gun, "����ǹ"},
        //{ WeaponsNameTypeMap.BunGun, "����ǹ"},
    };

    /// <summary>
    /// ��ȡȫ�������������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<WeaponsNameTypeMap, string> GetWeaponsNameDict()
    {
        return WeaponsNameDict;
    }
}
