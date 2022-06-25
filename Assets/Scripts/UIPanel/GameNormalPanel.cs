using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 负责管理战斗场景UI面板的类
/// </summary>

public class GameNormalPanel : MonoBehaviour, IBasePanel
{
    public const string Path = "Pictures/UI/"; // 管理的资源存放路径

    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList; // 卡槽UI（横向）
    public GameObject mCardBuilderList2;  // 卡槽UI（纵向）
    public CardModel mCardModel; // 卡片模型（选卡时会跟随鼠标的那种）

    private void Awake()
    {
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mCardModel = mCardControllerUI.transform.Find("Emp_CardModel").gameObject.GetComponent<CardModel>();
    }

    // 在上方UI中增加一个卡槽
    public void AddCardSlot(BaseCardBuilder CardBuilder)
    {
        if (mCardBuilderList.transform.childCount < 13)
        {
            CardBuilder.SetIndex(mCardBuilderList.transform.childCount);
            //CardBuilder.SetCardIndex(0);
            CardBuilder.transform.SetParent(mCardBuilderList.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
        }
        else if(mCardBuilderList2.transform.childCount < 5)
        {
            CardBuilder.SetIndex(13 + mCardBuilderList2.transform.childCount);
            //CardBuilder.SetCardIndex(0);
            CardBuilder.transform.SetParent(mCardBuilderList2.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
        }
        else
        {
            Debug.LogWarning("携带卡片已超过18张！！多余卡片槽不生效");
        }
    }

    /// <summary>
    /// 进入角色放置模式
    /// </summary>
    public void EnterCharacterConstructMode()
    {
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCharacter);
    }

    /// <summary>
    /// 离开角色放置模式
    /// </summary>
    public void ExitCharacterConstructMode()
    {
        mCardModel.HideCardModel();
    }

    /// <summary>
    /// 进入卡片放置模式，由某个卡片建造器被成功选取时触发
    /// </summary>
    public void EnterCardConstructMode()
    {
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCard);
    }

    /// <summary>
    /// 离开卡片放置模式，卡片建造成功或者取消建造时触发
    /// </summary>
    public void ExitCardConstructMode()
    {
        mCardModel.HideCardModel();
    }

    public void InitPanel()
    {
        // throw new System.NotImplementedException();
    }

    public void EnterPanel()
    {
        throw new System.NotImplementedException();
    }

    public void ExitPanel()
    {
        throw new System.NotImplementedException();
    }

    public void UpdatePanel()
    {
        throw new System.NotImplementedException();
    }
}
