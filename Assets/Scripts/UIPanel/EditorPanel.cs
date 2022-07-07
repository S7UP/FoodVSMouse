using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static BaseRound;

public class EditorPanel : BasePanel
{
    // 索引
    private GameObject MouseEditorUI;
    private Transform scrEnemyContentRoundListTrans;
    private Transform scrContentEnemyGroupListTrans; // 右侧显示组数按钮的滚动窗口
    private Text roundNavigationText; // 上方导航文字
    private Button Btn_Return; // 返回上级按钮
    private Button Btn_Save; // 保存
    private InputField InF_StageName; // 当前关卡名文本输入框
    private Button Btn_AddRoundInfo; // 添加一轮敌人按钮
    private Button Btn_DelRoundInfo; // 删除当前轮敌人按钮
    private InputField Inf_RoundName; // 当前轮名称
    private InputField Inf_Interval; // 当前组间隔
    private InputField Inf_EndTime; // 当前结束等待时间
    private Transform Emp_AddEnemyGroupTrans;
    private Button Btn_AddEnemyGroup; // 添加一组敌人按钮
    private Scr_SelectEnemyType scr_SelectEnemyType;

    private GameObject StageEditorUI;
    private Transform scrStageContentRoundListTrans;
    private InputField Inf_PrepareTime; // 关卡初始准备时间
    private Dropdown Dro_DefaultMode; // 关卡模式选项框
    private Transform scrContentApartListTrans; // 分路组滑动窗体容器
    private Button Btn_AddApart; // 添加一个新的分路组
    private Button Btn_DelApart; // 删除选定分路组
    private Transform scrContentApartEditViewTrans; // 分路组预览窗口容器
    private Transform Emp_ApartAddTrans;
    private Emp_ApartAdd emp_ApartAdd;
    private GameObject Img_ApartEditView;
    private Button Btn_AddStageRoundInfo; // 添加一个大轮敌人按钮

    private Transform scrContentRoundListTrans; // 显示轮数的滚动窗体

    // 当前待编辑的关卡信息
    private BaseStage.StageInfo currentStageInfo;
    // 当前轮级目录索引栈
    private Stack<BaseRound.RoundInfo> roundInfoStack;
    // 当前轮在其父级的下标栈
    private Stack<int> currentRoundIndexStack;
    // 当前左面板的按钮集合
    private List<Button> roundBtnList;
    // 当前右面板组集合
    private List<EnemyGroupUI> enemyGroupUIList;


    // 选定的分路组
    private List<int> currentApart;
    private int currentApartIndex;
    // 当前分路组Object
    private List<GameObject> apartGoList;
    // 当前分路组预览面板Object
    private List<Emp_ApartEdit> apartEditGoList;

    // 是否在UI更新中的状态
    private bool isUIUpdateState;

    protected override void Awake()
    {
        base.Awake();
        MouseEditorUI = transform.Find("MouseEditorUI").gameObject;
        scrEnemyContentRoundListTrans = transform.Find("MouseEditorUI").Find("Emp_RoundList").Find("Scr_RoundList").Find("Viewport").Find("Content");
        scrContentEnemyGroupListTrans = transform.Find("MouseEditorUI").Find("Emp_EnemyGroupList").Find("Viewport").Find("Content");
        roundNavigationText = transform.Find("MouseEditorUI").Find("Emp_RoundNavigation").Find("Tex_CurrentIndex").GetComponent<Text>();
        Btn_Return = transform.Find("MouseEditorUI").Find("Emp_RoundNavigation").Find("Btn_Return").GetComponent<Button>();
        Btn_Return.onClick.AddListener( delegate { ReturnLast(); });
        Btn_Save = transform.Find("Btn_Save").GetComponent<Button>();
        Btn_Save.onClick.AddListener(SaveAll);
        InF_StageName = transform.Find("Img_StageName").Find("InputField").GetComponent<InputField>();
        InF_StageName.onEndEdit.AddListener(delegate { OnStageNameInputFieldChanged(); });
        Btn_AddRoundInfo = transform.Find("MouseEditorUI").Find("Emp_RoundList").Find("Emp_OperateRound").Find("Btn_AddRoundInfo").GetComponent<Button>();
        Btn_AddRoundInfo.onClick.AddListener(delegate { AddNewRoundInfo(); });
        Btn_DelRoundInfo = transform.Find("MouseEditorUI").Find("Img_RoundInfo").Find("Btn_DelRoundInfo").GetComponent<Button>();
        Btn_DelRoundInfo.onClick.AddListener(() => { DelCurrentRoundInfo(); });
        Inf_RoundName = transform.Find("MouseEditorUI").Find("Img_RoundInfo").Find("Emp_Name").Find("InputField").GetComponent<InputField>();
        Inf_RoundName.onEndEdit.AddListener(delegate { OnRoundNameInFChange(); });
        Inf_Interval = transform.Find("MouseEditorUI").Find("Img_RoundInfo").Find("Emp_Interval").Find("InputField").GetComponent<InputField>();
        Inf_Interval.onEndEdit.AddListener(delegate { OnRoundIntervalInFChange(); });
        Inf_EndTime = transform.Find("MouseEditorUI").Find("Img_RoundInfo").Find("Emp_EndTime").Find("InputField").GetComponent<InputField>();
        Inf_EndTime.onEndEdit.AddListener(delegate { OnRoundEndTimeInFChange(); });
        Emp_AddEnemyGroupTrans = scrContentEnemyGroupListTrans.Find("Emp_EnemyGroupOperate");
        Btn_AddEnemyGroup = Emp_AddEnemyGroupTrans.Find("Button").GetComponent<Button>();
        Btn_AddEnemyGroup.onClick.AddListener(delegate { OnAddEnemyGroupClick(); });
        scr_SelectEnemyType = transform.Find("MouseEditorUI").Find("Emp_EnemyGroupList").Find("Scr_SelectEnemyType").GetComponent<Scr_SelectEnemyType>();
        scr_SelectEnemyType.gameObject.SetActive(false);

        StageEditorUI = transform.Find("StageEditorUI").gameObject;
        scrStageContentRoundListTrans = transform.Find("StageEditorUI").Find("Emp_RoundList").Find("Scr_RoundList").Find("Viewport").Find("Content");
        Inf_PrepareTime = StageEditorUI.transform.Find("Img_RoundInfo").Find("Emp_PrepareTime").Find("InputField").GetComponent<InputField>();
        Inf_PrepareTime.onEndEdit.AddListener(delegate { OnPrepareTimeInFChange(); });
        Dro_DefaultMode = StageEditorUI.transform.Find("Img_RoundInfo").Find("Emp_DefaultMode").Find("Dropdown").GetComponent<Dropdown>();
        Dro_DefaultMode.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        dataList.Add(new Dropdown.OptionData("半随机模式"));
        dataList.Add(new Dropdown.OptionData("固定模式"));
        Dro_DefaultMode.AddOptions(dataList);
        Dro_DefaultMode.value = (int)BaseStage.StageMode.HalfRandom;
        Dro_DefaultMode.onValueChanged.AddListener(delegate { OnDefaultModeDroChange(Dro_DefaultMode.value); });
        scrContentApartListTrans = StageEditorUI.transform.Find("Img_ApartList").Find("Scroll View").Find("Viewport").Find("Content");
        Btn_AddApart = StageEditorUI.transform.Find("Img_ApartList").Find("Btn_Add").GetComponent<Button>();
        Btn_AddApart.onClick.AddListener(delegate { OnAddNewApartClick(); });
        Btn_DelApart = StageEditorUI.transform.Find("Img_ApartList").Find("Btn_Del").GetComponent<Button>();
        Btn_DelApart.onClick.AddListener(delegate { OnDelApartClick(); });
        scrContentApartEditViewTrans = StageEditorUI.transform.Find("Img_ApartEditView").Find("Scroll View").Find("Viewport").Find("Content");
        Emp_ApartAddTrans = scrContentApartEditViewTrans.Find("Emp_ApartAdd");
        emp_ApartAdd = Emp_ApartAddTrans.GetComponent<Emp_ApartAdd>();
        Img_ApartEditView = StageEditorUI.transform.Find("Img_ApartEditView").gameObject;
        Btn_AddStageRoundInfo = StageEditorUI.transform.Find("Emp_RoundList").Find("Emp_OperateRound").Find("Btn_AddRoundInfo").GetComponent<Button>();
        Btn_AddStageRoundInfo.onClick.AddListener(delegate { AddNewRoundInfo(); });

        roundInfoStack = new Stack<BaseRound.RoundInfo>();
        currentRoundIndexStack = new Stack<int>();
        roundBtnList = new List<Button>();
        enemyGroupUIList = new List<EnemyGroupUI>();
        isUIUpdateState = false;
        apartGoList = new List<GameObject>();
        apartEditGoList = new List<Emp_ApartEdit>();
        currentApartIndex = -1;


        // test
        LoadStage(Test.TestStageName);
    }

    // Start is called before the first frame update
    void Start()
    {
        emp_ApartAdd.button.onClick.AddListener(delegate { OnApartAddClick(); });
        UpdateUI();
    }

    /// <summary>
    /// 从本地加载关卡内容编辑
    /// </summary>
    /// <param name="stageName"></param>
    public void LoadStage(string stageName)
    {
        currentStageInfo = BaseStage.Load(stageName);
        InF_StageName.text = currentStageInfo.name;
    }

    /// <summary>
    /// 获取当前持有的轮信息
    /// </summary>
    public BaseRound.RoundInfo GetCurrentRoundInfo()
    {
        if (roundInfoStack.Count <= 0)
        {
            // 栈中没有元素则将关卡的轮组在一起拼凑成一个大轮
            return currentStageInfo.GetTotalRoundInfo();
        }
        else
        {
            // 否则取栈顶
            return roundInfoStack.Peek();
        }
    }

    /// <summary>
    /// 获取当前面板显示的敌人组
    /// </summary>
    /// <returns></returns>
    public List<BaseEnemyGroup> GetCurrentEnemyGroupList()
    {
        if (roundInfoStack.Count > 0)
        {
            return GetCurrentRoundInfo().baseEnemyGroupList;
        }
        else
        {
            return null;
        }
    }

    // 更新UI显示一次
    public void UpdateUI()
    {
        isUIUpdateState = true;
        if (MouseEditorUI.activeSelf)
        {
            UpdateMouseEditorUI();
        }
        if (StageEditorUI.activeSelf)
        {
            UpdateStageEditorUI();
        }
        isUIUpdateState = false;
    }

    /// <summary>
    /// 对刷怪面板的编辑
    /// </summary>
    private void UpdateMouseEditorUI()
    {
        // 设置当前显示轮数的面板
        scrContentRoundListTrans = scrEnemyContentRoundListTrans;
        // 获取当前持有轮信息
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // 更新左面板
        Inf_RoundName.text = currentRoundInfo.name;
        Inf_Interval.text = currentRoundInfo.interval.ToString();
        Inf_EndTime.text = currentRoundInfo.endTime.ToString();

        UpdateRoundInfoContent();

        // 清空一次右面板
        foreach (var item in enemyGroupUIList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_EnemyGroup", item.gameObject);
        }
        enemyGroupUIList.Clear();
        // 更新右面板
        if (currentRoundInfo.baseEnemyGroupList != null)
        {
            foreach (var item in currentRoundInfo.baseEnemyGroupList)
            {
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_EnemyGroup");
                go.transform.SetParent(scrContentEnemyGroupListTrans);
                go.transform.localScale = Vector3.one;
                EnemyGroupUI enemyGroupUI = go.GetComponent<EnemyGroupUI>();
                enemyGroupUI.SetEnemyGroup(item);
                // 删除按钮重置监听
                {
                    Button btn = enemyGroupUI.GetDelButton();
                    btn.onClick.RemoveAllListeners();
                    int index = enemyGroupUIList.Count;
                    btn.onClick.AddListener(delegate { OnDelEnemyGroupClick(index); });
                }
                // 更换敌人按钮重置监听
                {
                    Button btn = enemyGroupUI.GetChangeEnemyTypeButton();
                    btn.onClick.RemoveAllListeners();
                    int index = enemyGroupUIList.Count;
                    btn.onClick.AddListener(delegate { OnChangeEnemyTypeClick(index); });
                }
                enemyGroupUIList.Add(enemyGroupUI);
            }
            Emp_AddEnemyGroupTrans.SetAsLastSibling();
            // 自动更新滚动框宽度
            UpdateScrollerContentWidth(scrContentEnemyGroupListTrans);
        }



        // 更新上面板
        string text = "";
        if (roundInfoStack.Count <= 0)
        {
            text += "--";
        }
        else
        {
            foreach (var item in roundInfoStack)
            {
                text = "/" + item.name + text;
            }
        }
        text = "当前索引：" + text;
        roundNavigationText.text = text;
    }

    /// <summary>
    /// 对编辑关卡面板的更新
    /// </summary>
    private void UpdateStageEditorUI()
    {
        // 设置当前显示轮数的面板
        scrContentRoundListTrans = scrStageContentRoundListTrans;

        Inf_PrepareTime.text = currentStageInfo.perpareTime.ToString();
        Dro_DefaultMode.value = (int)currentStageInfo.defaultMode;

        // 更新分路组面板
        {
            // 清空
            foreach (var item in apartGoList)
            {
                GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_Apart", item);
            }
            apartGoList.Clear();
            int index = 0;
            foreach (var item in currentStageInfo.apartList)
            {
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_Apart");
                go.transform.SetParent(scrContentApartListTrans);
                go.transform.localScale = Vector3.one;
                Text tex = go.transform.Find("Text").GetComponent<Text>();
                tex.text = "分路" + (index + 1).ToString() + "：";
                InputField inf = go.transform.Find("InputField").GetComponent<InputField>();
                inf.text = "";
                inf.readOnly = true;
                foreach (var i in item)
                {
                    inf.text += (i + 1).ToString();
                }
                Button btn = go.transform.Find("Button").GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int j = index;
                btn.onClick.AddListener(delegate { OnApartEditOnClick(j); });
                apartGoList.Add(go);
                // 变色
                if(index== currentApartIndex)
                {
                    Color co = go.GetComponent<Image>().color;
                    go.GetComponent<Image>().color = new Color(co.r, co.g, co.b, 0.5f);
                }
                else
                {
                    Color co = go.GetComponent<Image>().color;
                    go.GetComponent<Image>().color = new Color(co.r, co.g, co.b, 0);
                }
                index++;
            }
            // 自动更新滚动框高度
            UpdateScrollerContentHeight(scrContentApartListTrans);
        }


        // 更新分路预览面板
        if (currentApart != null && currentApartIndex > -1)
        {
            Img_ApartEditView.SetActive(true);
            // 清空
            foreach (var item in apartEditGoList)
            {
                GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_ApartEdit", item.gameObject);
            }
            apartEditGoList.Clear();
            int index = 0;
            foreach (var item in currentApart)
            {
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_ApartEdit");
                go.transform.SetParent(scrContentApartEditViewTrans);
                go.transform.localScale = Vector3.one;
                Emp_ApartEdit aprtEdit = go.GetComponent<Emp_ApartEdit>();
                aprtEdit.SetSelectedIndex(item);
                aprtEdit.SetPosition(index);
                apartEditGoList.Add(aprtEdit);
                int j = index;
                aprtEdit.Btn_Del.onClick.RemoveAllListeners();
                aprtEdit.Btn_Del.onClick.AddListener(delegate { OnDelApartEditClick(j); });
                index++;
            }
            // 添加按钮应该滚到最后一边
            Emp_ApartAddTrans.SetAsLastSibling();
            // 自动更新滚动框宽度
            UpdateScrollerContentWidth(scrContentApartEditViewTrans);
        }
        else
        {
            // 不显示
            Img_ApartEditView.SetActive(false);
        }

        UpdateRoundInfoContent(); 
    }

    /// <summary>
    /// 根据下标从小轮表中选中指定轮并进入
    /// </summary>
    /// <param name="roundIndex"></param>
    private void SelectRound(int roundIndex)
    {
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // 请君入栈
        roundInfoStack.Push(currentRoundInfo.roundInfoList[roundIndex]);
        currentRoundIndexStack.Push(roundIndex);
        // 进入的一定是轮数编辑的面板
        StageEditorUI.SetActive(false);
        MouseEditorUI.SetActive(true);
        //FoodEditorUI.SetActive(false);
        // 更新一次UI面板显示
        UpdateUI();
    }

    /// <summary>
    /// 返回上级按钮监听
    /// </summary>
    private BaseRound.RoundInfo ReturnLast()
    {
        BaseRound.RoundInfo roundInfo = null;
        if (roundInfoStack.Count > 0) 
        {
            // 枪打出头鸟
            roundInfo = roundInfoStack.Pop();
            currentRoundIndexStack.Pop();
        }

        if (roundInfoStack.Count <= 0)
        {
            // 已在根目录，退回关卡编辑
            StageEditorUI.SetActive(true);
            MouseEditorUI.SetActive(false);
            //FoodEditorUI.SetActive(false);
        }
        // 更新一次UI面板显示
        UpdateUI();
        return roundInfo;
    }

    /// <summary>
    /// 保存当前更改至本地
    /// </summary>
    private void SaveAll()
    {
        BaseStage.Save(currentStageInfo);
    }

    /// <summary>
    /// 当关卡名文本输入框改变时，模型同步更新
    /// </summary>
    private void OnStageNameInputFieldChanged()
    {
        if (!isUIUpdateState)
        {
            currentStageInfo.name = InF_StageName.text;
        }
    }

    /// <summary>
    /// 在当前目录下添加一轮新的敌人
    /// </summary>
    private void AddNewRoundInfo()
    {
        BaseRound.RoundInfo roundInfo = GetInitalRoundInfo();
        if (roundInfoStack.Count <= 0)
        {
            if(currentStageInfo.roundInfoList == null)
            {
                currentStageInfo.roundInfoList = new List<RoundInfo>();
            }
            roundInfo.name = "第" + (currentStageInfo.roundInfoList.Count + 1) + "轮";
            currentStageInfo.roundInfoList.Add(roundInfo);
        }
        else
        {
            BaseRound.RoundInfo current = GetCurrentRoundInfo();
            if (current.roundInfoList == null)
            {
                current.roundInfoList = new List<RoundInfo>();
            }
            roundInfo.name = "第" + (current.roundInfoList.Count + 1) + "轮";
            current.roundInfoList.Add(roundInfo);
        }
        UpdateUI();
    }

    /// <summary>
    /// 删除当前轮
    /// </summary>
    private void DelCurrentRoundInfo()
    {
        // 根目录不能删除！
        if (roundInfoStack.Count > 0)
        {
            // 删除当前轮之前要返回上一层，然后在上一层的集合中把当前轮剔除
            BaseRound.RoundInfo willRemoveRoundInfo = ReturnLast();
            // 现在相当于返回上一级目录了，然后从上级目录中把这轮移除
            BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
            currentRoundInfo.roundInfoList.Remove(willRemoveRoundInfo);
            UpdateUI();
        }
        else
        {
            // 正常情况碰不到这里的
            Debug.LogError("根目录不能删除！");
        }
    }

    /// <summary>
    /// 当轮名称发生变更时
    /// </summary>
    private void OnRoundNameInFChange()
    {
        if (currentRoundIndexStack.Count > 0 && !isUIUpdateState)
        {
            Debug.Log("OnRoundNameInFChange");
            BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
            currentRoundInfo.name = Inf_RoundName.text;
        }
    }

    /// <summary>
    /// 当组间隔发生改变时
    /// </summary>
    private void OnRoundIntervalInFChange()
    {
        if (currentRoundIndexStack.Count > 0 && !isUIUpdateState)
        {
            Debug.Log("OnRoundIntervalInFChange");
            BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
            int i;
            if (!int.TryParse(Inf_Interval.text, out i))
            {
                Inf_Interval.text = i.ToString();
            };
            currentRoundInfo.interval = i;
        }
    }

    /// <summary>
    /// 当结束等待发生改变时
    /// </summary>
    private void OnRoundEndTimeInFChange()
    {
        if (currentRoundIndexStack.Count > 0 && !isUIUpdateState)
        {
            Debug.Log("OnRoundEndTimeInFChange");
            BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
            int i;
            if (!int.TryParse(Inf_EndTime.text, out i))
            {
                Inf_EndTime.text = i.ToString();
            };
            currentRoundInfo.endTime = i;
        }
    }

    /// <summary>
    /// 当准备时间发生改变时
    /// </summary>
    private void OnPrepareTimeInFChange()
    {
        if (!isUIUpdateState || StageEditorUI.activeSelf)
        {
            Debug.Log("OnPerpareTimeInFChange");
            int i;
            if (!int.TryParse(Inf_PrepareTime.text, out i))
            {
                Inf_PrepareTime.text = i.ToString();
            };
            currentStageInfo.perpareTime = i;
        }
    }
    
    /// <summary>
    /// 当默认游戏模式发生改变时
    /// </summary>
    private void OnDefaultModeDroChange(int new_index)
    {
        if (!isUIUpdateState || StageEditorUI.activeSelf)
        {
            Debug.Log("OnDefaultModeDroChange");
            currentStageInfo.defaultMode = (BaseStage.StageMode)new_index;
        }
    }

    /// <summary>
    /// 当某个分路组的编辑按钮被点击时
    /// </summary>
    private void OnApartEditOnClick(int select_index)
    {
        currentApart = currentStageInfo.apartList[select_index];
        currentApartIndex = select_index;
        UpdateUI();
    }

    /// <summary>
    /// 当添加新分路组按钮被点击时
    /// </summary>
    private void OnAddNewApartClick()
    {
        currentStageInfo.apartList.Add(new List<int>());
        UpdateUI();
    }

    /// <summary>
    /// 当删除当前分路组按钮被点击时
    /// </summary>
    private void OnDelApartClick()
    {
        if(currentApartIndex > -1 && currentApartIndex< currentStageInfo.apartList.Count)
        {
            currentStageInfo.apartList.RemoveAt(currentApartIndex);
            // 这里如果apartList为空时，currentApartIndex会被减到-1，即默认没有选择的状态
            if(currentApartIndex >= currentStageInfo.apartList.Count)
            {
                currentApartIndex--;
            }
            if(currentApartIndex > -1)
            {
                currentApart = currentStageInfo.apartList[currentApartIndex];
            }
            UpdateUI();
        }
    }

    /// <summary>
    /// 当删除分路编辑被点击时
    /// </summary>
    private void OnDelApartEditClick(int index)
    {
        currentApart.RemoveAt(index);
        UpdateUI();
    }

    /// <summary>
    /// 当添加分路编辑被点击时
    /// </summary>
    private void OnApartAddClick()
    {
        int index = emp_ApartAdd.GetIndex();
        if(index >= 0)
        {
            currentApart.Add(index);
        }
        UpdateUI();
    }

    /// <summary>
    /// 根据元素自动更新滚动面板的宽度
    /// </summary>
    /// <param name="content"></param>
    private static void UpdateScrollerContentWidth(Transform content)
    {
        HorizontalLayoutGroup hor = content.GetComponent<HorizontalLayoutGroup>();
        float width = hor.padding.left + hor.padding.right;
        for (int i = 0; i < content.transform.childCount; i++)
        {
            width += content.GetChild(i).GetComponent<RectTransform>().rect.width + hor.spacing;
        }
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(width, content.GetComponent<RectTransform>().rect.height);
    }

    /// <summary>
    /// 根据元素自动更新滚动面板的高度
    /// </summary>
    /// <param name="content"></param>
    private static void UpdateScrollerContentHeight(Transform content)
    {
        VerticalLayoutGroup ver = content.GetComponent<VerticalLayoutGroup>();
        float height = ver.padding.top + ver.padding.bottom;
        for (int i = 0; i < content.transform.childCount; i++)
        {
            height += content.GetChild(i).GetComponent<RectTransform>().rect.height + ver.spacing;
        }
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().rect.width, height);
    }

    /// <summary>
    /// 更新当前用来显示轮信息的面板
    /// </summary>
    private void UpdateRoundInfoContent()
    {
        // 获取当前持有轮信息
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // 清空一次中面板的按钮并且移除监听
        foreach (var item in roundBtnList)
        {
            item.onClick.RemoveAllListeners();
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Btn_RoundInfo", item.gameObject);
        }
        roundBtnList.Clear();
        // 更新中面板
        if (currentRoundInfo.roundInfoList != null)
        {
            int i = 0;
            int wave_index = 0;
            foreach (var item in currentRoundInfo.roundInfoList)
            {
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Btn_RoundInfo");
                go.transform.SetParent(scrContentRoundListTrans);
                go.transform.localScale = Vector3.one;
                go.transform.Find("Text").GetComponent<Text>().text = item.name;
                Button btn = go.GetComponent<Button>();
                roundBtnList.Add(btn);
                int j = i;
                btn.onClick.AddListener(() => { SelectRound(j); });
                Toggle toggle = go.transform.Find("Toggle").GetComponent<Toggle>();
                toggle.onValueChanged.RemoveAllListeners();
                // 只有在关卡编辑面板中的轮可以设置为一大波的标记
                if (StageEditorUI.activeSelf)
                {
                    toggle.gameObject.SetActive(true);
                    toggle.onValueChanged.AddListener(delegate { OnWaveToggleValueChange(toggle, j); });
                    if (wave_index < currentStageInfo.waveIndexList.Count && i == currentStageInfo.waveIndexList[wave_index])
                    {
                        wave_index++;
                        toggle.isOn = true;
                    }
                    else
                    {
                        toggle.isOn = false;
                    }
                }
                else
                {
                    toggle.gameObject.SetActive(false);
                }

                i++;
            }
            // 自动更新滚动框高度
            UpdateScrollerContentHeight(scrContentRoundListTrans);
        }
    }


    /// <summary>
    /// 当一大波的标记被改变时
    /// </summary>
    private void OnWaveToggleValueChange(Toggle toggle, int index)
    {
        if (isUIUpdateState)
            return;
        if (toggle.isOn)
        {
            if(currentStageInfo.waveIndexList.Count > 0)
            {
                for (int i = 0; i < currentStageInfo.waveIndexList.Count; i++)
                {
                    if (index < currentStageInfo.waveIndexList[i])
                    {
                        currentStageInfo.waveIndexList.Insert(i, index);
                        return;
                    } 
                }
            }
            currentStageInfo.waveIndexList.Add(index);
        }
        else
        {
            currentStageInfo.waveIndexList.Remove(index);
        }
    }



    /// <summary>
    /// 当添加一组敌人按钮被点击时
    /// </summary>
    private void OnAddEnemyGroupClick()
    {
        GetCurrentEnemyGroupList().Add(BaseEnemyGroup.GetInitalEnemyGroupInfo());
        UpdateUI();
    }

    /// <summary>
    /// 当删除当前组敌人按钮被点击时
    /// </summary>
    private void OnDelEnemyGroupClick(int index)
    {
        GetCurrentEnemyGroupList().RemoveAt(index);
        UpdateUI();
    }

    /// <summary>
    /// 当更换敌人种类按钮被点击时
    /// </summary>
    private void OnChangeEnemyTypeClick(int index)
    {
        scr_SelectEnemyType.gameObject.SetActive(true);
        scr_SelectEnemyType.SetEnemyGroup(GetCurrentEnemyGroupList()[index]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
