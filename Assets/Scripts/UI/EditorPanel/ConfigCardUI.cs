using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑器中关卡配置卡片的UI
/// </summary>
public class ConfigCardUI : MonoBehaviour
{
    private EditorPanel mEditorPanel;
    private string cardPrefabPath = "EditorPanel/Btn_SelectedCardModel";
    private Transform CardContainer;
    private GameObject RightPanel;
    private Text Tex_Name;
    private Image Img_Display;
    private Dropdown Dro_RankLimit;
    private Dropdown Dro_ShapeLimit;
    private Button Btn_Exit;

    private Dictionary<FoodNameTypeMap, ConfigCardModel> cardDict = new Dictionary<FoodNameTypeMap, ConfigCardModel>();
    private ConfigCardModel currentSelectedModel;

    private bool IsUpdate;


    private void Awake()
    {
        CardContainer = transform.Find("AllCardListUI").Find("Img_center").Find("Emp_Container").Find("Scr").Find("Viewport").Find("Content");
        RightPanel = transform.Find("SelectedCardDisplayer").gameObject;
        Tex_Name = transform.Find("SelectedCardDisplayer").Find("Img_center").Find("Text").GetComponent<Text>();
        Img_Display = transform.Find("SelectedCardDisplayer").Find("Img_center").Find("Emp_Container").Find("Middle").Find("Img_Display").GetComponent<Image>();
        Dro_RankLimit = transform.Find("SelectedCardDisplayer").Find("Img_center").Find("Dro_RankLimit").GetComponent<Dropdown>();
        Dro_RankLimit.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= 16; i++)
        {
            dataList.Add(new Dropdown.OptionData(i + ""));
        }
        Dro_RankLimit.AddOptions(dataList);
        Dro_RankLimit.onValueChanged.AddListener(delegate { OnRankOrShapeDropDownChange(); });
        Dro_ShapeLimit = transform.Find("SelectedCardDisplayer").Find("Img_center").Find("Dro_ShapeLimit").GetComponent<Dropdown>();
        Dro_ShapeLimit.onValueChanged.AddListener(delegate { OnRankOrShapeDropDownChange(); });
        Btn_Exit = transform.Find("Btn_Exit").GetComponent<Button>();
        Btn_Exit.onClick.AddListener(delegate { mEditorPanel.SetConfigCardUIEnable(false); });
    }

    public void Initial()
    {
        foreach (var keyValuePair in cardDict)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, cardPrefabPath, keyValuePair.Value.gameObject);
        }
        cardDict.Clear();

        Dictionary<FoodNameTypeMap, List<string>> dict = FoodManager.GetAllBuildableFoodDict();
        foreach (var keyValuePair in dict)
        {
            List<string> l = keyValuePair.Value;
            int type = ((int)keyValuePair.Key);
            int shape = l.Count - 1;
            int level = 16;
            BaseCardBuilder.Attribute attr = GameManager.Instance.attributeManager.GetCardBuilderAttribute(type, shape);
            ConfigCardModel obj = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, cardPrefabPath).GetComponent<ConfigCardModel>();
            obj.SetConfigCardUI(this);
            obj.Initial();
            // 设置卡面初始信息
            obj.SetInfo(type, shape, level);
            // 设置点击监听
            obj.AddListener(delegate { SetCurrentSelectedModel(obj); });
            cardDict.Add(keyValuePair.Key, obj);
            obj.transform.SetParent(CardContainer);
            obj.transform.localScale = Vector3.one;
        }
        // 从当前关卡配置中读取卡片配置表，并应用到模型中
        List<AvailableCardInfo> list = GetAvailableCardInfoList();
        foreach (var item in list)
        {
            cardDict[(FoodNameTypeMap)item.type].SetInfo(item.type, item.maxShape, item.maxLevel);
            cardDict[(FoodNameTypeMap)item.type].isUpdate = true;
            cardDict[(FoodNameTypeMap)item.type].SetSelected(true);
            cardDict[(FoodNameTypeMap)item.type].isUpdate = false;
        }
        if(list.Count>0)
            currentSelectedModel = cardDict[(FoodNameTypeMap)list[0].type]; // 默认选择关卡配置的第一张卡作为选中卡
        // 更新面板
        UpdateRightPanel();
    }

    /// <summary>
    /// 更新右侧面板
    /// </summary>
    public void UpdateRightPanel()
    {
        IsUpdate = true;
        if (currentSelectedModel == null)
        {
            RightPanel.SetActive(false);
            return;
        }
        else
        {
            RightPanel.SetActive(true);
        }

        // 更新名字
        Tex_Name.text = currentSelectedModel.GetName();
        // 更新贴图
        Img_Display.sprite = GameManager.Instance.GetSprite("Food/" + ((int)currentSelectedModel.GetFoodType()) + "/" + currentSelectedModel.GetShape() + "/display");
        // 更新最大星级
        Dro_RankLimit.value = currentSelectedModel.GetRank();
        // 更新最大转职
        Dro_ShapeLimit.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i < FoodManager.GetAllBuildableFoodDict()[currentSelectedModel.GetFoodType()].Count; i++)
        {
            dataList.Add(new Dropdown.OptionData(i+""));
        }
        Dro_ShapeLimit.AddOptions(dataList);
        Dro_ShapeLimit.value = currentSelectedModel.GetShape();
        IsUpdate = false;
    }

    /// <summary>
    /// 向卡片配置模型中添加一个卡片配置
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <param name="rank"></param>
    public void AddAvailableCardInfo(int type, int shape, int rank)
    {
        List<AvailableCardInfo> l = GetAvailableCardInfoList();
        // 安全性检测
        foreach (var item in l)
        {
            if (type == item.type)
            {
                Debug.Log("当前卡片配置表中已存在该卡片！");
                return;
            }
        }
        // 添加
        l.Add(new AvailableCardInfo(type, shape, rank));
    }

    /// <summary>
    /// 向卡片配置模型中删除一个卡片配置
    /// </summary>
    /// <param name="type"></param>
    public void RemoveAvailableCardInfo(int type)
    {
        List<AvailableCardInfo> l = GetAvailableCardInfoList();
        int index = -1;
        for (int i = 0; i < l.Count; i++)
        {
            if(l[i].type == type)
            {
                index = i;
                break;
            }
        }
        if (index > -1)
            l.Remove(l[index]);
    }

    /// <summary>
    /// 设置当前选择的卡片模型
    /// </summary>
    private void SetCurrentSelectedModel(ConfigCardModel model)
    {
        if (model.IsSelected())
        {
            currentSelectedModel = model;
            // 更新右面版
            UpdateRightPanel();
        }
    }

    /// <summary>
    /// 获取当前关卡的可选用美食配置表
    /// </summary>
    /// <returns></returns>
    private List<AvailableCardInfo> GetAvailableCardInfoList()
    {
        return mEditorPanel.GetCurrentStageInfo().availableCardInfoList;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEditorPanel(EditorPanel panel)
    {
        mEditorPanel = panel;
    }

    /// <summary>
    /// 当右侧两个下拉框值改变时，同步改变关卡模型信息
    /// </summary>
    private void OnRankOrShapeDropDownChange()
    {
        // 如果是自动更新状态下触发，则不执行
        if (IsUpdate)
            return;

        List<AvailableCardInfo> list = GetAvailableCardInfoList();
        int type = ((int)currentSelectedModel.GetFoodType());
        foreach (var item in list)
        {
            if (type == item.type)
            {
                item.maxLevel = Dro_RankLimit.value;
                item.maxShape = Dro_ShapeLimit.value;
                // 更新模型
                currentSelectedModel.SetInfo(((int)currentSelectedModel.GetFoodType()), item.maxShape, item.maxLevel);
                // 更新右面板
                UpdateRightPanel();
                break;
            }
        }
        Debug.Log("未在卡片配置中找到对应的卡片");
    }
}
