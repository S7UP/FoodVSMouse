using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 关卡信息界面
/// </summary>
public class StageInfoUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Tweener ShowTween;
    private GameObject Emp_StageInfo;
    private GameObject Emp_EnemyInfo;
    private GameObject Scr_AddInfo;
    private Sprite btnSprite0; // 上方按钮暗
    private Sprite btnSprite1; // 上方按钮亮
    private Image[] imgList;

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
