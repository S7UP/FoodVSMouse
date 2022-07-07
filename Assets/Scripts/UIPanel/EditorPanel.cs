using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static BaseRound;

public class EditorPanel : BasePanel
{
    // ����
    private GameObject MouseEditorUI;
    private Transform scrEnemyContentRoundListTrans;
    private Transform scrContentEnemyGroupListTrans; // �Ҳ���ʾ������ť�Ĺ�������
    private Text roundNavigationText; // �Ϸ���������
    private Button Btn_Return; // �����ϼ���ť
    private Button Btn_Save; // ����
    private InputField InF_StageName; // ��ǰ�ؿ����ı������
    private Button Btn_AddRoundInfo; // ���һ�ֵ��˰�ť
    private Button Btn_DelRoundInfo; // ɾ����ǰ�ֵ��˰�ť
    private InputField Inf_RoundName; // ��ǰ������
    private InputField Inf_Interval; // ��ǰ����
    private InputField Inf_EndTime; // ��ǰ�����ȴ�ʱ��
    private Transform Emp_AddEnemyGroupTrans;
    private Button Btn_AddEnemyGroup; // ���һ����˰�ť
    private Scr_SelectEnemyType scr_SelectEnemyType;

    private GameObject StageEditorUI;
    private Transform scrStageContentRoundListTrans;
    private InputField Inf_PrepareTime; // �ؿ���ʼ׼��ʱ��
    private Dropdown Dro_DefaultMode; // �ؿ�ģʽѡ���
    private Transform scrContentApartListTrans; // ��·�黬����������
    private Button Btn_AddApart; // ���һ���µķ�·��
    private Button Btn_DelApart; // ɾ��ѡ����·��
    private Transform scrContentApartEditViewTrans; // ��·��Ԥ����������
    private Transform Emp_ApartAddTrans;
    private Emp_ApartAdd emp_ApartAdd;
    private GameObject Img_ApartEditView;
    private Button Btn_AddStageRoundInfo; // ���һ�����ֵ��˰�ť

    private Transform scrContentRoundListTrans; // ��ʾ�����Ĺ�������

    // ��ǰ���༭�Ĺؿ���Ϣ
    private BaseStage.StageInfo currentStageInfo;
    // ��ǰ�ּ�Ŀ¼����ջ
    private Stack<BaseRound.RoundInfo> roundInfoStack;
    // ��ǰ�����丸�����±�ջ
    private Stack<int> currentRoundIndexStack;
    // ��ǰ�����İ�ť����
    private List<Button> roundBtnList;
    // ��ǰ������鼯��
    private List<EnemyGroupUI> enemyGroupUIList;


    // ѡ���ķ�·��
    private List<int> currentApart;
    private int currentApartIndex;
    // ��ǰ��·��Object
    private List<GameObject> apartGoList;
    // ��ǰ��·��Ԥ�����Object
    private List<Emp_ApartEdit> apartEditGoList;

    // �Ƿ���UI�����е�״̬
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
        dataList.Add(new Dropdown.OptionData("�����ģʽ"));
        dataList.Add(new Dropdown.OptionData("�̶�ģʽ"));
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
    /// �ӱ��ؼ��عؿ����ݱ༭
    /// </summary>
    /// <param name="stageName"></param>
    public void LoadStage(string stageName)
    {
        currentStageInfo = BaseStage.Load(stageName);
        InF_StageName.text = currentStageInfo.name;
    }

    /// <summary>
    /// ��ȡ��ǰ���е�����Ϣ
    /// </summary>
    public BaseRound.RoundInfo GetCurrentRoundInfo()
    {
        if (roundInfoStack.Count <= 0)
        {
            // ջ��û��Ԫ���򽫹ؿ���������һ��ƴ�ճ�һ������
            return currentStageInfo.GetTotalRoundInfo();
        }
        else
        {
            // ����ȡջ��
            return roundInfoStack.Peek();
        }
    }

    /// <summary>
    /// ��ȡ��ǰ�����ʾ�ĵ�����
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

    // ����UI��ʾһ��
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
    /// ��ˢ�����ı༭
    /// </summary>
    private void UpdateMouseEditorUI()
    {
        // ���õ�ǰ��ʾ���������
        scrContentRoundListTrans = scrEnemyContentRoundListTrans;
        // ��ȡ��ǰ��������Ϣ
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // ���������
        Inf_RoundName.text = currentRoundInfo.name;
        Inf_Interval.text = currentRoundInfo.interval.ToString();
        Inf_EndTime.text = currentRoundInfo.endTime.ToString();

        UpdateRoundInfoContent();

        // ���һ�������
        foreach (var item in enemyGroupUIList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_EnemyGroup", item.gameObject);
        }
        enemyGroupUIList.Clear();
        // ���������
        if (currentRoundInfo.baseEnemyGroupList != null)
        {
            foreach (var item in currentRoundInfo.baseEnemyGroupList)
            {
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_EnemyGroup");
                go.transform.SetParent(scrContentEnemyGroupListTrans);
                go.transform.localScale = Vector3.one;
                EnemyGroupUI enemyGroupUI = go.GetComponent<EnemyGroupUI>();
                enemyGroupUI.SetEnemyGroup(item);
                // ɾ����ť���ü���
                {
                    Button btn = enemyGroupUI.GetDelButton();
                    btn.onClick.RemoveAllListeners();
                    int index = enemyGroupUIList.Count;
                    btn.onClick.AddListener(delegate { OnDelEnemyGroupClick(index); });
                }
                // �������˰�ť���ü���
                {
                    Button btn = enemyGroupUI.GetChangeEnemyTypeButton();
                    btn.onClick.RemoveAllListeners();
                    int index = enemyGroupUIList.Count;
                    btn.onClick.AddListener(delegate { OnChangeEnemyTypeClick(index); });
                }
                enemyGroupUIList.Add(enemyGroupUI);
            }
            Emp_AddEnemyGroupTrans.SetAsLastSibling();
            // �Զ����¹�������
            UpdateScrollerContentWidth(scrContentEnemyGroupListTrans);
        }



        // ���������
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
        text = "��ǰ������" + text;
        roundNavigationText.text = text;
    }

    /// <summary>
    /// �Ա༭�ؿ����ĸ���
    /// </summary>
    private void UpdateStageEditorUI()
    {
        // ���õ�ǰ��ʾ���������
        scrContentRoundListTrans = scrStageContentRoundListTrans;

        Inf_PrepareTime.text = currentStageInfo.perpareTime.ToString();
        Dro_DefaultMode.value = (int)currentStageInfo.defaultMode;

        // ���·�·�����
        {
            // ���
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
                tex.text = "��·" + (index + 1).ToString() + "��";
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
                // ��ɫ
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
            // �Զ����¹�����߶�
            UpdateScrollerContentHeight(scrContentApartListTrans);
        }


        // ���·�·Ԥ�����
        if (currentApart != null && currentApartIndex > -1)
        {
            Img_ApartEditView.SetActive(true);
            // ���
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
            // ��Ӱ�ťӦ�ù������һ��
            Emp_ApartAddTrans.SetAsLastSibling();
            // �Զ����¹�������
            UpdateScrollerContentWidth(scrContentApartEditViewTrans);
        }
        else
        {
            // ����ʾ
            Img_ApartEditView.SetActive(false);
        }

        UpdateRoundInfoContent(); 
    }

    /// <summary>
    /// �����±��С�ֱ���ѡ��ָ���ֲ�����
    /// </summary>
    /// <param name="roundIndex"></param>
    private void SelectRound(int roundIndex)
    {
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // �����ջ
        roundInfoStack.Push(currentRoundInfo.roundInfoList[roundIndex]);
        currentRoundIndexStack.Push(roundIndex);
        // �����һ���������༭�����
        StageEditorUI.SetActive(false);
        MouseEditorUI.SetActive(true);
        //FoodEditorUI.SetActive(false);
        // ����һ��UI�����ʾ
        UpdateUI();
    }

    /// <summary>
    /// �����ϼ���ť����
    /// </summary>
    private BaseRound.RoundInfo ReturnLast()
    {
        BaseRound.RoundInfo roundInfo = null;
        if (roundInfoStack.Count > 0) 
        {
            // ǹ���ͷ��
            roundInfo = roundInfoStack.Pop();
            currentRoundIndexStack.Pop();
        }

        if (roundInfoStack.Count <= 0)
        {
            // ���ڸ�Ŀ¼���˻عؿ��༭
            StageEditorUI.SetActive(true);
            MouseEditorUI.SetActive(false);
            //FoodEditorUI.SetActive(false);
        }
        // ����һ��UI�����ʾ
        UpdateUI();
        return roundInfo;
    }

    /// <summary>
    /// ���浱ǰ����������
    /// </summary>
    private void SaveAll()
    {
        BaseStage.Save(currentStageInfo);
    }

    /// <summary>
    /// ���ؿ����ı������ı�ʱ��ģ��ͬ������
    /// </summary>
    private void OnStageNameInputFieldChanged()
    {
        if (!isUIUpdateState)
        {
            currentStageInfo.name = InF_StageName.text;
        }
    }

    /// <summary>
    /// �ڵ�ǰĿ¼�����һ���µĵ���
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
            roundInfo.name = "��" + (currentStageInfo.roundInfoList.Count + 1) + "��";
            currentStageInfo.roundInfoList.Add(roundInfo);
        }
        else
        {
            BaseRound.RoundInfo current = GetCurrentRoundInfo();
            if (current.roundInfoList == null)
            {
                current.roundInfoList = new List<RoundInfo>();
            }
            roundInfo.name = "��" + (current.roundInfoList.Count + 1) + "��";
            current.roundInfoList.Add(roundInfo);
        }
        UpdateUI();
    }

    /// <summary>
    /// ɾ����ǰ��
    /// </summary>
    private void DelCurrentRoundInfo()
    {
        // ��Ŀ¼����ɾ����
        if (roundInfoStack.Count > 0)
        {
            // ɾ����ǰ��֮ǰҪ������һ�㣬Ȼ������һ��ļ����аѵ�ǰ���޳�
            BaseRound.RoundInfo willRemoveRoundInfo = ReturnLast();
            // �����൱�ڷ�����һ��Ŀ¼�ˣ�Ȼ����ϼ�Ŀ¼�а������Ƴ�
            BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
            currentRoundInfo.roundInfoList.Remove(willRemoveRoundInfo);
            UpdateUI();
        }
        else
        {
            // ������������������
            Debug.LogError("��Ŀ¼����ɾ����");
        }
    }

    /// <summary>
    /// �������Ʒ������ʱ
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
    /// �����������ı�ʱ
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
    /// �������ȴ������ı�ʱ
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
    /// ��׼��ʱ�䷢���ı�ʱ
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
    /// ��Ĭ����Ϸģʽ�����ı�ʱ
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
    /// ��ĳ����·��ı༭��ť�����ʱ
    /// </summary>
    private void OnApartEditOnClick(int select_index)
    {
        currentApart = currentStageInfo.apartList[select_index];
        currentApartIndex = select_index;
        UpdateUI();
    }

    /// <summary>
    /// ������·�·�鰴ť�����ʱ
    /// </summary>
    private void OnAddNewApartClick()
    {
        currentStageInfo.apartList.Add(new List<int>());
        UpdateUI();
    }

    /// <summary>
    /// ��ɾ����ǰ��·�鰴ť�����ʱ
    /// </summary>
    private void OnDelApartClick()
    {
        if(currentApartIndex > -1 && currentApartIndex< currentStageInfo.apartList.Count)
        {
            currentStageInfo.apartList.RemoveAt(currentApartIndex);
            // �������apartListΪ��ʱ��currentApartIndex�ᱻ����-1����Ĭ��û��ѡ���״̬
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
    /// ��ɾ����·�༭�����ʱ
    /// </summary>
    private void OnDelApartEditClick(int index)
    {
        currentApart.RemoveAt(index);
        UpdateUI();
    }

    /// <summary>
    /// ����ӷ�·�༭�����ʱ
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
    /// ����Ԫ���Զ����¹������Ŀ��
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
    /// ����Ԫ���Զ����¹������ĸ߶�
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
    /// ���µ�ǰ������ʾ����Ϣ�����
    /// </summary>
    private void UpdateRoundInfoContent()
    {
        // ��ȡ��ǰ��������Ϣ
        BaseRound.RoundInfo currentRoundInfo = GetCurrentRoundInfo();
        // ���һ�������İ�ť�����Ƴ�����
        foreach (var item in roundBtnList)
        {
            item.onClick.RemoveAllListeners();
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Btn_RoundInfo", item.gameObject);
        }
        roundBtnList.Clear();
        // ���������
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
                // ֻ���ڹؿ��༭����е��ֿ�������Ϊһ�󲨵ı��
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
            // �Զ����¹�����߶�
            UpdateScrollerContentHeight(scrContentRoundListTrans);
        }
    }


    /// <summary>
    /// ��һ�󲨵ı�Ǳ��ı�ʱ
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
    /// �����һ����˰�ť�����ʱ
    /// </summary>
    private void OnAddEnemyGroupClick()
    {
        GetCurrentEnemyGroupList().Add(BaseEnemyGroup.GetInitalEnemyGroupInfo());
        UpdateUI();
    }

    /// <summary>
    /// ��ɾ����ǰ����˰�ť�����ʱ
    /// </summary>
    private void OnDelEnemyGroupClick(int index)
    {
        GetCurrentEnemyGroupList().RemoveAt(index);
        UpdateUI();
    }

    /// <summary>
    /// �������������ఴť�����ʱ
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
