using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;
/// <summary>
/// ѡ���������ѱ�ѡ�еĿ�Ƭģ��
/// </summary>
public class SelectedCardModel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform RectTrans;
    private SelectedCardUI mSelectedCardUI;
    private RectTransform Emp_Card_RectTrans;
    private Text Tex_Cost;
    private Button Btn_Cancel;
    private Dropdown Dro_Rank;
    private Dropdown Dro_Shape;
    private Image mImage;
    private Button Btn_KeyButton;
    private InputField Inp_Key;
    private bool isUpdateKey;

    private AvailableCardInfo mAvailableCardInfo;   

    private void Awake()
    {
        RectTrans = GetComponent<RectTransform>();
        Emp_Card_RectTrans = transform.Find("Emp_Card").GetComponent<RectTransform>();
        Tex_Cost = Emp_Card_RectTrans.Find("Tex_Cost").GetComponent<Text>();
        Btn_Cancel = Emp_Card_RectTrans.Find("Btn_Cancel").GetComponent<Button>();
        Dro_Rank = Emp_Card_RectTrans.Find("Dro_Rank").GetComponent<Dropdown>();
        Dro_Shape = Emp_Card_RectTrans.Find("Dro_Shape").GetComponent<Dropdown>();
        mImage = Emp_Card_RectTrans.GetComponent<Image>();
        Btn_KeyButton = Emp_Card_RectTrans.Find("Btn_KeyButton").GetComponent<Button>();
        Inp_Key = Btn_KeyButton.transform.Find("InputField").GetComponent<InputField>();
    }

    public void SetAvailableCardInfo(AvailableCardInfo info)
    {
        Dictionary<FoodNameTypeMap, AvailableCardInfo> dict = mSelectedCardUI.GetCurrentAvailableCardDict();
        mAvailableCardInfo = info;
        // ����rank�����б�
        Dro_Rank.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= dict[(FoodNameTypeMap)info.type].maxLevel; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("UI/Rank2/"+i)));
        }
        Dro_Rank.AddOptions(dataList);
        Dro_Rank.value = info.maxLevel;
        Dro_Rank.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxLevel = Dro_Rank.value; });
        // ����shape�����б�
        Dro_Shape.ClearOptions();
        dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= dict[(FoodNameTypeMap)info.type].maxShape; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("Food/"+info.type+"/" + i + "/icon")));
        }
        Dro_Shape.AddOptions(dataList);
        Dro_Shape.value = info.maxShape;
        Dro_Shape.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxShape = Dro_Shape.value; });
        // ���÷����ı�
        Tex_Cost.text = GameManager.Instance.attributeManager.GetCardBuilderAttribute(info.type, Dro_Shape.value).GetCost(info.maxLevel)+"";
        // ���ü���
        Inp_Key.onValueChanged.RemoveAllListeners();
        Inp_Key.onValueChanged.AddListener(
            delegate {
                if (isUpdateKey)
                    return;
                int index = mSelectedCardUI.GetModelInListIndex(this);
                // ֻȡ���һλ������ַ�
                if (Inp_Key.text != null && !Inp_Key.text.Equals("") && Inp_Key.text.Length>0)
                {
                    char c = Inp_Key.text.ToCharArray()[0];
                    if (c >= 'a' && c <= 'z')
                    {
                        c -= 'a';
                        c += 'A';
                    }
                    isUpdateKey = true;
                    Inp_Key.text = c.ToString();
                    isUpdateKey = false;
                    mSelectedCardUI.UpdateKeyByIndex(index, c);
                }
                else
                {
                    mSelectedCardUI.UpdateKeyByIndex(index, '\0');
                }
            });
    }

    /// <summary>
    /// ���ű�ѡ�ж���
    /// </summary>
    public void PlaySelectedTween(Vector3 startPosition)
    {
        Hide();
        Tween t = Emp_Card_RectTrans.DOMove(startPosition, 0.05f);
        t.onComplete = delegate { Show();  Emp_Card_RectTrans.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutCubic); };
    }

    /// <summary>
    /// ����ȡ��ѡ�񶯻�
    /// </summary>
    /// <param name="endPosition"></param>
    public void PlayUnSelectedTween(Vector3 endPosition, Action EndEvent)
    {
        Tween t = Emp_Card_RectTrans.DOMove(endPosition, 0.25f);
        t.SetEase(Ease.OutCubic);
        t.onComplete = delegate { EndEvent(); };
    }

    /// <summary>
    /// ��ȡ��ǰ��Ƭ��Ϣ
    /// </summary>
    public AvailableCardInfo GetInfo()
    {
        return mAvailableCardInfo;
    }

    /// <summary>
    /// ����һ��ʵ��
    /// </summary>
    /// <returns></returns>
    public static SelectedCardModel CreateInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/SelectedCardModel").GetComponent<SelectedCardModel>();
    }

    /// <summary>
    /// ���յ�ǰʵ��
    /// </summary>
    public void ExecuteRecycle()
    {
        Btn_Cancel.onClick.RemoveAllListeners();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/SelectedCardModel", gameObject);
    }

    /// <summary>
    /// Ϊȡ����ť��Ӽ���
    /// </summary>
    public void AddListenerToCancelButton(UnityEngine.Events.UnityAction call)
    {
        Btn_Cancel.onClick.AddListener(call);
    }

    /// <summary>
    /// ����
    /// </summary>
    private void Hide()
    {
        Tex_Cost.enabled = false;
        Btn_Cancel.GetComponent<Image>().enabled = false;
        Dro_Rank.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        Dro_Shape.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        mImage.enabled = false;
    }

    /// <summary>
    /// ��ʾ
    /// </summary>
    private void Show()
    {
        Tex_Cost.enabled = true;
        Btn_Cancel.GetComponent<Image>().enabled = true;
        Dro_Rank.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        Dro_Shape.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        mImage.enabled = true;
    }

    public void SetSelectedCardUI(SelectedCardUI ui)
    {
        mSelectedCardUI = ui;
    }

    /// <summary>
    /// ����һ��Key���ڿ�Ƭ˳�����仯ʱ����
    /// </summary>
    public void UpdateKey()
    {
        isUpdateKey = true;
        int index = mSelectedCardUI.GetModelInListIndex(this);
        Inp_Key.text = mSelectedCardUI.GetKeyByIndex(index).ToString();
        isUpdateKey = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int type = mAvailableCardInfo.type;
        int shape = mAvailableCardInfo.maxShape;
        if (type != -1)
        {
            TextArea.Instance.SetText(FoodManager.GetFoodName(type, shape) + "\n" + FoodManager.GetVerySimpleFeature((FoodNameTypeMap)type));
            TextArea.Instance.SetLocalPosition(transform, new Vector2(RectTrans.sizeDelta.x / 2, -RectTrans.sizeDelta.y / 2), new Vector2(1, -1));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        int type = mAvailableCardInfo.type;
        if (type != -1)
            TextArea.ExecuteRecycle();
    }
}
