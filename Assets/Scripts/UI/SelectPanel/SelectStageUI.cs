using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 选关UI(左侧的那个）
/// </summary>
public class SelectStageUI : MonoBehaviour
{
    private const int ItemCountPerPage = 15; // 每页能容纳多少个小关
    // 由外界获取该实例时获得的引用
    private SelectPanel mSelectPanel; // 选关面板

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

    private int maxPage = 0; // 最大页数(0为0，1-15为1，16-30为2，依次类推)
    private int currentPageIndex = 0; // 当前页下标（页码为页下标+1）
    private int selectIndex = 0; // 被选中的按钮下标

    private void Awake()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        ShowTween = rectTransform.DOAnchorPosX(235, 0.5f);
        ShowTween.SetEase(Ease.InCubic); // 由慢到快
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
    /// 初始化方法
    /// </summary>
    public void Initial()
    {
        // 移除旧页面的按钮
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
    /// 根据当前场景的所有关卡来创建左侧列表
    /// </summary>
    public void UpdateStageList()
    {
        // 移除旧页面的按钮
        foreach (var item in Btn_StageList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/Btn_Stage", item.gameObject);
        }
        Btn_StageList.Clear();
        // 产生新的项
        List<BaseStage.StageInfo> list = mSelectPanel.GetCurrentSceneStageList();
        maxPage = Mathf.CeilToInt((float)list.Count / ItemCountPerPage);
        Tex_Page.text = (currentPageIndex+1)+"/"+ maxPage; // 更新页数显示
        
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
                // 如果这个选项卡对应的关卡为当前选中关卡，则要在外观上改成金色的，否则为蓝色的
                if (selectIndex == i)
                    b = btn;
                Btn_StageList.Add(btn);
            }
            if (b != null)
                OnBtnStageClick(b, selectIndex);
        }

    }

    /// <summary>
    /// 当某个关卡选项条被点击时
    /// </summary>
    public void OnBtnStageClick(Button btn, int index)
    {
        Debug.Log("index="+index);
        mSelectPanel.SetCurrentSelectedStageInfo(index);
        selectIndex = index;
        // 当前页所有按钮变蓝
        foreach (var item in Btn_StageList)
        {
            item.GetComponent<Image>().sprite = BtnUnSelectedSprite;
        }
        // 被选中的这个按钮变金
        btn.GetComponent<Image>().sprite = BtnSelectedSprite;
        mSelectPanel.UpdateUIByChangeStage();
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void OnNextPageClick()
    {
        currentPageIndex = Mathf.Min(currentPageIndex+1, maxPage-1);
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void OnLastPageClick()
    {
        currentPageIndex = Mathf.Max(currentPageIndex - 1, 0);
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// 最后一页
    /// </summary>
    public void OnFinalPageClick()
    {
        currentPageIndex = maxPage - 1;
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// 最前一页
    /// </summary>
    public void OnFirstPageClick()
    {
        currentPageIndex = 0;
        UpdateChangePageIndex();
        UpdateStageList();
    }

    /// <summary>
    /// 更新四个按钮可否点击的情况
    /// </summary>
    private void UpdateChangePageIndex()
    {
        Btn_NextPage.interactable = true;
        Btn_FinalPage.interactable = true;
        Btn_LastPage.interactable = true;
        Btn_FirstPage.interactable = true;
        // 若到达最大页数，则不能再前进
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


    /// <summary>
    /// 设置当前宿主面板
    /// </summary>
    /// <param name="panel"></param>
    public void SetSelectPanel(SelectPanel panel)
    {
        mSelectPanel = panel;
    }
}
