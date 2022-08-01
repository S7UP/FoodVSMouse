using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �ؿ���Ϣ����
/// </summary>
public class StageInfoUI : MonoBehaviour
{
    private SelectPanel mSelectPanel;
    private RectTransform rectTransform;
    private Tweener ShowTween;
    private GameObject Emp_StageInfo;
    private GameObject Emp_EnemyInfo;
    private GameObject Scr_AddInfo;
    private Sprite btnSprite0; // �Ϸ���ť��
    private Sprite btnSprite1; // �Ϸ���ť��
    private Image[] imgList;
    private Image Img_Map;
    private Text Tex_MapName;
    private Text Tex_StageName;
    private Text Tex_TimeLimit;
    private Text Tex_Background;
    private Text Tex_Illustrate;
    private Text Tex_AddInfo;

    private void Awake()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        ShowTween = rectTransform.DOAnchorPosX(-610.24f, 0.5f);
        ShowTween.SetEase(Ease.InCubic); // ��������
        ShowTween.Pause();
        ShowTween.SetAutoKill(false);

        Emp_StageInfo = transform.Find("Img_Center").Find("Emp_Container").Find("Emp_StageInfo").gameObject;
        Emp_EnemyInfo = transform.Find("Img_Center").Find("Emp_Container").Find("Emp_EnemyInfo").gameObject;
        Scr_AddInfo = transform.Find("Img_Center").Find("Emp_Container").Find("Scr_AddInfo").gameObject;

        btnSprite0 = GameManager.Instance.GetSprite("UI/SelectPanel/36");
        btnSprite1 = GameManager.Instance.GetSprite("UI/SelectPanel/33");

        imgList = new Image[3];
        imgList[0] = transform.Find("Img_Center").Find("Emp_BtnList").Find("Button").GetComponent<Image>();
        imgList[1] = transform.Find("Img_Center").Find("Emp_BtnList").Find("Button1").GetComponent<Image>();
        imgList[2] = transform.Find("Img_Center").Find("Emp_BtnList").Find("Button2").GetComponent<Image>();

        Img_Map = Emp_StageInfo.transform.Find("Scr_Map").Find("Viewport").Find("Content").Find("Img_Map").GetComponent<Image>();
        Tex_MapName = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_MapName").GetComponent<Text>();
        Tex_StageName = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_StageName").GetComponent<Text>();
        Tex_TimeLimit = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_TimeLimit").GetComponent<Text>();
        Tex_Background = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_Background").GetComponent<Text>();
        Tex_Illustrate = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_Illustrate").GetComponent<Text>();
        Tex_AddInfo = Scr_AddInfo.transform.Find("Viewport").Find("Content").Find("Text").GetComponent<Text>();
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Initial()
    {
        // ����ʾ����
        Emp_StageInfo.gameObject.SetActive(false);
        Emp_EnemyInfo.gameObject.SetActive(false);
        Scr_AddInfo.gameObject.SetActive(false);

        // ��ťȫ���ð�
        foreach (var img in imgList)
        {
            img.sprite = btnSprite0;
        }

        // Ĭ����ʾ��һ��
        Emp_StageInfo.gameObject.SetActive(true);
        imgList[0].sprite = btnSprite1;
    }

    /// <summary>
    /// ������ʾ��Ϣ
    /// </summary>
    public void UpdateInfo()
    {
        // �ؿ��鱨
        BaseStage.StageInfo info = mSelectPanel.GetCurrentSelectedStageInfo();
        Img_Map.sprite = GameManager.Instance.GetSprite("Chapter/"+ info.chapterIndex+"/"+ info.sceneIndex+"/0");
        Tex_MapName.text = "��ͼ���ƣ�"+ChapterManager.GetSceneName((ChapterNameTypeMap)info.chapterIndex, info.sceneIndex);
        Tex_StageName.text = "�ؿ����ƣ�" + info.name;
        if (info.isEnableTimeLimit)
        {
            Tex_TimeLimit.gameObject.SetActive(true);
            Tex_TimeLimit.text = "ʱ�����ƣ�" + info.totalSeconds + "��";
        }
        else
        {
            Tex_TimeLimit.gameObject.SetActive(false);
        }
        Tex_Background.text = info.background;
        Tex_Illustrate.text = info.illustrate;

        // ����˵��
        Tex_AddInfo.text = info.illustrate;
    }

    /// <summary>
    /// ��ʾ��ǰUI
    /// </summary>
    public void Show()
    {
        ShowTween.PlayForward();
    }

    /// <summary>
    /// ���ص�ǰUI
    /// </summary>
    public void Hide()
    {
        ShowTween.PlayBackwards();
    }

    public void SetSelectPanel(SelectPanel panel)
    {
        mSelectPanel = panel;
    }

    /////////////////////////////////////���·����Ǳ�¶����ť�õ�/////////////////////////////////
    
    /// <summary>
    /// �ؿ��鱨��ť���
    /// </summary>
    public void OnStageInfoClick()
    {
        imgList[0].sprite = btnSprite1;
        imgList[1].sprite = btnSprite0;
        imgList[2].sprite = btnSprite0;
        Emp_StageInfo.gameObject.SetActive(true);
        Emp_EnemyInfo.gameObject.SetActive(false);
        Scr_AddInfo.gameObject.SetActive(false);
    }

    /// <summary>
    /// �б��鱨��ť���
    /// </summary>
    public void OnEnemyInfoClick()
    {
        imgList[0].sprite = btnSprite0;
        imgList[1].sprite = btnSprite1;
        imgList[2].sprite = btnSprite0;
        Emp_StageInfo.gameObject.SetActive(false);
        Emp_EnemyInfo.gameObject.SetActive(true);
        Scr_AddInfo.gameObject.SetActive(false);
    }

    /// <summary>
    /// ����˵����ť���
    /// </summary>
    public void OnAddInfoClick()
    {
        imgList[0].sprite = btnSprite0;
        imgList[1].sprite = btnSprite0;
        imgList[2].sprite = btnSprite1;
        Emp_StageInfo.gameObject.SetActive(false);
        Emp_EnemyInfo.gameObject.SetActive(false);
        Scr_AddInfo.gameObject.SetActive(true);
    }
}
