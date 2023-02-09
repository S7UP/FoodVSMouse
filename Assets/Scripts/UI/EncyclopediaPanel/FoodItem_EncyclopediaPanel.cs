using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 百科面板左侧的美食图标按钮
/// </summary>
public class FoodItem_EncyclopediaPanel : MonoBehaviour
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
        Img_Display.sprite = GameManager.Instance.GetSprite("Food/" + type + "/0/display");
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "EncyclopediaPanel/FoodItem", gameObject);
    }

    public static FoodItem_EncyclopediaPanel GetInstance(int type, UnityAction call)
    {
        FoodItem_EncyclopediaPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "EncyclopediaPanel/FoodItem").GetComponent<FoodItem_EncyclopediaPanel>();
        item.Initial();
        item.SetParam(type, call);
        return item;
    }
}
