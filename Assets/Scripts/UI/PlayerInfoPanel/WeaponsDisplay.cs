using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// ����ѡ����ʾ��ť
/// </summary>
public class WeaponsDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PlayerInfoPanel mPlayerInfoPanel;
    private RectTransform RectTrans;

    public int type;
    public int shape;
    private Button Btn;
    private Image Img_Background;
    private Image Img_Icon;
    private GameObject Go_Mask;
    private Text Tex_Unlock;
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
            // ֪ͨ����ѡ�������
            mPlayerInfoPanel.OnClickWeaponsDisplay(type);
        });
        Img_Background = GetComponent<Image>();
        Img_Icon = transform.Find("Button").GetComponent<Image>();
        Go_Mask = transform.Find("Image").gameObject;
        Tex_Unlock = Go_Mask.transform.Find("Text").GetComponent<Text>();
    }

    public void Initial()
    {
        type = 0;
        shape = 0;
    }

    public void SetValues(PlayerInfoPanel ui, int type, int shape)
    {
        mPlayerInfoPanel = ui;
        this.type = type;
        this.shape = shape;
        Img_Icon.sprite = GameManager.Instance.GetSprite("Weapons/"+type+"/icon");
        bool isDeveloperMode = ConfigManager.IsDeveloperMode();
        if(isDeveloperMode || WeaponsManager.IsUnlock((WeaponsNameTypeMap)type) || WeaponsManager.TryUnlock((WeaponsNameTypeMap)type))
        {
            Go_Mask.SetActive(false);
            Btn.interactable = true;
        }
        else
        {
            Go_Mask.SetActive(true);
            Btn.interactable = false;
            Tex_Unlock.text = WeaponsManager.GetUnlockLevel((WeaponsNameTypeMap)type) + "������";
        }
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
    public static WeaponsDisplay GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "PlayerInfoPanel/WeaponsDisplay").GetComponent<WeaponsDisplay>();
    }

    /// <summary>
    /// ���ոö���
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "PlayerInfoPanel/WeaponsDisplay", gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (type != -1)
        {
            TextArea.Instance.SetText(WeaponsManager.GetName(type) + "\n���������" + WeaponsManager.GetInterval(type) + "\n" + WeaponsManager.GetInfo(type));
            TextArea.Instance.SetLocalPosition(transform, new Vector2(RectTrans.sizeDelta.x / 2, 0), new Vector2(1, -1));
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (type != -1)
            TextArea.ExecuteRecycle();
    }
}
