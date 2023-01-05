using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class EditorPanel_StartCardEditorUI : MonoBehaviour
{
    private EditorPanel mEditorPanel;

    private Transform Trans_EditArea;
    private GameObject GridItem; // 单格物件预制体，用于克隆

    private GameObject Go_SelectCardTypeUI;
    private Transform Trans_ScrContent;
    private GameObject Btn_FoodDisplay; // 可选美食单位展示预制体，用于克隆
    private Dictionary<FoodNameTypeMap, GameObject> Btn_FoodDisplayDict = new Dictionary<FoodNameTypeMap, GameObject>(); // 用于存放可选美食单位克隆对象
    private Button Btn_Exit;


    private GameObject[][] gridItemArray = new GameObject[9][]; // 存放右侧每个格子包含的实体对象
    private List<AvailableCardInfo>[][] startCardInfoList;

    private Vector2 currentSelectedGridItemVector2; // 当前选中格对应的格子坐标

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

        // 加载所有可选格
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
                // startCardInfoList初始每个List为空，因为带着空的初始化只会初始化出空的格子表示
                // GridItemUpdate(i, j);
                // 获取对应的button，然后分配给button功能
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

        // 请按顺序排列
        for (int j = 0; j < 7; j++)
        {
            for (int i = 0; i < 9; i++)
            {
                gridItemArray[i][j].transform.SetParent(Trans_EditArea);
                gridItemArray[i][j].transform.localScale = Vector2.one;
            }
        }

        // 加载所有可选美食
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
    /// 每次进入这个界面都要初始化一次
    /// </summary>
    public void Initial()
    {
        // 获取引用
        startCardInfoList = mEditorPanel.GetCurrentStageInfo().startCardInfoList;
        // 所有格子状态更新一次
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 7; j++)
                GridItemUpdate(i, j);

        // 设置0 0的格子为默认编辑状态
        OnEditButtonClick(0, 0);
        SelectCardTypeUIUpdate();
    }


    /// <summary>
    /// 当编辑按钮被点击时
    /// </summary>
    private void OnEditButtonClick(int xIndex, int yIndex) 
    {
        // 先将所有项置未选中
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                GameObject obj = gridItemArray[i][j];
                obj.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
        }

        // 把当前项置为选中
        {
            GameObject obj = gridItemArray[xIndex][yIndex];
            obj.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        }
       

        currentSelectedGridItemVector2 = new Vector2(xIndex, yIndex);
        // 显示SelectCardTypeUI
        Go_SelectCardTypeUI.SetActive(true);
    }

    /// <summary>
    /// 当左侧某个卡被点时
    /// </summary>
    private void OnFoodDisplayButtonClick(int type)
    {
        List<AvailableCardInfo> typeShapeList = startCardInfoList[(int)currentSelectedGridItemVector2.x][(int)currentSelectedGridItemVector2.y];

        // 先去检查当前选中格子有没有 这个卡 同类型的卡
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
            // 如果有的话，则取消选中该框框并移出集合
            typeShapeList.Remove(removeInfo);
            if (addInfo != null)
            {
                // 如果没有，则选中该框框并加入集合
                typeShapeList.Add(addInfo);
            }
        }
        else
        {
            addInfo = new AvailableCardInfo(type, 0, 0);
            typeShapeList.Add(addInfo);
        }

        // 更新一次左侧面板
        SelectCardTypeUIUpdate();
        // 更新当前格子的卡片预览显示
        GridItemUpdate((int)currentSelectedGridItemVector2.x, (int)currentSelectedGridItemVector2.y);
    }

    /// <summary>
    /// 更新一次左侧选卡UI
    /// </summary>
    private void SelectCardTypeUIUpdate()
    {
        // 把所有选择按钮UI置空
        foreach (var keyValuePair in Btn_FoodDisplayDict)
        {
            GameObject obj = keyValuePair.Value;
            obj.GetComponent<Image>().color = Color.white;
        }

        // 去当前已选择卡的表中找，并将每个结果对应的UI设为选中
        List<AvailableCardInfo> typeShapeList = startCardInfoList[(int)currentSelectedGridItemVector2.x][(int)currentSelectedGridItemVector2.y];
        foreach (var info in typeShapeList)
        {
            GameObject obj = Btn_FoodDisplayDict[(FoodNameTypeMap)info.type];
            obj.GetComponent<Image>().color = Color.yellow;
        }

    }

    /// <summary>
    /// 更新指定格子
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    private void GridItemUpdate(int xIndex, int yIndex)
    {
        Transform content = gridItemArray[xIndex][yIndex].transform.Find("Content");
        // 子项先暂时全部置空
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }
        // 根据对应的表来决定要克隆拓展子项吗
        for (int i = content.childCount; i < startCardInfoList[xIndex][yIndex].Count; i++)
        {
            GameObject obj = Instantiate(content.GetChild(0).gameObject);
            obj.transform.SetParent(content);
            obj.transform.localScale = Vector2.one * 0.5f;
            obj.transform.localPosition = Vector2.zero;
        }
        // 根据对应的表来激活子项，并填充子项的内容
        for (int i = 0; i < startCardInfoList[xIndex][yIndex].Count; i++)
        {
            GameObject img = content.GetChild(i).gameObject;
            img.SetActive(true);
            AvailableCardInfo info = startCardInfoList[xIndex][yIndex][i];
            img.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/" + info.type + "/" + info.maxShape + "/display");
        }

    }
}
