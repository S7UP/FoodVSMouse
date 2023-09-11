using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

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
    // ��һ�����BOSS��Ϣ
    private List<SelectEnemyInfo> bossTypeList;
    // �ڶ������BOSS��Ϣ
    private List<List<SelectEnemyInfo>> bossShapeList;
    // ItemList
    private List<Emp_SelectEnemyItem> itemList;


    private void Awake()
    {
        scrContentSelectEnemyTypeTrans = transform.Find("Viewport").Find("Content");
        Emp_ReturnSelectEnemyType = scrContentSelectEnemyTypeTrans.Find("Emp_ReturnLast").gameObject;
        Emp_ReturnSelectEnemyType.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { OnReturnSelectEnemeyTypeClick(); });

        selectedEnemyType = -1;
        selectedEnemyShape = -1;

        typeList = new List<SelectEnemyInfo>();
        shapeList = new List<List<SelectEnemyInfo>>();
        itemList = new List<Emp_SelectEnemyItem>();
        bossTypeList = new List<SelectEnemyInfo>();
        bossShapeList = new List<List<SelectEnemyInfo>>();
        // �ӱ��ض�ȡ���������

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

    private List<SelectEnemyInfo> GetTypeList()
    {
        if (editorPanel.GetCurrentRoundInfo().isBossRound)
            return bossTypeList;
        else
            return typeList;
    }

    private List<List<SelectEnemyInfo>> GetShapeList()
    {
        if (editorPanel.GetCurrentRoundInfo().isBossRound)
            return bossShapeList;
        else
            return shapeList;
    }

    /// <summary>
    /// ��ȡ��ӦType�������е��±�
    /// </summary>
    /// <returns></returns>
    private int GetSelectEnemyTypeArrayIndex()
    {
        int selectIndex = 0;
        foreach (var info in GetTypeList())
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
        selectedEnemyType = GetTypeList()[arrayIndex].attr.type;
    }

    public int GetSelectedEnemyShape()
    {
        return selectedEnemyShape;
    }

    public void SetSelectedEnemyShapeByArrayIndex(int arrayIndex)
    {
        selectedEnemyShape = GetShapeList()[GetSelectEnemyTypeArrayIndex()][arrayIndex].attr.shape;
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
            foreach (var info in GetTypeList())
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
                foreach (var info in GetShapeList()[selectIndex])
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
                Emp_ReturnSelectEnemyType.GetComponent<Image>().sprite = GetTypeList()[selectIndex].sprite;
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
    /// ��ȡ��������Ȼ�󹹽�ѡ�����ҳ��
    /// </summary>
    private void ConstructView()
    {
        // ��ȡС��
        foreach (var keyValuePair in MouseManager.GetAttributeDict())
        {
            MouseNameTypeMap type = keyValuePair.Key;
            Dictionary<int, MouseManager.MouseAttribute> shapeAttrDict = keyValuePair.Value;
            MouseManager.MouseAttribute attr = MouseManager.GetAttribute(type, 0);
            typeList.Add(new SelectEnemyInfo()
            {
                attr = new BaseUnit.Attribute() {
                    name = attr.name,
                    type = (int)type,
                    shape = 0, 
                },
                sprite = GameManager.Instance.GetSprite("Mouse/" + (int)type + "/" + 0 + "/icon")
            });

            List<SelectEnemyInfo> oneShapeList = new List<SelectEnemyInfo>();
            shapeList.Add(oneShapeList);
            foreach (var keyValuePair2 in shapeAttrDict)
            {
                int shape = keyValuePair2.Key;
                MouseManager.MouseAttribute attr2 = MouseManager.GetAttribute(type, shape);
                oneShapeList.Add(new SelectEnemyInfo()
                {
                    attr = new BaseUnit.Attribute()
                    {
                        name = attr2.name,
                        type = (int)type,
                        shape = shape,
                    },
                    sprite = GameManager.Instance.GetSprite("Mouse/" + (int)type + "/" + shape + "/icon")
                });
            }
        }

        // ��ȡBOSS
        foreach (var keyValuePair in BossManager.GetBossNameDict())
        {
            BossNameTypeMap type = keyValuePair.Key;
            MouseUnit.Attribute attr = GameManager.Instance.attributeManager.GetBossUnitAttribute((int)type, 0);
            bossTypeList.Add(new SelectEnemyInfo()
            {
                attr = attr.baseAttrbute,
                sprite = GameManager.Instance.GetSprite("Boss/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "/icon")
            });

            List<SelectEnemyInfo> oneShapeList = new List<SelectEnemyInfo>();
            bossShapeList.Add(oneShapeList);
            foreach (var keyValuePair2 in keyValuePair.Value)
            {
                int shape = keyValuePair2.Key;
                MouseUnit.Attribute attr2 = GameManager.Instance.attributeManager.GetBossUnitAttribute((int)type, shape);
                oneShapeList.Add(new SelectEnemyInfo()
                {
                    attr = attr2.baseAttrbute,
                    sprite = GameManager.Instance.GetSprite("Boss/" + attr2.baseAttrbute.type + "/" + attr2.baseAttrbute.shape + "/icon")
                });
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
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 pos = canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
        //    Debug.Log("mouseposition:" + pos);
        //    RectTransform rect = GetComponent<RectTransform>();
        //    if (pos.x > rect.position.x + rect.rect.width / 200 || pos.x < rect.position.x - rect.rect.width / 200 || pos.y > rect.position.y + rect.rect.height / 200 || pos.y < rect.position.y - rect.rect.height / 200)
        //    {
        //        gameObject.SetActive(false);
        //    }
        //}
    }
}
