using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 主线剧情面板
/// </summary>
public class MainlinePanel : BasePanel
{
    private Transform Trans_UI;
    private Image Img_Map;
    private Text Tex_Chapter;
    private Image Img_Rank;
    private Transform Trans_StageList;
    private List<Item_MainlinePanel> itemList = new List<Item_MainlinePanel>();
    private Button Btn_Last;
    private Button Btn_Next;
    private Sprite[] RankSpriteArray;
    public Func<bool> SuccessReward; // 当前选中关卡通关后的奖励

    protected override void Awake()
    {
        base.Awake();
        Trans_UI = transform.Find("UI");
        Img_Map = Trans_UI.Find("Img_Map").GetComponent<Image>();
        Tex_Chapter = Trans_UI.Find("Img_Chapter").Find("Tex_Chapter").GetComponent<Text>();
        Img_Rank = Trans_UI.Find("Img_Rank").Find("Rank").GetComponent<Image>();
        Trans_StageList = Trans_UI.Find("StageList");
        Btn_Last = Trans_UI.Find("Img_Chapter").Find("Btn_Last").GetComponent<Button>();
        Btn_Next = Trans_UI.Find("Img_Chapter").Find("Btn_Next").GetComponent<Button>();
        RankSpriteArray = new Sprite[4] {
            GameManager.Instance.GetSprite("UI/Difficulty/Easy"),
            GameManager.Instance.GetSprite("UI/Difficulty/Normal"),
            GameManager.Instance.GetSprite("UI/Difficulty/Hard"),
            GameManager.Instance.GetSprite("UI/Difficulty/Lunatic")
        };
    }

    public override void InitPanel()
    {
        base.InitPanel();
        EnterLastAvailablePage();
        UpdateDisplay();
        SuccessReward = null; // 重置成无奖励
    }

    /// <summary>
    /// 更新画面显示的内容
    /// </summary>
    private void UpdateDisplay()
    {
        MainlineManager.Chapter_Static currentChapter = MainlineManager.GetCurrentChapter();

        // 更新按钮可点击情况
        if (MainlineManager.currentIndex <= 0)
            Btn_Last.interactable = false;
        else
            Btn_Last.interactable = true;
        if (MainlineManager.currentIndex >= MainlineManager.ChapterList.chapterList.Count-1)
            Btn_Next.interactable = false;
        else
            Btn_Next.interactable = true;

        Img_Map.sprite = GameManager.Instance.GetSprite("UI/MainlinePanel/Maps/"+currentChapter.mapId);
        Tex_Chapter.text = currentChapter.name;
        // 更新关卡选项表的信息以及其显示部分
        UpdateItemListAndItsDisplay();
        // 更新难度显示
        Img_Rank.sprite = RankSpriteArray[PlayerData.GetInstance().GetDifficult()];
    }

    /// <summary>
    /// 更新关卡选项表的信息以及其显示部分
    /// </summary>
    private void UpdateItemListAndItsDisplay()
    {
        MainlineManager.Chapter_Static currentChapter = MainlineManager.GetCurrentChapter();

        foreach (var item in itemList)
        {
            item.ExecuteRecycle();
        }
        itemList.Clear();

        // 根据当前的章节信息来读取关卡和玩家存档内容
        foreach (var static_info in currentChapter.stageList)
        {
            MainlineManager.StageInfo_Local local_info = MainlineManager.GetLocalStageInfo(static_info.id);
            BaseStage.StageInfo info = BaseStage.Load(static_info.chapterId, static_info.sceneId, static_info.stageId);
            Item_MainlinePanel item = Item_MainlinePanel.GetInstance(delegate { 
                PlayerData.GetInstance().SetCurrentStageInfo(info);
                mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
                // 添加通关后的奖励
                SuccessReward = delegate
                {
                    if (local_info.rank == -1)
                    {
                        // 首通
                        float exp = MainlineManager.GetFirstPassExp(static_info.id);
                        GameNormalPanel panel = mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel] as GameNormalPanel;
                        if (panel != null)
                            panel.SetExpTips("恭喜你第一次通过本关！奖励" + exp + "点经验值！");
                        PlayerData.GetInstance().AddExp(exp);
                        // 更新通过最高的难度记录
                        local_info.rank = Mathf.Max(PlayerData.GetInstance().GetDifficult(), local_info.rank);
                        MainlineManager.Save();
                        return true;
                    }
                    else
                    {
                        // 更新通过最高的难度记录
                        local_info.rank = Mathf.Max(PlayerData.GetInstance().GetDifficult(), local_info.rank);
                        MainlineManager.Save();
                        return false;
                    }
                };
            },
                local_info.rank, !local_info.isUnlocked, static_info.id + " " + info.name, static_info.unlockCondition);
            itemList.Add(item);
            item.transform.SetParent(Trans_StageList);
            item.transform.localScale = Vector2.one;
        }
    }


    /// <summary>
    /// 进入最后一个可以进的界面（一般是根据玩家当前进度而定）
    /// </summary>
    public void EnterLastAvailablePage()
    {
        int level = PlayerData.GetInstance().GetLevel();
        if (level <= 7)
            MainlineManager.currentIndex = 0;
        else if(level <= 14)
            MainlineManager.currentIndex = 1;
    }

    /// <summary>
    /// 当关闭该面板按钮被点击时（由外部赋值给按钮）
    /// </summary>
    public void OnClickExit()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].ExitPanel();
    }

    /// <summary>
    /// 当点击上一章的按钮时（由外部赋值给按钮）
    /// </summary>
    public void OnClickBtn_Last()
    {
        MainlineManager.currentIndex--;
        UpdateDisplay();
    }

    /// <summary>
    /// 当点击下一章的按钮时（由外部赋值给按钮）
    /// </summary>
    public void OnClickBtn_Next()
    {
        MainlineManager.currentIndex++;
        UpdateDisplay();
    }

    /// <summary>
    /// 当更换难度按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickChangeRank()
    {
        mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].EnterPanel();
    }
}
