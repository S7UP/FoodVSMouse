using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 百科全书面板
/// </summary>
public class EncyclopediaPanel : BasePanel
{
    private Transform Trans_DisplayPanel;
    private Transform Trans_LeftContainer;
    private ScrollRect Scr_MouseItem;
    private RectTransform RectTrans_MouseItemContent;
    private List<MouseItem_EncyclopediaPanel> mouseItemList = new List<MouseItem_EncyclopediaPanel>();
    private MouseItem_EncyclopediaPanel currentMouseItem; // 当前选中的老鼠

    private MousePanel_EncyclopediaPanel MousePanel; // 老鼠信息显示面板

    private Button Btn_Exit;

    protected override void Awake()
    {
        base.Awake();
        Trans_DisplayPanel = transform.Find("DisplayPanel");
        Trans_LeftContainer = Trans_DisplayPanel.Find("Img_center").Find("Emp_Container");
        Scr_MouseItem = Trans_LeftContainer.Find("Scr_MouseItem").GetComponent<ScrollRect>();
        RectTrans_MouseItemContent = Scr_MouseItem.content.GetComponent<RectTransform>();

        MousePanel = transform.Find("MousePanel").GetComponent<MousePanel_EncyclopediaPanel>();
        Btn_Exit = transform.Find("Btn_Exit").GetComponent<Button>();
    }

    public override void InitPanel()
    {
        // 刷新面板
        MousePanel.Initial();
        foreach (var item in mouseItemList)
        {
            item.ExecuteRecycle();
        }
        mouseItemList.Clear();

        // 读取老鼠顺序表来填充UI
        List<Vector2> typeShapeList = MouseManager.GetMouseTypeShapeSeqList();
        foreach (var v2 in typeShapeList)
        {
            int type = (int)v2.x;
            int shape = (int)v2.y;
            MouseItem_EncyclopediaPanel mouseItem = null;
            mouseItem = MouseItem_EncyclopediaPanel.GetInstance(type, shape, delegate { SetCurrentMouseItem(mouseItem); });
            mouseItem.transform.SetParent(Scr_MouseItem.content);
            mouseItem.transform.localScale = Vector2.one;
            mouseItemList.Add(mouseItem);
        }
        SetCurrentMouseItem(mouseItemList[0]);
        // 自适应滑动窗口的高度
        RectTrans_MouseItemContent.sizeDelta = new Vector2(RectTrans_MouseItemContent.sizeDelta.x, 10 + 125*(1 + typeShapeList.Count/5));

        Btn_Exit.onClick.RemoveAllListeners();
        Btn_Exit.onClick.AddListener(delegate { mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].ExitPanel(); });
    }

    /// <summary>
    /// 设置当前选中的老鼠图例
    /// </summary>
    /// <param name="mouseItem"></param>
    public void SetCurrentMouseItem(MouseItem_EncyclopediaPanel mouseItem)
    {
        currentMouseItem = mouseItem;
        UpdateMousePanel();
    }

    /// <summary>
    /// 更新老鼠信息面板
    /// </summary>
    private void UpdateMousePanel()
    {
        int type = currentMouseItem.type;
        int shape = currentMouseItem.shape;
        MousePanel.Initial();
        MousePanel.UpdateByParam(type, shape);
    }
}
