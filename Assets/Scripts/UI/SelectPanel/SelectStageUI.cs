using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// ѡ��UI(�����Ǹ���
/// </summary>
public class SelectStageUI : MonoBehaviour
{
    private const int ItemCountPerPage = 15; // ÿҳ�����ɶ��ٸ�С��
    // ������ȡ��ʵ��ʱ��õ�����
    private SelectPanel mSelectPanel; // ѡ�����

    private RectTransform rectTransform;
    private Tweener ShowTween;
    private Transform Trans_StageList;
    private Text Tex_Page;
    private Button Btn_NextPage;
    private Button Btn_FinalPage;
    private Button Btn_LastPage;
    private Button Btn_FirstPage;
    private List<Button> Btn_StageList = new List<Button>();
    private Sprite BtnUnSelectedSprite;
    private Sprite BtnSelectedSprite;

    private int maxPage = 0; // ���ҳ��(0Ϊ0��1-15Ϊ1��16-30Ϊ2����������)
    private int currentPageIndex = 0; // ��ǰҳ�±꣨ҳ��Ϊҳ�±�+1��
    private int selectIndex = 0; // ��ѡ�еİ�ť�±�

    private void Awake()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        ShowTween = rectTransform.DOAnchorPosX(235, 0.5f);
        ShowTween.SetEase(Ease.InCubic); // ��������
        ShowTween.Pause();
        ShowTween.SetAutoKill(false);

        Trans_StageList = transform.Find("Emp_Container").Find("StageList");
        Tex_Page = transform.Find("Tex_Page").GetComponent<Text>();
        Btn_NextPage = transform.Find("Btn_NextPage").GetComponent<Button>();
        Btn_FinalPage = transform.Find("Btn_FinalPage").GetComponent<Button>();
        Btn_LastPage = transform.Find("Btn_LastPage").GetComponent<Button>();
        Btn_FirstPage = transform.Find("Btn_FirstPage").GetComponent<Button>();
        BtnUnSelectedSprite = GameManager.Instance.GetSprite("UI/SelectPanel/181");
        BtnSelectedSprite = GameManager.Instance.GetSprite("UI/SelectPanel/183");
    }

    /// <summary>
    /// ��ʼ������
    /// </summary>
    public void Initial()
    {
        // �Ƴ���ҳ��İ�ť
        foreach (var item in Btn_StageList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_Stage", item.gameObject);
        }
        Btn_StageList.Clear();
        maxPage = 0;
        currentPageIndex = 0;
        selectIndex = 0;
    }


    /// <summary>
    /// ���ݵ�ǰ���������йؿ�����������б�
    /// </summary>
    public void UpdateStageList()
    {
        // �Ƴ���ҳ��İ�ť
        foreach (var item in Btn_StageList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_Stage", item.gameObject);
        }
        Btn_StageList.Clear();
        // �����µ���
        List<BaseStage.StageInfo> list = mSelectPanel.GetCurrentSceneStageList();
        maxPage = Mathf.CeilToInt((float)list.Count / ItemCountPerPage);
        Tex_Page.text = (currentPageIndex+1)+"/"+ maxPage; // ����ҳ����ʾ
        
        if (list != null)
        {
            Button b = null;
            int startIndex = ItemCountPerPage * currentPageIndex;
            int endIndex = Mathf.Min((currentPageIndex + 1) * ItemCountPerPage, list.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                BaseStage.StageInfo info = list[i];
                Button btn = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/Btn_Stage").GetComponent<Button>();
                btn.transform.Find("Text").GetComponent<Text>().text = info.name;
                int j = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(delegate { OnBtnStageClick(btn, j); });
                btn.transform.SetParent(Trans_StageList);
                btn.transform.localScale = Vector3.one;
                btn.transform.SetAsFirstSibling();
                // ������ѡ���Ӧ�Ĺؿ�Ϊ��ǰѡ�йؿ�����Ҫ������ϸĳɽ�ɫ�ģ�����Ϊ��ɫ��
                if (selectIndex == i)
                    b = btn;
                Btn_StageList.Add(btn);
            }
            if (b != null)
                OnBtnStageClick(b, selectIndex);
        }

    }

    /// <summary>
    /// ��ĳ���ؿ�ѡ���������ʱ
    /// </summary>
    public void OnBtnStageClick(Button btn, int index)
    {
        Debug.Log("index="+index);
        mSelectPanel.SetCurrentSelectedStageInfo(index);
        selectIndex = index;
        // ��ǰҳ���а�ť����
        foreach (var item in Btn_StageList)
        {
            item.GetComponent<Image>().sprite = BtnUnSelectedSprite;
        }
        // ��ѡ�е������ť���
        btn.GetComponent<Image>().sprite = BtnSelectedSprite;
        mSelectPanel.UpdateUIByChangeStage();
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void OnNextPageClick()
    {
        currentPageIndex = Mathf.Min(currentPageIndex+1, maxPage-1);
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void OnLastPageClick()
    {
        currentPageIndex = Mathf.Max(currentPageIndex - 1, 0);
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// ���һҳ
    /// </summary>
    public void OnFinalPageClick()
    {
        currentPageIndex = maxPage - 1;
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// ��ǰһҳ
    /// </summary>
    public void OnFirstPageClick()
    {
        currentPageIndex = 0;
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// �����ĸ���ť�ɷ��������
    /// </summary>
    private void UpdateChangePageIndex()
    {
        Btn_NextPage.interactable = true;
        Btn_FinalPage.interactable = true;
        Btn_LastPage.interactable = true;
        Btn_FirstPage.interactable = true;
        // ���������ҳ����������ǰ��
        if (currentPageIndex == maxPage - 1)
        {
            Btn_NextPage.interactable = false;
            Btn_FinalPage.interactable = false;
        }
        if (currentPageIndex == 0)
        {
            Btn_LastPage.interactable = false;
            Btn_FirstPage.interactable = false;
        }
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


    /// <summary>
    /// ���õ�ǰ�������
    /// </summary>
    /// <param name="panel"></param>
    public void SetSelectPanel(SelectPanel panel)
    {
        mSelectPanel = panel;
    }
}
