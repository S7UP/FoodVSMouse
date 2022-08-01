using DG.Tweening;

using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 选关界面-配置选择界面
/// </summary>
public class SelectEquipmentUI : MonoBehaviour
{
    private SelectPanel mSelectPanel;
    private RectTransform rectTransform;
    private Tweener ShowTween;

    private SelectedCardUI mSelectedCardUI;
    private AvailableCardUI mAvailableCardUI;
    private SelectWeaponsUI mSelectWeaponsUI;

    private void Awake()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        ShowTween = rectTransform.DOAnchorPosY(-600f, 0.5f);
        ShowTween.SetEase(Ease.InCubic); // 由慢到快
        ShowTween.Pause();
        ShowTween.SetAutoKill(false);

        mSelectedCardUI = transform.Find("SelectedCardUI").GetComponent<SelectedCardUI>();
        mSelectedCardUI.SetSelectEquipmentUI(this);
        mAvailableCardUI = transform.Find("AvailableCardUI").GetComponent<AvailableCardUI>();
        mAvailableCardUI.SetSelectEquipmentUI(this);
        mSelectWeaponsUI = transform.Find("SelectWeaponsUI").GetComponent<SelectWeaponsUI>();
        mSelectWeaponsUI.SetSelectEquipmentUI(this);

    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initial()
    {
        //mSelectedCardUI.Initial();
        //mAvailableCardUI.Initial();
    }

    /// <summary>
    /// 加载当前选中关的信息并且补全UI
    /// </summary>
    public void LoadAndFixUI()
    {
        mAvailableCardUI.Initial();
        mAvailableCardUI.LoadAvailableCardInfoFromStage(mSelectPanel.GetCurrentSelectedStageInfo());
        // SelectedCardUI.Initial()要于AvailableCardUI.Initial()之后
        mSelectedCardUI.Initial();
        mSelectWeaponsUI.Initial();
    }

    /// <summary>
    /// 从可选卡池中选择一张卡到已选卡上
    /// </summary>
    public void SelectCard(FoodNameTypeMap type)
    {
        int cardCount = 18;
        if (GetCurrentSelectedStageInfo().isEnableCardCount)
            cardCount = GetCurrentSelectedStageInfo().cardCount;
        if (mSelectedCardUI.GetSelectedCardCount() < cardCount)
        {
            // 在卡槽没有超上限时选卡
            AvailableCardInfo info = mAvailableCardUI.GetCardModelInfo(type);
            mSelectedCardUI.AddCard(info.type, info.maxShape, info.maxLevel);
        }
    }

    /// <summary>
    /// 从已选卡中取消选择一张卡并放回至可选卡池
    /// </summary>
    /// <param name="type"></param>
    public void CancelSelectCard(FoodNameTypeMap type)
    {
        mSelectedCardUI.RemoveCard((int)type);
    }

    /// <summary>
    /// 获取当前可选用卡片的字典
    /// </summary>
    /// <returns></returns>
    public Dictionary<FoodNameTypeMap, AvailableCardInfo> GetCurrentAvailableCardDict()
    {
        return mAvailableCardUI.GetCurrentAvailableCardDict();
    }

    /// <summary>
    /// 获取可选卡片UI对应卡片的绝对位置
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Vector3 GetAvailableCardModelPosition(FoodNameTypeMap type)
    {
        return mAvailableCardUI.GetCardModelPosition(type);
    }

    /// <summary>
    /// 设置可选卡片UI的选中状态
    /// </summary>
    /// <param name="type"></param>
    /// <param name="selected"></param>
    public void SetAvailableCardModelSelect(FoodNameTypeMap type, bool selected)
    {
        mAvailableCardUI.SetCardModelSelect(type, selected);
    }

    /// <summary>
    /// 显示当前UI
    /// </summary>
    public void Show()
    {
        ShowTween.PlayForward();
    }

    /// <summary>
    /// 隐藏当前UI
    /// </summary>
    public void Hide()
    {
        ShowTween.PlayBackwards();
    }

    public void SetSelectPanel(SelectPanel panel)
    {
        mSelectPanel = panel;
    }

    public BaseStage.StageInfo GetCurrentSelectedStageInfo()
    {
        return mSelectPanel.GetCurrentSelectedStageInfo();
    }

    /// <summary>
    /// 进入战斗场景
    /// </summary>
    public void EnterCombatScene()
    {
        // 把当前选择的卡组转存至playerData,以供下一个场景读取
        GameManager.Instance.playerData.SetCurrentSelectedCardInfoList(mSelectedCardUI.GetCurrentSelectedCardGroup());
        GameManager.Instance.playerData.SetCurrentCardKeyList(mSelectedCardUI.GetCurrentSelectedKeyGroup());
        BaseStage.StageInfo info = mSelectPanel.GetCurrentSelectedStageInfo();
        GameManager.Instance.playerData.SetCurrentChapterStageValue(info.chapterIndex, info.sceneIndex, info.stageIndex);
        GameManager.Instance.playerData.SetWeaponsInfo(mSelectWeaponsUI.GetSelctedWeaponsInfo());
        GameManager.Instance.playerData.SetCharacterInfo(mSelectWeaponsUI.GetSelctedCharacterInfo());
        mSelectPanel.EnterComBatScene();
    }
}
