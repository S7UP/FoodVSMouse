using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 百科面板左侧的地形图标按钮
/// </summary>
public class TerrainItem_EncyclopediaPanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Display;
    public int type;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        Img_Display = transform.Find("Display").GetComponent<Image>();
    }

    public void Initial()
    {
        type = 0;
        Img_Display.sprite = null;
        Btn.onClick.RemoveAllListeners();
    }

    public void SetParam(int type, UnityAction call)
    {
        this.type = type;
        Img_Display.sprite = GameManager.Instance.GetSprite("Environment/" + type);
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "EncyclopediaPanel/TerrainItem", gameObject);
    }

    public static TerrainItem_EncyclopediaPanel GetInstance(int type, UnityAction call)
    {
        TerrainItem_EncyclopediaPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "EncyclopediaPanel/TerrainItem").GetComponent<TerrainItem_EncyclopediaPanel>();
        item.Initial();
        item.SetParam(type, call);
        return item;
    }
}
