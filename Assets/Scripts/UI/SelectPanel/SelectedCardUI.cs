using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// 配置选择-已选择卡组与卡片
/// </summary>
public class SelectedCardUI : MonoBehaviour
{
    private SelectEquipmentUI mSelectEquipmentUI;
    private List<SelectedCardModel> cardModelList = new List<SelectedCardModel>();
    private List<Button> Btn_CardGroup = new List<Button>();
    private Text Tex_CardCount;
    private Transform SelectedCardListTrans;
    private Transform CardGroupListTrans;
    private Button Btn_Save;

    private List<List<AvailableCardInfo>> cardGroupList;
    private List<AvailableCardInfo> currentSelectedCardGroup;

    private void Awake()
    {
        Tex_CardCount = transform.Find("Img_center").Find("Tex_CardCount").GetComponent<Text>();
        SelectedCardListTrans = transform.Find("Img_center").Find("Emp_Container").Find("SelectedCardList");
        CardGroupListTrans = transform.Find("Img_center").Find("Emp_CardGroupList");
        Btn_Save = transform.Find("Img_center").Find("Btn_Save").GetComponent<Button>();
        Btn_Save.onClick.AddListener(delegate { SaveCurrentStageCardGroupList(); });
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initial()
    {
        // 获取卡组表
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
        cardGroupList = GameManager.Instance.playerData.LoadCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        if (cardGroupList.Count < 8) // 默认8个卡片组
            for (int i = cardGroupList.Count; i < 8; i++)
            {
                cardGroupList.Add(new List<AvailableCardInfo>());
            }
        // 更新卡组按钮表
        UpdateCardGroupButtonList();
        SetCurrentSelectedCardGroup(0); // 默认第一个为当前选用卡组

    }

    /// <summary>
    /// 设置当前卡片组
    /// </summary>
    private void SetCurrentSelectedCardGroup(int arrayIndex)
    {
        currentSelectedCardGroup = cardGroupList[arrayIndex];
        // 填充卡片模型
        UpdateCardModelList();
        // 被选中的卡片组按钮高亮，其他置暗色
        foreach (var item in Btn_CardGroup)
        {
            item.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/SelectPanel/36");
        }
        Btn_CardGroup[arrayIndex].GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/SelectPanel/33");
        // 更新文本显示
        UpdateCardCountText();
    }

    /// <summary>
    /// 更新卡组按钮表
    /// </summary>
    private void UpdateCardGroupButtonList()
    {
        foreach (var item in Btn_CardGroup)
        {
            item.onClick.RemoveAllListeners();
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_CardGroup", item.gameObject);
        }
        Btn_CardGroup.Clear();
        int i = 0;
        foreach (var item in cardGroupList)
        {
            int j = i;
            Button btn = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/Btn_CardGroup").GetComponent<Button>();
            btn.onClick.AddListener(delegate { SetCurrentSelectedCardGroup(j); });
            btn.transform.Find("Text").GetComponent<Text>().text = "卡片组"+(i+1);
            btn.transform.SetParent(CardGroupListTrans);
            btn.transform.localScale = Vector3.one;
            Btn_CardGroup.Add(btn);
            i++;
        }
    }

    /// <summary>
    /// 更新卡片模型
    /// </summary>
    private void UpdateCardModelList()
    {
        // 清除原本的卡片模型
        foreach (var item in cardModelList)
        {
            item.ExecuteRecycle();
            // 在可选卡片UI中标记为未选中
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.GetInfo().type, false);
        }
        cardModelList.Clear();
        // 填充卡片模型
        foreach (var item in currentSelectedCardGroup)
        {
            SelectedCardModel model = SelectedCardModel.CreateInstance();
            model.SetAvailableCardInfo(item);
            model.transform.SetParent(SelectedCardListTrans);
            model.transform.localScale = Vector3.one;
            cardModelList.Add(model);
            // 播放飞入动画
            model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)item.type));
            // 在可选卡片UI中标记为被选中
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.type, true);
            // 添加取消监听
            model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        }
    }

    /// <summary>
    /// 添加一张卡进卡槽
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <param name="level"></param>
    public SelectedCardModel AddCard(int type, int shape, int level)
    {
        AvailableCardInfo info = new AvailableCardInfo(type, shape, level);
        SelectedCardModel model = SelectedCardModel.CreateInstance();
        model.SetAvailableCardInfo(info);
        model.transform.SetParent(SelectedCardListTrans);
        model.transform.localScale = Vector3.one;
        cardModelList.Add(model);
        currentSelectedCardGroup.Add(info);
        // 播放飞入动画
        model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type));
        // 在可选卡片UI中标记为被选中
        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, true);
        // 更新文本显示
        UpdateCardCountText();
        // 添加取消监听
        model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        return model;
    }

    /// <summary>
    /// 从卡槽中移除卡片并放回卡池
    /// </summary>
    /// <returns></returns>
    public void RemoveCard(int type)
    {
        AvailableCardInfo info = null;
        foreach (var item in currentSelectedCardGroup)
        {
            if (item.type == type)
            {
                info = item;
                break;
            }
        }
        if (info != null)
        {
            // 从当前卡组中移除对应卡片
            currentSelectedCardGroup.Remove(info);
            // 移除对应模型并播放动画
            SelectedCardModel model = null;
            foreach (var item in cardModelList)
            {
                if (item.GetInfo().type == type)
                {
                    model = item;
                    break;
                }
            }
            if (model != null)
            {
                cardModelList.Remove(model);
                // 播放移除动画（在动画结束时会回收对象并标记为未选中）
                model.PlayUnSelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type), 
                    delegate {
                        // 回收对象
                        if(model.isActiveAndEnabled)
                            model.ExecuteRecycle();
                        // 在可选UI中标记为未选中
                        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, false);
                    });
                // 更新文本显示
                UpdateCardCountText();
            }
            else
            {
                Debug.Log("未在已选卡池中找到type=" + type + "的卡片模型！");
            }
        }
        else
        {
            Debug.Log("未在已选卡池中找到type="+type+"的卡！");
        }
    }

    /// <summary>
    /// 更新携带卡片数文本显示
    /// </summary>
    private void UpdateCardCountText()
    {
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
        int cardCount = 18;
        if (info.isEnableCardCount)
            cardCount = info.cardCount;
        Tex_CardCount.text = "携带卡片数量："+ GetSelectedCardCount() + "/"+ cardCount;
    }

    /// <summary>
    /// 保存当前卡组至存档
    /// </summary>
    private void SaveCurrentStageCardGroupList()
    {
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
        GameManager.Instance.playerData.SaveCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, cardGroupList);
    }

    /// <summary>
    /// 获取当前选择的卡片组
    /// </summary>
    public List<AvailableCardInfo> GetCurrentSelectedCardGroup()
    {
        return currentSelectedCardGroup;
    }


    public void SetSelectEquipmentUI(SelectEquipmentUI ui)
    {
        mSelectEquipmentUI = ui;
    }

    /// <summary>
    /// 获取当前选择的卡片数
    /// </summary>
    /// <returns></returns>
    public int GetSelectedCardCount()
    {
        return currentSelectedCardGroup.Count;
    }
}
