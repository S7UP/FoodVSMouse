using System.Collections.Generic;
using System;
/// <summary>
/// ��Ƭ��������������Ŀǰ��Ҫ�����ṩ��̬��������ö���¼���
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
    public void AfterBuildBigFire(BaseCardBuilder builder)
    {
        if (builder.mShape < 2)
        {
            builder.mCostDict["Fire"] = builder.mCostDict["Fire"] + 50f;
        }
    }

    /// <summary>
    /// �Ƴ��Ƕ�ת�����ֵЧ��
    /// </summary>
    public void AfterDestructeBigFire(BaseCardBuilder builder)
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
    public void OnIceCreamTrySelectedOther(BaseCardBuilder icecreamBuilder, BaseCardBuilder nextBuilder)
    {
        // �����ͨ��������������뿪���״̬���Ҹÿ���CDû��ת�ã���ֱ�����ĵ�����ܲ��øÿ�CD����
        if (nextBuilder != null && !nextBuilder.IsColdDown() && icecreamBuilder != nextBuilder)
        {
            icecreamBuilder.FullCD();
            nextBuilder.ResetCD();
            GameController.Instance.mCardController.CancelSelectCard();
        }
    }

}
