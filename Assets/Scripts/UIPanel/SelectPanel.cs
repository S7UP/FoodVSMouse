using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// ѡ���������
/// </summary>
public class SelectPanel : BasePanel
{
    private Chapter[] ChapterArray = new Chapter[4]; // Ŀǰ�������½�
    private int chapterIndex = 0; // �½��±�
    private Chapter mChapter; // ��ǰ�½�ʵ�������ǻ����ϵĴ��ͼ��
    private SelectStageUI mSelectStageUI; // ѡ��UI���
    private Button Btn_LastChapter;
    private Button Btn_NextChapter;
    private Image Mask;
    private Tween MaskTween1;
    private Tween MaskTween2;
    private Button Btn_ReturnToMain;

    private List<BaseStage.StageInfo> currentSceneStageList; // ��ǰ�����Ĺؿ��б�

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
    /// ��ʼ��
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
    /// ��ȡ��ǰ���������йؿ�
    /// </summary>
    public List<BaseStage.StageInfo> GetCurrentSceneStageList()
    {
        return currentSceneStageList;
    }

    /// <summary>
    /// ���뵱ǰ���������йؿ�����ĳ������������ʱ����
    /// </summary>
    public void LoadcurrentSceneStageList()
    {
        if(mChapter!=null && mChapter.selectedSceneIndex != -1)
        {
            // ��ȡ�ó����е����йؿ�
            currentSceneStageList = GameManager.Instance.attributeManager.GetStageInfoListFromScene((int)mChapter.chapterType, mChapter.selectedSceneIndex);
            // �������ѡ��UI����
            if (mSelectStageUI != null)
            {
                mSelectStageUI.UpdateStageList();
            }
        }
    }

    /// <summary>
    /// ����ѡ�ء��ؿ���Ϣ����ӽ���
    /// </summary>
    public void EnterSelectStageUIAndStageInfoUI()
    {
        MaskTween2.Pause();
        MaskTween1.Restart();
        mSelectStageUI.Show();
    }

    /// <summary>
    /// �˳�ѡ�ء��ؿ���Ϣ����ӽ��沢����ѡ�񳡾�
    /// </summary>
    public void ExitSelectStageUIAndStageInfoUI()
    {
        MaskTween1.Pause();
        MaskTween2.Restart();
        mSelectStageUI.Hide();
        // ����ѡ�����
        mSelectStageUI.Initial();
    }

    /// <summary>
    /// ���л��������ؿ�ʱ��Ӧ������һ�����UI
    /// </summary>
    public void UpdateUIByChangeStage()
    {
        //mSelectEquipmentUI.LoadAndFixUI(); // ���ݵ�ǰѡ�йظ�������
        //mStageInfoUI.UpdateInfo(); // �����Ҳ�ؿ���Ϣ
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

    //////////////////////////////////////////////////////������˽�з���////////////////////////////////////////////
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
    /// �����½ڶ���ͬʱ����������ͼ��
    /// </summary>
    private Chapter LoadChapter(ChapterType t)
    {
        // ��ʱ�ø��յ��ĵ�ͼ��������Ҫ��ȡ�����ط��Ľӿ���ȷ�����ص����ĸ��½�
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
    /// ���������˵������ʱ
    /// </summary>
    private void OnReturnToMainClick()
    {
        GameManager.Instance.EnterMainScene();
    }
}
