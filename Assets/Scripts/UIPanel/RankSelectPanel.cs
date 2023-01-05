using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 难度选择面板
/// </summary>
public class RankSelectPanel : BasePanel
{
    private Sprite[] normalSprArray;
    private Sprite[] highLightSprArray;

    private Image Img_Easy;
    private Image Img_Normal;
    private Image Img_Hard;
    private Image Img_Lunatic;
    private Image[] ImgArray;

    private Button Btn_Easy;
    private Button Btn_Normal;
    private Button Btn_Hard;
    private Button Btn_Lunatic;
    private Button[] BtnArray;

    protected override void Awake()
    {
        base.Awake();
        Img_Easy = transform.Find("Background").Find("Btn_Easy").GetComponent<Image>();
        Img_Normal = transform.Find("Background").Find("Btn_Normal").GetComponent<Image>();
        Img_Hard = transform.Find("Background").Find("Btn_Hard").GetComponent<Image>();
        Img_Lunatic = transform.Find("Background").Find("Btn_Lunatic").GetComponent<Image>();
        ImgArray = new Image[4] { Img_Easy, Img_Normal, Img_Hard, Img_Lunatic };
        normalSprArray = new Sprite[4] {
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Easy"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Normal"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Hard"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Lunatic")
        };
        highLightSprArray = new Sprite[4] {
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Easy_Hightlight"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Normal_Hightlight"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Hard_Hightlight"),
            GameManager.Instance.GetSprite("UI/RankSelectPanel/Lunatic_Hightlight")
        };

        Btn_Easy = Img_Easy.GetComponent<Button>();
        Btn_Normal = Img_Normal.GetComponent<Button>();
        Btn_Hard = Img_Hard.GetComponent<Button>();
        Btn_Lunatic = Img_Lunatic.GetComponent<Button>();
        BtnArray = new Button[4] { Btn_Easy, Btn_Normal, Btn_Hard, Btn_Lunatic };
    }

    private void UpdateDisplay()
    {
        int diff = PlayerData.GetInstance().GetDifficult();
        for (int i = 0; i < ImgArray.Length; i++)
        {
            if(i == diff)
            {
                ImgArray[i].sprite = highLightSprArray[i];
                BtnArray[i].interactable = false;
            }
            else
            {
                ImgArray[i].sprite = normalSprArray[i];
                BtnArray[i].interactable = true;
            }
        }
    }

    /// <summary>
    /// 当难度标签按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickDiffItem(int diff)
    {
        PlayerData.GetInstance().SetDifficult(diff);
        PlayerData.GetInstance().Save();
        UpdateDisplay();

        if (mUIFacade.currentScenePanelDict.ContainsKey(StringManager.MainlinePanel))
        {
            mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].InitPanel(); // 刷新一下主线面板以更新难度
        }
    }

    /// <summary>
    /// 当关闭按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickExit()
    {
        mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].ExitPanel();
    }

    public override void EnterPanel()
    {
        base.EnterPanel();
        UpdateDisplay();
    }

    public override void InitPanel()
    {
        base.InitPanel();
        UpdateDisplay();
    }
}
