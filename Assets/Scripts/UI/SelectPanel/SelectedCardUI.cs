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
    private ScrollRect Scr_SelectedCardList;
    private Transform CardGroupListTrans;
    private Button Btn_Save;
    private Dropdown Dro_Rank;
    

    private List<List<AvailableCardInfo>> cardGroupList;
    private List<List<char>> keyGroupList;
    private List<AvailableCardInfo> currentSelectedCardGroup;
    private List<char> currentSelectedKeyGroup;

    private void Awake()
    {
        Tex_CardCount = transform.Find("Img_center").Find("Tex_CardCount").GetComponent<Text>();
        // SelectedCardListTrans = transform.Find("Img_center").Find("Emp_Container").Find("Scr").Find("Viewport").Find("Content").GetComponent<RectTransform>();
        Scr_SelectedCardList = transform.Find("Img_center").Find("Emp_Container").Find("Scr").GetComponent<ScrollRect>();
        CardGroupListTrans = transform.Find("Img_center").Find("Emp_CardGroupList");
        Btn_Save = transform.Find("Img_center").Find("Btn_Save").GetComponent<Button>();
        Btn_Save.onClick.AddListener(delegate { SaveCurrentStageCardGroupList(); });
        Dro_Rank = transform.Find("Img_center").Find("Emp_ChangeRank").Find("Dro_Rank").GetComponent<Dropdown>();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initial()
    {
        // 获取卡组表
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
        cardGroupList = GameManager.Instance.playerData.LoadCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        keyGroupList = GameManager.Instance.playerData.LoadKeyGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        if (cardGroupList.Count < 4) // 默认4个卡片组
            for (int i = cardGroupList.Count; i < 4; i++)
            {
                cardGroupList.Add(new List<AvailableCardInfo>());
            }
        if(keyGroupList.Count<4)
            for (int i = keyGroupList.Count; i < 4; i++)
            {
                keyGroupList.Add(new List<char>());
            }
        foreach (var keyGroup in keyGroupList)
        {
            // 自动补全至18槽的key，默认值为'\0'
            if(keyGroup.Count < 18)
            {
                for (int i = keyGroup.Count; i < 18; i++)
                {
                    keyGroup.Add('\0');
                }
            }
        }
        // 更新卡组按钮表
        UpdateCardGroupButtonList();
        SetCurrentSelectedCardGroup(0); // 默认第一个为当前选用卡组

        // 设置快捷选择星级
        {
            Dro_Rank.onValueChanged.RemoveAllListeners();
            Dro_Rank.ClearOptions();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
            for (int i = 0; i <= 16; i++)
            {
                dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("UI/Rank2/" + i)));
            }
            Dro_Rank.AddOptions(dataList);
            Dro_Rank.value = 5;
            Dro_Rank.onValueChanged.AddListener(delegate
            {
                foreach (var model in cardModelList)
                {
                    model.SetLevel(Dro_Rank.value);
                } });
            }
    }

    /// <summary>
    /// 设置当前卡片组
    /// </summary>
    private void SetCurrentSelectedCardGroup(int arrayIndex)
    {
        currentSelectedCardGroup = cardGroupList[arrayIndex];
        currentSelectedKeyGroup = keyGroupList[arrayIndex];
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
        UpdateDisplayKey();
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
        // 根据可选卡列表强制更新已选卡列表信息，使得已选卡限制在可选卡的范围内
        UpdateCurrentSelectedCardGroupByAvailableCardUI();
        // 强制更新卡片数量最大值为卡片数量上限
        UpdateCurrentSelectedCardGroupByCountLimit();
        // 根据卡的数量自适应宽度
        Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
        Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
        // 填充卡片模型
        foreach (var item in currentSelectedCardGroup)
        {
            SelectedCardModel model = SelectedCardModel.CreateInstance();
            model.SetSelectedCardUI(this);
            model.SetAvailableCardInfo(item);
            model.transform.SetParent(Scr_SelectedCardList.content);
            model.transform.localScale = Vector3.one;
            cardModelList.Add(model);
            // 播放飞入动画
            model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)item.type));
            // 在可选卡片UI中标记为被选中
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.type, true);
            // 添加取消监听
            model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        }
        UpdateDisplayKey();
    }

    /// <summary>
    /// 根据可选卡列表强制更新已选卡列表信息，使得已选卡限制在可选卡的范围内
    /// </summary>
    private void UpdateCurrentSelectedCardGroupByAvailableCardUI()
    {
        Dictionary<FoodNameTypeMap, AvailableCardInfo> dict = GetCurrentAvailableCardDict();
        List<AvailableCardInfo> removeList = new List<AvailableCardInfo>();
        foreach (var item in currentSelectedCardGroup)
        {
            // 如果可选卡里没有此类型卡，就直接进入移除名单内
            if (!dict.ContainsKey((FoodNameTypeMap)item.type))
            {
                removeList.Add(item);
                continue;
            }
            // 转职情况 和 星级情况 取可选卡与当前选用卡的最小值
            item.maxShape = Mathf.Min(item.maxShape, dict[(FoodNameTypeMap)item.type].maxShape);
            item.maxLevel = Mathf.Min(item.maxLevel, dict[(FoodNameTypeMap)item.type].maxLevel);
        }
        // 移除
        foreach (var item in removeList)
        {
            currentSelectedCardGroup.Remove(item);
        }
    }

    /// <summary>
    /// 强制更新卡片数量最大值为卡片数量上限
    /// </summary>
    private void UpdateCurrentSelectedCardGroupByCountLimit()
    {
        if (PlayerData.GetInstance().GetCurrentStageInfo().isEnableCardCount)
        {
            int count = currentSelectedCardGroup.Count;
            for (int i = PlayerData.GetInstance().GetCurrentStageInfo().cardCount; i < count; i++)
            {
                currentSelectedCardGroup.RemoveAt(currentSelectedCardGroup.Count - 1);
            }
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
        model.SetSelectedCardUI(this);
        model.SetAvailableCardInfo(info);
        model.transform.SetParent(Scr_SelectedCardList.content);
        model.transform.localScale = Vector3.one;
        cardModelList.Add(model);
        currentSelectedCardGroup.Add(info);
        // 根据卡片数量自适应宽度
        Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
        Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
        // 播放飞入动画
        model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type));
        // 在可选卡片UI中标记为被选中
        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, true);
        // 更新文本显示
        UpdateCardCountText();
        // 添加取消监听
        model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        UpdateDisplayKey();
        return model;
    }

    /// <summary>
    /// 从卡槽中移除卡片并放回卡池
    /// </summary>
    /// <returns></returns>
    public void RemoveCard(int type)
    {
        AvailableCardInfo info = null;
        int index = 0;
        foreach (var item in currentSelectedCardGroup)
        {
            if (item.type == type)
            {
                info = item;
                break;
            }
            index++;
        }
        if (info != null)
        {
            // 从当前卡组中移除对应卡片
            currentSelectedCardGroup.Remove(info);
            // 从键位表移除对应键位
            currentSelectedKeyGroup.Remove(currentSelectedKeyGroup[index]);
            // 根据卡片数量自适应宽度
            Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
            Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
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
                UpdateDisplayKey();
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
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
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
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
        GameManager.Instance.playerData.SaveCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, cardGroupList);
        GameManager.Instance.playerData.SaveKeyGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, keyGroupList);
    }

    /// <summary>
    /// 获取当前选择的卡片组
    /// </summary>
    public List<AvailableCardInfo> GetCurrentSelectedCardGroup()
    {
        return currentSelectedCardGroup;
    }

    /// <summary>
    /// 获取当前的键位控制表
    /// </summary>
    /// <returns></returns>
    public List<char> GetCurrentSelectedKeyGroup()
    {
        return currentSelectedKeyGroup;
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

    /// <summary>
    /// 获取某个模型在数组的下标
    /// </summary>
    /// <returns></returns>
    public int GetModelInListIndex(SelectedCardModel model)
    {
        int i = 0;
        foreach (var item in cardModelList)
        {
            if (item == model)
                return i;
            i++;
        }
        return -1;
    }

    /// <summary>
    /// 根据下标获取当前键位
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public char GetKeyByIndex(int index)
    {
        if(index >= currentSelectedKeyGroup.Count)
            for (int i = currentSelectedKeyGroup.Count; i <= index; i++)
            {
                currentSelectedKeyGroup.Add(default);
            }
        return currentSelectedKeyGroup[index];
    }

    /// <summary>
    /// 根据下标更新键位表
    /// </summary>
    /// <param name="index"></param>
    /// <param name="key"></param>
    public void UpdateKeyByIndex(int index, char key)
    {
        if (!key.Equals('\0'))
        {
            for (int i = 0; i < currentSelectedKeyGroup.Count; i++)
            {
                if (currentSelectedKeyGroup[i].Equals(key))
                {
                    currentSelectedKeyGroup[i] = '\0';
                    cardModelList[i].UpdateKey();
                    break;
                }
            }
        }
        currentSelectedKeyGroup[index] = key;
    }

    /// <summary>
    /// 更新卡片显示的键位
    /// </summary>
    public void UpdateDisplayKey()
    {
        foreach (var item in cardModelList)
        {
            item.UpdateKey();
        }
    }

    /// <summary>
    /// 获取可以选择的卡片表
    /// </summary>
    /// <returns></returns>
    public Dictionary<FoodNameTypeMap, AvailableCardInfo> GetCurrentAvailableCardDict()
    {
        return mSelectEquipmentUI.GetCurrentAvailableCardDict();
    }
}
