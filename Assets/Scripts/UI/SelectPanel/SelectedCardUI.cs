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
    /// ��ʼ��
    /// </summary>
    public void Initial()
    {
        // ��ȡ�����
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
        cardGroupList = GameManager.Instance.playerData.LoadCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex);
        if (cardGroupList.Count < 8) // Ĭ��8����Ƭ��
            for (int i = cardGroupList.Count; i < 8; i++)
            {
                cardGroupList.Add(new List<AvailableCardInfo>());
            }
        // ���¿��鰴ť��
        UpdateCardGroupButtonList();
        SetCurrentSelectedCardGroup(0); // Ĭ�ϵ�һ��Ϊ��ǰѡ�ÿ���

    }

    /// <summary>
    /// ���õ�ǰ��Ƭ��
    /// </summary>
    private void SetCurrentSelectedCardGroup(int arrayIndex)
    {
        currentSelectedCardGroup = cardGroupList[arrayIndex];
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
        // ��俨Ƭģ��
        foreach (var item in currentSelectedCardGroup)
        {
            SelectedCardModel model = SelectedCardModel.CreateInstance();
            model.SetAvailableCardInfo(item);
            model.transform.SetParent(SelectedCardListTrans);
            model.transform.localScale = Vector3.one;
            cardModelList.Add(model);
            // ���ŷ��붯��
            model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)item.type));
            // �ڿ�ѡ��ƬUI�б��Ϊ��ѡ��
            mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)item.type, true);
            // ���ȡ������
            model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
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
        model.SetAvailableCardInfo(info);
        model.transform.SetParent(SelectedCardListTrans);
        model.transform.localScale = Vector3.one;
        cardModelList.Add(model);
        currentSelectedCardGroup.Add(info);
        // ���ŷ��붯��
        model.PlaySelectedTween(mSelectEquipmentUI.GetAvailableCardModelPosition((FoodNameTypeMap)type));
        // �ڿ�ѡ��ƬUI�б��Ϊ��ѡ��
        mSelectEquipmentUI.SetAvailableCardModelSelect((FoodNameTypeMap)type, true);
        // �����ı���ʾ
        UpdateCardCountText();
        // ���ȡ������
        model.AddListenerToCancelButton(delegate { mSelectEquipmentUI.CancelSelectCard((FoodNameTypeMap)model.GetInfo().type); });
        return model;
    }

    /// <summary>
    /// �ӿ������Ƴ���Ƭ���Żؿ���
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
            // �ӵ�ǰ�������Ƴ���Ӧ��Ƭ
            currentSelectedCardGroup.Remove(info);
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
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
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
        BaseStage.StageInfo info = mSelectEquipmentUI.GetCurrentSelectedStageInfo();
        GameManager.Instance.playerData.SaveCardGroupList(info.chapterIndex, info.sceneIndex, info.stageIndex, cardGroupList);
    }

    /// <summary>
    /// ��ȡ��ǰѡ��Ŀ�Ƭ��
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
    /// ��ȡ��ǰѡ��Ŀ�Ƭ��
    /// </summary>
    /// <returns></returns>
    public int GetSelectedCardCount()
    {
        return currentSelectedCardGroup.Count;
    }
}
