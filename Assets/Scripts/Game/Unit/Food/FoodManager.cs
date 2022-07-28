using System.Collections.Generic;
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
        //{ FoodNameTypeMap.WoodenDisk, new List<string>(){"ľ����" } },
        //{ FoodNameTypeMap.CottonCandy, new List<string>(){"�޻���" } },
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
    };


    /// <summary>
    /// ֱ�ӻ�ȡ��ǰ�汾������ʳ����תְ��
    /// </summary>
    /// <returns></returns>
    public static Dictionary<FoodNameTypeMap, List<string>> GetAllFoodDict()
    {
        return foodNameDict;
    }
}
