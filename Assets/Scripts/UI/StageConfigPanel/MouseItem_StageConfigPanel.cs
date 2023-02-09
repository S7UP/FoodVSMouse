using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 关卡情报面板左侧的老鼠图标按钮
/// </summary>
public class MouseItem_StageConfigPanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Display;
    private Text Tex_Num;
    public int type;
    public int shape;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        Img_Display = transform.Find("Display").GetComponent<Image>();
        Tex_Num = transform.Find("Text").GetComponent<Text>();
    }

    public void Initial()
    {
        type = 0;
        shape = 0;
        Img_Display.sprite = null;
        Btn.onClick.RemoveAllListeners();
    }

    public void SetParam(int type, int shape, int num, UnityAction call)
    {
        this.type = type;
        this.shape = shape;
        Img_Display.sprite = GameManager.Instance.GetSprite("Mouse/" + type + "/" + shape + "/icon");
        Tex_Num.text = "x" + num;
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/MouseItem", gameObject);
    }

    public static MouseItem_StageConfigPanel GetInstance(int type, int shape, int num, UnityAction call)
    {
        MouseItem_StageConfigPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/MouseItem").GetComponent<MouseItem_StageConfigPanel>();
        item.Initial();
        item.SetParam(type, shape, num, call);
        return item;
    }
}
