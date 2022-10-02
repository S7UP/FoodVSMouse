using System.Collections.Generic;

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
        { FoodNameTypeMap.Heater, new List<string>(){"����","���ӿ���","���ҿ���" } },
        { FoodNameTypeMap.WoodenDisk, new List<string>(){"ľ����" } },
        { FoodNameTypeMap.CottonCandy, new List<string>(){"�޻���" } },
        { FoodNameTypeMap.IceBucket, new List<string>(){"��Ͱը��" } },
        { FoodNameTypeMap.MelonShield, new List<string>(){"��Ƥ����","��̹�Ƥ����" } },
        //{ FoodNameTypeMap.LightningBread, new List<string>(){"�׵糤�����","���ܳ������","���������" } },
        //{ FoodNameTypeMap.FlourBag, new List<string>(){"��۴�" } },
        { FoodNameTypeMap.WineBottleBoom, new List<string>(){"��ƿը��" } },
        { FoodNameTypeMap.CokeBoom, new List<string>(){"����ը��" } },
        { FoodNameTypeMap.BoiledWaterBoom, new List<string>(){"��ˮ��ը��" } },
        { FoodNameTypeMap.WiskyBoom, new List<string>(){"��ʿ��ը��" } },
        { FoodNameTypeMap.PineappleBreadBoom, new List<string>(){"���ܱ�ը���","���ǲ������","�ʹڲ������" } },
        //{ FoodNameTypeMap.OilLight, new List<string>(){"�͵�","�����͵�" } },
        //{ FoodNameTypeMap.Fan, new List<string>(){"������" } },
        { FoodNameTypeMap.MouseCatcher, new List<string>(){"�������" } },
        { FoodNameTypeMap.SpicyStringBoom, new List<string>(){"������ը��" } },
        { FoodNameTypeMap.SugarGourd, new List<string>(){"�Ǻ�«�ڵ�","ˮ���Ǻ�«�ڵ�","�߲��Ǻ�«�ڵ�" } },
        { FoodNameTypeMap.MushroomDestroyer, new List<string>(){ "�����ƻ���" } },
        { FoodNameTypeMap.PokerShield, new List<string>(){ "�˿˻���" } },
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
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }
}
