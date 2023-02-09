using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class TerrainPanel_EncyclopediaPanel : MonoBehaviour
{
    private RectTransform RectTrans_Background;
    private RectTransform RectTrans_Display;
    private Image Img_Display;
    private Text Tex_Name;

    private ScrollRect Scr_Descript;
    private RectTransform RectTrans_Descript;
    private Text Tex_Descript;
    private RectTransform RectTrans_Tip;
    private ScrollRect Scr_Tip;
    private Text Tex_Tip;
    private Dropdown Dro_Mode;

    public void Awake()
    {
        RectTrans_Background = transform.Find("Img_Background").GetComponent<RectTransform>();
        RectTrans_Display = RectTrans_Background.Find("Img_Display").GetComponent<RectTransform>();
        Img_Display = RectTrans_Display.GetComponent<Image>();
        Tex_Name = transform.Find("Tex_Name").GetComponent<Text>();

        Scr_Descript = transform.Find("Descript").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Descript = Scr_Descript.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Descript = RectTrans_Descript.GetComponent<Text>();
        Scr_Tip = transform.Find("Tip").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Tip = Scr_Tip.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Tip = RectTrans_Tip.GetComponent<Text>();
        Dro_Mode = transform.Find("Descript").Find("Dro_Mode").GetComponent<Dropdown>();
        OnAwakeSetDropDown();
    }

    /// <summary>
    /// 在Awake里设置下拉列表
    /// </summary>
    private void OnAwakeSetDropDown()
    {
        // 设置描述模式下拉列表
        Dro_Mode.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        {
            dataList.Add(new Dropdown.OptionData("简洁版"));
            dataList.Add(new Dropdown.OptionData("详细版"));
        }
        Dro_Mode.AddOptions(dataList);
        Dro_Mode.value = 0;
        Dro_Mode.onValueChanged.AddListener(delegate {
            (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel] as EncyclopediaPanel).UpdateTerrainPanel();
        });
    }

    public void Initial()
    {
        Img_Display.sprite = null;
        Tex_Name.text = "";
        Tex_Descript.text = "";
        Tex_Tip.text = "";
    }

    /// <summary>
    /// 传入老鼠的种类和变种，然后更新整个面板的信息
    /// </summary>
    /// <param name="type"></param>
    public void UpdateByParam(int type)
    {
        // 切换图标并自动同步尺寸
        Img_Display.sprite = GameManager.Instance.GetSprite("Environment/"+type);
        // Img_Display.SetNativeSize();

        // 名称、描述、提示
        Tex_Name.text = EnvironmentManager.GetName(type);
        if(Dro_Mode.value == 0)
            Tex_Descript.text = EnvironmentManager.GetSimpleInfo(type);
        else
            Tex_Descript.text = EnvironmentManager.GetDetailedInfo(type);
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
        Tex_Tip.text = EnvironmentManager.GetTips(type);
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
