using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class EnemyGroupUI : MonoBehaviour
{
    // ����
    public BaseEnemyGroup mBaseEnemyGroup;

    // ����
    private EditorPanel mEditorPanel;
    private GameObject Emp_MaskList;
    private Image Img_Mouse;
    private Image[] Img_MaskList;
    private Text Tex_Type;
    private Text Tex_Shape;
    private Dropdown Dro_StartIndex;
    private Dropdown Dro_Number;
    private Dropdown Dro_ApartIndex;
    private Button Btn_Del;
    private Button Btn_EnemyType;
    private RectTransform rectTransform;
    private GameObject Emp_Hp;
    private InputField Inf_Hp;

    private void Awake()
    {
        // ���ó�ʼ��
        Emp_MaskList = transform.Find("Img_Mouse").Find("Emp_MaskList").gameObject;
        Img_Mouse = transform.Find("Img_Mouse").GetComponent<Image>();
        Img_MaskList = new Image[Emp_MaskList.transform.childCount];
        for (int i = 0; i < Emp_MaskList.transform.childCount; i++)
        {
            Img_MaskList[i] = Emp_MaskList.transform.GetChild(i).GetComponent<Image>();
            Img_MaskList[i].enabled = false; // Ĭ�϶������ص�
        }
        Tex_Type = transform.Find("Img_Mouse").Find("Emp_Info").Find("Tex_Type").GetComponent<Text>();
        Tex_Shape = transform.Find("Img_Mouse").Find("Emp_Info").Find("Tex_Shape").GetComponent<Text>();
        Dro_StartIndex = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_StartIndex").Find("Dropdown").GetComponent<Dropdown>();
        Dro_Number = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_Number").Find("Dropdown").GetComponent<Dropdown>();
        Dro_ApartIndex = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_ApartIndex").Find("Dropdown").GetComponent<Dropdown>();
        rectTransform = transform.GetComponent<RectTransform>();

        Btn_Del = transform.Find("Img_Mouse").Find("Emp_Info").Find("Btn_Del").GetComponent<Button>();
        Btn_EnemyType = transform.Find("Img_Mouse").GetComponent<Button>();

        Emp_Hp = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_Hp").gameObject;
        Inf_Hp = Emp_Hp.transform.Find("InputField").GetComponent<InputField>();
    }

    public void Initial()
    {
        Dro_StartIndex.gameObject.SetActive(true);
        Dro_Number.gameObject.SetActive(true);
        Dro_ApartIndex.gameObject.SetActive(true);

        Dro_StartIndex.onValueChanged.RemoveAllListeners();
        Dro_Number.onValueChanged.RemoveAllListeners();
        Dro_ApartIndex.onValueChanged.RemoveAllListeners();
        // �������ʼ��
        Dro_StartIndex.ClearOptions();
        Dro_Number.ClearOptions();
        Dro_ApartIndex.ClearOptions();
        // ����
        Dro_StartIndex.onValueChanged.RemoveAllListeners();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i < 7; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = (i + 1).ToString();
            dataList.Add(data);
        }
        Dro_StartIndex.AddOptions(dataList);
        Dro_StartIndex.value = 0;
        Dro_StartIndex.onValueChanged.AddListener(delegate
        {
            Dro_StartIndexOnValueChanged();
        });
        // ����
        Dro_Number.onValueChanged.RemoveAllListeners();
        List<Dropdown.OptionData> dataList2 = new List<Dropdown.OptionData>();
        for (int i = 0; i <= 7; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = (i).ToString();
            dataList2.Add(data);
        }
        Dro_Number.AddOptions(dataList2);                                                              
        Dro_Number.value = 1;
        Dro_Number.onValueChanged.AddListener(delegate
        {
            Dro_NumberOnValueChanged();
        });
        // HP
        Inf_Hp.onEndEdit.RemoveAllListeners();
        Inf_Hp.onEndEdit.AddListener(delegate 
        {
            float value = int.Parse(Inf_Hp.text);
            if (value <= 0)
                value = 100;
            Inf_HpOnValueChanged(value);
        });
        // ��·��
        Dro_ApartIndex.onValueChanged.RemoveAllListeners();
        List<Dropdown.OptionData> dataList3 = new List<Dropdown.OptionData>();
        for (int i = 0; i < mEditorPanel.GetCurrentStageInfo().apartList.Count; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = (i + 1).ToString();
            dataList3.Add(data);
        }
        Dro_ApartIndex.AddOptions(dataList3);
        Dro_ApartIndex.value = 0;
        Dro_ApartIndex.onValueChanged.AddListener(delegate
        {
            Dro_ApartIndexOnValueChanged();
        });
    }

    /// <summary>
    /// ��ȡɾ����ť����
    /// </summary>
    /// <returns></returns>
    public Button GetDelButton()
    {
        return Btn_Del;
    }

    /// <summary>
    /// ��ȡ�����������ఴť����
    /// </summary>
    /// <returns></returns>
    public Button GetChangeEnemyTypeButton()
    {
        return Btn_EnemyType;
    }

    /// <summary>
    /// ��ʼ��������ֵ�����ı���¼�
    /// </summary>
    private void Dro_StartIndexOnValueChanged()
    {
        mBaseEnemyGroup.mStartIndex = Dro_StartIndex.value;
        DisplayUpdate();
    }

    /// <summary>
    /// ����������ֵ�����ı���¼�
    /// </summary>
    private void Dro_NumberOnValueChanged()
    {
        mBaseEnemyGroup.mCount = Dro_Number.value;
        DisplayUpdate();
    }

    /// <summary>
    /// Ѫ�������ֵ�����ı���¼�
    /// </summary>
    private void Inf_HpOnValueChanged(float hp)
    {
        mBaseEnemyGroup.mHp = hp;
        DisplayUpdate();
    }

    /// <summary>
    /// ��·��������ֵ�����ı��
    /// </summary>
    private void Dro_ApartIndexOnValueChanged()
    {
        mBaseEnemyGroup.mApartIndex = Dro_ApartIndex.value;
        DisplayUpdate();
    }


    /// <summary>
    /// ���ⲿ���ð󶨵�EnemyGroup
    /// </summary>
    /// <param name="baseEnemyGroup"></param>
    public void SetEnemyGroup(BaseEnemyGroup baseEnemyGroup)
    {
        mBaseEnemyGroup = baseEnemyGroup;
        ChangeMouseInfo(baseEnemyGroup.mEnemyInfo);
        DisplayUpdate();
        // ������ѡ�����
        Dro_StartIndex.value = mBaseEnemyGroup.mStartIndex;
        if (mEditorPanel.GetCurrentRoundInfo().isBossRound)
        {
            // �����BOSS�֣���ȡ��������ʾ��ͬ����BOSSֻ����1ֻ��
            Dro_Number.gameObject.SetActive(false);
            // ��ʾѪ������
            Inf_Hp.gameObject.SetActive(true);
            Inf_Hp.text = mBaseEnemyGroup.mHp.ToString();
        }
        else
        {
            Dro_Number.gameObject.SetActive(true);
            // ����Ѫ������
            Inf_Hp.gameObject.SetActive(false);

            Dro_Number.value = mBaseEnemyGroup.mCount;
        }
        // ����о�ˢ����Խ���ˣ���ô������Ϊ0
        if (mBaseEnemyGroup.mApartIndex > Dro_ApartIndex.options.Count - 1)
        {
            mBaseEnemyGroup.mApartIndex = 0;
        }
        Dro_ApartIndex.value = mBaseEnemyGroup.mApartIndex;
    }

    public void ChangeMouseInfo(BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        mBaseEnemyGroup.mEnemyInfo = enemyInfo;
        // �޸���ͼ
        if (mEditorPanel.GetCurrentRoundInfo().isBossRound)
            Img_Mouse.sprite = GameManager.Instance.GetSprite("Boss/" + enemyInfo.type + "/" + enemyInfo.shape + "/display");
        else
            Img_Mouse.sprite = GameManager.Instance.GetSprite("Mouse/" + enemyInfo.type + "/" + enemyInfo.shape + "/display");
        Img_Mouse.SetNativeSize();
        // ����rect
        float w = Img_Mouse.GetComponent<RectTransform>().rect.width + Emp_MaskList.GetComponent<RectTransform>().rect.width;
        rectTransform.localPosition = new Vector3(w/2, rectTransform.localPosition.y, rectTransform.localPosition.z);
        rectTransform.sizeDelta = new Vector2(w, rectTransform.sizeDelta.y);
    }



    // ����ʾ����һ�θ���
    public void DisplayUpdate()
    {
        // �����������ʾ����
        Tex_Type.text = "�����ţ�"+mBaseEnemyGroup.mEnemyInfo.type;
        Tex_Shape.text = "���ֱ�ţ�" + mBaseEnemyGroup.mEnemyInfo.shape;

        // ���������з���
        for (int i = 0; i < Img_MaskList.Length; i++)
        {
            Img_MaskList[i].enabled = false;
        }

        // �������޸ķ�����ɫ

        // ���������ʼ���Լ����������غ���ʾ����
        int startIndex = mBaseEnemyGroup.mStartIndex;
        int rowCount = 7; // ��ǰ��·�������
        for (int i = 0; i < mBaseEnemyGroup.mCount; i++)
        {
            int index = (startIndex + i)% rowCount;
            Img_MaskList[index].enabled = true;
        }

    }

    public void SetEditorPanel(EditorPanel panel)
    {
        mEditorPanel = panel;
    }

    /// <summary>
    /// ��������ػ���ʱ��ִ�г�ʼ������
    /// </summary>
    private void OnDisable()
    {
        mBaseEnemyGroup = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
