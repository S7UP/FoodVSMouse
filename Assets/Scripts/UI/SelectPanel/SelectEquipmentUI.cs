
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ѡ�ؽ���-����ѡ�����
/// </summary>
public class SelectEquipmentUI : MonoBehaviour
{
    private SelectedCardUI mSelectedCardUI;
    private AvailableCardUI mAvailableCardUI;

    private void Awake()
    {
        mSelectedCardUI = transform.Find("SelectedCardUI").GetComponent<SelectedCardUI>();
        mSelectedCardUI.SetSelectEquipmentUI(this);
        mAvailableCardUI = transform.Find("AvailableCardUI").GetComponent<AvailableCardUI>();
        mAvailableCardUI.SetSelectEquipmentUI(this);
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
        mAvailableCardUI.LoadAvailableCardInfoFromStage(PlayerData.GetInstance().GetCurrentStageInfo());
        mSelectedCardUI.Initial();
    }

    /// <summary>
    /// �ӿ�ѡ������ѡ��һ�ſ�����ѡ����
    /// </summary>
    public void SelectCard(FoodNameTypeMap type)
    {
        int cardCount = 18;
        if (PlayerData.GetInstance().GetCurrentStageInfo().isEnableCardCount)
            cardCount = PlayerData.GetInstance().GetCurrentStageInfo().cardCount;
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
    /// ����ս������
    /// </summary>
    public void EnterCombatScene()
    {
        // �ѵ�ǰѡ��Ŀ���ת����playerData,�Թ���һ��������ȡ
        GameManager.Instance.playerData.SetCurrentSelectedCardInfoList(mSelectedCardUI.GetCurrentSelectedCardGroup());
        GameManager.Instance.playerData.SetCurrentCardKeyList(mSelectedCardUI.GetCurrentSelectedKeyGroup());
    }
}
