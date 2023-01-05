using System.Collections.Generic;
/// <summary>
/// 宝石管理器（封装静态方法）
/// </summary>
public class JewelManager
{
    private static Dictionary<JewelNameTypeMap, string> JewelNameDict = new Dictionary<JewelNameTypeMap, string>() 
    {
        { JewelNameTypeMap.Ice, "冰冻宝石"},
        { JewelNameTypeMap.Sacred, "神圣宝石"},
        { JewelNameTypeMap.Bombing, "轰炸宝石"},
        { JewelNameTypeMap.Laser, "激光宝石"},
        { JewelNameTypeMap.CatEyes, "猫眼宝石"},
    };

    /// <summary>
    /// 获取全部宝石的名字字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<JewelNameTypeMap, string> GetJewelNameDict()
    {
        return JewelNameDict;
    }
}
