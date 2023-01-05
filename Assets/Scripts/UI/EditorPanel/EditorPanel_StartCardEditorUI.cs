using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class EditorPanel_StartCardEditorUI : MonoBehaviour
{
    private EditorPanel mEditorPanel;

    private Transform Trans_EditArea;
    private GameObject GridItem; // �������Ԥ���壬���ڿ�¡

    private GameObject Go_SelectCardTypeUI;
    private Transform Trans_ScrContent;
    private GameObject Btn_FoodDisplay; // ��ѡ��ʳ��λչʾԤ���壬���ڿ�¡
    private Dictionary<FoodNameTypeMap, GameObject> Btn_FoodDisplayDict = new Dictionary<FoodNameTypeMap, GameObject>(); // ���ڴ�ſ�ѡ��ʳ��λ��¡����
    private Button Btn_Exit;


    private GameObject[][] gridItemArray = new GameObject[9][]; // ����Ҳ�ÿ�����Ӱ�����ʵ�����
    private List<AvailableCardInfo>[][] startCardInfoList;

    private Vector2 currentSelectedGridItemVector2; // ��ǰѡ�и��Ӧ�ĸ�������

    private void Awake()
    {
        Trans_EditArea = transform.Find("EditArea");
        GridItem = Trans_EditArea.Find("GridItem").gameObject;

        Go_SelectCardTypeUI = transform.Find("SelectCardTypeUI").gameObject;
        Trans_ScrContent = Go_SelectCardTypeUI.transform.Find("Scr").Find("Viewport").Find("Content");
        Btn_FoodDisplay = Trans_ScrContent.Find("Btn_FoodDisplay").gameObject;

        Btn_Exit = transform.Find("Btn_Exit").GetComponent<Button>();
        Btn_Exit.onClick.AddListener(delegate {
            mEditorPanel.SetStartCardEditorUIEnable(false);
        });

        // �������п�ѡ��
        for (int i = 0; i < 9; i++)
        {
            gridItemArray[i] = new GameObject[7];
            for (int j = 0; j < 7; j++)
            {
                if (i == 0 && j == 0)
                    gridItemArray[i][j] = GridItem;
                else
                {
                    gridItemArray[i][j] = GameObject.Instantiate(GridItem);
                }
                GameObject obj = gridItemArray[i][j];
                // startCardInfoList��ʼÿ��ListΪ�գ���Ϊ���ſյĳ�ʼ��ֻ���ʼ�����յĸ��ӱ�ʾ
                // GridItemUpdate(i, j);
                // ��ȡ��Ӧ��button��Ȼ������button����
                Button btn = obj.transform.Find("Button").GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int xIndex = i;
                int yIndex = j;
                btn.onClick.AddListener(delegate {
                    OnEditButtonClick(xIndex, yIndex);
                    SelectCardTypeUIUpdate();
                });
            }
        }

        // �밴˳������
        for (int j = 0; j < 7; j++)
        {
            for (int i = 0; i < 9; i++)
            {
                gridItemArray[i][j].transform.SetParent(Trans_EditArea);
                gridItemArray[i][j].transform.localScale = Vector2.one;
            }
        }

        // �������п�ѡ��ʳ
        {
            int i = 0;
            foreach (var keyValuePair in FoodManager.GetAllBuildableFoodDict())
            {
                if (i == 0)
                {
                    Btn_FoodDisplayDict.Add(keyValuePair.Key, Btn_FoodDisplay);
                }
                else
                {
                    GameObject o = Instantiate(Btn_FoodDisplay.gameObject);
                    Btn_FoodDisplayDict.Add(keyValuePair.Key, o);
                    o.transform.SetParent(Trans_ScrContent);
                    o.transform.localScale = Vector2.one;
                }
                GameObject obj = Btn_FoodDisplayDict[keyValuePair.Key];
                obj.transform.Find("Image").GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/" + (int)keyValuePair.Key + "/0/display");
                Button btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int type = (int)keyValuePair.Key;
                btn.onClick.AddListener(delegate {
                    OnFoodDisplayButtonClick(type);
                });
                i++;
            }
        }

    }

    public void SetEditorPanel(EditorPanel p)
    {
        mEditorPanel = p;
    }

    /// <summary>
    /// ÿ�ν���������涼Ҫ��ʼ��һ��
    /// </summary>
    public void Initial()
    {
        // ��ȡ����
        startCardInfoList = mEditorPanel.GetCurrentStageInfo().startCardInfoList;
        // ���и���״̬����һ��
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 7; j++)
                GridItemUpdate(i, j);

        // ����0 0�ĸ���ΪĬ�ϱ༭״̬
        OnEditButtonClick(0, 0);
        SelectCardTypeUIUpdate();
    }


    /// <summary>
    /// ���༭��ť�����ʱ
    /// </summary>
    private void OnEditButtonClick(int xIndex, int yIndex) 
    {
        // �Ƚ���������δѡ��
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                GameObject obj = gridItemArray[i][j];
                obj.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
        }

        // �ѵ�ǰ����Ϊѡ��
        {
            GameObject obj = gridItemArray[xIndex][yIndex];
            obj.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        }
       

        currentSelectedGridItemVector2 = new Vector2(xIndex, yIndex);
        // ��ʾSelectCardTypeUI
        Go_SelectCardTypeUI.SetActive(true);
    }

    /// <summary>
    /// �����ĳ��������ʱ
    /// </summary>
    private void OnFoodDisplayButtonClick(int type)
    {
        List<AvailableCardInfo> typeShapeList = startCardInfoList[(int)currentSelectedGridItemVector2.x][(int)currentSelectedGridItemVector2.y];

        // ��ȥ��鵱ǰѡ�и�����û�� ����� ͬ���͵Ŀ�
        bool removeFlag = false;
        AvailableCardInfo removeInfo = null;
        AvailableCardInfo addInfo = null;
        foreach (var info in typeShapeList)
        {
            if(info.type == type)
            {
                removeFlag = true;
                removeInfo = info;
                break;
            }

            FoodInGridType t = BaseCardBuilder.GetFoodInGridType(type);
            if (BaseCardBuilder.GetFoodInGridType(info.type) == t)
            {
                removeFlag = true;
                removeInfo = info;
                addInfo = new AvailableCardInfo(type, 0, 0);
                break;
            }
        }
        
        if (removeFlag)
        {
            // ����еĻ�����ȡ��ѡ�иÿ���Ƴ�����
            typeShapeList.Remove(removeInfo);
            if (addInfo != null)
            {
                // ���û�У���ѡ�иÿ�򲢼��뼯��
                typeShapeList.Add(addInfo);
            }
        }
        else
        {
            addInfo = new AvailableCardInfo(type, 0, 0);
            typeShapeList.Add(addInfo);
        }

        // ����һ��������
        SelectCardTypeUIUpdate();
        // ���µ�ǰ���ӵĿ�ƬԤ����ʾ
        GridItemUpdate((int)currentSelectedGridItemVector2.x, (int)currentSelectedGridItemVector2.y);
    }

    /// <summary>
    /// ����һ�����ѡ��UI
    /// </summary>
    private void SelectCardTypeUIUpdate()
    {
        // ������ѡ��ťUI�ÿ�
        foreach (var keyValuePair in Btn_FoodDisplayDict)
        {
            GameObject obj = keyValuePair.Value;
            obj.GetComponent<Image>().color = Color.white;
        }

        // ȥ��ǰ��ѡ�񿨵ı����ң�����ÿ�������Ӧ��UI��Ϊѡ��
        List<AvailableCardInfo> typeShapeList = startCardInfoList[(int)currentSelectedGridItemVector2.x][(int)currentSelectedGridItemVector2.y];
        foreach (var info in typeShapeList)
        {
            GameObject obj = Btn_FoodDisplayDict[(FoodNameTypeMap)info.type];
            obj.GetComponent<Image>().color = Color.yellow;
        }

    }

    /// <summary>
    /// ����ָ������
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    private void GridItemUpdate(int xIndex, int yIndex)
    {
        Transform content = gridItemArray[xIndex][yIndex].transform.Find("Content");
        // ��������ʱȫ���ÿ�
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }
        // ���ݶ�Ӧ�ı�������Ҫ��¡��չ������
        for (int i = content.childCount; i < startCardInfoList[xIndex][yIndex].Count; i++)
        {
            GameObject obj = Instantiate(content.GetChild(0).gameObject);
            obj.transform.SetParent(content);
            obj.transform.localScale = Vector2.one * 0.5f;
            obj.transform.localPosition = Vector2.zero;
        }
        // ���ݶ�Ӧ�ı������������������������
        for (int i = 0; i < startCardInfoList[xIndex][yIndex].Count; i++)
        {
            GameObject img = content.GetChild(i).gameObject;
            img.SetActive(true);
            AvailableCardInfo info = startCardInfoList[xIndex][yIndex][i];
            img.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/" + info.type + "/" + info.maxShape + "/display");
        }

    }
}
