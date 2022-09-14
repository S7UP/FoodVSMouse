using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责管理战斗场景UI面板的类
/// </summary>

public class GameNormalPanel : MonoBehaviour, IBasePanel
{
    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList; // 卡槽UI（横向）
    public GameObject mCardBuilderList2;  // 卡槽UI（纵向）
    public CardModel mCardModel; // 卡片模型（选卡时会跟随鼠标的那种）
    public Transform mShovelSlot2Trans; // 铲子堆放处
    public ShovelModel mShovelModel; // 铲子模型
    public Text Tex_Pause; // 暂停文本
    public GameObject Mask; // 黑色遮罩
    public GameObject MenuUI; // 菜单
    public GameObject Img_Hp; // 血量实例
    private Text Tex_Hp; // 血量文本
    private GameObject WinPanel;
    private Animator Ani_Win;
    private GameObject LosePanel;
    private Animator Ani_Lose;

    public bool IsInCharacterConstructMode; // 是否在角色放置模式

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // 切场景时不销毁
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mShovelSlot2Trans = mCardControllerUI.transform.Find("ShovelSlot2");
        mShovelModel = mCardControllerUI.transform.Find("Emp_ShovelModel").gameObject.GetComponent<ShovelModel>();
        Tex_Pause = transform.Find("Btn_Pause").Find("Text").GetComponent<Text>();
        Mask = transform.Find("Mask").gameObject;
        MenuUI = transform.Find("MenuUI").gameObject;
        Img_Hp = transform.Find("Img_Hp").gameObject;
        Tex_Hp = transform.Find("Img_Hp").Find("Text").GetComponent<Text>();
        //mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 铲子归位
        WinPanel = transform.Find("WinPanel").gameObject;
        Ani_Win = WinPanel.transform.Find("Img_Win").GetComponent<Animator>();
        LosePanel = transform.Find("LosePanel").gameObject;
        Ani_Lose = LosePanel.transform.Find("Img_Lose").GetComponent<Animator>();
    }

    public void InitPanel()
    {
        Debug.Log("InitPanel()");
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 铲子归位
        Tex_Pause.text = "暂停游戏";
        Mask.SetActive(false);
        MenuUI.SetActive(false);
        Img_Hp.SetActive(false);
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        IsInCharacterConstructMode = false;
    }

    /// <summary>
    /// 在GameController中的初始化方法
    /// </summary>
    public void InitInGameController()
    {
        mCardModel = GameObject.Find("Emp_CardModel").GetComponent<CardModel>();
        mCardModel.HideCardModel(); // 隐藏卡片模型
        ExitCharacterConstructMode();
        ExitCardConstructMode();
        ExitCardRemoveMode();
    }

    /// <summary>
    /// 当暂停按钮被点击时
    /// </summary>
    public void OnPauseButtonClick()
    {
        // 在人物放置阶段点暂停是没有任何吊用的
        if (IsInCharacterConstructMode)
            return;

        // 先获取目前是暂停还是非暂停状态
        if (GameController.Instance.isPause)
        {
            // 如果是暂停状态，则解除暂停
            GameController.Instance.Resume();
            // 设置文字
            Tex_Pause.text = "暂停游戏";
        }
        else
        {
            // 如果是非暂停状态，则暂停
            GameController.Instance.Pause();
            // 设置文字
            Tex_Pause.text = "解除暂停";
        }
    }

    /// <summary>
    /// 当菜单按钮被点击时
    /// </summary>
    public void OnMenuButtonClick()
    {
        SetMenuEnable(true);
    }

    /// <summary>
    /// 当返回游戏按钮被点击时
    /// </summary>
    public void OnReturnToGameClick()
    {
        SetMenuEnable(false);
    }

    /// <summary>
    /// 当重新尝试被点击时
    /// </summary>
    public void OnRetryClick()
    {
        InitPanel();
        GameController.Instance.Restart();
    }

    /// <summary>
    /// 当返回选择配置被点击时
    /// </summary>
    public void OnReturnToSelcetClick()
    {
        GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// 当返回主菜单被点击时
    /// </summary>
    public void OnReturnToMainClick()
    {
        GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterMainScene();
    }

    /// <summary>
    /// 设置菜单显示
    /// </summary>
    private void SetMenuEnable(bool enable)
    {
        Mask.SetActive(enable);
        MenuUI.SetActive(enable);
        // 在人物放置阶段不执行后续逻辑
        if (IsInCharacterConstructMode)
            return;

        if (enable)
            GameController.Instance.Pause();
        else
            GameController.Instance.Resume();
    }

    /// <summary>
    /// 清除所有卡槽
    /// </summary>
    public void ClearAllSlot()
    {
        List<BaseCardBuilder> list = new List<BaseCardBuilder>();
        for (int i = 0; i < mCardBuilderList.transform.childCount; i++)
        {
            list.Add(mCardBuilderList.transform.GetChild(i).GetComponent<BaseCardBuilder>());
        }
        for (int i = 0; i < mCardBuilderList2.transform.childCount; i++)
        {
            list.Add(mCardBuilderList2.transform.GetChild(i).GetComponent<BaseCardBuilder>());
        }
        foreach (var item in list)
        {
            item.Recycle();
        }
    }

    // 在上方UI中增加一个卡槽
    public bool AddCardSlot(BaseCardBuilder CardBuilder)
    {
        if (mCardBuilderList.transform.childCount < 13)
        {
            CardBuilder.SetIndex(mCardBuilderList.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
            return true;
        }
        else if(mCardBuilderList2.transform.childCount < 5)
        {
            CardBuilder.SetIndex(13 + mCardBuilderList2.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList2.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
            return true;
        }
        else
        {
            Debug.LogWarning("携带卡片已超过18张！！多余卡片槽不生效");
            return false;
        }
    }

    /// <summary>
    /// 进入角色放置模式
    /// </summary>
    public void EnterCharacterConstructMode()
    {
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCharacter);
        Img_Hp.SetActive(false);
        IsInCharacterConstructMode = true;
    }

    /// <summary>
    /// 离开角色放置模式
    /// </summary>
    public void ExitCharacterConstructMode()
    {
        mCardModel.HideCardModel();
        Img_Hp.SetActive(true);
        IsInCharacterConstructMode = false;
    }

    /// <summary>
    /// 进入卡片放置模式，由某个卡片建造器被成功选取时触发
    /// </summary>
    public void EnterCardConstructMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCard);
    }

    /// <summary>
    /// 离开卡片放置模式，卡片建造成功或者取消建造时触发
    /// </summary>
    public void ExitCardConstructMode()
    {
        mCardModel.HideCardModel();
    }

    /// <summary>
    /// 进入卡片移除模式
    /// </summary>
    public void EnterCardRemoveMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mShovelModel.ShowModel();
    }

    /// <summary>
    /// 离开卡片移除模式
    /// </summary>
    public void ExitCardRemoveMode()
    {
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 归位
        mShovelModel.HideModel();
    }

    /// <summary>
    /// 当铲子槽位被点击（触发）
    /// </summary>
    public void OnShovelSlotTrigger()
    {
        if (IsInCharacterConstructMode)
            return;
        GameController.Instance.mCardController.SelectShovel();
    }

    /// <summary>
    /// 设置人物HP文本与位置
    /// </summary>
    /// <param name="hp"></param>
    public void SetCharacterHpTextAndPosition(float hp, Vector3 position)
    {
        Img_Hp.transform.position = position;
        Tex_Hp.text = "HP:" + hp;
    }

    /// <summary>
    /// 进入胜利结算界面
    /// </summary>
    public void EnterWinPanel()
    {
        InitPanel();
        WinPanel.SetActive(true);
        Ani_Win.Play("win", 0, 0);
    }

    /// <summary>
    /// 进入失败结算界面
    /// </summary>
    public void EnterLosePanel()
    {
        InitPanel();
        LosePanel.SetActive(true);
        Ani_Win.Play("lose", 0, 0);
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

    private void OnDestroy()
    {
        Debug.Log("Destory GameNormalPanel");
    }
}
