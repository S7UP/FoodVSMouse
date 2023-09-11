using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 卡片建造器管理器（目前主要用来提供静态方法，并枚举事件）
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
    // 金卡表
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
        /// 建造前
        /// </summary>
        BeforeBuildActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 建造后
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
        /// 销毁前
        /// </summary>
        BeforeDestructeActionDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 销毁后
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
        /// 进入选取状态时
        /// </summary>
        OnSelectedEnterDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 在选取状态中
        /// </summary>
        OnDuringSelectedDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 离开选取状态
        /// </summary>
        OnSelectedExitDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 尝试选取时
        /// </summary>
        OnTrySelectDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>>();
        {

        }

        /// <summary>
        /// 在尝试选择其他的卡槽时
        /// </summary>
        OnTrySelectOtherDict = new Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>>();
        {
            Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> d = OnTrySelectOtherDict;
            d.Add(FoodNameTypeMap.IceCream, OnIceCreamTrySelectedOther);
        }

        /// <summary>
        /// 可以种在格子上吗
        /// </summary>
        CanConstructeInGridDict = new Dictionary<FoodNameTypeMap, Func<BaseGrid, bool>>();
        {
            Dictionary<FoodNameTypeMap, Func<BaseGrid, bool>> d = CanConstructeInGridDict;
            // d.Add(FoodNameTypeMap.WoodenDisk, WoodenDiskCanConstructeInGrid);
            // d.Add(FoodNameTypeMap.CottonCandy, CottonCandyCanConstructeInGrid);
        }
    }

    /// <summary>
    /// 设置卡片建造器的默认事件
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
    // 下面的不建议直接往下翻看，都是枚举各种特殊卡片的情况
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// 种植非二转大火后增值效果
    /// </summary>
    private void AfterBuildBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
        }
    }

    /// <summary>
    /// 移除非二转大火后减值效果
    /// </summary>
    private void AfterDestructeBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 50f;
        }
    }

    /// <summary>
    /// 当选择冰淇淋的情况下尝试选其他卡槽
    /// </summary>
    /// <param name="icecreamBuilder"></param>
    /// <param name="nextBuilder"></param>
    private void OnIceCreamTrySelectedOther(BaseCardBuilder icecreamBuilder, BaseCardBuilder nextBuilder)
    {
        // 如果是通过点击其他卡槽离开这个状态，且该卡槽CD没有转好，则直接消耗掉冰淇淋并让该卡CD重置
        if (nextBuilder != null && !nextBuilder.IsColdDown() && icecreamBuilder != nextBuilder && !CardBuilderManager.IsGoldenCard((FoodNameTypeMap)nextBuilder.mType))
        {
            icecreamBuilder.Cost();
            int cd = nextBuilder.mCDLeft;
            nextBuilder.ResetCD();
            GameController.Instance.mCardController.CancelSelectCard();
            // 如果是二转冰淇淋还会返还CD
            if (icecreamBuilder.mShape >= 2)
                icecreamBuilder.mCDLeft = UnityEngine.Mathf.Min(icecreamBuilder.mCD, cd);
        }
    }

    /// <summary>
    /// 种植木盘子后的增值效果
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
    /// 移除木盘子后减值效果
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
    /// 木盘子是否可以种在格子上
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    //private bool WoodenDiskCanConstructeInGrid(BaseGrid grid)
    //{
    //    return grid.IsContainGridType(GridType.Water);
    //}

    /// <summary>
    /// 种植扑克护罩后的增值效果
    /// </summary>
    private void AfterBuildPokerShield(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 100f;
        }
    }

    /// <summary>
    /// 移除扑克护罩后减值效果
    /// </summary>
    private void AfterDestructePokerShield(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 100f;
        }
    }

    /// <summary>
    /// 种植棉花糖后的增值效果
    /// </summary>
    private void AfterBuildCottonCandy(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
    }

    /// <summary>
    /// 移除棉花糖后减值效果
    /// </summary>
    private void AfterDestructeCottonCandy(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 50f;
    }

    /// <summary>
    /// 棉花糖是否可以种在格子上
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    //private bool CottonCandyCanConstructeInGrid(BaseGrid grid)
    //{
    //    return grid.IsContainGridType(GridType.Lava);
    //}

    /// <summary>
    /// 种植巧克力面包后的增值效果
    /// </summary>
    private void AfterBuildChocolateBread(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 75f;
    }

    /// <summary>
    /// 移除巧克力面包后的减值效果
    /// </summary>
    private void AfterDestructeChocolateBread(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 75f;
    }

    /// <summary>
    /// 种植雷电后增值效果
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
    /// 移除雷电后减值效果
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
    /// 种植冰桶炸弹后增值效果
    /// </summary>
    private void AfterBuildIceBucketFoodUnit(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 75f;
    }

    /// <summary>
    /// 移除冰桶炸弹后减值效果
    /// </summary>
    private void AfterDestructeIceBucketFoodUnit(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 75f;
    }

    /// <summary>
    /// 种植开水壶炸弹后增值效果
    /// </summary>
    private void AfterBuildBoiledWaterBoom(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 100f;
    }

    /// <summary>
    /// 移除开水壶炸弹后减值效果
    /// </summary>
    private void AfterDestructeBoiledWaterBoom(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = builder.mCostDict["Fire"] - 100f;
    }

    /// <summary>
    /// 种植旋转咖啡喷壶
    /// </summary>
    private void AfterBuildSpinCoffee(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = (float)builder.attr.GetCost(0) + 50*Mathf.Pow(builder.GetAllProducts().Count, 2);
    }

    /// <summary>
    /// 移除非二转大火后减值效果
    /// </summary>
    private void AfterDestructeSpinCoffee(BaseCardBuilder builder)
    {
        builder.mCostDict["Fire"] = (float)builder.attr.GetCost(0) + 50 * Mathf.Pow(builder.GetAllProducts().Count, 2);
    }
}
