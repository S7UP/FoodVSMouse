using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// ����ѡ��-��ѡ�����뿨Ƭ
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
    /// ��ʼ��
    /// </summary>
    public void Initial()
    {
        // ��ȡ�����
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
        cardGroupList = GameManager.Instance.playerData.LoadCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        keyGroupList = GameManager.Instance.playerData.LoadKeyGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        if (cardGroupList.Count < 4) // Ĭ��4����Ƭ��
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
            // �Զ���ȫ��18�۵�key��Ĭ��ֵΪ'\0'
            if(keyGroup.Count < 18)
            {
                for (int i = keyGroup.Count; i < 18; i++)
                {
                    keyGroup.Add('\0');
                }
            }
        }
        // ���¿��鰴ť��
        UpdateCardGroupButtonList();
        SetCurrentSelectedCardGroup(0); // Ĭ�ϵ�һ��Ϊ��ǰѡ�ÿ���

        // ���ÿ��ѡ���Ǽ�
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
    /// ���õ�ǰ��Ƭ��
    /// </summary>
    private void SetCurrentSelectedCardGroup(int arrayIndex)
    {
        currentSelectedCardGroup = cardGroupList[arrayIndex];
        currentSelectedKeyGroup = keyGroupList[arrayIndex];
        // ��俨Ƭģ��
        UpdateCardModelList();
        // ��ѡ�еĿ�Ƭ�鰴ť�����������ð�ɫ
        foreach (var item in Btn_CardGroup)
        {
            item.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/SelectPanel/36");
        }
        Btn_CardGroup[arrayIndex].GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/SelectPanel/33");
        // �����ı���ʾ
        UpdateCardCountText();
        UpdateDisplayKey();
    }

    /// <summary>
    /// ���¿��鰴ť��
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
            btn.transform.Find("Text").GetComponent<Text>().text = "��Ƭ��"+(i+1);
            btn.transform.SetParent(CardGroupListTrans);
            btn.transform.localScale = Vector3.one;
            Btn_CardGroup.Add(btn);
            i++;
        }
    }

    /// <summary>
    /// ���¿�Ƭģ��
    /// </summary>
    private void UpdateCardModelList()
    {
        // ���ԭ���Ŀ�Ƭģ��
        foreach (var item in cardModelList)
        {
            item.ExecuteRecycle();
            // �ڿ�ѡ��ƬUI�б��Ϊδѡ��
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.GetInfo().type, false);
        }
        cardModelList.Clear();
        // ���ݿ�ѡ���б�ǿ�Ƹ�����ѡ���б���Ϣ��ʹ����ѡ�������ڿ�ѡ���ķ�Χ��
        UpdateCurrentSelectedCardGroupByAvailableCardUI();
        // ǿ�Ƹ��¿�Ƭ�������ֵΪ��Ƭ��������
        UpdateCurrentSelectedCardGroupByCountLimit();
        // ���ݿ�����������Ӧ���
        Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
        Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
        // ��俨Ƭģ��
        foreach (var item in currentSelectedCardGroup)
        {
            SelectedCardModel model = SelectedCardModel.CreateInstance();
            model.SetSelectedCardUI(this);
            model.SetAvailableCardInfo(item);
            model.transform.SetParent(Scr_SelectedCardList.content);
            model.transform.localScale = Vector3.one;
            cardModelList.Add(model);
            // ���ŷ��붯��
            model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)item.type));
            // �ڿ�ѡ��ƬUI�б��Ϊ��ѡ��
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.type, true);
            // ���ȡ������
            model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        }
        UpdateDisplayKey();
    }

    /// <summary>
    /// ���ݿ�ѡ���б�ǿ�Ƹ�����ѡ���б���Ϣ��ʹ����ѡ�������ڿ�ѡ���ķ�Χ��
    /// </summary>
    private void UpdateCurrentSelectedCardGroupByAvailableCardUI()
    {
        Dictionary<FoodNameTypeMap, AvailableCardInfo> dict = GetCurrentAvailableCardDict();
        List<AvailableCardInfo> removeList = new List<AvailableCardInfo>();
        foreach (var item in currentSelectedCardGroup)
        {
            // �����ѡ����û�д����Ϳ�����ֱ�ӽ����Ƴ�������
            if (!dict.ContainsKey((FoodNameTypeMap)item.type))
            {
                removeList.Add(item);
                continue;
            }
            // תְ��� �� �Ǽ���� ȡ��ѡ���뵱ǰѡ�ÿ�����Сֵ
            item.maxShape = Mathf.Min(item.maxShape, dict[(FoodNameTypeMap)item.type].maxShape);
            item.maxLevel = Mathf.Min(item.maxLevel, dict[(FoodNameTypeMap)item.type].maxLevel);
        }
        // �Ƴ�
        foreach (var item in removeList)
        {
            currentSelectedCardGroup.Remove(item);
        }
    }

    /// <summary>
    /// ǿ�Ƹ��¿�Ƭ�������ֵΪ��Ƭ��������
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
    /// ���һ�ſ�������
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
        // ���ݿ�Ƭ��������Ӧ���
        Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
        Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
        // ���ŷ��붯��
        model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type));
        // �ڿ�ѡ��ƬUI�б��Ϊ��ѡ��
        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, true);
        // �����ı���ʾ
        UpdateCardCountText();
        // ���ȡ������
        model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        UpdateDisplayKey();
        return model;
    }

    /// <summary>
    /// �ӿ������Ƴ���Ƭ���Żؿ���
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
            // �ӵ�ǰ�������Ƴ���Ӧ��Ƭ
            currentSelectedCardGroup.Remove(info);
            // �Ӽ�λ���Ƴ���Ӧ��λ
            currentSelectedKeyGroup.Remove(currentSelectedKeyGroup[index]);
            // ���ݿ�Ƭ��������Ӧ���
            Scr_SelectedCardList.content.sizeDelta = new Vector2(currentSelectedCardGroup.Count * 50.68f, Scr_SelectedCardList.content.sizeDelta.y);
            Scr_SelectedCardList.horizontalNormalizedPosition = 1.0f;
            // �Ƴ���Ӧģ�Ͳ����Ŷ���
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
                // �����Ƴ��������ڶ�������ʱ����ն��󲢱��Ϊδѡ�У�
                model.PlayUnSelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type), 
                    delegate {
                        // ���ն���
                        if(model.isActiveAndEnabled)
                            model.ExecuteRecycle();
                        // �ڿ�ѡUI�б��Ϊδѡ��
                        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, false);
                    });
                // �����ı���ʾ
                UpdateCardCountText();
                UpdateDisplayKey();
            }
            else
            {
                Debug.Log("δ����ѡ�������ҵ�type=" + type + "�Ŀ�Ƭģ�ͣ�");
            }
        }
        else
        {
            Debug.Log("δ����ѡ�������ҵ�type="+type+"�Ŀ���");
        }
    }

    /// <summary>
    /// ����Я����Ƭ���ı���ʾ
    /// </summary>
    private void UpdateCardCountText()
    {
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
        int cardCount = 18;
        if (info.isEnableCardCount)
            cardCount = info.cardCount;
        Tex_CardCount.text = "Я����Ƭ������"+ GetSelectedCardCount() + "/"+ cardCount;
    }

    /// <summary>
    /// ���浱ǰ�������浵
    /// </summary>
    private void SaveCurrentStageCardGroupList()
    {
        BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
        GameManager.Instance.playerData.SaveCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, cardGroupList);
        GameManager.Instance.playerData.SaveKeyGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, keyGroupList);
    }

    /// <summary>
    /// ��ȡ��ǰѡ��Ŀ�Ƭ��
    /// </summary>
    public List<AvailableCardInfo> GetCurrentSelectedCardGroup()
    {
        return currentSelectedCardGroup;
    }

    /// <summary>
    /// ��ȡ��ǰ�ļ�λ���Ʊ�
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
    /// ��ȡ��ǰѡ��Ŀ�Ƭ��
    /// </summary>
    /// <returns></returns>
    public int GetSelectedCardCount()
    {
        return currentSelectedCardGroup.Count;
    }

    /// <summary>
    /// ��ȡĳ��ģ����������±�
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
    /// �����±��ȡ��ǰ��λ
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
    /// �����±���¼�λ��
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
    /// ���¿�Ƭ��ʾ�ļ�λ
    /// </summary>
    public void UpdateDisplayKey()
    {
        foreach (var item in cardModelList)
        {
            item.UpdateKey();
        }
    }

    /// <summary>
    /// ��ȡ����ѡ��Ŀ�Ƭ��
    /// </summary>
    /// <returns></returns>
    public Dictionary<FoodNameTypeMap, AvailableCardInfo> GetCurrentAvailableCardDict()
    {
        return mSelectEquipmentUI.GetCurrentAvailableCardDict();
    }
}
