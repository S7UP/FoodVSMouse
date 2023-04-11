using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 可选择的卡片模型
/// </summary>
public class Btn_AvailableCard : MonoBehaviour, ICanvasRaycastFilter, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform RectTrans;
    private AvailableCardInfo availableCardInfo;
    private Button button;
    private bool isSelected;
    private Image Img_Display;
    private Text Tex_Cost;
    private Image Img_Level;
    private GameObject Mask;
    

    private void Awake()
    {
        RectTrans = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        Mask = transform.Find("Mask").gameObject;
        Img_Display = transform.Find("Img_Display").GetComponent<Image>();
        Tex_Cost = transform.Find("Tex_Cost").GetComponent<Text>();
        Img_Level = transform.Find("Img_Level").GetComponent<Image>();
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    public void Initial()
    {
        isSelected = false;
        Mask.SetActive(false);
    }

    /// <summary>
    /// 根据卡片的限制条件来生成卡片最终模型
    /// </summary>
    /// <param name="info"></param>
    public void UpdateByAvailableCardInfo(AvailableCardInfo info)
    {
        availableCardInfo = info;
        Img_Display.sprite = GameManager.Instance.GetSprite("Food/"+info.type+"/"+info.maxShape+"/icon");
        Tex_Cost.text = GameManager.Instance.attributeManager.GetCardBuilderAttribute(info.type, info.maxShape).GetCost(info.maxLevel).ToString();
        Img_Level.sprite = GameManager.Instance.GetSprite("UI/Rank2/" + info.maxLevel);
    }

    /// <summary>
    /// 获取限制条件信息
    /// </summary>
    /// <returns></returns>
    public AvailableCardInfo GetAvailableCardInfo()
    {
        return availableCardInfo;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// 添加按钮监听
    /// </summary>
    /// <param name="call"></param>
    public void AddListener(UnityEngine.Events.UnityAction call)
    {
        button.onClick.AddListener(call);
    }

    /// <summary>
    /// 设置选择状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        // 如果一样则无事发生
        if (isSelected == selected)
            return;
        isSelected = selected;
        if (selected)
        {
            // 显示遮罩以变黑
            Mask.SetActive(true);
        }
        else
        {
            // 隐藏遮罩
            Mask.SetActive(false);
        }

    }

    /// <summary>
    /// 若该卡片被选中则不可再被点击
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return !IsSelected();
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public void ExecuteRecycle()
    {
        button.onClick.RemoveAllListeners();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_AvailableCard", this.gameObject);
    }

    /// <summary>
    /// 获取一个本类的实例
    /// </summary>
    /// <returns></returns>
    public static Btn_AvailableCard GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/Btn_AvailableCard").GetComponent<Btn_AvailableCard>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int type = availableCardInfo.type;
        int shape = availableCardInfo.maxShape;
        if (type != -1)
        {
            TextArea.Instance.SetText(FoodManager.GetFoodName(type, shape) + "\n" + FoodManager.GetVerySimpleFeature((FoodNameTypeMap)type));
            TextArea.Instance.SetLocalPosition(transform, new Vector2(RectTrans.sizeDelta.x / 2, -RectTrans.sizeDelta.y / 2), new Vector2(1, -1));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        int type = availableCardInfo.type;
        if (type != -1)
            TextArea.ExecuteRecycle();
    }
}
