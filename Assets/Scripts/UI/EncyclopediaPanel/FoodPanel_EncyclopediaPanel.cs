using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class FoodPanel_EncyclopediaPanel : MonoBehaviour
{
    private Transform Trans_BaseInfo;
    private RectTransform RectTrans_Background;
    private Dropdown Dro_Rank;
    private Dropdown Dro_Shape;
    private RectTransform RectTrans_Display;
    private Image Img_Display;
    private Text Tex_Name;
    private Text Tex_TypeName;
    private Text Tex_CostValue;
    private Text Tex_CDValue;
    private Text Tex_HpValue;
    private Text Tex_AttackValue;
    private Text Tex_AttackSpeedValue;

    private ScrollRect Scr_Descript;
    private RectTransform RectTrans_Descript;
    private Dropdown Dro_Mode;
    private Text Tex_Descript;
    private RectTransform RectTrans_Tip;
    private ScrollRect Scr_Tip;
    private Text Tex_Tip;

    public void Awake()
    {
        Trans_BaseInfo = transform.Find("BaseInfo");
        RectTrans_Background = Trans_BaseInfo.Find("Img_Background").GetComponent<RectTransform>();
        RectTrans_Display = RectTrans_Background.Find("Img_Display").GetComponent<RectTransform>();
        Dro_Rank = RectTrans_Background.Find("Dro_Rank").GetComponent<Dropdown>();
        Dro_Shape = RectTrans_Background.Find("Dro_Shape").GetComponent<Dropdown>();
        Img_Display = RectTrans_Display.GetComponent<Image>();
        Tex_Name = Trans_BaseInfo.Find("Tex_Name").GetComponent<Text>();
        Tex_TypeName = Trans_BaseInfo.Find("Tex_TypeName").GetComponent<Text>();
        Tex_CostValue = Trans_BaseInfo.Find("Tex_Cost").Find("Value").GetComponent<Text>();
        Tex_CDValue = Trans_BaseInfo.Find("Tex_CD").Find("Value").GetComponent<Text>();
        Tex_HpValue = Trans_BaseInfo.Find("Tex_Hp").Find("Value").GetComponent<Text>();
        Tex_AttackValue = Trans_BaseInfo.Find("Tex_Attack").Find("Value").GetComponent<Text>();
        Tex_AttackSpeedValue = Trans_BaseInfo.Find("Tex_AttackSpeed").Find("Value").GetComponent<Text>();

        Scr_Descript = transform.Find("Descript").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Descript = Scr_Descript.content.transform.Find("Text").GetComponent<RectTransform>();
        Dro_Mode = transform.Find("Descript").Find("Dro_Mode").GetComponent<Dropdown>();
        Tex_Descript = RectTrans_Descript.GetComponent<Text>();
        Scr_Tip = transform.Find("Tip").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Tip = Scr_Tip.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Tip = RectTrans_Tip.GetComponent<Text>();
        OnAwakeSetDropDown();
    }

    /// <summary>
    /// 在Awake里设置下拉列表
    /// </summary>
    private void OnAwakeSetDropDown()
    {
        // 设置rank下拉列表
        Dro_Rank.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= 16; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("UI/Rank2/" + i)));
        }
        Dro_Rank.AddOptions(dataList);
        Dro_Rank.value = 0;
        Dro_Rank.onValueChanged.AddListener(delegate {
            (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel] as EncyclopediaPanel).UpdateFoodPanel();
        });
        // 设置shape下拉列表
        UpdateDropShape(0);
        // 设置描述模式下拉列表
        Dro_Mode.ClearOptions();
        dataList = new List<Dropdown.OptionData>();
        {
            dataList.Add(new Dropdown.OptionData("简洁版"));
            dataList.Add(new Dropdown.OptionData("详细版"));
        }
        Dro_Mode.AddOptions(dataList);
        Dro_Mode.value = 0;
        Dro_Mode.onValueChanged.AddListener(delegate {
            (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel] as EncyclopediaPanel).UpdateFoodPanel();
        });
    }

    public void Initial()
    {
        Img_Display.sprite = null;
        Tex_Name.text = "";
        Tex_TypeName.text = "";
        Tex_HpValue.text = "";
        Tex_AttackValue.text = "";
        Tex_AttackSpeedValue.text = "";
        Tex_Descript.text = "";
        Tex_Tip.text = "";
        Tex_CostValue.text = "";
        Tex_CDValue.text = "";
    }

    public void UpdateDropShape(int type)
    {
        int shape = Dro_Shape.value;
        // 设置shape下拉列表
        Dro_Shape.ClearOptions();
        int count = FoodManager.GetShapeCount((FoodNameTypeMap)type);
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        {
            dataList.Add(new Dropdown.OptionData("默认"));
            if(count > 1)
                dataList.Add(new Dropdown.OptionData("一转"));
            if(count > 2)
                dataList.Add(new Dropdown.OptionData("二转"));
        }
        Dro_Shape.AddOptions(dataList);
        Dro_Shape.onValueChanged.RemoveAllListeners();
        Dro_Shape.value = Mathf.Min(shape, count - 1);
        Dro_Shape.onValueChanged.AddListener(delegate {
            UpdateDropShape(type);
            (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel] as EncyclopediaPanel).UpdateFoodPanel();
        });
    }

    /// <summary>
    /// 传入美食的种类和变种，然后更新整个面板的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public void UpdateByParam(int type)
    {
        int shape = Mathf.Min(Dro_Shape.value, FoodManager.GetShapeCount((FoodNameTypeMap)type) - 1);
        int level = Dro_Rank.value;
        // 切换图标并自动同步尺寸
        float w_h_rate = RectTrans_Background.sizeDelta.x / RectTrans_Background.sizeDelta.y;
        Img_Display.sprite = GameManager.Instance.GetSprite("Food/"+type+"/"+shape+"/display");
        Img_Display.SetNativeSize();
        //float new_w_h_rate = RectTrans_Display.sizeDelta.x / RectTrans_Display.sizeDelta.y;
        //if(new_w_h_rate > w_h_rate)
        //    RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.x, RectTrans_Background.sizeDelta.x / new_w_h_rate);
        //else
        //    RectTrans_Display.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.y * new_w_h_rate, RectTrans_Background.sizeDelta.y);
        // 显示基础属性
        FoodUnit.Attribute attr = GetAttribute(type, shape);
        Tex_HpValue.text = Mathf.FloorToInt(FoodManager.GetHp((FoodNameTypeMap)type, level, shape)).ToString();
        Tex_AttackValue.text = (FoodManager.GetAttack((FoodNameTypeMap)type, level, shape)).ToString("F2");
        Tex_AttackSpeedValue.text = (attr.baseAttrbute.baseAttackSpeed).ToString("F2");
        BaseCardBuilder.Attribute attr2 = GetCardBuilderAttribute(type, shape);
        Tex_CostValue.text = attr2.GetCost(level).ToString();
        Tex_CDValue.text = attr2.GetCD(level).ToString();

        // 名称、描述、提示
        Tex_Name.text = FoodManager.GetFoodName(type, shape);
        Tex_TypeName.text = FoodManager.GetFoodInGridTypeName(type);
        if(Dro_Mode.value == 0)
            Tex_Descript.text = FoodManager.GetSimpleFeature((FoodNameTypeMap)type, level, shape);
        else
            Tex_Descript.text = FoodManager.GetDetailedFeature((FoodNameTypeMap)type, level, shape);
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
        Tex_Tip.text = FoodManager.GetUseTips((FoodNameTypeMap)type, level, shape);
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

    private FoodUnit.Attribute GetAttribute(int type, int shape)
    {
        return GameManager.Instance.attributeManager.GetFoodUnitAttribute(type, shape);
    }

    private BaseCardBuilder.Attribute GetCardBuilderAttribute(int type, int shape)
    {
        return GameManager.Instance.attributeManager.GetCardBuilderAttribute(type, shape);
    }
}
