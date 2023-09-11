using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
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
        { FoodNameTypeMap.IceCream, new List<string>(){"冰淇淋","果蔬冰淇淋","极寒冰沙" } },
        { FoodNameTypeMap.WaterPipe, new List<string>(){"双向水管","控温双向水管","合金水管" } },
        { FoodNameTypeMap.ThreeLinesVine, new List<string>(){"三线酒架","强力三线酒架","终结者酒架" } },
        { FoodNameTypeMap.Heater, new List<string>(){"火盆","电子烤盘","熔岩烤盘","金牛座精灵" } },
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"木盘子" ,"友情木盘子"} },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"棉花糖" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"冰桶炸弹" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"瓜皮护罩","尖刺瓜皮护罩" } },
        { FoodNameTypeMap.Takoyaki, new List<string>(){"章鱼烧","两栖章鱼烧","火影章鱼烧" } },
        { FoodNameTypeMap.FlourBag, new List<string>(){"面粉袋","超重面粉袋","粉碎性骨折铅袋" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"酒瓶炸弹" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"可乐炸弹" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"开水壶炸弹" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"威士忌炸弹" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"菠萝爆炸面包","独角菠萝面包","皇冠菠萝面包" } },
        { FoodNameTypeMap.SpinCoffee, new List<string>(){"旋转咖啡喷壶","迷雾旋转咖啡喷壶","原子旋转咖啡喷壶" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"换气扇" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"老鼠夹子","节能老鼠夹子","黑猫老鼠夹子" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"麻辣串炸弹" } },
        { FoodNameTypeMap.CoffeeCup, new List<string>(){ "咖啡杯","花纹咖啡杯","骨瓷咖啡杯" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"糖葫芦炮弹","水果糖葫芦炮弹","七彩糖葫芦炮弹" } },
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "毒菌破坏者" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "扑克护罩" } },
        { FoodNameTypeMap.SaladPitcher, new List<string>(){ "色拉投手","果蔬色拉投手", "凯撒色拉投手" } },
        { FoodNameTypeMap.ChocolatePitcher, new List<string>(){ "巧克力投手", "浓情巧克力投手", "脆心巧克力投手" } },
        { FoodNameTypeMap.TofuPitcher, new List<string>(){ "臭豆腐投手", "什锦臭豆腐投手", "铁板臭豆腐投手" } },
        { FoodNameTypeMap.EggPitcher, new List<string>(){ "煮蛋器投手", "威力煮蛋器", "强袭煮蛋器" } },
        { FoodNameTypeMap.IceEggPitcher, new List<string>(){ "冰煮蛋器投手", "冰威力煮蛋器", "冰河煮蛋器" } },
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
    /// 攻击力随星级变化的卡片
    /// </summary>
    public static List<FoodNameTypeMap> AttackLevelMap = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.WaterPipe, FoodNameTypeMap.ThreeLinesVine, FoodNameTypeMap.Takoyaki, FoodNameTypeMap.SpinCoffee, FoodNameTypeMap.SugarGourd,
        FoodNameTypeMap.SaladPitcher, FoodNameTypeMap.ChocolatePitcher, FoodNameTypeMap.TofuPitcher, FoodNameTypeMap.EggPitcher, FoodNameTypeMap.HotDog,
        FoodNameTypeMap.CoffeeCup, FoodNameTypeMap.IceEggPitcher
    };

    /// <summary>
    /// 生命值随星级变化的卡片
    /// </summary>
    public static List<FoodNameTypeMap> HpLevelMap = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.CherryPudding, FoodNameTypeMap.WoodenDisk, FoodNameTypeMap.CottonCandy, FoodNameTypeMap.MelonShield,
        FoodNameTypeMap.PokerShield, FoodNameTypeMap.ToastBread, FoodNameTypeMap.ChocolateBread, FoodNameTypeMap.RaidenBaguette
    };

    private static ExcelManager.CSV _FoodTypeInfoCsv;
    private static Dictionary<FoodNameTypeMap, int> FoodType_FoodTypeInfoCsvRowMap = new Dictionary<FoodNameTypeMap, int>(); // 美食种类对美食信息表行的映射关系
    public static ExcelManager.CSV FoodTypeInfoCsv { get { if (_FoodTypeInfoCsv == null) Load(); return _FoodTypeInfoCsv; } }

    public static void Load()
    {
        if (_FoodTypeInfoCsv == null)
        {
            _FoodTypeInfoCsv = ExcelManager.ReadCSV("FoodTypeInfo", 6);
            // 填充种类号对行数的映射表
            for (int i = 0; i < _FoodTypeInfoCsv.GetRow(); i++)
            {
                int _foodType;
                if(int.TryParse(_FoodTypeInfoCsv.GetValue(i, 0), out _foodType))
                {
                    FoodNameTypeMap foodType = (FoodNameTypeMap)_foodType;
                    if (!FoodType_FoodTypeInfoCsvRowMap.ContainsKey(foodType))
                    {
                        FoodType_FoodTypeInfoCsvRowMap.Add(foodType, i);
                    }
                    else
                    {
                        Debug.LogError("在读取美食信息时，存在两个种类号相同的美食！");
                    }
                }
                else
                {
                    Debug.LogError("在读取美食信息时，第"+(i+2)+"行的美食种类号非法！");
                }
            }
        }
    }

    /// <summary>
    /// 通过传入的美食种类来获取该美食在信息表的行下标
    /// </summary>
    private static int GetFoodTypeInfoCsvRow(FoodNameTypeMap type)
    {
        if (FoodType_FoodTypeInfoCsvRowMap.ContainsKey(type))
            return FoodType_FoodTypeInfoCsvRowMap[type];
        else
            return 0;
    }

    /// <summary>
    /// 获取某个美食拥有的变种数目（没有转职为1，只有一转为2，有一二转为3，依次类推，若读取不到返回0）
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static int GetShapeCount(FoodNameTypeMap type)
    {
        if (foodNameDict.ContainsKey(type))
            return foodNameDict[type].Count;
        else
            return 0;
    }

    public static int GetShapeCount(int type)
    {
        return GetShapeCount((FoodNameTypeMap)type);
    }

    /// <summary>
    /// 获取某个美食的名字
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetFoodName(FoodNameTypeMap type, int shape)
    {
        if (foodNameDict.ContainsKey(type) && foodNameDict[type].Count > shape)
            return foodNameDict[type][shape];
        else
            return "未知";
    }

    public static string GetFoodName(int type, int shape)
    {
        return GetFoodName((FoodNameTypeMap)type, shape);
    }

    /// <summary>
    /// 获取美食在格子上的分类
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FoodInGridType GetFoodInGridType(int type)
    {
        return BaseCardBuilder.GetFoodInGridType(type);
    }

    public static FoodInGridType GetFoodInGridType(FoodNameTypeMap type)
    {
        return GetFoodInGridType((int)type);
    }

    /// <summary>
    /// 获取简洁的功能描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetSimpleFeature(FoodNameTypeMap type, int level, int shape)
    {
        Func<string, int, int, string> func = GetSimpleParamReplaceFunc(type);
        if (func != null)
        {
            return func(FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 2), level, shape);
        }
        return FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 2);
    }

    public static string GetSimpleFeature(int type, int level, int shape)
    {
        return GetSimpleFeature((FoodNameTypeMap)type, level, shape);
    }

    /// <summary>
    /// 获取详细的功能描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetDetailedFeature(FoodNameTypeMap type, int level, int shape)
    {
        Func<string, int, int, string> func = GetDetailedParamReplaceFunc(type);
        if (func != null)
        {
            return func(FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 3), level, shape);
        }
        return FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 3);
    }

    public static string GetDetailedFeature(int type, int level, int shape)
    {
        return GetDetailedFeature((FoodNameTypeMap)type, level, shape);
    }

    /// <summary>
    /// 获取超简洁的功能描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetVerySimpleFeature(FoodNameTypeMap type)
    {
        return FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 5);
    }

    public static string GetVerySimpleFeature(int type)
    {
        return GetVerySimpleFeature((FoodNameTypeMap)type);
    }

    /// <summary>
    /// 获取使用小贴士
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetUseTips(FoodNameTypeMap type, int level, int shape)
    {
        Func<string, int, int, string> func = GetTipsParamReplaceFunc(type);
        if (func != null)
        {
            return func(FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 4), level, shape);
        }
        return FoodTypeInfoCsv.GetValue(GetFoodTypeInfoCsvRow(type), 4);
    }

    public static string GetUseTips(int type, int level, int shape)
    {
        return GetUseTips((FoodNameTypeMap)type, level, shape);
    }

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
    /// 获取攻击力
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static float GetAttack(FoodNameTypeMap type, int level, int shape)
    {
        FoodUnit.Attribute attr = GetAttribute(type, shape);
        if (AttackLevelMap.Contains(type))
        {
            return (float)(attr.baseAttrbute.baseAttack + attr.valueList[level]);
        }
        else
        {
            return (float)attr.baseAttrbute.baseAttack;
        }
    }

    /// <summary>
    /// 获取生命值
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static float GetHp(FoodNameTypeMap type, int level, int shape)
    {
        FoodUnit.Attribute attr = GetAttribute(type, shape);
        if (HpLevelMap.Contains(type))
        {
            return (float)(attr.baseAttrbute.baseHP + attr.valueList[level]);
        }
        else
        {
            return (float)attr.baseAttrbute.baseHP;
        }
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
        unit.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
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
    public static BaseUnit GetSpecificRowFarthestLeftCanTargetedAlly(int rowIndex, float start_temp_x, bool canSelectedCharacter)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // 三个基础条件：可被选取、是美食或者人物单位、必须存活
            // 外加一个比较条件，就是单位横坐标比当前比较横坐标小
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || (canSelectedCharacter && item is CharacterUnit)) && item.IsAlive() && item.transform.position.x < temp_x)
            {
                // 如果目标是美食单位，则需要判断目标必须为默认类型卡片或者护罩类型卡片，否则不能作为选取目标
                if(item is FoodUnit)
                {
                    FoodUnit f = item as FoodUnit;
                    if (!f.GetFoodInGridType().Equals(FoodInGridType.Default) && !f.GetFoodInGridType().Equals(FoodInGridType.Shield) && !f.GetFoodInGridType().Equals(FoodInGridType.Bomb))
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
    public static BaseUnit GetSpecificRowFarthestRightCanTargetedAlly(int rowIndex, float start_temp_x, bool canSelectedCharacter)
    {
        return GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, start_temp_x, float.MaxValue, canSelectedCharacter);
    }

    public static BaseUnit GetSpecificRowFarthestRightCanTargetedAlly(int rowIndex, float min_x, float max_x, bool canSelectedCharacter)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = min_x;
        foreach (var item in list)
        {
            // 三个基础条件：可被选取、是美食或者人物单位、必须存活
            // 外加一个比较条件，就是单位横坐标比当前比较横坐标大
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || (canSelectedCharacter && item is CharacterUnit)) && item.IsAlive() && item.transform.position.x > temp_x && item.transform.position.x <= max_x)
            {
                // 如果目标是美食单位，则需要判断目标必须为默认类型卡片或者护罩类型卡片或者炸弹类型卡片，否则不能作为选取目标
                if (item is FoodUnit)
                {
                    FoodUnit f = item as FoodUnit;
                    if (!f.GetFoodInGridType().Equals(FoodInGridType.Default) && !f.GetFoodInGridType().Equals(FoodInGridType.Shield) && !f.GetFoodInGridType().Equals(FoodInGridType.Bomb))
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
    public static List<int> GetRowListWhichHasMaxConditionAllyCount(List<int> rowList, Func<BaseUnit, bool> ConditionFunc)
    {
        List<int> list = new List<int>();
        int max = 0;
        // for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
        foreach (var rowIndex in rowList)
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
    public static List<int> GetRowListWhichHasMinConditionAllyCount(List<int> rowList, Func<BaseUnit, bool> ConditionFunc)
    {
        List<int> list = new List<int>();
        int min = int.MaxValue;
        foreach (var rowIndex in rowList)
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
    /// 获取 <符合一定条件> 的友方单位数量 最多 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMaxConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        return GetRowListWhichHasMaxConditionAllyCount(new List<int>() { 0, 1, 2, 3, 4, 5, 6 }, ConditionFunc);
    }

    /// <summary>
    /// 获取 <符合一定条件> 的友方单位数量 最少 的行
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMinConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        return GetRowListWhichHasMinConditionAllyCount(new List<int>() { 0, 1, 2, 3, 4, 5, 6 }, ConditionFunc);
    }

    /// <summary>
    /// 判断方法：当前目标是否是可以被作为目标的友方单位
    /// </summary>
    private static Func<BaseUnit, bool> IsCanTargetedAlly = (unit) => 
    {
        if (unit is FoodUnit)
        {
            FoodUnit f = unit as FoodUnit;
            return IsAttackableFoodType(f);
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

    private static FoodUnit.Attribute GetAttribute(FoodNameTypeMap type, int shape)
    {
        return GameManager.Instance.attributeManager.GetFoodUnitAttribute((int)type, shape);
    }

    private static BaseCardBuilder.Attribute GetCardBuilderAttribute(FoodNameTypeMap type, int shape)
    {
        return GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)type, shape);
    }

    private static Dictionary<FoodInGridType, string> FoodInGridTypeNameDict = new Dictionary<FoodInGridType, string>() {
        { FoodInGridType.Default, "普通类卡片"},
        { FoodInGridType.Bomb, "炸弹类卡片"},
        { FoodInGridType.WaterVehicle, "水载具类卡片"},
        { FoodInGridType.LavaVehicle, "空载具类卡片"},
        { FoodInGridType.Shield, "护罩类卡片" },
        { FoodInGridType.NoAttach, "非依附类卡片"}
    };

    /// <summary>
    /// 获取美食在格子上分类的名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetFoodInGridTypeName(int type) 
    {
        FoodInGridType t = GetFoodInGridType(type);
        if (FoodInGridTypeNameDict.ContainsKey(t))
            return FoodInGridTypeNameDict[t];
        return t.ToString();
    }

    /// <summary>
    /// 对美食的信息的参数替换方法（只能枚举了）
    /// </summary>
    private static Dictionary<FoodNameTypeMap, Func<string, int, int, string>[]> foodInfoParamReplaceFuncDict = new Dictionary<FoodNameTypeMap, Func<string, int, int, string>[]>() {
        { FoodNameTypeMap.SmallStove, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.SmallStove, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // 功能（详细）
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.SmallStove, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.SmallStove, 1).GetCost(level);
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                str = ParamManager.GetStringByReplaceParam(str, "cost", cost.ToString());
                return str;
            },
            // 小贴士
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.SmallStove, shape).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            } }
        },
        { FoodNameTypeMap.CupLight, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CupLight, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // 功能（详细）
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.CupLight, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.CupLight, 1).GetCost(level);
                string sec = ((float)CupLight.GetGrowTime(shape, level)/60).ToString("#0.00");
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                str = ParamManager.GetStringByReplaceParam(str, "cost", cost.ToString());
                str = ParamManager.GetStringByReplaceParam(str, "growTime", sec);
                return str;
            },
            // 小贴士
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CupLight, shape).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            } }
        },
        { FoodNameTypeMap.CherryPudding, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CherryPudding, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // 功能（详细）
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CherryPudding, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // 小贴士
            null
        } },
        { FoodNameTypeMap.Heater, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            (s, level, shape)=> {
                double dmgRate = GetAttribute(FoodNameTypeMap.Heater, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "dmg_rate", dmgRate.ToString("#0.00"));
            },
            // 功能（详细）
            (s, level, shape)=> {
                double dmgRate = GetAttribute(FoodNameTypeMap.Heater, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "dmg_rate", dmgRate.ToString("#0.00"));
            },
            // 小贴士
            null }
        },
        { FoodNameTypeMap.CoffeePowder, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            (s, level, shape)=> {
                double time = GetAttribute(FoodNameTypeMap.CoffeePowder, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "time", time.ToString("#0.0"));
            },
            // 功能（详细）
            (s, level, shape)=> {
                double time = GetAttribute(FoodNameTypeMap.CoffeePowder, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "time", time.ToString("#0.0"));
            },
            // 小贴士
            null }
        },
        { FoodNameTypeMap.BigStove, new Func<string, int, int, string>[3]{
            // 功能（简洁）
            null,
            // 功能（详细）
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.SmallStove, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.BigStove, 1).GetCost(level);
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                return str;
            },
            // 小贴士
            null}
        },
        // 模版
        //{ FoodNameTypeMap.SmallStove, new Func<string, int, int, string>[3]{
        //    // 功能（简洁）
        //    (s, level, shape)=> { },
        //    // 功能（详细）
        //    (s, level, shape)=> { },
        //    // 小贴士
        //    (s, level, shape)=> { } }
        //},
    };

    /// <summary>
    /// 获取对简洁描述的参数处理方法
    /// </summary>
    /// <returns></returns>
    private static Func<string, int, int, string> GetSimpleParamReplaceFunc(FoodNameTypeMap type)
    {
        if (foodInfoParamReplaceFuncDict.ContainsKey(type))
        {
            return foodInfoParamReplaceFuncDict[type][0];
        }
        return null;
    }

    /// <summary>
    /// 获取对详细描述的参数处理方法
    /// </summary>
    /// <returns></returns>
    private static Func<string, int, int, string> GetDetailedParamReplaceFunc(FoodNameTypeMap type)
    {
        if (foodInfoParamReplaceFuncDict.ContainsKey(type))
        {
            return foodInfoParamReplaceFuncDict[type][1];
        }
        return null;
    }

    /// <summary>
    /// 获取对小贴士的参数处理方法
    /// </summary>
    /// <returns></returns>
    private static Func<string, int, int, string> GetTipsParamReplaceFunc(FoodNameTypeMap type)
    {
        if (foodInfoParamReplaceFuncDict.ContainsKey(type))
        {
            return foodInfoParamReplaceFuncDict[type][2];
        }
        return null;
    }

    /// <summary>
    /// 是不是可作为攻击目标的美食单位（目前默认只有护罩类、普通类、炸弹类是）
    /// </summary>
    /// <returns></returns>
    public static bool IsAttackableFoodType(FoodUnit f)
    {
        return f.GetFoodInGridType().Equals(FoodInGridType.Default) || f.GetFoodInGridType().Equals(FoodInGridType.Bomb) || f.GetFoodInGridType().Equals(FoodInGridType.Shield);
    }
}
