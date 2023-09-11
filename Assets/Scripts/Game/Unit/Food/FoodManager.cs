using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ��ʳ����������̬�洢��ʳ�����Ϣ��
/// </summary>
public class FoodManager
{
    private static Dictionary<FoodNameTypeMap, List<string>> foodNameDict = new Dictionary<FoodNameTypeMap, List<string>>() {
        { FoodNameTypeMap.SmallStove, new List<string>(){"С��¯","�չ�¯","̫���ܸ�Ч¯" } },
        { FoodNameTypeMap.CupLight, new List<string>(){"�Ʊ���","���ܵ�","��Ч���ܵ�" } },
        { FoodNameTypeMap.BigStove, new List<string>(){"���¯","���ܻ�¯","����ȼ��¯" } },
        { FoodNameTypeMap.CoffeePowder, new List<string>(){"���ȷ�" } },
        { FoodNameTypeMap.CherryPudding, new List<string>(){"ӣ�ҷ�������","���ܷ�������" } },
        { FoodNameTypeMap.IceCream, new List<string>(){"�����","���߱����","������ɳ" } },
        { FoodNameTypeMap.WaterPipe, new List<string>(){"˫��ˮ��","����˫��ˮ��","�Ͻ�ˮ��" } },
        { FoodNameTypeMap.ThreeLinesVine, new List<string>(){"���߾Ƽ�","ǿ�����߾Ƽ�","�ս��߾Ƽ�" } },
        { FoodNameTypeMap.Heater, new List<string>(){"����","���ӿ���","���ҿ���","��ţ������" } },
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"ľ����" ,"����ľ����"} },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"�޻���" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"��Ͱը��" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"��Ƥ����","��̹�Ƥ����" } },
        { FoodNameTypeMap.Takoyaki, new List<string>(){"������","����������","��Ӱ������" } },
        { FoodNameTypeMap.FlourBag, new List<string>(){"��۴�","������۴�","�����Թ���Ǧ��" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"��ƿը��" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"����ը��" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"��ˮ��ը��" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"��ʿ��ը��" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"���ܱ�ը���","���ǲ������","�ʹڲ������" } },
        { FoodNameTypeMap.SpinCoffee, new List<string>(){"��ת�������","������ת�������","ԭ����ת�������" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"������" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"�������","�����������","��è�������" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"������ը��" } },
        { FoodNameTypeMap.CoffeeCup, new List<string>(){ "���ȱ�","���ƿ��ȱ�","�Ǵɿ��ȱ�" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"�Ǻ�«�ڵ�","ˮ���Ǻ�«�ڵ�","�߲��Ǻ�«�ڵ�" } },
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "�����ƻ���" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "�˿˻���" } },
        { FoodNameTypeMap.SaladPitcher, new List<string>(){ "ɫ��Ͷ��","����ɫ��Ͷ��", "����ɫ��Ͷ��" } },
        { FoodNameTypeMap.ChocolatePitcher, new List<string>(){ "�ɿ���Ͷ��", "Ũ���ɿ���Ͷ��", "�����ɿ���Ͷ��" } },
        { FoodNameTypeMap.TofuPitcher, new List<string>(){ "������Ͷ��", "ʲ��������Ͷ��", "���������Ͷ��" } },
        { FoodNameTypeMap.EggPitcher, new List<string>(){ "����Ͷ��", "��������", "ǿϮ����" } },
        { FoodNameTypeMap.IceEggPitcher, new List<string>(){ "������Ͷ��", "����������", "��������" } },
        { FoodNameTypeMap.ToastBread, new List<string>(){ "��˾���" } },
        { FoodNameTypeMap.ChocolateBread, new List<string>(){ "�ɿ������", "��ܽ���" } },
        { FoodNameTypeMap.RaidenBaguette, new List<string>(){ "�׵糤�����", "�����׵糤�����", "���������" } },
        { FoodNameTypeMap.HotDog, new List<string>(){ "�ȹ�����", "�ȹ�������", "�ȹ���������" } }
    };

    /// <summary>
    /// �����Ϳ�Ƭ
    /// </summary>
    public static List<FoodNameTypeMap> DenfenceCard = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.MelonShield, FoodNameTypeMap.ChocolateBread, FoodNameTypeMap.ToastBread, FoodNameTypeMap.PineappleBreadBoom, FoodNameTypeMap.RaidenBaguette
    };

    /// <summary>
    /// ���������Ǽ��仯�Ŀ�Ƭ
    /// </summary>
    public static List<FoodNameTypeMap> AttackLevelMap = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.WaterPipe, FoodNameTypeMap.ThreeLinesVine, FoodNameTypeMap.Takoyaki, FoodNameTypeMap.SpinCoffee, FoodNameTypeMap.SugarGourd,
        FoodNameTypeMap.SaladPitcher, FoodNameTypeMap.ChocolatePitcher, FoodNameTypeMap.TofuPitcher, FoodNameTypeMap.EggPitcher, FoodNameTypeMap.HotDog,
        FoodNameTypeMap.CoffeeCup, FoodNameTypeMap.IceEggPitcher
    };

    /// <summary>
    /// ����ֵ���Ǽ��仯�Ŀ�Ƭ
    /// </summary>
    public static List<FoodNameTypeMap> HpLevelMap = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.CherryPudding, FoodNameTypeMap.WoodenDisk, FoodNameTypeMap.CottonCandy, FoodNameTypeMap.MelonShield,
        FoodNameTypeMap.PokerShield, FoodNameTypeMap.ToastBread, FoodNameTypeMap.ChocolateBread, FoodNameTypeMap.RaidenBaguette
    };

    private static ExcelManager.CSV _FoodTypeInfoCsv;
    private static Dictionary<FoodNameTypeMap, int> FoodType_FoodTypeInfoCsvRowMap = new Dictionary<FoodNameTypeMap, int>(); // ��ʳ�������ʳ��Ϣ���е�ӳ���ϵ
    public static ExcelManager.CSV FoodTypeInfoCsv { get { if (_FoodTypeInfoCsv == null) Load(); return _FoodTypeInfoCsv; } }

    public static void Load()
    {
        if (_FoodTypeInfoCsv == null)
        {
            _FoodTypeInfoCsv = ExcelManager.ReadCSV("FoodTypeInfo", 6);
            // �������Ŷ�������ӳ���
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
                        Debug.LogError("�ڶ�ȡ��ʳ��Ϣʱ�����������������ͬ����ʳ��");
                    }
                }
                else
                {
                    Debug.LogError("�ڶ�ȡ��ʳ��Ϣʱ����"+(i+2)+"�е���ʳ����ŷǷ���");
                }
            }
        }
    }

    /// <summary>
    /// ͨ���������ʳ��������ȡ����ʳ����Ϣ������±�
    /// </summary>
    private static int GetFoodTypeInfoCsvRow(FoodNameTypeMap type)
    {
        if (FoodType_FoodTypeInfoCsvRowMap.ContainsKey(type))
            return FoodType_FoodTypeInfoCsvRowMap[type];
        else
            return 0;
    }

    /// <summary>
    /// ��ȡĳ����ʳӵ�еı�����Ŀ��û��תְΪ1��ֻ��һתΪ2����һ��תΪ3���������ƣ�����ȡ��������0��
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
    /// ��ȡĳ����ʳ������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string GetFoodName(FoodNameTypeMap type, int shape)
    {
        if (foodNameDict.ContainsKey(type) && foodNameDict[type].Count > shape)
            return foodNameDict[type][shape];
        else
            return "δ֪";
    }

    public static string GetFoodName(int type, int shape)
    {
        return GetFoodName((FoodNameTypeMap)type, shape);
    }

    /// <summary>
    /// ��ȡ��ʳ�ڸ����ϵķ���
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
    /// ��ȡ���Ĺ�������
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
    /// ��ȡ��ϸ�Ĺ�������
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
    /// ��ȡ�����Ĺ�������
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
    /// ��ȡʹ��С��ʿ
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
    /// ֱ�ӻ�ȡ��ǰ�汾���пɽ������ʳ����תְ��
    /// </summary>
    /// <returns></returns>
    public static Dictionary<FoodNameTypeMap, List<string>> GetAllBuildableFoodDict()
    {
        return foodNameDict;
    }

    /// <summary>
    /// ���ĳ����ʳ��λ�Ƿ����������͵ĵ�λ
    /// </summary>
    /// <returns></returns>
    public static bool IsProductionType(FoodUnit unit)
    {
        return unit.mType >= (int)FoodNameTypeMap.SmallStove && unit.mType <= (int)FoodNameTypeMap.BigStove;
    }

    /// <summary>
    /// ��ȡ������
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
    /// ��ȡ����ֵ
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
    /// ��һ����ʳ����ȡ�����������ʳ
    /// </summary>
    /// <param name="list">��ʳ��</param>
    /// <param name="count">ȡ���������</param>
    /// <returns></returns>
    public static List<FoodUnit> GetRandomUnitList(List<FoodUnit> list, int count)
    {
        List<FoodUnit> randList = new List<FoodUnit>();
        if (count <= 0 || list==null)
            return randList;

        // ����һ��List������ԭ����List����Ӱ��
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
    /// Ϊ��Ƭ���ը��������
    /// </summary>
    public static void AddBombModifier(BaseUnit unit)
    {
        // ��ȡ100%���ˣ��Լ����߻ҽ���ɱЧ��
        unit.NumericBox.Defense.SetBase(1);
        unit.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }

    /// <summary>
    /// ��ȡ�ض���������ߵĿɹ����ѷ���λ
    /// </summary>
    /// <param name="rowIndex">���±�</param>
    /// <param name="start_temp_x">��ʼ�ȽϺ����꣨λ�ڸ������Ҳ�ĵ�λ�ᱻ���Ե���</param>
    /// <returns>�������Ŀɹ������ѷ���λ����Ϊ����û��Ŀ��</returns>
    public static BaseUnit GetSpecificRowFarthestLeftCanTargetedAlly(int rowIndex, float start_temp_x, bool canSelectedCharacter)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // ���������������ɱ�ѡȡ������ʳ�������ﵥλ��������
            // ���һ���Ƚ����������ǵ�λ������ȵ�ǰ�ȽϺ�����С
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || (canSelectedCharacter && item is CharacterUnit)) && item.IsAlive() && item.transform.position.x < temp_x)
            {
                // ���Ŀ������ʳ��λ������Ҫ�ж�Ŀ�����ΪĬ�����Ϳ�Ƭ���߻������Ϳ�Ƭ����������ΪѡȡĿ��
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
    /// ��ȡ�ض��������ұߵĿɹ����ѷ���λ
    /// </summary>
    /// <param name="rowIndex">���±�</param>
    /// <param name="start_temp_x">��ʼ�ȽϺ����꣨λ�ڸ��������ĵ�λ�ᱻ���Ե���</param>
    /// <returns>������ҵĿɹ������ѷ���λ����Ϊ����û��Ŀ��</returns>
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
            // ���������������ɱ�ѡȡ������ʳ�������ﵥλ��������
            // ���һ���Ƚ����������ǵ�λ������ȵ�ǰ�ȽϺ������
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || (canSelectedCharacter && item is CharacterUnit)) && item.IsAlive() && item.transform.position.x > temp_x && item.transform.position.x <= max_x)
            {
                // ���Ŀ������ʳ��λ������Ҫ�ж�Ŀ�����ΪĬ�����Ϳ�Ƭ���߻������Ϳ�Ƭ����ը�����Ϳ�Ƭ����������ΪѡȡĿ��
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
    /// ��ȡ <����һ������> ���ѷ���λ���� ��� ����
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
                // ���Ŀ���ڴ���������ж���Ϊ�棬�����+1
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
    /// ��ȡ <����һ������> ���ѷ���λ���� ���� ����
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
                // ���Ŀ���ڴ���������ж���Ϊ�棬�����+1
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
    /// ��ȡ <����һ������> ���ѷ���λ���� ��� ����
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMaxConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        return GetRowListWhichHasMaxConditionAllyCount(new List<int>() { 0, 1, 2, 3, 4, 5, 6 }, ConditionFunc);
    }

    /// <summary>
    /// ��ȡ <����һ������> ���ѷ���λ���� ���� ����
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMinConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        return GetRowListWhichHasMinConditionAllyCount(new List<int>() { 0, 1, 2, 3, 4, 5, 6 }, ConditionFunc);
    }

    /// <summary>
    /// �жϷ�������ǰĿ���Ƿ��ǿ��Ա���ΪĿ����ѷ���λ
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
    /// ��ȡ ����Ϊ����Ŀ����ѷ���λ ��� ����
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMaxCanTargetedAllyCount()
    {
        return GetRowListWhichHasMaxConditionAllyCount(IsCanTargetedAlly);
    }

    /// <summary>
    /// ��ȡ ����Ϊ����Ŀ����ѷ���λ ���� ����
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListWhichHasMinCanTargetedAllyCount()
    {
        return GetRowListWhichHasMinConditionAllyCount(IsCanTargetedAlly);
    }


    /// <summary>
    /// ��ȡ �ض� ����
    /// 1��ȡ����<�����ض�����>�ĵ�λ����Ϊ���еĴ���
    /// 2�������д�����ȡ����<�����ض�����2>�ĵ�λ������Щ����Ķ�Ӧ����Ϊ����з��س�ȥ
    /// </summary>
    /// <returns></returns>
    public static List<int> GetRowListBySpecificConditions(Func<BaseUnit, BaseUnit, bool> RowCompareFunc, Func<BaseUnit, BaseUnit, int> LastCompareFunc)
    {
        List<int> list = new List<int>();
        BaseUnit compareUnit = null; // ��ǰ��Ϊ���۱�׼�ĵ�λ
        for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
        {
            BaseUnit rowStandUnit = null; // ��ǰ�д���λ
            foreach (var unit in GameController.Instance.mAllyList[rowIndex])
            {
                // ���Ŀ���ڴ���������ж���Ϊ�棬����unit��ȡ����ǰ����unit
                if (RowCompareFunc(rowStandUnit, unit))
                    rowStandUnit = unit;
            }
            // ����ȽϽ��Ϊ1�����ڣ��������List��Ȼ��ѵ�ǰ�Ƚ��м�����У��������۱�׼�ĵ�λΪ��ǰ����Ƚ���
            // ����ȽϽ��Ϊ0�����ڣ�����ֻ�ѵ�ǰ�Ƚ��м������
            // ����ȽϽ��Ϊ-1��С�ڣ�����������ֵ������ʲôҲ������
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
        { FoodInGridType.Default, "��ͨ�࿨Ƭ"},
        { FoodInGridType.Bomb, "ը���࿨Ƭ"},
        { FoodInGridType.WaterVehicle, "ˮ�ؾ��࿨Ƭ"},
        { FoodInGridType.LavaVehicle, "���ؾ��࿨Ƭ"},
        { FoodInGridType.Shield, "�����࿨Ƭ" },
        { FoodInGridType.NoAttach, "�������࿨Ƭ"}
    };

    /// <summary>
    /// ��ȡ��ʳ�ڸ����Ϸ��������
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
    /// ����ʳ����Ϣ�Ĳ����滻������ֻ��ö���ˣ�
    /// </summary>
    private static Dictionary<FoodNameTypeMap, Func<string, int, int, string>[]> foodInfoParamReplaceFuncDict = new Dictionary<FoodNameTypeMap, Func<string, int, int, string>[]>() {
        { FoodNameTypeMap.SmallStove, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.SmallStove, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.SmallStove, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.SmallStove, 1).GetCost(level);
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                str = ParamManager.GetStringByReplaceParam(str, "cost", cost.ToString());
                return str;
            },
            // С��ʿ
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.SmallStove, shape).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            } }
        },
        { FoodNameTypeMap.CupLight, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CupLight, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.CupLight, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.CupLight, 1).GetCost(level);
                string sec = ((float)CupLight.GetGrowTime(shape, level)/60).ToString("#0.00");
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                str = ParamManager.GetStringByReplaceParam(str, "cost", cost.ToString());
                str = ParamManager.GetStringByReplaceParam(str, "growTime", sec);
                return str;
            },
            // С��ʿ
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CupLight, shape).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            } }
        },
        { FoodNameTypeMap.CherryPudding, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CherryPudding, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                double cost = GetCardBuilderAttribute(FoodNameTypeMap.CherryPudding, 1).GetCost(level);
                return ParamManager.GetStringByReplaceParam(s, "cost", cost.ToString());
            },
            // С��ʿ
            null
        } },
        { FoodNameTypeMap.Heater, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            (s, level, shape)=> {
                double dmgRate = GetAttribute(FoodNameTypeMap.Heater, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "dmg_rate", dmgRate.ToString("#0.00"));
            },
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                double dmgRate = GetAttribute(FoodNameTypeMap.Heater, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "dmg_rate", dmgRate.ToString("#0.00"));
            },
            // С��ʿ
            null }
        },
        { FoodNameTypeMap.CoffeePowder, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            (s, level, shape)=> {
                double time = GetAttribute(FoodNameTypeMap.CoffeePowder, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "time", time.ToString("#0.0"));
            },
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                double time = GetAttribute(FoodNameTypeMap.CoffeePowder, shape).valueList[level];
                return ParamManager.GetStringByReplaceParam(s, "time", time.ToString("#0.0"));
            },
            // С��ʿ
            null }
        },
        { FoodNameTypeMap.BigStove, new Func<string, int, int, string>[3]{
            // ���ܣ���ࣩ
            null,
            // ���ܣ���ϸ��
            (s, level, shape)=> {
                float value = GetAttribute(FoodNameTypeMap.SmallStove, shape).valueList[level];
                double cost = GameManager.Instance.attributeManager.GetCardBuilderAttribute((int)FoodNameTypeMap.BigStove, 1).GetCost(level);
                string str = ParamManager.GetStringByReplaceParam(s, "value", "1/"+value+"="+1/value);
                return str;
            },
            // С��ʿ
            null}
        },
        // ģ��
        //{ FoodNameTypeMap.SmallStove, new Func<string, int, int, string>[3]{
        //    // ���ܣ���ࣩ
        //    (s, level, shape)=> { },
        //    // ���ܣ���ϸ��
        //    (s, level, shape)=> { },
        //    // С��ʿ
        //    (s, level, shape)=> { } }
        //},
    };

    /// <summary>
    /// ��ȡ�Լ�������Ĳ���������
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
    /// ��ȡ����ϸ�����Ĳ���������
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
    /// ��ȡ��С��ʿ�Ĳ���������
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
    /// �ǲ��ǿ���Ϊ����Ŀ�����ʳ��λ��ĿǰĬ��ֻ�л����ࡢ��ͨ�ࡢը�����ǣ�
    /// </summary>
    /// <returns></returns>
    public static bool IsAttackableFoodType(FoodUnit f)
    {
        return f.GetFoodInGridType().Equals(FoodInGridType.Default) || f.GetFoodInGridType().Equals(FoodInGridType.Bomb) || f.GetFoodInGridType().Equals(FoodInGridType.Shield);
    }
}
