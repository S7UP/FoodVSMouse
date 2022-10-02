using System.Collections.Generic;

using UnityEngine;

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
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"木盘子" } },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"棉花糖" } },
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
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "毒菌破坏者" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "扑克护罩" } },
    };


    /// <summary>
    /// 直接获取当前版本所有可建造的美食及其转职名
    /// </summary>
    /// <returns></returns>
    public static Dictionary<FoodNameTypeMap, List<string>> GetAllBuildableFoodDict()
    {
        return foodNameDict;
    }

    /// <summary>
    /// 检测某个美食单位是否是生产类型的单位
    /// </summary>
    /// <returns></returns>
    public static bool IsProductionType(FoodUnit unit)
    {
        return unit.mType >= (int)FoodNameTypeMap.SmallStove && unit.mType <= (int)FoodNameTypeMap.BigStove;
    }

    /// <summary>
    /// 从一个美食表中取出随机几个美食
    /// </summary>
    /// <param name="list">美食表</param>
    /// <param name="count">取的随机数量</param>
    /// <returns></returns>
    public static List<FoodUnit> GetRandomUnitList(List<FoodUnit> list, int count)
    {
        List<FoodUnit> randList = new List<FoodUnit>();
        if (count <= 0 || list==null)
            return randList;

        // 拷贝一份List，保护原来的List不受影响
        List<FoodUnit> cp_list = new List<FoodUnit>();
        foreach (var item in list)
        {
            cp_list.Add(item);
        }

        count = Mathf.Min(count, cp_list.Count);

        for (int i = 0; i < count; i++)
        {
            int index = GameController.Instance.GetRandomInt(0, cp_list.Count);
            randList.Add(cp_list[index]);
            cp_list.Remove(cp_list[index]);
        }
        return randList;
    }

    /// <summary>
    /// 为卡片添加炸弹的修饰
    /// </summary>
    public static void AddBombModifier(BaseUnit unit)
    {
        // 获取100%减伤，以及免疫灰烬秒杀效果
        unit.NumericBox.Defense.SetBase(1);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }
}
