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
    
    // ����
    private Transform scrContentSelectEnemyTypeTrans; // ѡ����˹�������
    private GameObject Emp_ReturnSelectEnemyType; // �����ϼ�ѡ���������
    private Canvas canvas;
    public EditorPanel editorPanel;

    // �󶨵ĵ�����
    private BaseEnemyGroup mEnemyGroup;
    // ��ǰѡ��ĵ�������
    private int selectedEnemyType;
    // ��ǰѡ��������˵ı���
    private int selectedEnemyShape;
    // ��һ�������Ϣ
    private List<SelectEnemyInfo> typeList;
    // �ڶ��������Ϣ
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
        // �ӱ��ض�ȡ���������
        //Application.streamingAssetsPath

        // ��ȡ����Json�ļ�����ѡ����˽���
        ConstructView();
    }

    /// <summary>
    /// ��������ʱȡ��EnemyGroup��
    /// </summary>
    private void OnDisable()
    {
        mEnemyGroup = null;
        selectedEnemyType = -1;
        selectedEnemyShape = -1;
        // ��յ�ǰUI���
        foreach (var item in itemList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_SelectEnemyItem", item.gameObject);
        }
        itemList.Clear();
    }

    /// <summary>
    /// ���ð󶨵ĵ�����
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
    /// ��ȡ��ӦType�������е��±�
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
    /// ������󶨵�EnemyGroupͬ��
    /// </summary>
    public void UpdateSelectedEnemyShape()
    {
        selectedEnemyShape = mEnemyGroup.mEnemyInfo.shape;
    }

    /// <summary>
    /// ��UI���и���
    /// </summary>
    public void UpdateUIAndModel()
    {
        // ��յ�ǰUI���
        foreach (var item in itemList)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Emp_SelectEnemyItem", item.gameObject);
        }
        itemList.Clear();

        // ����UI
        if(selectedEnemyType == -1)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        if (selectedEnemyShape == -1)
        {
            // ѡ������������
            Emp_ReturnSelectEnemyType.SetActive(false);
            int i = 0;
            foreach (var info in typeList)
            {
                Emp_SelectEnemyItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_SelectEnemyItem").GetComponent<Emp_SelectEnemyItem>();
                item.SetMaster(this.gameObject);
                item.SetSprite(info.sprite); // �޸���ʾ��ͼ
                item.SetArrayIndex(itemList.Count);
                item.transform.SetParent(scrContentSelectEnemyTypeTrans);
                item.transform.localScale = Vector3.one;
                // ��ѡ���������ʾ
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
            // ѡ����˱��ֽ���
            Emp_ReturnSelectEnemyType.SetActive(true);
            int selectIndex = GetSelectEnemyTypeArrayIndex();
            if(selectIndex > -1)
            {
                foreach (var info in shapeList[selectIndex])
                {
                    Emp_SelectEnemyItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_SelectEnemyItem").GetComponent<Emp_SelectEnemyItem>();
                    item.SetMaster(this.gameObject);
                    item.SetSprite(info.sprite); // �޸���ʾ��ͼ
                    item.SetArrayIndex(itemList.Count);
                    item.transform.SetParent(scrContentSelectEnemyTypeTrans);
                    item.transform.localScale = Vector3.one;
                    // ��ѡ���������ʾ
                    if (selectedEnemyShape == info.attr.shape)
                    {
                        item.SetSelected(true);
                    }
                    itemList.Add(item);
                }
                // ���·����ϼ���ͼƬ
                Emp_ReturnSelectEnemyType.GetComponent<Image>().sprite = typeList[selectIndex].sprite;
                // �����ϼ���ťʼ��������ǰ
                Emp_ReturnSelectEnemyType.transform.SetAsFirstSibling();
            }
            // �������ģ�ͳɹ��ˣ����û������ѡ������һ���ˣ��������ظ�obj
            if (UpdateModel())
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ����ʵ��
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
    /// ����ѡ����˱���ҳ���ϵ�������ϼ����水ťʱ
    /// </summary>
    private void OnReturnSelectEnemeyTypeClick()
    {
        selectedEnemyShape = -1;
        UpdateUIAndModel();
    }

    /// <summary>
    /// ��ȡ����Json�ļ�����ѡ�����ҳ��
    /// </summary>
    private void ConstructView()
    {
        //DirectoryInfo direction = new DirectoryInfo(Application.dataPath + "/Resources/Json/Mouse"); // ��ȡJSON�ļ����µ������ļ�
        DirectoryInfo direction = new DirectoryInfo(Application.streamingAssetsPath + "/Json/Mouse"); // ��ȡJSON�ļ����µ������ļ�
        FileInfo[] files = direction.GetFiles("*");
        foreach (var typeFile in files)
        {
            Debug.Log("typeFileName = " + typeFile.Name);
            int type = int.Parse(typeFile.Name.Replace(typeFile.Extension, "")); // ȥ��׺
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
                // ֻ��ȡJSON
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
        // �������UI��Χ֮�����������θ�UI
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
