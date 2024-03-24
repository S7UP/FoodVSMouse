
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// ѡ�ؽ���-����ѡ�����
/// </summary>
public class SelectEquipmentUI : MonoBehaviour
{
    private SelectedCardUI mSelectedCardUI;
    private AvailableCardUI mAvailableCardUI;
    private Toggle Tog_NoLimit;

    private void Awake()
    {
        mSelectedCardUI = transform.Find("SelectedCardUI").GetComponent<SelectedCardUI>();
        mSelectedCardUI.SetSelectEquipmentUI(this);
        mAvailableCardUI = transform.Find("AvailableCardUI").GetComponent<AvailableCardUI>();
        mAvailableCardUI.SetSelectEquipmentUI(this);

        Tog_NoLimit = transform.Find("Tog_NoLimit").GetComponent<Toggle>();
        {
            RectTransform rect = Tog_NoLimit.GetComponent<RectTransform>();
            // ������Ĵ�����
            {
                EventTrigger trigger = Tog_NoLimit.GetComponent<EventTrigger>();
                EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
                e.AddListener(delegate {
                    TextArea.Instance.SetText("��Ҫͨ���������ѶȲſɽ�������ģʽ��ʤ�������¼�κγɼ���");
                    TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.rect.width, 0), new Vector2(1, 0));
                });
                trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = e });
            }
            // ����˳��Ĵ�����
            {
                EventTrigger trigger = Tog_NoLimit.GetComponent<EventTrigger>();
                EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
                e.AddListener(delegate {
                    TextArea.ExecuteRecycle();
                });
                trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit, callback = e });
            }
        }
        Tog_NoLimit.isOn = false;
        Tog_NoLimit.interactable = false;
        Tog_NoLimit.onValueChanged.AddListener(delegate {
            PlayerData data = PlayerData.GetInstance();
            BaseStage.StageInfo info = data.GetCurrentStageInfo();
            PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
            string id = info_dynamic.id;
            info_dynamic.isNoLimit = Tog_NoLimit.isOn;
            // ����Ǵӹ���������Ĺؿ�������Ը���һ��ѡ�����
            if (id != null)
            {
                StageInfoManager.StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(id);
                local_info.isNoLimit = Tog_NoLimit.isOn;
                StageInfoManager.Save();
            }
            RefreshCardUI(); // ˢ��һ��ѡ��UI
        });
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
        PlayerData data = PlayerData.GetInstance();
        PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
        string id = info_dynamic.id;
        if (id != null)
        {
            // ����Ǵӹ���������Ĺؿ������һ��ͨ�������ֻ��������ͨ������ѡ�����
            StageInfoManager.StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(id);
            if (local_info.rank >= 3)
            {
                Tog_NoLimit.interactable = true;
                Tog_NoLimit.isOn = local_info.isNoLimit;
            }
            else
            {
                Tog_NoLimit.interactable = false;
                Tog_NoLimit.isOn = false;
            }
        }
        else
        {
            Tog_NoLimit.interactable = true;
            Tog_NoLimit.isOn = false;
        }

        RefreshCardUI();
    }

    /// <summary>
    /// ˢ����ѡ�����ѡ����UI
    /// </summary>
    private void RefreshCardUI()
    {
        PlayerData data = PlayerData.GetInstance();
        BaseStage.StageInfo info = data.GetCurrentStageInfo();
        mAvailableCardUI.Initial();
        mAvailableCardUI.LoadAvailableCardInfoFromStage(info, Tog_NoLimit.isOn);
        mSelectedCardUI.Initial();
    }

    /// <summary>
    /// �ӿ�ѡ������ѡ��һ�ſ�����ѡ����
    /// </summary>
    public void SelectCard(FoodNameTypeMap type)
    {
        int cardCount = 18;
        if (PlayerData.GetInstance().GetCurrentStageInfo().isEnableCardCount && !PlayerData.GetInstance().GetCurrentDynamicStageInfo().isNoLimit)
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
        PlayerData.StageInfo_Dynamic info_dynamic = PlayerData.GetInstance().GetCurrentDynamicStageInfo();
        info_dynamic.isNoLimit = Tog_NoLimit.isOn;
        info_dynamic.selectedCardInfoList = mSelectedCardUI.GetCurrentSelectedCardGroup();
        info_dynamic.cardKeyList = mSelectedCardUI.GetCurrentSelectedKeyGroup();
    }
}
