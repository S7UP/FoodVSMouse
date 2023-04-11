using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;
/// <summary>
/// 选择配置中已被选中的卡片模型
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
        // 设置rank下拉列表
        Dro_Rank.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= dict[(FoodNameTypeMap)info.type].maxLevel; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("UI/Rank2/"+i)));
        }
        Dro_Rank.AddOptions(dataList);
        Dro_Rank.value = info.maxLevel;
        Dro_Rank.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxLevel = Dro_Rank.value; });
        // 设置shape下拉列表
        Dro_Shape.ClearOptions();
        dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= dict[(FoodNameTypeMap)info.type].maxShape; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("Food/"+info.type+"/" + i + "/icon")));
        }
        Dro_Shape.AddOptions(dataList);
        Dro_Shape.value = info.maxShape;
        Dro_Shape.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxShape = Dro_Shape.value; });
        // 设置费用文本
        Tex_Cost.text = GameManager.Instance.attributeManager.GetCardBuilderAttribute(info.type, Dro_Shape.value).GetCost(info.maxLevel)+"";
        // 设置键控
        Inp_Key.onValueChanged.RemoveAllListeners();
        Inp_Key.onValueChanged.AddListener(
            delegate {
                if (isUpdateKey)
                    return;
                int index = mSelectedCardUI.GetModelInListIndex(this);
                // 只取最后一位输入的字符
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
    /// 播放被选中动画
    /// </summary>
    public void PlaySelectedTween(Vector3 startPosition)
    {
        Hide();
        Tween t = Emp_Card_RectTrans.DOMove(startPosition, 0.05f);
        t.onComplete = delegate { Show();  Emp_Card_RectTrans.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutCubic); };
    }

    /// <summary>
    /// 播放取消选择动画
    /// </summary>
    /// <param name="endPosition"></param>
    public void PlayUnSelectedTween(Vector3 endPosition, Action EndEvent)
    {
        Tween t = Emp_Card_RectTrans.DOMove(endPosition, 0.25f);
        t.SetEase(Ease.OutCubic);
        t.onComplete = delegate { EndEvent(); };
    }

    /// <summary>
    /// 获取当前卡片信息
    /// </summary>
    public AvailableCardInfo GetInfo()
    {
        return mAvailableCardInfo;
    }

    /// <summary>
    /// 创建一个实例
    /// </summary>
    /// <returns></returns>
    public static SelectedCardModel CreateInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/SelectedCardModel").GetComponent<SelectedCardModel>();
    }

    /// <summary>
    /// 回收当前实例
    /// </summary>
    public void ExecuteRecycle()
    {
        Btn_Cancel.onClick.RemoveAllListeners();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/SelectedCardModel", gameObject);
    }

    /// <summary>
    /// 为取消按钮添加监听
    /// </summary>
    public void AddListenerToCancelButton(UnityEngine.Events.UnityAction call)
    {
        Btn_Cancel.onClick.AddListener(call);
    }

    /// <summary>
    /// 隐藏
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
    /// 显示
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
    /// 更新一次Key，在卡片顺序发生变化时调用
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
