using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// ��Ƭ��������������Ŀǰ��Ҫ�����ṩ��̬��������ö���¼���
/// </summary>
public class CardBuilderManager
{
    private static CardBuilderManager Instance;
    
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> BeforeBuildActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> AfterBuildActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> BeforeDestructeActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> AfterDestructeActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnSelectedEnterDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> OnDuringSelectedDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnSelectedExitDict; 
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> OnTrySelectDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnTrySelectOtherDict;
    private Dictionary<FoodNameTypeMap, Func<BaseGrid, bool>> CanConstructeInGridDict;
    // �𿨱�
    private static List<FoodNameTypeMap> GoldenCardList = new List<FoodNameTypeMap>() {
        FoodNameTypeMap.BigStove,
        FoodNameTypeMap.SpinCoffee,
        FoodNameTypeMap.Takoyaki,
        FoodNameTypeMap.ChocolateBread,
        FoodNameTypeMap.TofuPitcher,
        FoodNameTypeMap.IceEggPitcher,
        FoodNameTypeMap.SugarGourd
    };

    public static bool IsGoldenCard(FoodNameTypeMap type)
    {
        return GoldenCardList.Contains(type);
    }

    public static CardBuilderManager GetInstance()
    {
        if (Instance == null)
            Instance = new CardBuilderManager();
        return Instance;
    }

    public CardBuilderManager() 
    {
        /// <summary>
        /// ����ǰ
        /// </summary>
        BeforeBuildActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// �����
        /// </summary>
        AfterBuildActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {
            Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> d = AfterBuildActionDict;
            d.Add(FoodNameTypeMap.BigStove, AfterBuildBigFire);
            d.Add(FoodNameTypeMap.WoodenDisk, AfterBuildWoodenDisk);
            d.Add(FoodNameTypeMap.PokerShield, AfterBuildPokerShield);
            d.Add(FoodNameTypeMap.CottonCandy, AfterBuildCottonCandy);
            d.Add(FoodNameTypeMap.ChocolateBread, AfterBuildChocolateBread);
            //d.Add(FoodNameTypeMap.RaidenBaguette, AfterBuildRaidenBaguette);
            d.Add(FoodNameTypeMap.IceBucket, AfterBuildIceBucketFoodUnit);
            d.Add(FoodNameTypeMap.BoiledWaterBoom, AfterBuildBoiledWaterBoom);
            //d.Add(FoodNameTypeMap.SpinCoffee, AfterBuildSpinCoffee);
        }

        /// <summary>
        /// ����ǰ
        /// </summary>
        BeforeDestructeActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// ���ٺ�
        /// </summary>
        AfterDestructeActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {
            Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> d = AfterDestructeActionDict;
            d.Add(FoodNameTypeMap.BigStove, AfterDestructeBigFire);
            d.Add(FoodNameTypeMap.WoodenDisk, AfterDestructeWoodenDisk);
            d.Add(FoodNameTypeMap.PokerShield, AfterDestructePokerShield);
            d.Add(FoodNameTypeMap.CottonCandy, AfterDestructeCottonCandy);
            d.Add(FoodNameTypeMap.ChocolateBread, AfterDestructeChocolateBread);
            //d.Add(FoodNameTypeMap.RaidenBaguette, AfterDestructeRaidenBaguette);
            d.Add(FoodNameTypeMap.IceBucket, AfterDestructeIceBucketFoodUnit);
            d.Add(FoodNameTypeMap.BoiledWaterBoom, AfterDestructeBoiledWaterBoom);
            //d.Add(FoodNameTypeMap.SpinCoffee, AfterDestructeSpinCoffee);
        }

        /// <summary>
        /// ����ѡȡ״̬ʱ
        /// </summary>
        OnSelectedEnterDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// ��ѡȡ״̬��
        /// </summary>
        OnDuringSelectedDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// �뿪ѡȡ״̬
        /// </summary>
        OnSelectedExitDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// ����ѡȡʱ
        /// </summary>
        OnTrySelectDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// �ڳ���ѡ�������Ŀ���ʱ
        /// </summary>
        OnTrySelectOtherDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {
            Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> d = OnTrySelectOtherDict;
            d.Add(FoodNameTypeMap.IceCream, OnIceCreamTrySelectedOther);
        }

        /// <summary>
        /// �������ڸ�������
        /// </summary>
        CanConstructeInGridDict = new Dictionary<FoodNameTypeMap, Func<BaseGrid, bool>>();
        {
            Dictionary<FoodNameTypeMap, Func<BaseGrid, bool>> d = CanConstructeInGridDict;
            // d.Add(FoodNameTypeMap.WoodenDisk, WoodenDiskCanConstructeInGrid);
            // d.Add(FoodNameTypeMap.CottonCandy, CottonCandyCanConstructeInGrid);
        }
    }

    /// <summary>
    /// ���ÿ�Ƭ��������Ĭ���¼�
    /// </summary>
    public void SetDefaultActionListener(BaseCardBuilder cardBuilder)
    {
        FoodNameTypeMap type = (FoodNameTypeMap)cardBuilder.mType;
        if (BeforeBuildActionDict.ContainsKey(type))
        {
            cardBuilder.AddBeforeBuildAction(BeforeBuildActionDict[type]);
        }
        if (AfterBuildActionDict.ContainsKey(type))
        {
            cardBuilder.AddAfterBuildAction(AfterBuildActionDict[type]);
        }
        if (BeforeDestructeActionDict.ContainsKey(type))
        {
            cardBuilder.AddBeforeDestructeAction(BeforeDestructeActionDict[type]);
        }
        if (AfterDestructeActionDict.ContainsKey(type))
        {
            cardBuilder.AddAfterDestructeAction(AfterDestructeActionDict[type]);
        }
        if (OnSelectedEnterDict.ContainsKey(type))
        {
            cardBuilder.SetOnSelectedEnterEvent(OnSelectedEnterDict[type]);
        }
        if (OnDuringSelectedDict.ContainsKey(type))
        {
            cardBuilder.SetOnDuringSelectedEvent(OnDuringSelectedDict[type]);
        }
        if (OnSelectedExitDict.ContainsKey(type))
        {
            cardBuilder.SetOnSelectedExitEvent(OnSelectedExitDict[type]);
        }
        if (OnTrySelectDict.ContainsKey(type))
        {
            cardBuilder.SetOnTrySelectEvent(OnTrySelectDict[type]);
        }
        if (OnTrySelectOtherDict.ContainsKey(type))
        {
            cardBuilder.SetOnTrySelectOtherEvent(OnTrySelectOtherDict[type]);
        }
        if (CanConstructeInGridDict.ContainsKey(type))
        {
            cardBuilder.AddCanConstructeInGridList(CanConstructeInGridDict[type]);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    // ����Ĳ�����ֱ�����·���������ö�ٸ������⿨Ƭ�����
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// ��ֲ�Ƕ�ת������ֵЧ��
    /// </summary>
    private void AfterBuildBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
        }
    }

    /// <summary>
    /// �Ƴ��Ƕ�ת�����ֵЧ��
    /// </summary>
    private void AfterDestructeBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 50f;
        }
    }

    /// <summary>
    /// ��ѡ�����ܵ�����³���ѡ��������
    /// </summary>
    /// <param name="icecreamBuilder"></param>
    /// <param name="nextBuilder"></param>
    private void OnIceCreamTrySelectedOther(BaseCardBuilder icecreamBuilder, BaseCardBuilder nextBuilder)
    {
        // �����ͨ��������������뿪���״̬���Ҹÿ���CDû��ת�ã���ֱ�����ĵ�����ܲ��øÿ�CD����
        if (nextBuilder != null && !nextBuilder.IsColdDown() && icecreamBuilder != nextBuilder && !CardBuilderManager.IsGoldenCard((FoodNameTypeMap)nextBuilder.mType))
        {
            icecreamBuilder.Cost();
            int cd = nextBuilder.mCDLeft;
            nextBuilder.ResetCD();
            GameController.Instance.mCardController.CancelSelectCard();
            // ����Ƕ�ת����ܻ��᷵��CD
            if (icecreamBuilder.mShape >= 2)
                icecreamBuilder.mCDLeft = UnityEngine.Mathf.Min(icecreamBuilder.mCD, cd);
        }
    }

    /// <summary>
    /// ��ֲľ���Ӻ����ֵЧ��
    /// </summary>
    private void AfterBuildWoodenDisk(BaseCardBuilder builder)
    {
        if (builder.mShape < 1)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 10f;
        }
        else
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 5f;
        }
    }

    /// <summary>
    /// �Ƴ�ľ���Ӻ��ֵЧ��
    /// </summary>
    private void AfterDestructeWoodenDisk(BaseCardBuilder builder)
    {
        if (builder.mShape < 1)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 10f;
        }
        else
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 5f;
        }
    }

    /// <summary>
    /// ľ�����Ƿ�������ڸ�����
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    //private bool WoodenDiskCanConstructeInGrid(BaseGrid grid)
    //{
    //    return grid.IsContainGridType(GridType.Water);
    //}

    /// <summary>
    /// ��ֲ�˿˻��ֺ����ֵЧ��
    /// </summary>
    private void AfterBuildPokerShield(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 100f;
        }
    }

    /// <summary>
    /// �Ƴ��˿˻��ֺ��ֵЧ��
    /// </summary>
    private void AfterDestructePokerShield(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 100f;
        }
    }

    /// <summary>
    /// ��ֲ�޻��Ǻ����ֵЧ��
    /// </summary>
    private void AfterBuildCottonCandy(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
    }

    /// <summary>
    /// �Ƴ��޻��Ǻ��ֵЧ��
    /// </summary>
    private void AfterDestructeCottonCandy(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 50f;
    }

    /// <summary>
    /// �޻����Ƿ�������ڸ�����
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    //private bool CottonCandyCanConstructeInGrid(BaseGrid grid)
    //{
    //    return grid.IsContainGridType(GridType.Lava);
    //}

    /// <summary>
    /// ��ֲ�ɿ�����������ֵЧ��
    /// </summary>
    private void AfterBuildChocolateBread(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 75f;
    }

    /// <summary>
    /// �Ƴ��ɿ��������ļ�ֵЧ��
    /// </summary>
    private void AfterDestructeChocolateBread(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 75f;
    }

    /// <summary>
    /// ��ֲ�׵����ֵЧ��
    /// </summary>
    private void AfterBuildRaidenBaguette(BaseCardBuilder builder)
    {
        if (builder.mShape < 1)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 300f;
        }
        else
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 250f;
        }
    }

    /// <summary>
    /// �Ƴ��׵���ֵЧ��
    /// </summary>
    private void AfterDestructeRaidenBaguette(BaseCardBuilder builder)
    {
        if (builder.mShape < 1)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 300f;
        }
        else
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 250f;
        }
    }

    /// <summary>
    /// ��ֲ��Ͱը������ֵЧ��
    /// </summary>
    private void AfterBuildIceBucketFoodUnit(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 75f;
    }

    /// <summary>
    /// �Ƴ���Ͱը�����ֵЧ��
    /// </summary>
    private void AfterDestructeIceBucketFoodUnit(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 75f;
    }

    /// <summary>
    /// ��ֲ��ˮ��ը������ֵЧ��
    /// </summary>
    private void AfterBuildBoiledWaterBoom(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 100f;
    }

    /// <summary>
    /// �Ƴ���ˮ��ը�����ֵЧ��
    /// </summary>
    private void AfterDestructeBoiledWaterBoom(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 100f;
    }

    /// <summary>
    /// ��ֲ��ת�������
    /// </summary>
    private void AfterBuildSpinCoffee(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = (float)builder.attr.GetCost(0) + 50*Mathf.Pow(builder.GetAllProducts().Count, 2);
    }

    /// <summary>
    /// �Ƴ��Ƕ�ת�����ֵЧ��
    /// </summary>
    private void AfterDestructeSpinCoffee(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = (float)builder.attr.GetCost(0) + 50 * Mathf.Pow(builder.GetAllProducts().Count, 2);
    }
}
