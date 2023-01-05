using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static MainlinePanel;
/// <summary>
/// ���߾������
/// </summary>
public class MainlinePanel : BasePanel
{
    private const string path = "Mainline/";
    private const string localPath = "Mainline";
    private bool isLoad; // �Ƿ���ع��ˣ���ֹ�ظ����أ�
    
    /// <summary>
    /// ���������½ڱ���̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static_List
    {
        public List<Chapter_Static> chapterList;
    }

    /// <summary>
    /// �½���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public int mapId;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// �ؿ���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // ����Ψһid
        public int chapterId;
        public int sceneId;
        public int stageId;
        public string unlockCondition; // ������������������˵����
    }

    /// <summary>
    /// �ؿ���Ϣ������Ҵ浵��أ���̬���ݣ�
    /// </summary>
    [System.Serializable]
    public class StageInfo_Local
    {
        public string id; // ����Ψһid�������澲̬���ݵ�ӳ�䣩
        public bool isUnlocked; // �Ƿ����
        public int rank = -1; // ͨ��������Ѷ�(-1����δͨ����

        public StageInfo_Local(string id)
        {
            this.id = id;
        }
    }

    /// <summary>
    /// ����ڱ��ص����ߴ浵
    /// </summary>
    [System.Serializable]
    public class MainlineSaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    private static Chapter_Static_List chapterList;
    private int currentIndex; // ��ǰѡ�е��½��±꣨�����������±꣩
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
    /// ����ȫ����̬�����߹ؿ���Ϣ
    /// </summary>
    private void LoadAll()
    {
        if (!isLoad)
        {
            // ���ؾ�̬���߹ؿ�
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                chapterList = result;
            }
            // ����Ҵ浵���عؿ����
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
    /// �ӱ��ش浵��ȡ�ؿ���������
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
    /// ����һ�α��صĴ浵
    /// </summary>
    private void SaveOnLocal()
    {
        JsonManager.SaveOnLocal<MainlineSaveInLocal>(mainlineSaveInLocal, localPath);
    }

    /// <summary>
    /// ���»�����ʾ������
    /// </summary>
    private void UpdateDisplay()
    {
        Chapter_Static currentChapter = GetCurrentChapter();

        // ���°�ť�ɵ�����
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
        Chapter_Static currentChapter = GetCurrentChapter();

        foreach (var item in itemList)
        {
            item.ExecuteRecycle();
        }
        itemList.Clear();

        // ���ݵ�ǰ���½���Ϣ����ȡ�ؿ�����Ҵ浵����
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
    /// ��ȡ��ǰ�½���Ϣ
    /// </summary>
    /// <returns></returns>
    private Chapter_Static GetCurrentChapter()
    {
        return chapterList.chapterList[currentIndex];
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
        currentIndex--;
        UpdateDisplay();
    }

    /// <summary>
    /// �������һ�µİ�ťʱ�����ⲿ��ֵ����ť��
    /// </summary>
    public void OnClickBtn_Next()
    {
        currentIndex++;
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
