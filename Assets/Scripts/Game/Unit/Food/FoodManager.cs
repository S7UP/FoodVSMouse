using System.Collections.Generic;
/// <summary>
/// 美食管理器（静态存储美食相关信息）
/// </summary>
public class FoodManager
{
    private static Dictionary<FoodNameTypeMap, List<string>> foodNameDict = new Dictionary<FoodNameTypeMap, List<string>>() {
        { FoodNameTypeMap.SmallStove, new List<string>(){"小火炉","日光炉","太阳能高效炉" } },
        { FoodNameTypeMap.CupLight, new List<string>(){"酒杯灯","节能灯","高效节能灯" } },
        { FoodNameTypeMap.BigStove, new List<string>(){"大火炉","高能火炉","超能燃气炉" } },
        { FoodNameTypeMap.CoffeePowder, new List<string>(){"咖啡粉" } },
        { FoodNameTypeMap.CherryPudding, new List<string>(){"樱桃反弹布丁","节能反弹布丁" } },
        { FoodNameTypeMap.IceCream, new List<string>(){"冰淇淋","果蔬冰淇淋" } },
        { FoodNameTypeMap.WaterPipe, new List<string>(){"双向水管","控温双向水管","合金水管" } },
        { FoodNameTypeMap.ThreeLinesVine, new List<string>(){"三线酒架","强力三线酒架","终结者酒架" } },
        { FoodNameTypeMap.Heater, new List<string>(){"火盆","电子烤盘","熔岩烤盘" } },
        //{ FoodNameTypeMap.WoodenDisk, new List<string>(){"木盘子" } },
        //{ FoodNameTypeMap.CottonCandy, new List<string>(){"棉花糖" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"冰桶炸弹" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"瓜皮护罩","尖刺瓜皮护罩" } },
        //{ FoodNameTypeMap.LightningBread, new List<string>(){"雷电长棍面包","节能长棍面包","负离子面包" } },
        //{ FoodNameTypeMap.FlourBag, new List<string>(){"面粉袋" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"酒瓶炸弹" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"可乐炸弹" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"开水壶炸弹" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"威士忌炸弹" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"菠萝爆炸面包","独角菠萝面包","皇冠菠萝面包" } },
        //{ FoodNameTypeMap.OilLight, new List<string>(){"油灯","高亮油灯" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"换气扇" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"老鼠夹子" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"麻辣串炸弹" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"糖葫芦炮弹","水果糖葫芦炮弹","七彩糖葫芦炮弹" } },
    };


    /// <summary>
    /// 直接获取当前版本所有美食及其转职名
    /// </summary>
    /// <returns></returns>
    public static Dictionary<FoodNameTypeMap, List<string>> GetAllFoodDict()
    {
        return foodNameDict;
    }
}
