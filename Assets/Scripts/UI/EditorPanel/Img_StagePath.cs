using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑面板上的关卡路径面板
/// </summary>
public class Img_StagePath : MonoBehaviour
{
    private EditorPanel mEditorPanel;

    private Dropdown Dro_StageType;

    private GameObject Emp_ChapterArea;
    private Dropdown Dro_Chapter;
    private Dropdown Dro_Scene;
    private Dropdown Dro_Stage;

    private int chapterIndex;
    private int sceneIndex;
    private int stageIndex;

    private void Awake()
    {
        Dro_StageType = transform.Find("Emp_StageType").Find("Dropdown").GetComponent<Dropdown>();
        Emp_ChapterArea = transform.Find("Emp_ChapterArea").gameObject;
        Dro_Chapter = Emp_ChapterArea.transform.Find("Emp_Chapter").Find("Dropdown").GetComponent<Dropdown>();
        Dro_Scene = Emp_ChapterArea.transform.Find("Emp_Scene").Find("Dropdown").GetComponent<Dropdown>();
        Dro_Stage = Emp_ChapterArea.transform.Find("Emp_Stage").Find("Dropdown").GetComponent<Dropdown>();

        Dro_Chapter.onValueChanged.AddListener(delegate { chapterIndex = Dro_Chapter.value; sceneIndex = 0; stageIndex = 0; UpdateSceneDropDown(); UpdateStageDropDown(); LoadStage(); mEditorPanel.ChangeBackground(chapterIndex, sceneIndex); });
        Dro_Scene.onValueChanged.AddListener(delegate { sceneIndex = Dro_Scene.value; stageIndex = 0; UpdateStageDropDown(); LoadStage(); mEditorPanel.ChangeBackground(chapterIndex, sceneIndex); });
        UpdateStageDropDown(); // 选设置一次
        Dro_Stage.onValueChanged.AddListener(delegate {
            if (Dro_Stage.value > 0)
            {
                // 加载对应关卡
                stageIndex = Dro_Stage.value - 1; 
            }
            else
            {
                // 添加新关卡
                stageIndex = GameManager.Instance.attributeManager.GetStageInfoListFromScene(chapterIndex, sceneIndex).Count;
                BaseStage.Save(new BaseStage.StageInfo()
                {
                    name = "新建关卡" + stageIndex,
                    chapterIndex = chapterIndex,
                    sceneIndex = sceneIndex,
                    stageIndex = stageIndex,
                    apartList = new List<List<int>>() { new List<int>(){ 0, 1, 2, 3, 4, 5, 6} },
                    waveIndexList = new List<int>(),
                    roundInfoList = new List<BaseRound.RoundInfo>(),
                    availableCardInfoList = new List<AvailableCardInfo>()
                });;
                // 强制从本地读取一遍这个关卡到内存中
                GameManager.Instance.attributeManager.GetStageInfo(chapterIndex, sceneIndex, stageIndex);
                // 更新一下当前下拉列表（要把新建的关卡搞进来）
                UpdateStageDropDown();
            }
            LoadStage();
        });
    }

    public void Initial()
    {
        Dro_StageType.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        dataList.Add(new Dropdown.OptionData("章节"));
        // dataList.Add(new Dropdown.OptionData("未分类"));
        Dro_StageType.AddOptions(dataList);
        Dro_StageType.value = 0;

        InitChapterArea();
    }



    /// <summary>
    /// 设置宿主Panel，由宿主调用
    /// </summary>
    public void SetEditorPanel(EditorPanel panel)
    {
        mEditorPanel = panel;
    }

    public int GetChapterIndex()
    {
        return chapterIndex;
    }

    public int GetSceneIndex()
    {
        return sceneIndex;
    }

    public int GetStageIndex()
    {
        return stageIndex;
    }

    public void SetStageIndex(int index)
    {
        stageIndex = index;
    }

    /// <summary>
    /// 更新关卡下拉列表项
    /// </summary>
    public void UpdateStageDropDown()
    {
        Dro_Stage.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        dataList.Add(new Dropdown.OptionData("添加新关卡"));
        foreach (var item in GameManager.Instance.attributeManager.GetStageInfoListFromScene(chapterIndex, sceneIndex))
        {
            dataList.Add(new Dropdown.OptionData(item.name));
        }
        Dro_Stage.AddOptions(dataList);
        Dro_Stage.value = stageIndex + 1;
    }

    ////////////////////////////////////////////////以下为私有方法//////////////////////////////////////////////////////

    /// <summary>
    /// 当选定章节关卡类型时，激活对应GameObject并且初始化
    /// </summary>
    private void InitChapterArea()
    {
        chapterIndex = 0;
        sceneIndex = 0;
        stageIndex = 0;

        Dro_Chapter.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        foreach (var item in ChapterManager.GetChapterNameList())
        {
            dataList.Add(new Dropdown.OptionData(item));
        }
        
        // dataList.Add(new Dropdown.OptionData("未分类"));
        Dro_Chapter.AddOptions(dataList);
        Dro_Chapter.value = chapterIndex;
        UpdateSceneDropDown();
        UpdateStageDropDown();
    }

    /// <summary>
    /// 更新场景选择下拉列表项
    /// </summary>
    private void UpdateSceneDropDown()
    {
        Dro_Scene.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        foreach (var item in ChapterManager.GetSceneNameList((ChapterNameTypeMap)chapterIndex))
        {
            dataList.Add(new Dropdown.OptionData(item));
        }
        Dro_Scene.AddOptions(dataList);
        Dro_Scene.value = sceneIndex;
    }

    /// <summary>
    /// 加载关卡相关内容
    /// </summary>
    private void LoadStage()
    {
        BaseStage.StageInfo info = GameManager.Instance.attributeManager.GetStageInfo(chapterIndex, sceneIndex, stageIndex);
        mEditorPanel.LoadStage(info);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
