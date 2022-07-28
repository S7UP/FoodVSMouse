using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 选关界面-配置选择界面-可选用卡片界面
/// </summary>
public class AvailableCardUI : MonoBehaviour
{
    private SelectEquipmentUI mSelectEquipmentUI;

    /// <summary>
    /// 可选择的卡片字典
    /// </summary>
    private Dictionary<FoodNameTypeMap, Btn_AvailableCard> availableCardDict = new Dictionary<FoodNameTypeMap, Btn_AvailableCard>();
    private Transform contentTrans;

    private void Awake()
    {
        contentTrans = transform.Find("Img_center").Find("Emp_Container").Find("Scr").Find("Viewport").Find("Content");
    }

    public void Initial()
    {
        foreach (var item in availableCardDict)
        {
            item.Value.ExecuteRecycle();
        }
        availableCardDict.Clear();
    }

    /// <summary>
    /// 从当前关卡中加载可以选择的卡组信息
    /// </summary>
    public void LoadAvailableCardInfoFromStage(BaseStage.StageInfo stageInfo)
    {
        if (stageInfo.isEnableCardLimit)
        {
            // 有卡片限制条件则按卡片限制条件来
            if (stageInfo.availableCardInfoList != null)
            {
                // 先升序排序一下
                stageInfo.SortAvailableCardInfoList();
                foreach (var info in stageInfo.availableCardInfoList)
                {
                    if (!availableCardDict.ContainsKey((FoodNameTypeMap)info.type))
                    {
                        Btn_AvailableCard c = Btn_AvailableCard.GetInstance();
                        c.Initial();
                        c.UpdateByAvailableCardInfo(info);
                        c.transform.SetParent(contentTrans);
                        c.transform.localScale = Vector3.one;
                        availableCardDict.Add((FoodNameTypeMap)info.type, c);
                        c.AddListener(delegate { mSelectEquipmentUI.SelectCard((FoodNameTypeMap)c.GetAvailableCardInfo().type); });
                    }
                }
            }
        }
        else
        {
            // 否则读取全部卡片
            Dictionary<FoodNameTypeMap, List<string>> dict = FoodManager.GetAllFoodDict();
            foreach (var keyValuePair in dict)
            {
                List<string> l = keyValuePair.Value;
                AvailableCardInfo info = new AvailableCardInfo(((int)keyValuePair.Key), l.Count - 1, 16);
                Btn_AvailableCard c = Btn_AvailableCard.GetInstance();
                c.Initial();
                c.UpdateByAvailableCardInfo(info);
                c.transform.SetParent(contentTrans);
                c.transform.localScale = Vector3.one;
                availableCardDict.Add((FoodNameTypeMap)info.type, c);
                c.AddListener(delegate { mSelectEquipmentUI.SelectCard((FoodNameTypeMap)c.GetAvailableCardInfo().type); });
            }
        }
    }

    /// <summary>
    /// 获取该UI下指定卡片模型的限制条件
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AvailableCardInfo GetCardModelInfo(FoodNameTypeMap type)
    {
        if (availableCardDict.ContainsKey(type))
            return availableCardDict[type].GetAvailableCardInfo();
        else
            Debug.Log("未找到对应卡片模型！");
        return null;
    }

    /// <summary>
    /// 获取该UI下指定卡片模型的绝对位置
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Vector3 GetCardModelPosition(FoodNameTypeMap type)
    {
        if (availableCardDict.ContainsKey(type))
            return availableCardDict[type].transform.position;
        else
            Debug.Log("未找到对应卡片模型！");
        return Vector3.zero;
    }

    /// <summary>
    /// 设置卡片模型的选择状态
    /// </summary>
    /// <param name="type"></param>
    /// <param name="selected"></param>
    public void SetCardModelSelect(FoodNameTypeMap type, bool selected)
    {
        if (availableCardDict.ContainsKey(type))
            availableCardDict[type].SetSelected(selected);
        else
            Debug.Log("未找到对应卡片模型！");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectEquipmentUI(SelectEquipmentUI ui)
    {
        mSelectEquipmentUI = ui;
    }
}
