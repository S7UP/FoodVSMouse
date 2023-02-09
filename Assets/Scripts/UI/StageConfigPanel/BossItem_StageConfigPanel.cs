using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 关卡情报面板左侧Boss图标按钮
/// </summary>
public class BossItem_StageConfigPanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Display;
    public int type;
    public int shape;
    public float maxHp;

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

    public void SetParam(int type, int shape, float hp, UnityAction call)
    {
        this.type = type;
        this.shape = shape;
        maxHp = hp;
        Img_Display.sprite = GameManager.Instance.GetSprite("Boss/" + type + "/" + shape + "/icon");
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/BossItem", gameObject);
    }

    public static BossItem_StageConfigPanel GetInstance(int type, int shape, float hp, UnityAction call)
    {
        BossItem_StageConfigPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/BossItem").GetComponent<BossItem_StageConfigPanel>();
        item.Initial();
        item.SetParam(type, shape, hp, call);
        return item;
    }
}
