using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// ��ѡ��Ŀ�Ƭģ��
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
    /// ��ʼ������
    /// </summary>
    public void Initial()
    {
        isSelected = false;
        Mask.SetActive(false);
    }

    /// <summary>
    /// ���ݿ�Ƭ���������������ɿ�Ƭ����ģ��
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
    /// ��ȡ����������Ϣ
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
    /// ��Ӱ�ť����
    /// </summary>
    /// <param name="call"></param>
    public void AddListener(UnityEngine.Events.UnityAction call)
    {
        button.onClick.AddListener(call);
    }

    /// <summary>
    /// ����ѡ��״̬
    /// </summary>
    public void SetSelected(bool selected)
    {
        // ���һ�������·���
        if (isSelected == selected)
            return;
        isSelected = selected;
        if (selected)
        {
            // ��ʾ�����Ա��
            Mask.SetActive(true);
        }
        else
        {
            // ��������
            Mask.SetActive(false);
        }

    }

    /// <summary>
    /// ���ÿ�Ƭ��ѡ���򲻿��ٱ����
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return !IsSelected();
    }

    /// <summary>
    /// ִ�л���
    /// </summary>
    public void ExecuteRecycle()
    {
        button.onClick.RemoveAllListeners();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_AvailableCard", this.gameObject);
    }

    /// <summary>
    /// ��ȡһ�������ʵ��
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
