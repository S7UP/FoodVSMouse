using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 关卡信息界面
/// </summary>
public class StageInfoUI : MonoBehaviour
{
    private SelectPanel mSelectPanel;
    private RectTransform rectTransform;
    private Tweener ShowTween;
    private GameObject Emp_StageInfo;
    private GameObject Emp_EnemyInfo;
    private GameObject Scr_AddInfo;
    private Sprite btnSprite0; // 上方按钮暗
    private Sprite btnSprite1; // 上方按钮亮
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
        ShowTween.SetEase(Ease.InCubic); // 由慢到快
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
    /// 初始化
    /// </summary>
    public void Initial()
    {
        // 不显示画面
        Emp_StageInfo.gameObject.SetActive(false);
        Emp_EnemyInfo.gameObject.SetActive(false);
        Scr_AddInfo.gameObject.SetActive(false);

        // 按钮全部置暗
        foreach (var img in imgList)
        {
            img.sprite = btnSprite0;
        }

        // 默认显示第一个
        Emp_StageInfo.gameObject.SetActive(true);
        imgList[0].sprite = btnSprite1;
    }

    /// <summary>
    /// 更新显示信息
    /// </summary>
    public void UpdateInfo()
    {
        // 关卡情报
        BaseStage.StageInfo info = mSelectPanel.GetCurrentSelectedStageInfo();
        Img_Map.sprite = GameManager.Instance.GetSprite("Chapter/"+ info.chapterIndex+"/"+ info.sceneIndex+"/0");
        Tex_MapName.text = "地图名称："+ChapterManager.GetSceneName((ChapterNameTypeMap)info.chapterIndex, info.sceneIndex);
        Tex_StageName.text = "关卡名称：" + info.name;
        if (info.isEnableTimeLimit)
        {
            Tex_TimeLimit.gameObject.SetActive(true);
            Tex_TimeLimit.text = "时间限制：" + info.totalSeconds + "秒";
        }
        else
        {
            Tex_TimeLimit.gameObject.SetActive(false);
        }
        Tex_Background.text = info.background;
        Tex_Illustrate.text = info.illustrate;

        // 附加说明
        Tex_AddInfo.text = info.illustrate;
    }

    /// <summary>
    /// 显示当前UI
    /// </summary>
    public void Show()
    {
        ShowTween.PlayForward();
    }

    /// <summary>
    /// 隐藏当前UI
    /// </summary>
    public void Hide()
    {
        ShowTween.PlayBackwards();
    }

    public void SetSelectPanel(SelectPanel panel)
    {
        mSelectPanel = panel;
    }

    /////////////////////////////////////以下方法是暴露给按钮用的/////////////////////////////////
    
    /// <summary>
    /// 关卡情报按钮点击
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
    /// 敌兵情报按钮点击
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
    /// 附加说明按钮点击
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
