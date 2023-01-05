using System.Collections.Generic;
using System;
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
        { FoodNameTypeMap.Heater, new List<string>(){"火盆","电子烤盘","熔岩烤盘","金牛座精灵" } },
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"木盘子" ,"友情木盘子"} },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"棉花糖" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"冰桶炸弹" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"瓜皮护罩","尖刺瓜皮护罩" } },
        { FoodNameTypeMap.Takoyaki, new List<string>(){"章鱼烧","两栖章鱼烧","火影章鱼烧" } },
        //{ FoodNameTypeMap.FlourBag, new List<string>(){"面粉袋" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"酒瓶炸弹" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"可乐炸弹" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"开水壶炸弹" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"威士忌炸弹" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"菠萝爆炸面包","独角菠萝面包","皇冠菠萝面包" } },
        { FoodNameTypeMap.SpinCoffee, new List<string>(){"旋转咖啡喷壶","迷雾旋转咖啡喷壶","原子旋转咖啡喷壶" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"换气扇" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"老鼠夹子","节能老鼠夹子","黑猫老鼠夹子" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"麻辣串炸弹" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"糖葫芦炮弹","水果糖葫芦炮弹","七彩糖葫芦炮弹" } },
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "毒菌破坏者" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "扑克护罩" } },
        { FoodNameTypeMap.SaladPitcher, new List<string>(){ "色拉投手","果蔬色拉投手", "凯撒色拉投手" } },
        { FoodNameTypeMap.ChocolatePitcher, new List<string>(){ "巧克力投手", "浓情巧克力投手", "脆心巧克力投手" } },
        { FoodNameTypeMap.TofuPitcher, new List<string>(){ "臭豆腐投手", "什锦臭豆腐投手", "铁板臭豆腐投手" } },
        { FoodNameTypeMap.EggPitcher, new List<string>(){ "煮蛋器投手", "威力煮蛋器", "强袭煮蛋器" } },
        { FoodNameTypeMap.ToastBread, new List<string>(){ "土司面包" } },
        { FoodNameTypeMap.ChocolateBread, new List<string>(){ "巧克力面包", "德芙面包" } },
        { FoodNameTypeMap.RaidenBaguette, new List<string>(){ "雷电长棍面包", "节能雷电长棍面包", "负离子面包" } },
        { FoodNameTypeMap.HotDog, new List<string>(){ "热狗大炮", "热狗高射炮", "热狗高射榴弹炮" } }
    };

    /// <summary>
    /// 防御型卡片
    /// </summary>
    public static List<FoodNameTypeMap> DenfenceCard = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.MelonShield, FoodNameTypeMap.ChocolateBread, FoodNameTypeMap.ToastBread, FoodNameTypeMap.PineappleBreadBoom, FoodNameTypeMap.RaidenBaguette
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
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }

    /// <summary>
    /// 获取特定行中最左边的可攻击友方单位
    /// </summary>
    /// <param name="rowIndex">行下标</param>
    /// <param name="start_temp_x">起始比较横坐标（位于该坐标右侧的单位会被忽略掉）</param>
    /// <returns>返回最靠左的可攻击的友方单位，若为空则没有目标</returns>
    public static BaseUnit GetSpecificRowFarthestLeftCanTargetedAlly(int rowIndex, float start_temp_x)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // 三个基础条件：可被选取、是美食或者人物单位、必须存活
            // 外加一个比较条件，就是单位横坐标比当前比较横坐标小
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || item is CharacterUnit) && item.IsAlive() && item.transform.position.x < temp_x)
            {
                // 如果目标是美食单位，则需要判断目标必须为默认类型卡片或者护罩类型卡片，否则不能作为选取目标
                if(item is FoodUnit)
                {
                    FoodUnit f = item as FoodUnit;
                    if (!f.GetFoodInGridType().Equals(FoodInGridType.Default) && !f.GetFoodInGridType().Equals(FoodInGridType.Shield))
                        continue;
                }
                temp_x = item.transform.position.x;
                targetUnit = item;
            }
        }
        return targetUnit;
    }


    /// <summary>
    /// 获取特定行中最右边的可攻击友方单位
    /// </summary>
    /// <param name="rowIndex">行下标</param>
    /// <param name="start_temp_x">起始比较横坐标（位于该坐标左侧的单位会被忽略掉）</param>
    /// <returns>返回最靠右的可攻击的友方单位，若为空则没有目标</returns>
    public static BaseUnit GetSpecificRowFarthestRightCanTargetedAlly(int rowIndex, float start_temp_x)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // 三个基础条件：可被选取、是美食或者人物单位、必须存活
            // 外加一个比较条件，就是单位横坐标比当前比较横坐标大
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || item is CharacterUnit) && item.IsAlive() && item.transform.position.x > temp_x)
            {
                // 如果目标是美食单位，则需要判断目标必须为默认类型卡片或者护罩类型卡片，否则不能作为选取目标
                if (item is FoodUnit)
                {
                    FoodUnit f = item as FoodUnit;
                    if (!f.GetFoodInGridType().Equals(FoodInGridType.Default) && !f.GetFoodInGridType().Equals(FoodInGridType.Shield))
                        continue;
                }
                temp_x = item.transform.position.x;
                targetUnit = item;
            }
        }
        return targetUnit;
    }

    /// <summary>
    /// 获取 <符合一定条件> 的友方单位数量 最多 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMaxConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        List<int> list = new List<int>();
        int max = 0;
        for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
        {
            int count = 0;
            foreach (var unit in GameController.Instance.mAllyList[rowIndex])
            {
                // 如果目标在传入的条件判断中为真，则计数+1
                if (ConditionFunc(unit))
                    count++;
            }
            if (count == max)
            {
                list.Add(rowIndex);
            }
            else if (count > max)
            {
                list.Clear();
                list.Add(rowIndex);
                max = count;
            }
        }
        return list;
    }

    /// <summary>
    /// 获取 <符合一定条件> 的友方单位数量 最少 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMinConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        List<int> list = new List<int>();
        int min = int.MaxValue;
        for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
        {
            int count = 0;
            foreach (var unit in GameController.Instance.mAllyList[rowIndex])
            {
                // 如果目标在传入的条件判断中为真，则计数+1
                if (ConditionFunc(unit))
                    count++;
            }
            if (count == min)
            {
                list.Add(rowIndex);
            }
            else if (count < min)
            {
                list.Clear();
                list.Add(rowIndex);
                min = count;
            }
        }
        return list;
    }

    /// <summary>
    /// 判断方法：当前目标是否是可以被作为目标的友方单位
    /// </summary>
    private static Func<BaseUnit, bool> IsCanTargetedAlly = (unit) => 
    {
        if (unit is FoodUnit)
        {
            FoodUnit f = unit as FoodUnit;
            FoodInGridType t = f.GetFoodInGridType();
            if (t.Equals(FoodInGridType.Default) || t.Equals(FoodInGridType.Shield))
                return true;
        }
        return false;
    };

    /// <summary>
    /// 获取 可作为攻击目标的友方单位 最多 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMaxCanTargetedAllyCount()
    {
        return GetRowListWhichHasMaxConditionAllyCount(IsCanTargetedAlly);
    }

    /// <summary>
    /// 获取 可作为攻击目标的友方单位 最少 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMinCanTargetedAllyCount()
    {
        return GetRowListWhichHasMinConditionAllyCount(IsCanTargetedAlly);
    }


    /// <summary>
    /// 获取 特定 的行
    /// 1、取出最<符合特定条件>的单位，作为该行的代表
    /// 2、在所有代表中取出最<符合特定条件2>的单位，把这些代表的对应行作为结果行返回出去
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListBySpecificConditions(Func<BaseUnit, BaseUnit, bool> RowCompareFunc, Func<BaseUnit, BaseUnit, int> LastCompareFunc)
    {
        List<int> list = new List<int>();
        BaseUnit compareUnit = null; // 当前作为评价标准的单位
        for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
        {
            BaseUnit rowStandUnit = null; // 当前行代表单位
            foreach (var unit in GameController.Instance.mAllyList[rowIndex])
            {
                // 如果目标在传入的条件判断中为真，则用unit来取代当前代表unit
                if (RowCompareFunc(rowStandUnit, unit))
                    rowStandUnit = unit;
            }
            // 如果比较结果为1（大于），则清空List，然后把当前比较行加入表中，重置评价标准的单位为当前传入比较者
            // 如果比较结果为0（等于），则只把当前比较行加入表中
            // 如果比较结果为-1（小于）（或者其他值），则什么也不发生
            int result = LastCompareFunc(compareUnit, rowStandUnit);
            if (result > 0)
            {
                list.Clear();
                list.Add(rowIndex);
                compareUnit = rowStandUnit;
            }
            else if(result == 0)
            {
                list.Add(rowIndex);
            }
        }
        return list;
    }
}
