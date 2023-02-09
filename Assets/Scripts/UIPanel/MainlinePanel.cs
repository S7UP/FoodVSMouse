using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ���߾������
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
    public Func<bool> SuccessReward; // ��ǰѡ�йؿ�ͨ�غ�Ľ���

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
        SuccessReward = null; // ���ó��޽���
    }

    /// <summary>
    /// ���»�����ʾ������
    /// </summary>
    private void UpdateDisplay()
    {
        MainlineManager.Chapter_Static currentChapter = MainlineManager.GetCurrentChapter();

        // ���°�ť�ɵ�����
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
        // ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
        UpdateItemListAndItsDisplay();
        // �����Ѷ���ʾ
        Img_Rank.sprite = RankSpriteArray[PlayerData.GetInstance().GetDifficult()];
    }

    /// <summary>
    /// ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
    /// </summary>
    private void UpdateItemListAndItsDisplay()
    {
        MainlineManager.Chapter_Static currentChapter = MainlineManager.GetCurrentChapter();

        foreach (var item in itemList)
        {
            item.ExecuteRecycle();
        }
        itemList.Clear();

        // ���ݵ�ǰ���½���Ϣ����ȡ�ؿ�����Ҵ浵����
        foreach (var static_info in currentChapter.stageList)
        {
            MainlineManager.StageInfo_Local local_info = MainlineManager.GetLocalStageInfo(static_info.id);
            BaseStage.StageInfo info = BaseStage.Load(static_info.chapterId, static_info.sceneId, static_info.stageId);
            Item_MainlinePanel item = Item_MainlinePanel.GetInstance(delegate { 
                PlayerData.GetInstance().SetCurrentStageInfo(info);
                mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
                // ���ͨ�غ�Ľ���
                SuccessReward = delegate
                {
                    if (local_info.rank == -1)
                    {
                        // ��ͨ
                        float exp = MainlineManager.GetFirstPassExp(static_info.id);
                        GameNormalPanel panel = mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel] as GameNormalPanel;
                        if (panel != null)
                            panel.SetExpTips("��ϲ���һ��ͨ�����أ�����" + exp + "�㾭��ֵ��");
                        PlayerData.GetInstance().AddExp(exp);
                        // ����ͨ����ߵ��Ѷȼ�¼
                        local_info.rank = Mathf.Max(PlayerData.GetInstance().GetDifficult(), local_info.rank);
                        MainlineManager.Save();
                        return true;
                    }
                    else
                    {
                        // ����ͨ����ߵ��Ѷȼ�¼
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
    /// �������һ�����Խ��Ľ��棨һ���Ǹ�����ҵ�ǰ���ȶ�����
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
    /// ���رո���尴ť�����ʱ�����ⲿ��ֵ����ť��
    /// </summary>
    public void OnClickExit()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].ExitPanel();
    }

    /// <summary>
    /// �������һ�µİ�ťʱ�����ⲿ��ֵ����ť��
    /// </summary>
    public void OnClickBtn_Last()
    {
        MainlineManager.currentIndex--;
        UpdateDisplay();
    }

    /// <summary>
    /// �������һ�µİ�ťʱ�����ⲿ��ֵ����ť��
    /// </summary>
    public void OnClickBtn_Next()
    {
        MainlineManager.currentIndex++;
        UpdateDisplay();
    }

    /// <summary>
    /// �������ѶȰ�ť�����ʱ�����ⲿ��ֵ��
    /// </summary>
    public void OnClickChangeRank()
    {
        mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].EnterPanel();
    }
}
