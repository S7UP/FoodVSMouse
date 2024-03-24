
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 选关界面-配置选择界面
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
            // 鼠标进入的触发器
            {
                EventTrigger trigger = Tog_NoLimit.GetComponent<EventTrigger>();
                EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
                e.AddListener(delegate {
                    TextArea.Instance.SetText("需要通过遗忘级难度才可解锁，此模式下胜利不会记录任何成绩。");
                    TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.rect.width, 0), new Vector2(1, 0));
                });
                trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = e });
            }
            // 鼠标退出的触发器
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
            // 如果是从公测面版进入的关卡，则可以更新一下选择情况
            if (id != null)
            {
                StageInfoManager.StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(id);
                local_info.isNoLimit = Tog_NoLimit.isOn;
                StageInfoManager.Save();
            }
            RefreshCardUI(); // 刷新一次选卡UI
        });
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
        PlayerData data = PlayerData.GetInstance();
        PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
        string id = info_dynamic.id;
        if (id != null)
        {
            // 如果是从公测面版进入的关卡，检测一下通过情况，只有遗忘级通过才能选择解限
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
    /// 刷新已选卡与可选卡的UI
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
    /// 从可选卡池中选择一张卡到已选卡上
    /// </summary>
    public void SelectCard(FoodNameTypeMap type)
    {
        int cardCount = 18;
        if (PlayerData.GetInstance().GetCurrentStageInfo().isEnableCardCount && !PlayerData.GetInstance().GetCurrentDynamicStageInfo().isNoLimit)
            cardCount = PlayerData.GetInstance().GetCurrentStageInfo().cardCount;
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
    /// 进入战斗场景
    /// </summary>
    public void EnterCombatScene()
    {
        // 把当前选择的卡组转存至playerData,以供下一个场景读取
        PlayerData.StageInfo_Dynamic info_dynamic = PlayerData.GetInstance().GetCurrentDynamicStageInfo();
        info_dynamic.isNoLimit = Tog_NoLimit.isOn;
        info_dynamic.selectedCardInfoList = mSelectedCardUI.GetCurrentSelectedCardGroup();
        info_dynamic.cardKeyList = mSelectedCardUI.GetCurrentSelectedKeyGroup();
    }
}
