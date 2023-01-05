using System.Collections.Generic;
using System;
using UnityEngine;

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
        { FoodNameTypeMap.IceCream, new List<string>(){"�����","���߱����" } },
        { FoodNameTypeMap.WaterPipe, new List<string>(){"˫��ˮ��","����˫��ˮ��","�Ͻ�ˮ��" } },
        { FoodNameTypeMap.ThreeLinesVine, new List<string>(){"���߾Ƽ�","ǿ�����߾Ƽ�","�ս��߾Ƽ�" } },
        { FoodNameTypeMap.Heater, new List<string>(){"����","���ӿ���","���ҿ���","��ţ������" } },
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"ľ����" ,"����ľ����"} },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"�޻���" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"��Ͱը��" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"��Ƥ����","��̹�Ƥ����" } },
        { FoodNameTypeMap.Takoyaki, new List<string>(){"������","����������","��Ӱ������" } },
        //{ FoodNameTypeMap.FlourBag, new List<string>(){"��۴�" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"��ƿը��" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"����ը��" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"��ˮ��ը��" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"��ʿ��ը��" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"���ܱ�ը���","���ǲ������","�ʹڲ������" } },
        { FoodNameTypeMap.SpinCoffee, new List<string>(){"��ת�������","������ת�������","ԭ����ת�������" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"������" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"�������","�����������","��è�������" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"������ը��" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"�Ǻ�«�ڵ�","ˮ���Ǻ�«�ڵ�","�߲��Ǻ�«�ڵ�" } },
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "�����ƻ���" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "�˿˻���" } },
        { FoodNameTypeMap.SaladPitcher, new List<string>(){ "ɫ��Ͷ��","����ɫ��Ͷ��", "����ɫ��Ͷ��" } },
        { FoodNameTypeMap.ChocolatePitcher, new List<string>(){ "�ɿ���Ͷ��", "Ũ���ɿ���Ͷ��", "�����ɿ���Ͷ��" } },
        { FoodNameTypeMap.TofuPitcher, new List<string>(){ "������Ͷ��", "ʲ��������Ͷ��", "���������Ͷ��" } },
        { FoodNameTypeMap.EggPitcher, new List<string>(){ "����Ͷ��", "��������", "ǿϮ����" } },
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
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
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
    public static BaseUnit GetSpecificRowFarthestLeftCanTargetedAlly(int rowIndex, float start_temp_x)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // ���������������ɱ�ѡȡ������ʳ�������ﵥλ��������
            // ���һ���Ƚ����������ǵ�λ������ȵ�ǰ�ȽϺ�����С
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || item is CharacterUnit) && item.IsAlive() && item.transform.position.x < temp_x)
            {
                // ���Ŀ������ʳ��λ������Ҫ�ж�Ŀ�����ΪĬ�����Ϳ�Ƭ���߻������Ϳ�Ƭ����������ΪѡȡĿ��
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
    /// ��ȡ�ض��������ұߵĿɹ����ѷ���λ
    /// </summary>
    /// <param name="rowIndex">���±�</param>
    /// <param name="start_temp_x">��ʼ�ȽϺ����꣨λ�ڸ��������ĵ�λ�ᱻ���Ե���</param>
    /// <returns>������ҵĿɹ������ѷ���λ����Ϊ����û��Ŀ��</returns>
    public static BaseUnit GetSpecificRowFarthestRightCanTargetedAlly(int rowIndex, float start_temp_x)
    {
        BaseUnit targetUnit = null;
        List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(rowIndex);
        float temp_x = start_temp_x;
        foreach (var item in list)
        {
            // ���������������ɱ�ѡȡ������ʳ�������ﵥλ��������
            // ���һ���Ƚ����������ǵ�λ������ȵ�ǰ�ȽϺ������
            if (UnitManager.CanBeSelectedAsTarget(null, item) && (item is FoodUnit || item is CharacterUnit) && item.IsAlive() && item.transform.position.x > temp_x)
            {
                // ���Ŀ������ʳ��λ������Ҫ�ж�Ŀ�����ΪĬ�����Ϳ�Ƭ���߻������Ϳ�Ƭ����������ΪѡȡĿ��
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
    /// ��ȡ <����һ������> ���ѷ���λ���� ��� ����
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
    public static List<int> GetRowListWhichHasMinConditionAllyCount(Func<BaseUnit, bool> ConditionFunc)
    {
        List<int> list = new List<int>();
        int min = int.MaxValue;
        for (int rowIndex = 0; rowIndex < GameController.Instance.mAllyList.Length; rowIndex++)
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
    /// �жϷ�������ǰĿ���Ƿ��ǿ��Ա���ΪĿ����ѷ���λ
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
}
