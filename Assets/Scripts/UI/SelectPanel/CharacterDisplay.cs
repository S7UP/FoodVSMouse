using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ��ɫѡ����ʾ��ť
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    private PlayerInfoPanel mPlayerInfoPanel;

    public int type;
    private Button Btn;
    private Image Img_Background;
    private Image Img_Icon;
    private bool isUpdate;
    private Color[] colorArray = new Color[] { new Color(0, (float)67 / 255, (float)128 / 255, 1.0f), new Color(1.0f, 1.0f, 0, 1.0f) };

    private void Awake()
    {
        Btn = transform.Find("Button").GetComponent<Button>();
        Btn.onClick.AddListener(delegate {
            if (isUpdate)
                return;
            // ֪ͨ����ѡ�����װ
            mPlayerInfoPanel.OnClickCharacterDisplay(type);
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
        Img_Icon.sprite = GameManager.Instance.GetSprite("Character/" + type + "/icon");
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
    public static CharacterDisplay GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "PlayerInfoPanel/CharacterDisplay").GetComponent<CharacterDisplay>();
    }

    /// <summary>
    /// ���ոö���
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "PlayerInfoPanel/CharacterDisplay", gameObject);
    }
}
