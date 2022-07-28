using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �ؿ���Ϣ����
/// </summary>
public class StageInfoUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Tweener ShowTween;
    private GameObject Emp_StageInfo;
    private GameObject Emp_EnemyInfo;
    private GameObject Scr_AddInfo;
    private Sprite btnSprite0; // �Ϸ���ť��
    private Sprite btnSprite1; // �Ϸ���ť��
    private Image[] imgList;

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
