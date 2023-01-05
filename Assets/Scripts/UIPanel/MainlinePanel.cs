using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static MainlinePanel;
/// <summary>
/// 主线剧情面板
/// </summary>
public class MainlinePanel : BasePanel
{
    private const string path = "Mainline/";
    private const string localPath = "Mainline";
    private bool isLoad; // 是否加载过了（防止重复加载）
    
    /// <summary>
    /// 所有主线章节表（静态数据）
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static_List
    {
        public List<Chapter_Static> chapterList;
    }

    /// <summary>
    /// 章节信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public int mapId;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// 关卡信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // 自身唯一id
        public int chapterId;
        public int sceneId;
        public int stageId;
        public string unlockCondition; // 解锁的条件（仅文字说明）
    }

    /// <summary>
    /// 关卡信息（与玩家存档相关，动态数据）
    /// </summary>
    [System.Serializable]
    public class StageInfo_Local
    {
        public string id; // 自身唯一id（是上面静态数据的映射）
        public bool isUnlocked; // 是否解锁
        public int rank = -1; // 通过的最高难度(-1代表未通过）

        public StageInfo_Local(string id)
        {
            this.id = id;
        }
    }

    /// <summary>
    /// 存放在本地的主线存档
    /// </summary>
    [System.Serializable]
    public class MainlineSaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    private static Chapter_Static_List chapterList;
    private int currentIndex; // 当前选中的章节下标（就是上面标的下标）
    private static MainlineSaveInLocal mainlineSaveInLocal;

    private Transform Trans_UI;
    private Image Img_Map;
    private Text Tex_Chapter;
    private Image Img_Rank;
    private Transform Trans_StageList;
    private List<Item_MainlinePanel> itemList = new List<Item_MainlinePanel>();
    private Button Btn_Last;
    private Button Btn_Next;
    private Sprite[] RankSpriteArray;

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
        LoadAll();
        UpdateDisplay();
    }

    /// <summary>
    /// 加载全部静态的主线关卡信息
    /// </summary>
    private void LoadAll()
    {
        if (!isLoad)
        {
            // 加载静态主线关卡
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                chapterList = result;
            }
            // 从玩家存档加载关卡情况
            {
                if(!JsonManager.TryLoadFromLocal(localPath, out mainlineSaveInLocal))
                {
                    mainlineSaveInLocal = new MainlineSaveInLocal();
                    SaveOnLocal();
                }
            }
            isLoad = true;
        }
    }

    /// <summary>
    /// 从本地存档获取关卡的完成情况
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private StageInfo_Local GetLocalStageInfo(string id)
    {
        if (!mainlineSaveInLocal.localStageInfoDict.ContainsKey(id))
        {
            mainlineSaveInLocal.localStageInfoDict.Add(id, new StageInfo_Local(id));
            SaveOnLocal();
        }
        return mainlineSaveInLocal.localStageInfoDict[id];
    }

    /// <summary>
    /// 保存一次本地的存档
    /// </summary>
    private void SaveOnLocal()
    {
        JsonManager.SaveOnLocal<MainlineSaveInLocal>(mainlineSaveInLocal, localPath);
    }

    /// <summary>
    /// 更新画面显示的内容
    /// </summary>
    private void UpdateDisplay()
    {
        Chapter_Static currentChapter = GetCurrentChapter();

        // 更新按钮可点击情况
        if (currentIndex <= 0)
            Btn_Last.interactable = false;
        else
            Btn_Last.interactable = true;
        if (currentIndex >= chapterList.chapterList.Count-1)
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
        Chapter_Static currentChapter = GetCurrentChapter();

        foreach (var item in itemList)
        {
            item.ExecuteRecycle();
        }
        itemList.Clear();

        // 根据当前的章节信息来读取关卡和玩家存档内容
        foreach (var static_info in currentChapter.stageList)
        {
            StageInfo_Local local_info = GetLocalStageInfo(static_info.id);
            BaseStage.StageInfo info = BaseStage.Load(static_info.chapterId, static_info.sceneId, static_info.stageId);
            Item_MainlinePanel item = Item_MainlinePanel.GetInstance(delegate { 
                PlayerData.GetInstance().SetCurrentStageInfo(info);
                mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
            },
                local_info.rank, !local_info.isUnlocked, static_info.id + " " + info.name, static_info.unlockCondition);
            itemList.Add(item);
            item.transform.SetParent(Trans_StageList);
            item.transform.localScale = Vector2.one;
        }
    }

    /// <summary>
    /// 获取当前章节信息
    /// </summary>
    /// <returns></returns>
    private Chapter_Static GetCurrentChapter()
    {
        return chapterList.chapterList[currentIndex];
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
        currentIndex--;
        UpdateDisplay();
    }

    /// <summary>
    /// 当点击下一章的按钮时（由外部赋值给按钮）
    /// </summary>
    public void OnClickBtn_Next()
    {
        currentIndex++;
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
