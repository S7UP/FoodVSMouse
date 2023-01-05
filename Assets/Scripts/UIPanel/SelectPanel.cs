using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 选择配置面版
/// </summary>
public class SelectPanel : BasePanel
{
    private Chapter[] ChapterArray = new Chapter[4]; // 目前仅三个章节
    private int chapterIndex = 0; // 章节下标
    private Chapter mChapter; // 当前章节实例（就是画面上的大地图）
    private SelectStageUI mSelectStageUI; // 选关UI面板
    private Button Btn_LastChapter;
    private Button Btn_NextChapter;
    private Image Mask;
    private Tween MaskTween1;
    private Tween MaskTween2;
    private Button Btn_ReturnToMain;

    private List<BaseStage.StageInfo> currentSceneStageList; // 当前场景的关卡列表

    protected override void Awake()
    {
        base.Awake();
        mSelectStageUI = transform.Find("SelectStageUI").GetComponent<SelectStageUI>();
        mSelectStageUI.SetSelectPanel(this);
        Btn_LastChapter = transform.Find("Btn_LastChapter").GetComponent<Button>();
        Btn_NextChapter = transform.Find("Btn_NextChapter").GetComponent<Button>();
        Btn_ReturnToMain = transform.Find("Btn_ReturnToMain").GetComponent<Button>();
        Btn_ReturnToMain.onClick.AddListener(OnReturnToMainClick);
        Mask = transform.Find("Mask").GetComponent<Image>();
        //Mask.gameObject.SetActive(false);
        MaskTween1 = Mask.DOColor(new Color(0, 0, 0, 0.5f), 1);
        MaskTween1.OnPlay(delegate { Mask.raycastTarget = true; });
        MaskTween1.Pause();
        MaskTween1.SetAutoKill(false);

        MaskTween2 = Mask.DOColor(new Color(0, 0, 0, 0), 1);
        MaskTween2.OnPlay(delegate { Mask.raycastTarget = false; });
        MaskTween2.Pause();
        MaskTween2.SetAutoKill(false);

        Mask.raycastTarget = false;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void InitPanel()
    {
        UpdateChapterButtonState();
        ChangeCurrentChapter();
        mSelectStageUI.Hide();
        //mStageInfoUI.Hide();
        //mSelectEquipmentUI.Hide();
        mChapter.gameObject.SetActive(true);

        mChapter.Initial();
        mSelectStageUI.Initial();
        //mStageInfoUI.Initial();
        //mSelectEquipmentUI.Initial();

        currentSceneStageList = null;
        MaskTween2.Restart();
    }

    /// <summary>
    /// 获取当前场景的所有关卡
    /// </summary>
    public List<BaseStage.StageInfo> GetCurrentSceneStageList()
    {
        return currentSceneStageList;
    }

    /// <summary>
    /// 载入当前场景的所有关卡，由某个场景被点中时调用
    /// </summary>
    public void LoadcurrentSceneStageList()
    {
        if(mChapter!=null && mChapter.selectedSceneIndex != -1)
        {
            // 获取该场景中的所有关卡
            currentSceneStageList = GameManager.Instance.attributeManager.GetStageInfoListFromScene((int)mChapter.chapterType, mChapter.selectedSceneIndex);
            // 更新左侧选关UI内容
            if (mSelectStageUI != null)
            {
                mSelectStageUI.UpdateStageList();
            }
        }
    }

    /// <summary>
    /// 进入选关、关卡信息浏览子界面
    /// </summary>
    public void EnterSelectStageUIAndStageInfoUI()
    {
        MaskTween2.Pause();
        MaskTween1.Restart();
        mSelectStageUI.Show();
    }

    /// <summary>
    /// 退出选关、关卡信息浏览子界面并重新选择场景
    /// </summary>
    public void ExitSelectStageUIAndStageInfoUI()
    {
        MaskTween1.Pause();
        MaskTween2.Restart();
        mSelectStageUI.Hide();
        // 重置选关面板
        mSelectStageUI.Initial();
    }

    /// <summary>
    /// 当切换到其他关卡时，应当更新一下相关UI
    /// </summary>
    public void UpdateUIByChangeStage()
    {
        //mSelectEquipmentUI.LoadAndFixUI(); // 根据当前选中关更新配置
        //mStageInfoUI.UpdateInfo(); // 更新右侧关卡信息
    }

    public void EnterLastChapter()
    {
        chapterIndex--;
        UpdateChapterButtonState();
        ChangeCurrentChapter();
    }

    public void EnterNextChapter()
    {
        chapterIndex++;
        UpdateChapterButtonState();
        ChangeCurrentChapter();
    }

    private void UpdateChapterButtonState()
    {
        if (chapterIndex >= ChapterArray.Length - 1)
            Btn_NextChapter.interactable = false;
        else
            Btn_NextChapter.interactable = true;
        if (chapterIndex <= 0)
            Btn_LastChapter.interactable = false;
        else
            Btn_LastChapter.interactable = true;
    }

    //////////////////////////////////////////////////////以下是私有方法////////////////////////////////////////////
    private void ChangeCurrentChapter()
    {
        if (mChapter != null)
            mChapter.gameObject.SetActive(false);
        mChapter = ChapterArray[chapterIndex];
        if (mChapter == null)
        {
            mChapter = LoadChapter((ChapterType)chapterIndex);
            ChapterArray[chapterIndex] = mChapter;
        }
        mChapter.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 加载章节对象（同时构建场景地图）
    /// </summary>
    private Chapter LoadChapter(ChapterType t)
    {
        // 暂时用浮空岛的地图，后续需要读取其它地方的接口来确定加载的是哪个章节
        Chapter c = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/Chapter/" + ((int)t)).GetComponent<Chapter>();
        Vector2 size = c.transform.localScale;
        c.transform.SetParent(transform);
        c.transform.SetAsFirstSibling();
        c.transform.localScale = size;
        c.SetInfo(this, t);
        c.gameObject.SetActive(false);
        return c;
    }

    /// <summary>
    /// 当返回主菜单被点击时
    /// </summary>
    private void OnReturnToMainClick()
    {
        GameManager.Instance.EnterMainScene();
    }
}
