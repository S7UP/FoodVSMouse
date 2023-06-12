using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// ��ʯѡ����ʾ��ť
/// </summary>
public class JewelDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PlayerInfoPanel mPlayerInfoPanel;
    private RectTransform RectTrans;

    public int type;
    private Button Btn;
    private Image Img_Background;
    private Image Img_Icon;
    private bool isUpdate;
    private Color[] colorArray = new Color[] { new Color(0, (float)67/255, (float)128/255, 1.0f), new Color(1.0f, 1.0f, 0, 1.0f) };

    private void Awake()
    {
        RectTrans = GetComponent<RectTransform>();
        Btn = transform.Find("Button").GetComponent<Button>();
        Btn.onClick.AddListener(delegate
        {
            if (isUpdate)
                return;
            // ֪ͨ����ѡ��ñ�ʯ
            mPlayerInfoPanel.OnClickJewelDisplay(type);
        });
        Img_Background = GetComponent<Image>();
        Img_Icon = transform.Find("Button").GetComponent<Image>();
    }

    public void Initial()
    {
        type = 0;
    }

    public void SetValues(PlayerInfoPanel ui, int type)
    {
        mPlayerInfoPanel = ui;
        this.type = type;
        Img_Icon.sprite = GameManager.Instance.GetSprite("Jewel/"+type);
    }

    public void CancelSelected()
    {
        Img_Background.color = colorArray[0];
    }

    public void SetSelected()
    {
        Img_Background.color = colorArray[1];
    }

    /// <summary>
    /// ��ȡһ��ʵ��
    /// </summary>
    /// <returns></returns>
    public static JewelDisplay GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "PlayerInfoPanel/JewelDisplay").GetComponent<JewelDisplay>();
    }

    /// <summary>
    /// ���ոö���
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "PlayerInfoPanel/JewelDisplay", gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (type != -1)
        {
            TextArea.Instance.SetText(JewelManager.GetName(type) + "\n��ʼ��ȴ��"+JewelManager.GetStartEnergy(type)+"\n����ȴ��"+JewelManager.GetMaxEnergy(type) + "\n" + JewelManager.GetInfo(type));
            TextArea.Instance.SetLocalPosition(transform, new Vector2(RectTrans.sizeDelta.x / 2, 0), new Vector2(1, -1));
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (type != -1)
            TextArea.ExecuteRecycle();
    }
}
