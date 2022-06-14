using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static BaseEnemyGroup;

public class EnemyGroupUI : MonoBehaviour
{
    // 属性
    public BaseEnemyGroup mBaseEnemyGroup;

    // 引用
    private GameObject Emp_MaskList;
    private Image Img_Mouse;
    private Image[] Img_MaskList;
    private Text Tex_Type;
    private Text Tex_Shape;
    private Dropdown Dro_StartIndex;
    private Dropdown Dro_Number;
    private Button Btn_Del;
    private Button Btn_EnemyType;
    private RectTransform rectTransform;

    private void Awake()
    {
        // 引用初始化
        Emp_MaskList = transform.Find("Img_Mouse").Find("Emp_MaskList").gameObject;
        Img_Mouse = transform.Find("Img_Mouse").GetComponent<Image>();
        Img_MaskList = new Image[Emp_MaskList.transform.childCount];
        for (int i = 0; i < Emp_MaskList.transform.childCount; i++)
        {
            Img_MaskList[i] = Emp_MaskList.transform.GetChild(i).GetComponent<Image>();
            Img_MaskList[i].enabled = false; // 默认都先隐藏掉
        }
        Tex_Type = transform.Find("Img_Mouse").Find("Emp_Info").Find("Tex_Type").GetComponent<Text>();
        Tex_Shape = transform.Find("Img_Mouse").Find("Emp_Info").Find("Tex_Shape").GetComponent<Text>();
        Dro_StartIndex = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_StartIndex").Find("Dropdown").GetComponent<Dropdown>();
        Dro_Number = transform.Find("Img_Mouse").Find("Emp_Info").Find("Emp_Number").Find("Dropdown").GetComponent<Dropdown>();
        rectTransform = transform.GetComponent<RectTransform>();
        // 下拉框初始化
        Dro_StartIndex.ClearOptions();
        Dro_Number.ClearOptions();
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

        Btn_Del = transform.Find("Img_Mouse").Find("Emp_Info").Find("Btn_Del").GetComponent<Button>();
        Btn_EnemyType = transform.Find("Img_Mouse").GetComponent<Button>();
        //DisplayUpdate();
    }

    /// <summary>
    /// 获取删除按钮引用
    /// </summary>
    /// <returns></returns>
    public Button GetDelButton()
    {
        return Btn_Del;
    }

    /// <summary>
    /// 获取更换敌人种类按钮引用
    /// </summary>
    /// <returns></returns>
    public Button GetChangeEnemyTypeButton()
    {
        return Btn_EnemyType;
    }

    /// <summary>
    /// 初始行下拉框被点时的事件
    /// </summary>
    private void Dro_StartIndexOnValueChanged()
    {
        mBaseEnemyGroup.mStartIndex = Dro_StartIndex.value;
        DisplayUpdate();
    }

    /// <summary>
    /// 数量下拉框被点时的事件
    /// </summary>
    private void Dro_NumberOnValueChanged()
    {
        mBaseEnemyGroup.mCount = Dro_Number.value;
        DisplayUpdate();
    }


    /// <summary>
    /// 从外部设置绑定的EnemyGroup
    /// </summary>
    /// <param name="baseEnemyGroup"></param>
    public void SetEnemyGroup(BaseEnemyGroup baseEnemyGroup)
    {
        mBaseEnemyGroup = baseEnemyGroup;
        ChangeMouseInfo(baseEnemyGroup.mEnemyInfo);
        DisplayUpdate();
        // 下拉框选项更新
        Dro_StartIndex.value = mBaseEnemyGroup.mStartIndex;
        Dro_Number.value = mBaseEnemyGroup.mCount;
    }

    public void ChangeMouseInfo(BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        mBaseEnemyGroup.mEnemyInfo = enemyInfo;
        // 修改贴图
        Img_Mouse.sprite = GameManager.Instance.GetSprite("Mouse/"+enemyInfo.type+"/"+enemyInfo.shape+"/display");
        Img_Mouse.SetNativeSize();
        // 设置rect
        float w = Img_Mouse.GetComponent<RectTransform>().rect.width + Emp_MaskList.GetComponent<RectTransform>().rect.width;
        rectTransform.localPosition = new Vector3(w/2, rectTransform.localPosition.y, rectTransform.localPosition.z);
        rectTransform.sizeDelta = new Vector2(w, rectTransform.sizeDelta.y);
    }



    // 对显示进行一次更新
    public void DisplayUpdate()
    {
        // 种类与变种显示更新
        Tex_Type.text = "种类编号："+mBaseEnemyGroup.mEnemyInfo.type;
        Tex_Shape.text = "变种编号：" + mBaseEnemyGroup.mEnemyInfo.shape;

        // 先隐藏所有方块
        for (int i = 0; i < Img_MaskList.Length; i++)
        {
            Img_MaskList[i].enabled = false;
        }

        // 根据组修改方块颜色

        // 根据老鼠初始行以及数量来隐藏和显示方块
        int startIndex = mBaseEnemyGroup.mStartIndex;
        int rowCount = 7; // 当前分路组的行数
        for (int i = 0; i < mBaseEnemyGroup.mCount; i++)
        {
            int index = (startIndex + i)% rowCount;
            Img_MaskList[index].enabled = true;
        }

    }

    /// <summary>
    /// 当被对象池回收时，执行初始化操作
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
