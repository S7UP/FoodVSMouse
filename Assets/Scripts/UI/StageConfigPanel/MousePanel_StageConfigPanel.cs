using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 关卡情报面板右侧的老鼠信息面板
/// </summary>
public class MousePanel_StageConfigPanel : MonoBehaviour
{
    private Transform Trans_BaseInfo;
    private RectTransform RectTrans_Background;
    private RectTransform RectTrans_Display;
    private Image Img_Display;
    private Text Tex_Name;
    private Text Tex_TypeName;
    private Text Tex_HpValue;
    private Text Tex_AttackValue;
    private Text Tex_AttackSpeedValue;
    private Text Tex_MoveSpeedValue;

    private ScrollRect Scr_Descript;
    private RectTransform RectTrans_Descript;
    private Text Tex_Descript;
    private RectTransform RectTrans_Tip;
    private ScrollRect Scr_Tip;
    private Text Tex_Tip;

    private void Awake()
    {
        Trans_BaseInfo = transform.Find("BaseInfo");
        RectTrans_Background = Trans_BaseInfo.Find("Img_Background").GetComponent<RectTransform>();
        RectTrans_Display = RectTrans_Background.Find("Img_Display").GetComponent<RectTransform>();
        Img_Display = RectTrans_Display.GetComponent<Image>();
        Tex_Name = Trans_BaseInfo.Find("Tex_Name").GetComponent<Text>();
        Tex_TypeName = Trans_BaseInfo.Find("Tex_TypeName").GetComponent<Text>();
        Tex_HpValue = Trans_BaseInfo.Find("Tex_Hp").Find("Value").GetComponent<Text>();
        Tex_AttackValue = Trans_BaseInfo.Find("Tex_Attack").Find("Value").GetComponent<Text>();
        Tex_AttackSpeedValue = Trans_BaseInfo.Find("Tex_AttackSpeed").Find("Value").GetComponent<Text>();
        Tex_MoveSpeedValue = Trans_BaseInfo.Find("Tex_MoveSpeed").Find("Value").GetComponent<Text>();

        Scr_Descript = transform.Find("Descript").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Descript = Scr_Descript.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Descript = RectTrans_Descript.GetComponent<Text>();
        Scr_Tip = transform.Find("Tip").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Tip = Scr_Tip.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Tip = RectTrans_Tip.GetComponent<Text>();
    }

    public void Initial()
    {
        Img_Display.sprite = null;
        Tex_Name.text = "";
        Tex_TypeName.text = "";
        Tex_HpValue.text = "";
        Tex_AttackValue.text = "";
        Tex_AttackSpeedValue.text = "";
        Tex_MoveSpeedValue.text = "";
        Tex_Descript.text = "";
        Tex_Tip.text = "";
    }

    /// <summary>
    /// 传入老鼠的种类和变种，然后更新整个面板的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public void UpdateByMouseParam(int type, int shape)
    {
        // 切换图标并自动同步尺寸
        float w_h_rate = RectTrans_Background.sizeDelta.x / RectTrans_Background.sizeDelta.y;
        Img_Display.sprite = GameManager.Instance.GetSprite("Mouse/"+type+"/"+shape+"/display");
        Img_Display.SetNativeSize();
        float new_w_h_rate = RectTrans_Display.sizeDelta.x / RectTrans_Display.sizeDelta.y;
        if(new_w_h_rate > w_h_rate)
            RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.x, RectTrans_Background.sizeDelta.x / new_w_h_rate);
        else
            RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.y * new_w_h_rate, RectTrans_Background.sizeDelta.y);
        // 显示基础属性
        MouseUnit.Attribute attr = GameManager.Instance.attributeManager.GetMouseUnitAttribute(type, shape);
        Tex_HpValue.text = Mathf.FloorToInt((float)attr.baseAttrbute.baseHP).ToString();
        Tex_AttackValue.text = (attr.baseAttrbute.baseAttack).ToString("F2");
        Tex_AttackSpeedValue.text = (attr.baseAttrbute.baseAttackSpeed).ToString("F2");
        Tex_MoveSpeedValue.text = (attr.baseAttrbute.baseMoveSpeed).ToString("F2");

        // 名称、描述、提示
        MouseManager.MouseTypeInfo mouseTypeInfo = MouseManager.GetMouseTypeInfo(type);
        MouseManager.MouseInfo mouseInfo = MouseManager.GetMouseInfo(type, shape);
        Tex_Name.text = mouseInfo.name;
        Tex_TypeName.text = mouseTypeInfo.name;
        Tex_Descript.text = mouseTypeInfo.descript +"\n\n"+ mouseInfo.descript;
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Descript.sizeDelta.x / Tex_Descript.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Descript.text.Length / countPerRow + 2); // 计算需要多少行
            foreach (var c in Tex_Descript.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            RectTrans_Descript.sizeDelta = new Vector2(RectTrans_Descript.sizeDelta.x, Tex_Descript.fontSize*rowCount);
            Scr_Descript.content.sizeDelta = new Vector2(Scr_Descript.content.sizeDelta.x, RectTrans_Descript.sizeDelta.y);
        }
        Tex_Tip.text = mouseTypeInfo.tip + "\n\n"+ mouseInfo.tip;
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Tip.sizeDelta.x / Tex_Tip.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Tip.text.Length / countPerRow + 1); // 计算需要多少行
            foreach (var c in Tex_Tip.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            RectTrans_Tip.sizeDelta = new Vector2(RectTrans_Tip.sizeDelta.x, Tex_Tip.fontSize * rowCount);
            Scr_Tip.content.sizeDelta = new Vector2(Scr_Tip.content.sizeDelta.x, RectTrans_Tip.sizeDelta.y);
        }
    }


    /// <summary>
    /// 传入Boss的种类和变种，然后更新整个面板的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public void UpdateByBossParam(int type, int shape, float hp)
    {
        // 切换图标并自动同步尺寸
        float w_h_rate = RectTrans_Background.sizeDelta.x / RectTrans_Background.sizeDelta.y;
        Img_Display.sprite = GameManager.Instance.GetSprite("Boss/" + type + "/" + shape + "/display");
        Img_Display.SetNativeSize();
        float new_w_h_rate = RectTrans_Display.sizeDelta.x / RectTrans_Display.sizeDelta.y;
        if (new_w_h_rate > w_h_rate)
            RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.x, RectTrans_Background.sizeDelta.x / new_w_h_rate);
        else
            RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.y * new_w_h_rate, RectTrans_Background.sizeDelta.y);
        // 显示基础属性
        Tex_HpValue.text = hp.ToString();
        Tex_AttackValue.text = "--";
        Tex_AttackSpeedValue.text = "--";
        Tex_MoveSpeedValue.text = "--";

        // 名称、描述、提示
        Tex_Name.text = BossManager.GetBossName(type, shape);
        Tex_TypeName.text = "鼠军头目";
        Tex_Descript.text = BossManager.GetSimpleInfo(type, shape);
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Descript.sizeDelta.x / Tex_Descript.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Descript.text.Length / countPerRow + 2); // 计算需要多少行
            foreach (var c in Tex_Descript.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            RectTrans_Descript.sizeDelta = new Vector2(RectTrans_Descript.sizeDelta.x, Tex_Descript.fontSize * rowCount);
            Scr_Descript.content.sizeDelta = new Vector2(Scr_Descript.content.sizeDelta.x, RectTrans_Descript.sizeDelta.y);
        }
        Tex_Tip.text = BossManager.GetTips(type, shape);
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Tip.sizeDelta.x / Tex_Tip.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Tip.text.Length / countPerRow + 1); // 计算需要多少行
            foreach (var c in Tex_Tip.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            RectTrans_Tip.sizeDelta = new Vector2(RectTrans_Tip.sizeDelta.x, Tex_Tip.fontSize * rowCount);
            Scr_Tip.content.sizeDelta = new Vector2(Scr_Tip.content.sizeDelta.x, RectTrans_Tip.sizeDelta.y);
        }
    }
}
