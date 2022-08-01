using System.Collections.Generic;
/// <summary>
/// 武器管理器（封装静态方法）
/// </summary>
public class WeaponsManager
{
    private static Dictionary<WeaponsNameTypeMap, string> WeaponsNameDict = new Dictionary<WeaponsNameTypeMap, string>() 
    {
        { WeaponsNameTypeMap.IceSpoonCrossbowGun, "冰勺弩枪"},
        { WeaponsNameTypeMap.CatGun, "猫猫枪"},
        //{ WeaponsNameTypeMap.V5Gun, "威武枪"},
        //{ WeaponsNameTypeMap.BunGun, "包子枪"},
    };

    /// <summary>
    /// 获取全部武器的名字字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<WeaponsNameTypeMap, string> GetWeaponsNameDict()
    {
        return WeaponsNameDict;
    }
}
