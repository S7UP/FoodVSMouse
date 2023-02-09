using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 百科面板左侧的Boss图标按钮
/// </summary>
public class BossItem_EncyclopediaPanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Display;
    public int type;
    public int shape;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        Img_Display = transform.Find("Display").GetComponent<Image>();
    }

    public void Initial()
    {
        type = 0;
        shape = 0;
        Img_Display.sprite = null;
        Btn.onClick.RemoveAllListeners();
    }

    public void SetParam(int type, int shape, UnityAction call)
    {
        this.type = type;
        this.shape = shape;
        Img_Display.sprite = GameManager.Instance.GetSprite("Boss/" + type + "/" + shape + "/icon");
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "EncyclopediaPanel/BossItem", gameObject);
    }

    public static BossItem_EncyclopediaPanel GetInstance(int type, int shape, UnityAction call)
    {
        BossItem_EncyclopediaPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "EncyclopediaPanel/BossItem").GetComponent<BossItem_EncyclopediaPanel>();
        item.Initial();
        item.SetParam(type, shape, call);
        return item;
    }
}
