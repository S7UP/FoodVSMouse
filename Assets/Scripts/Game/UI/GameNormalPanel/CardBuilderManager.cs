using System.Collections.Generic;
using System;
/// <summary>
/// 卡片建造器管理器（目前主要用来提供静态方法，并枚举事件）
/// </summary>
public class CardBuilderManager
{
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> BeforeBuildActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> AfterBuildActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> BeforeDestructeActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> AfterDestructeActionDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnSelectedEnterDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> OnDuringSelectedDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnSelectedExitDict; 
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder>> OnTrySelectDict;
    private Dictionary<FoodNameTypeMap, Action<BaseCardBuilder, BaseCardBuilder>> OnTrySelectOtherDict; 

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
    public void AfterBuildBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
        }
    }

    /// <summary>
    /// 移除非二转大火后减值效果
    /// </summary>
    public void AfterDestructeBigFire(BaseCardBuilder builder)
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
    public void OnIceCreamTrySelectedOther(BaseCardBuilder icecreamBuilder, BaseCardBuilder nextBuilder)
    {
        // 如果是通过点击其他卡槽离开这个状态，且该卡槽CD没有转好，则直接消耗掉冰淇淋并让该卡CD重置
        if (nextBuilder != null && !nextBuilder.IsColdDown() && icecreamBuilder != nextBuilder)
        {
            icecreamBuilder.FullCD();
            nextBuilder.ResetCD();
            GameController.Instance.mCardController.CancelSelectCard();
        }
    }

}
