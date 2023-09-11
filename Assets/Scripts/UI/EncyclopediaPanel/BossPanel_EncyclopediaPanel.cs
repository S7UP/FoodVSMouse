using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class BossPanel_EncyclopediaPanel : MonoBehaviour
{
    private Text Tex_Name;

    private GameObject Go_Descript;
    private ScrollRect Scr_Descript;
    private RectTransform RectTrans_Descript;
    private Dropdown Dro_Mode;
    private Text Tex_Descript;
    private GameObject Go_Tip;
    private RectTransform RectTrans_Tip;
    private ScrollRect Scr_Tip;
    private Text Tex_Tip;
    private GameObject Go_Background;
    private RectTransform RectTrans_Background;
    private ScrollRect Scr_Background;
    private Text Tex_Background;
    private Dropdown Dro_Change;


    public void Awake()
    {
        Tex_Name = transform.Find("Tex_Name").GetComponent<Text>();

        Go_Descript = transform.Find("Descript").gameObject;
        Scr_Descript = transform.Find("Descript").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Descript = Scr_Descript.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Descript = RectTrans_Descript.GetComponent<Text>();
        Go_Tip = transform.Find("Tip").gameObject;
        Scr_Tip = transform.Find("Tip").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Tip = Scr_Tip.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Tip = RectTrans_Tip.GetComponent<Text>();
        Dro_Mode = transform.Find("Descript").Find("Dro_Mode").GetComponent<Dropdown>();
        Go_Background = transform.Find("Background").gameObject;
        Scr_Background = transform.Find("Background").Find("Scr").GetComponent<ScrollRect>();
        RectTrans_Background = Scr_Background.content.transform.Find("Text").GetComponent<RectTransform>();
        Tex_Background = RectTrans_Background.GetComponent<Text>();
        Dro_Change = transform.Find("Dro_Change").GetComponent<Dropdown>();
        Go_Background.SetActive(false);
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
            (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel] as EncyclopediaPanel).UpdateBossPanel();
        });
        // 设置信息选择
        Dro_Change.ClearOptions();
        dataList = new List<Dropdown.OptionData>();
        {
            dataList.Add(new Dropdown.OptionData("BOSS情报"));
            dataList.Add(new Dropdown.OptionData("背景故事"));
        }
        Dro_Change.AddOptions(dataList);
        Dro_Change.value = 0;
        Dro_Change.onValueChanged.AddListener(delegate {
            if(Dro_Change.value == 0)
            {
                Go_Descript.SetActive(true);
                Go_Tip.SetActive(true);
                Go_Background.SetActive(false);
            }
            else
            {
                Go_Descript.SetActive(false);
                Go_Tip.SetActive(false);
                Go_Background.SetActive(true);
            }
        });
    }

    public void Initial()
    {
        Tex_Name.text = "";
        Tex_Descript.text = "";
        Tex_Tip.text = "";
        // Dro_Change.value = 0;
    }

    /// <summary>
    /// 传入BOSS的种类和变种，然后更新整个面板的信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public void UpdateByParam(int type, int shape)
    {
        // 名称、描述、提示
        Tex_Name.text = BossManager.GetBossName(type, shape);
        if(Dro_Mode.value == 0)
            Tex_Descript.text = BossManager.GetSimpleInfo(type, shape);
        else
            Tex_Descript.text = BossManager.GetDetailedInfo(type, shape);
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Descript.rect.width / Tex_Descript.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Descript.text.Length / countPerRow + 2); // 计算需要多少行
            foreach (var c in Tex_Descript.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            // RectTrans_Descript.sizeDelta = new Vector2(RectTrans_Descript.sizeDelta.x, Tex_Descript.fontSize*rowCount);
            Scr_Descript.content.sizeDelta = new Vector2(Scr_Descript.content.sizeDelta.x, Tex_Descript.fontSize * rowCount);
        }
        Tex_Tip.text = BossManager.GetTips(type, shape);
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Tip.rect.width / Tex_Tip.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Tip.text.Length / countPerRow + 1); // 计算需要多少行
            foreach (var c in Tex_Tip.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            // RectTrans_Tip.sizeDelta = new Vector2(RectTrans_Tip.sizeDelta.x, Tex_Tip.fontSize * rowCount);
            Scr_Tip.content.sizeDelta = new Vector2(Scr_Tip.content.sizeDelta.x, Tex_Tip.fontSize * rowCount);
        }
        // 背景故事
        Tex_Background.text = BossManager.GetBackground(type, shape);
        {
            int countPerRow = Mathf.FloorToInt(RectTrans_Background.rect.width / Tex_Background.fontSize);
            int rowCount = Mathf.CeilToInt((float)Tex_Background.text.Length / countPerRow + 1); // 计算需要多少行
            foreach (var c in Tex_Background.text.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            // RectTrans_Background.sizeDelta = new Vector2(RectTrans_Background.sizeDelta.x, Tex_Background.fontSize * rowCount);
            Scr_Background.content.sizeDelta = new Vector2(Scr_Background.content.sizeDelta.x, Tex_Background.fontSize * rowCount);
        }
    }
}
