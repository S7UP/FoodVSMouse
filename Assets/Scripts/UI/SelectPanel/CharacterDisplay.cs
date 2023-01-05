using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 角色选择显示按钮
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
            // 通知宿主选择该套装
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
    /// 获取一个实例
    /// </summary>
    /// <returns></returns>
    public static CharacterDisplay GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "PlayerInfoPanel/CharacterDisplay").GetComponent<CharacterDisplay>();
    }

    /// <summary>
    /// 回收该对象
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "PlayerInfoPanel/CharacterDisplay", gameObject);
    }
}
