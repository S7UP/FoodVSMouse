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

    // 美食面板
    private ScrollRect Scr_FoodItem;
    private RectTransform RectTrans_FoodItemContent;
    private List<FoodItem_EncyclopediaPanel> foodItemList = new List<FoodItem_EncyclopediaPanel>();
    private FoodItem_EncyclopediaPanel currentFoodItem; // 当前选中的美食
    private FoodPanel_EncyclopediaPanel FoodPanel;

    // 老鼠面板
    private ScrollRect Scr_MouseItem;
    private RectTransform RectTrans_MouseItemContent;
    private List<MouseItem_EncyclopediaPanel> mouseItemList = new List<MouseItem_EncyclopediaPanel>();
    private MouseItem_EncyclopediaPanel currentMouseItem; // 当前选中的老鼠
    private MousePanel_EncyclopediaPanel MousePanel; // 老鼠信息显示面板

    // BOSS面板
    private ScrollRect Scr_BossItem;
    private RectTransform RectTrans_BossItemContent;
    private List<BossItem_EncyclopediaPanel> bossItemList = new List<BossItem_EncyclopediaPanel>();
    private BossItem_EncyclopediaPanel currentBossItem; // 当前选中的BOSS
    private BossPanel_EncyclopediaPanel BossPanel; // BOSS信息显示面板

    // 地形面板
    private ScrollRect Scr_TerrainItem;
    private RectTransform RectTrans_TerrainItemContent;
    private List<TerrainItem_EncyclopediaPanel> terrainItemList = new List<TerrainItem_EncyclopediaPanel>();
    private TerrainItem_EncyclopediaPanel currentTerrainItem; // 当前选中的BOSS
    private TerrainPanel_EncyclopediaPanel TerrainPanel; // BOSS信息显示面板

    // Tip面板
    private ScrollRect Scr_OtherRight;
    private GameObject Go_OtherPanel;
    private ScrollRect Scr_OtherLeft;

    private Transform Trans_BtnList;
    private Button Btn_FoodInfo;
    private Button Btn_MouseInfo;
    private Button Btn_BossInfo;
    private Button Btn_TerrainInfo;
    private Button Btn_Other;

    private Button Btn_Exit;

    protected override void Awake()
    {
        base.Awake();
        Trans_DisplayPanel = transform.Find("DisplayPanel");
        Trans_LeftContainer = Trans_DisplayPanel.Find("Img_center").Find("Emp_Container").Find("Middle");
        Scr_MouseItem = Trans_LeftContainer.Find("Scr_MouseItem").GetComponent<ScrollRect>();
        RectTrans_MouseItemContent = Scr_MouseItem.content.GetComponent<RectTransform>();
        MousePanel = transform.Find("MousePanel").GetComponent<MousePanel_EncyclopediaPanel>();

        Scr_FoodItem = Trans_LeftContainer.Find("Scr_FoodItem").GetComponent<ScrollRect>();
        RectTrans_FoodItemContent = Scr_FoodItem.content.GetComponent<RectTransform>();
        FoodPanel = transform.Find("FoodPanel").GetComponent<FoodPanel_EncyclopediaPanel>();

        Scr_BossItem = Trans_LeftContainer.Find("Scr_BossItem").GetComponent<ScrollRect>();
        RectTrans_BossItemContent = Scr_BossItem.content.GetComponent<RectTransform>();
        BossPanel = transform.Find("BossPanel").GetComponent<BossPanel_EncyclopediaPanel>();

        Scr_TerrainItem = Trans_LeftContainer.Find("Scr_TerrainItem").GetComponent<ScrollRect>();
        RectTrans_TerrainItemContent = Scr_TerrainItem.content.GetComponent<RectTransform>();
        TerrainPanel = transform.Find("TerrainPanel").GetComponent<TerrainPanel_EncyclopediaPanel>();

        Scr_OtherLeft = Trans_LeftContainer.Find("Scr_Other").GetComponent<ScrollRect>();
        Go_OtherPanel = transform.Find("OtherPanel").gameObject;
        Scr_OtherRight = Go_OtherPanel.transform.Find("Img_center").Find("GameObject").Find("Scr").GetComponent<ScrollRect>();

        Trans_BtnList = transform.Find("DisplayPanel").Find("BtnList");
        Btn_FoodInfo = Trans_BtnList.Find("Btn_FoodInfo").GetComponent<Button>();
        Btn_MouseInfo = Trans_BtnList.Find("Btn_MouseInfo").GetComponent<Button>();
        Btn_BossInfo = Trans_BtnList.Find("Btn_BossInfo").GetComponent<Button>();
        Btn_TerrainInfo = Trans_BtnList.Find("Btn_TerrainInfo").GetComponent<Button>();
        Btn_Other = Trans_BtnList.Find("Btn_Other").GetComponent<Button>();
        Btn_Exit = transform.Find("Btn_Exit").GetComponent<Button>();
    }

    public override void InitPanel()
    {
        // 美食左UI处理
        {
            FoodPanel.Initial();
            foreach (var item in foodItemList)
            {
                item.ExecuteRecycle();
            }
            foodItemList.Clear();
            // 读取美食顺序表来填充UI
            for (int i = 0; i < FoodManager.FoodTypeInfoCsv.GetRow(); i++)
            {
                int type = int.Parse(FoodManager.FoodTypeInfoCsv.GetValue(i, 0));
                FoodItem_EncyclopediaPanel foodItem = null;
                foodItem = FoodItem_EncyclopediaPanel.GetInstance(type, delegate { SetCurrentFoodItem(foodItem); });
                foodItem.transform.SetParent(Scr_FoodItem.content);
                foodItem.transform.localScale = Vector2.one;
                foodItemList.Add(foodItem);
            }
            SetCurrentFoodItem(foodItemList[0]);
            // 自适应滑动窗口的高度
            RectTrans_FoodItemContent.sizeDelta = new Vector2(RectTrans_FoodItemContent.sizeDelta.x, 10 + 125 * (1 + FoodManager.FoodTypeInfoCsv.GetRow() / 5));
        }
        // 老鼠左UI处理
        {
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
            RectTrans_MouseItemContent.sizeDelta = new Vector2(RectTrans_MouseItemContent.sizeDelta.x, 10 + 125 * (1 + typeShapeList.Count / 5));
        }
        // BOSS左UI处理
        {
            BossPanel.Initial();
            foreach (var item in bossItemList)
            {
                item.ExecuteRecycle();
            }
            bossItemList.Clear();
            // 读取老鼠顺序表来填充UI
            for (int i = 0; i < BossManager.BossInfoCsv.GetRow(); i++)
            {
                int type = int.Parse(BossManager.BossInfoCsv.GetValue(i, 0));
                int shape = int.Parse(BossManager.BossInfoCsv.GetValue(i, 1));
                BossItem_EncyclopediaPanel bossItem = null;
                bossItem = BossItem_EncyclopediaPanel.GetInstance(type, shape, delegate { SetCurrentBossItem(bossItem); });
                bossItem.transform.SetParent(Scr_BossItem.content);
                bossItem.transform.localScale = Vector2.one;
                bossItemList.Add(bossItem);
            }
            SetCurrentBossItem(bossItemList[0]);
            // 自适应滑动窗口的高度
            RectTrans_BossItemContent.sizeDelta = new Vector2(RectTrans_BossItemContent.sizeDelta.x, 10 + 125 * (1 + BossManager.BossInfoCsv.GetRow() / 5));
        }
        // 环境左UI处理
        {
            TerrainPanel.Initial();
            foreach (var item in terrainItemList)
            {
                item.ExecuteRecycle();
            }
            terrainItemList.Clear();
            // 读取老鼠顺序表来填充UI
            for (int i = 0; i < EnvironmentManager.InfoCsv.GetRow(); i++)
            {
                int type = int.Parse(EnvironmentManager.InfoCsv.GetValue(i, 0));
                TerrainItem_EncyclopediaPanel terrainItem = null;
                terrainItem = TerrainItem_EncyclopediaPanel.GetInstance(type, delegate { SetCurrentTerrainItem(terrainItem); });
                terrainItem.transform.SetParent(Scr_TerrainItem.content);
                terrainItem.transform.localScale = Vector2.one;
                terrainItemList.Add(terrainItem);
            }
            SetCurrentTerrainItem(terrainItemList[0]);
            // 自适应滑动窗口的高度
            RectTrans_TerrainItemContent.sizeDelta = new Vector2(RectTrans_TerrainItemContent.sizeDelta.x, 10 + 125 * (1 + EnvironmentManager.InfoCsv.GetRow() / 5));
        }
        // 术语左面板
        {
            Text text = Scr_OtherLeft.content.transform.Find("Text").GetComponent<Text>();
            RectTransform Rect = text.GetComponent<RectTransform>();
            string s = "";
            for (int i = 0; i < TermManager.InfoCsv.GetRow(); i++)
            {
                s += (i + 1) + "、" + TermManager.GetName(i) + "：" + TermManager.GetInfo(i)+"\n\n";
            }
            int countPerRow = Mathf.FloorToInt((Rect.sizeDelta.x) / text.fontSize);
            int rowCount = Mathf.CeilToInt((float)s.Length / countPerRow + 2); // 计算需要多少行
            foreach (var c in s.ToCharArray())
            {
                if (c.Equals('\n'))
                    rowCount++;
            }
            text.text = s;
            //Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, text.fontSize * rowCount);
            Scr_OtherLeft.content.sizeDelta = new Vector2(Scr_OtherLeft.content.sizeDelta.x, text.fontSize * rowCount);
        }

        FoodPanel.gameObject.SetActive(false);
        MousePanel.gameObject.SetActive(false);
        BossPanel.gameObject.SetActive(false);
        TerrainPanel.gameObject.SetActive(false);
        Go_OtherPanel.SetActive(false);
        Scr_FoodItem.gameObject.SetActive(false);
        Scr_MouseItem.gameObject.SetActive(false);
        Scr_BossItem.gameObject.SetActive(false);
        Scr_TerrainItem.gameObject.SetActive(false);
        Scr_OtherLeft.gameObject.SetActive(false);

        // 默认为美食面板
        OnClickFoodInfo();

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

    /// <summary>
    /// 设置当前选中的美食图例
    /// </summary>
    /// <param name="foodItem"></param>
    public void SetCurrentFoodItem(FoodItem_EncyclopediaPanel foodItem)
    {
        currentFoodItem = foodItem;
        UpdateFoodPanel();
        FoodPanel.UpdateDropShape(foodItem.type);
    }

    /// <summary>
    /// 更新美食信息面板
    /// </summary>
    public void UpdateFoodPanel()
    {
        int type = currentFoodItem.type;
        FoodPanel.Initial();
        FoodPanel.UpdateByParam(type);
    }

    /// <summary>
    /// 设置当前选中的BOSS图例
    /// </summary>
    /// <param name="mouseItem"></param>
    public void SetCurrentBossItem(BossItem_EncyclopediaPanel bossItem)
    {
        currentBossItem = bossItem;
        UpdateBossPanel();
    }

    /// <summary>
    /// 更新BOSS信息面板
    /// </summary>
    public void UpdateBossPanel()
    {
        int type = currentBossItem.type;
        int shape = currentBossItem.shape;
        BossPanel.Initial();
        BossPanel.UpdateByParam(type, shape);
    }

    /// <summary>
    /// 设置当前选中的老鼠图例
    /// </summary>
    /// <param name="mouseItem"></param>
    public void SetCurrentTerrainItem(TerrainItem_EncyclopediaPanel mouseItem)
    {
        currentTerrainItem = mouseItem;
        UpdateTerrainPanel();
    }

    /// <summary>
    /// 更新地形信息面板
    /// </summary>
    public void UpdateTerrainPanel()
    {
        int type = currentTerrainItem.type;
        TerrainPanel.Initial();
        TerrainPanel.UpdateByParam(type);
    }

    /// <summary>
    /// 当美食图鉴按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickFoodInfo()
    {
        FoodPanel.gameObject.SetActive(true);
        MousePanel.gameObject.SetActive(false);
        BossPanel.gameObject.SetActive(false);
        TerrainPanel.gameObject.SetActive(false);
        Go_OtherPanel.SetActive(false);
        Scr_FoodItem.gameObject.SetActive(true);
        Scr_MouseItem.gameObject.SetActive(false);
        Scr_BossItem.gameObject.SetActive(false);
        Scr_TerrainItem.gameObject.SetActive(false);
        Scr_OtherLeft.gameObject.SetActive(false);
        Btn_FoodInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/SelectedItem");
        Btn_MouseInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_BossInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_TerrainInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
    }

    /// <summary>
    /// 当老鼠图鉴按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickMouseInfo()
    {
        FoodPanel.gameObject.SetActive(false);
        MousePanel.gameObject.SetActive(true);
        BossPanel.gameObject.SetActive(false);
        TerrainPanel.gameObject.SetActive(false);
        Go_OtherPanel.SetActive(false);
        Scr_FoodItem.gameObject.SetActive(false);
        Scr_MouseItem.gameObject.SetActive(true);
        Scr_BossItem.gameObject.SetActive(false);
        Scr_TerrainItem.gameObject.SetActive(false);
        Scr_OtherLeft.gameObject.SetActive(false);
        Btn_FoodInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_MouseInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/SelectedItem");
        Btn_BossInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_TerrainInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_Other.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
    }

    /// <summary>
    /// 当BOSS图鉴按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickBossInfo()
    {
        FoodPanel.gameObject.SetActive(false);
        MousePanel.gameObject.SetActive(false);
        BossPanel.gameObject.SetActive(true);
        TerrainPanel.gameObject.SetActive(false);
        Go_OtherPanel.SetActive(false);
        Scr_FoodItem.gameObject.SetActive(false);
        Scr_MouseItem.gameObject.SetActive(false);
        Scr_BossItem.gameObject.SetActive(true);
        Scr_TerrainItem.gameObject.SetActive(false);
        Scr_OtherLeft.gameObject.SetActive(false);
        Btn_FoodInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_MouseInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_BossInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/SelectedItem");
        Btn_TerrainInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_Other.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
    }

    /// <summary>
    /// 当环境信息按钮被点击时（由外部赋值）
    /// </summary>
    public void OnClickTerrainInfo()
    {
        FoodPanel.gameObject.SetActive(false);
        MousePanel.gameObject.SetActive(false);
        BossPanel.gameObject.SetActive(false);
        TerrainPanel.gameObject.SetActive(true);
        Go_OtherPanel.SetActive(false);
        Scr_FoodItem.gameObject.SetActive(false);
        Scr_MouseItem.gameObject.SetActive(false);
        Scr_BossItem.gameObject.SetActive(false);
        Scr_TerrainItem.gameObject.SetActive(true);
        Scr_OtherLeft.gameObject.SetActive(false);
        Btn_FoodInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_MouseInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_BossInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_TerrainInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/SelectedItem");
        Btn_Other.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
    }

    /// <summary>
    /// 当提示等其他信息被点击时（由外部赋值）
    /// </summary>
    public void OnClickOtherInfo()
    {
        FoodPanel.gameObject.SetActive(false);
        MousePanel.gameObject.SetActive(false);
        BossPanel.gameObject.SetActive(false);
        TerrainPanel.gameObject.SetActive(false);
        Go_OtherPanel.SetActive(true);
        Scr_FoodItem.gameObject.SetActive(false);
        Scr_MouseItem.gameObject.SetActive(false);
        Scr_BossItem.gameObject.SetActive(false);
        Scr_TerrainItem.gameObject.SetActive(false);
        Scr_OtherLeft.gameObject.SetActive(true);
        Btn_FoodInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_MouseInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_BossInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_TerrainInfo.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/UnselectedItem");
        Btn_Other.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/EncyclopediaPanel/SelectedItem");
    }
}
