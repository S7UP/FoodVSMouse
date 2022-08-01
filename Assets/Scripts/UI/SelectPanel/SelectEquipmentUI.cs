using DG.Tweening;

using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ѡ�ؽ���-����ѡ�����
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
        ShowTween.SetEase(Ease.InCubic); // ��������
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
    /// ��ʼ��
    /// </summary>
    public void Initial()
    {
        //mSelectedCardUI.Initial();
        //mAvailableCardUI.Initial();
    }

    /// <summary>
    /// ���ص�ǰѡ�йص���Ϣ���Ҳ�ȫUI
    /// </summary>
    public void LoadAndFixUI()
    {
        mAvailableCardUI.Initial();
        mAvailableCardUI.LoadAvailableCardInfoFromStage(mSelectPanel.GetCurrentSelectedStageInfo());
        // SelectedCardUI.Initial()Ҫ��AvailableCardUI.Initial()֮��
        mSelectedCardUI.Initial();
        mSelectWeaponsUI.Initial();
    }

    /// <summary>
    /// �ӿ�ѡ������ѡ��һ�ſ�����ѡ����
    /// </summary>
    public void SelectCard(FoodNameTypeMap type)
    {
        int cardCount = 18;
        if (GetCurrentSelectedStageInfo().isEnableCardCount)
            cardCount = GetCurrentSelectedStageInfo().cardCount;
        if (mSelectedCardUI.GetSelectedCardCount() < cardCount)
        {
            // �ڿ���û�г�����ʱѡ��
            AvailableCardInfo info = mAvailableCardUI.GetCardModelInfo(type);
            mSelectedCardUI.AddCard(info.type, info.maxShape, info.maxLevel);
        }
    }

    /// <summary>
    /// ����ѡ����ȡ��ѡ��һ�ſ����Ż�����ѡ����
    /// </summary>
    /// <param name="type"></param>
    public void CancelSelectCard(FoodNameTypeMap type)
    {
        mSelectedCardUI.RemoveCard((int)type);
    }

    /// <summary>
    /// ��ȡ��ǰ��ѡ�ÿ�Ƭ���ֵ�
    /// </summary>
    /// <returns></returns>
    public Dictionary<FoodNameTypeMap, AvailableCardInfo> GetCurrentAvailableCardDict()
    {
        return mAvailableCardUI.GetCurrentAvailableCardDict();
    }

    /// <summary>
    /// ��ȡ��ѡ��ƬUI��Ӧ��Ƭ�ľ���λ��
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Vector3 GetAvailableCardModelPosition(FoodNameTypeMap type)
    {
        return mAvailableCardUI.GetCardModelPosition(type);
    }

    /// <summary>
    /// ���ÿ�ѡ��ƬUI��ѡ��״̬
    /// </summary>
    /// <param name="type"></param>
    /// <param name="selected"></param>
    public void SetAvailableCardModelSelect(FoodNameTypeMap type, bool selected)
    {
        mAvailableCardUI.SetCardModelSelect(type, selected);
    }

    /// <summary>
    /// ��ʾ��ǰUI
    /// </summary>
    public void Show()
    {
        ShowTween.PlayForward();
    }

    /// <summary>
    /// ���ص�ǰUI
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
    /// ����ս������
    /// </summary>
    public void EnterCombatScene()
    {
        // �ѵ�ǰѡ��Ŀ���ת����playerData,�Թ���һ��������ȡ
        GameManager.Instance.playerData.SetCurrentSelectedCardInfoList(mSelectedCardUI.GetCurrentSelectedCardGroup());
        GameManager.Instance.playerData.SetCurrentCardKeyList(mSelectedCardUI.GetCurrentSelectedKeyGroup());
        BaseStage.StageInfo info = mSelectPanel.GetCurrentSelectedStageInfo();
        GameManager.Instance.playerData.SetCurrentChapterStageValue(info.chapterIndex, info.sceneIndex, info.stageIndex);
        GameManager.Instance.playerData.SetWeaponsInfo(mSelectWeaponsUI.GetSelctedWeaponsInfo());
        GameManager.Instance.playerData.SetCharacterInfo(mSelectWeaponsUI.GetSelctedCharacterInfo());
        mSelectPanel.EnterComBatScene();
    }
}
