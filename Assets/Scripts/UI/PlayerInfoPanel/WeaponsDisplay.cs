using UnityEngine.UI;
using UnityEngine;
/// <summary>
/// 武器选择显示按钮
/// </summary>
public class WeaponsDisplay : MonoBehaviour
{
    private PlayerInfoPanel mPlayerInfoPanel;

    public int type;
    public int shape;
    private Button Btn;
    private Image Img_Background;
    private Image Img_Icon;
    private bool isUpdate;
    private Color[] colorArray = new Color[] { new Color(0, (float)67/255, (float)128/255, 1.0f), new Color(1.0f, 1.0f, 0, 1.0f) };

    private void Awake()
    {
        Btn = transform.Find("Button").GetComponent<Button>();
        Btn.onClick.AddListener(delegate
        {
            if (isUpdate)
                return;
            // 通知宿主选择该武器
            mPlayerInfoPanel.OnClickWeaponsDisplay(type);
        });
        Img_Background = GetComponent<Image>();
        Img_Icon = transform.Find("Button").GetComponent<Image>();
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
        Img_Icon.sprite = GameManager.Instance.GetSprite("Weapons/"+type+"/"+shape+"/icon");
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
    public static WeaponsDisplay GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "PlayerInfoPanel/WeaponsDisplay").GetComponent<WeaponsDisplay>();
    }

    /// <summary>
    /// 回收该对象
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "PlayerInfoPanel/WeaponsDisplay", gameObject);
    }
}
