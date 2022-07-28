using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Scr_SelectEnemyType : MonoBehaviour
{
    private struct SelectEnemyInfo
    {
        public BaseUnit.Attribute attr;
        public Sprite sprite; 
    };
    
    // 引用
    private Transform scrContentSelectEnemyTypeTrans; // 选择敌人滚动窗体
    private GameObject Emp_ReturnSelectEnemyType; // 返回上级选择敌人种类
    private Canvas canvas;
    public EditorPanel editorPanel;

    // 绑定的敌人组
    private BaseEnemyGroup mEnemyGroup;
    // 当前选择的敌人种类
    private int selectedEnemyType;
    // 当前选择种类敌人的变种
    private int selectedEnemyShape;
    // 第一层敌人信息
    private List<SelectEnemyInfo> typeList;
    // 第二层敌人信息
    private List<List<SelectEnemyInfo>> shapeList;
    // ItemList
    private List<Emp_SelectEnemyItem> itemList;


    private void Awake()
    {
        scrContentSelectEnemyTypeTrans = transform.Find("Viewport").Find("Content");
        Emp_ReturnSelectEnemyType = scrContentSelectEnemyTypeTrans.Find("Emp_ReturnLast").gameObject;
        Emp_ReturnSelectEnemyType.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { OnReturnSelectEnemeyTypeClick(); });
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        selectedEnemyType = -1;
        selectedEnemyShape = -1;

        typeList = new List<SelectEnemyInfo>();
        shapeList = new List<List<SelectEnemyInfo>>();
        itemList = new List<Emp_SelectEnemyItem>();
        // 从本地读取种类与变种
        //Application.streamingAssetsPath

        // 读取本地Json文件构建选择敌人界面
        ConstructView();
    }

    /// <summary>
    /// 当被禁用时取消EnemyGroup绑定
    /// </summary>
    private void OnDisable()
    {
        mEnemyGroup = null;
        selectedEnemyType = -1;
        selectedEnemyShape = -1;
        // 清空当前UI面板
        foreach (var item in itemList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_SelectEnemyItem", item.gameObject);
        }
        itemList.Clear();
    }

    /// <summary>
    /// 设置绑定的敌人组
    /// </summary>
    /// <param name="enemyGroup"></param>
    public void SetEnemyGroup(BaseEnemyGroup enemyGroup)
    {
        mEnemyGroup = enemyGroup;
        selectedEnemyType = enemyGroup.mEnemyInfo.type;
        selectedEnemyShape = -1;
        UpdateUIAndModel();
    }

    /// <summary>
    /// 获取对应Type在数组中的下标
    /// </summary>
    /// <returns></returns>
    private int GetSelectEnemyTypeArrayIndex()
    {
        int selectIndex = 0;
        foreach (var info in typeList)
        {
            if (info.attr.type == selectedEnemyType)
                return selectIndex;
            selectIndex++;
        }
        return -1;
    }

    public int GetSelectedEnemyType()
    {
        return selectedEnemyType;
    }

    public void SetSelectedEnemyTypeByArrayIndex(int arrayIndex)
    {
        selectedEnemyType = typeList[arrayIndex].attr.type;
    }

    public int GetSelectedEnemyShape()
    {
        return selectedEnemyShape;
    }

    public void SetSelectedEnemyShapeByArrayIndex(int arrayIndex)
    {
        selectedEnemyShape = shapeList[GetSelectEnemyTypeArrayIndex()][arrayIndex].attr.shape;
    }

    /// <summary>
    /// 设置与绑定的EnemyGroup同步
    /// </summary>
    public void UpdateSelectedEnemyShape()
    {
        selectedEnemyShape = mEnemyGroup.mEnemyInfo.shape;
    }

    /// <summary>
    /// 对UI进行更新
    /// </summary>
    public void UpdateUIAndModel()
    {
        // 清空当前UI面板
        foreach (var item in itemList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_SelectEnemyItem", item.gameObject);
        }
        itemList.Clear();

        // 更新UI
        if(selectedEnemyType == -1)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        if (selectedEnemyShape == -1)
        {
            // 选择敌人种类界面
            Emp_ReturnSelectEnemyType.SetActive(false);
            int i = 0;
            foreach (var info in typeList)
            {
                Emp_SelectEnemyItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_SelectEnemyItem").GetComponent<Emp_SelectEnemyItem>();
                item.SetMaster(this.gameObject);
                item.SetSprite(info.sprite); // 修改显示贴图
                item.SetArrayIndex(itemList.Count);
                item.transform.SetParent(scrContentSelectEnemyTypeTrans);
                item.transform.localScale = Vector3.one;
                // 被选中项高亮显示
                if (selectedEnemyType == info.attr.type)
                {
                    item.SetSelected(true);
                }
                itemList.Add(item);
                i++;
            }
        }
        else
        {
            // 选择敌人变种界面
            Emp_ReturnSelectEnemyType.SetActive(true);
            int selectIndex = GetSelectEnemyTypeArrayIndex();
            if(selectIndex > -1)
            {
                foreach (var info in shapeList[selectIndex])
                {
                    Emp_SelectEnemyItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_SelectEnemyItem").GetComponent<Emp_SelectEnemyItem>();
                    item.SetMaster(this.gameObject);
                    item.SetSprite(info.sprite); // 修改显示贴图
                    item.SetArrayIndex(itemList.Count);
                    item.transform.SetParent(scrContentSelectEnemyTypeTrans);
                    item.transform.localScale = Vector3.one;
                    // 被选中项高亮显示
                    if (selectedEnemyShape == info.attr.shape)
                    {
                        item.SetSelected(true);
                    }
                    itemList.Add(item);
                }
                // 更新返回上级的图片
                Emp_ReturnSelectEnemyType.GetComponent<Image>().sprite = typeList[selectIndex].sprite;
                // 返回上级按钮始终置于最前
                Emp_ReturnSelectEnemyType.transform.SetAsFirstSibling();
            }
            // 如果更新模型成功了（即用户在这层选择了任一敌人），则隐藏该obj
            if (UpdateModel())
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    private bool UpdateModel()
    {
        bool flag = false;
        if(selectedEnemyType != -1 && selectedEnemyType != mEnemyGroup.mEnemyInfo.type)
        {
            mEnemyGroup.mEnemyInfo.type = selectedEnemyType;
            flag = true;
        }
        if (selectedEnemyShape != -1 && selectedEnemyShape != mEnemyGroup.mEnemyInfo.shape)
        {
            mEnemyGroup.mEnemyInfo.shape = selectedEnemyShape;
            flag = true;
        }
        if (flag)
            editorPanel.UpdateUI();
        return flag;
    }



    /// <summary>
    /// 当在选择敌人变种页面上点击返回上级界面按钮时
    /// </summary>
    private void OnReturnSelectEnemeyTypeClick()
    {
        selectedEnemyShape = -1;
        UpdateUIAndModel();
    }

    /// <summary>
    /// 读取本地Json文件构建选择敌人页面
    /// </summary>
    private void ConstructView()
    {
        //DirectoryInfo direction = new DirectoryInfo(Application.dataPath + "/Resources/Json/Mouse"); // 获取JSON文件夹下的所有文件
        DirectoryInfo direction = new DirectoryInfo(Application.streamingAssetsPath + "/Json/Mouse"); // 获取JSON文件夹下的所有文件
        FileInfo[] files = direction.GetFiles("*");
        foreach (var typeFile in files)
        {
            Debug.Log("typeFileName = " + typeFile.Name);
            int type = int.Parse(typeFile.Name.Replace(typeFile.Extension, "")); // 去后缀
            {
                string path = "Mouse/" + typeFile.Name.Replace(typeFile.Extension, "") + "/0";
                Debug.Log("path = " + path);
                // MouseUnit.Attribute attr = JsonManager.Load<MouseUnit.Attribute>(path);
                MouseUnit.Attribute attr = GameManager.Instance.attributeManager.GetMouseUnitAttribute(type, 0);
                typeList.Add(new SelectEnemyInfo()
                {
                    attr = attr.baseAttrbute,
                    sprite = GameManager.Instance.GetSprite("Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "/icon")
                });
            }
            List<SelectEnemyInfo> oneShapeList = new List<SelectEnemyInfo>();
            shapeList.Add(oneShapeList);

            //FileInfo[] shapeFiles = new DirectoryInfo(Application.dataPath + "/Resources/Json/Mouse/"+type).GetFiles("*");
            FileInfo[] shapeFiles = new DirectoryInfo(Application.streamingAssetsPath + "/Json/Mouse/" + type).GetFiles("*");
            foreach (var f in shapeFiles)
            {
                Debug.Log("shapeFileName = " + f.FullName);
                string name = f.Name.Replace(f.Extension, "");
                // 只读取JSON
                if (name.EndsWith(".json"))
                {
                    Debug.Log("Name = " + name);
                    int shape = int.Parse(name.Replace(".json", ""));
                    {
                        string path = "Mouse/" + typeFile.Name.Replace(typeFile.Extension, "") + "/" + shape;
                        // MouseUnit.Attribute attr = JsonManager.Load<MouseUnit.Attribute>(path);
                        MouseUnit.Attribute attr = GameManager.Instance.attributeManager.GetMouseUnitAttribute(type, shape);
                        oneShapeList.Add(new SelectEnemyInfo()
                        {
                            attr = attr.baseAttrbute,
                            sprite = GameManager.Instance.GetSprite("Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "/icon")
                        });
                    }
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 当鼠标在UI范围之外点击，则屏蔽该UI
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("mouseposition:" + pos);
            RectTransform rect = GetComponent<RectTransform>();
            if (pos.x > rect.position.x + rect.rect.width / 200 || pos.x < rect.position.x - rect.rect.width / 200 || pos.y > rect.position.y + rect.rect.height / 200 || pos.y < rect.position.y - rect.rect.height / 200)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
