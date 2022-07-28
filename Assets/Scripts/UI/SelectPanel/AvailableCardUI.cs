using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ѡ�ؽ���-����ѡ�����-��ѡ�ÿ�Ƭ����
/// </summary>
public class AvailableCardUI : MonoBehaviour
{
    private SelectEquipmentUI mSelectEquipmentUI;

    /// <summary>
    /// ��ѡ��Ŀ�Ƭ�ֵ�
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
    /// �ӵ�ǰ�ؿ��м��ؿ���ѡ��Ŀ�����Ϣ
    /// </summary>
    public void LoadAvailableCardInfoFromStage(BaseStage.StageInfo stageInfo)
    {
        if (stageInfo.isEnableCardLimit)
        {
            // �п�Ƭ���������򰴿�Ƭ����������
            if (stageInfo.availableCardInfoList != null)
            {
                // ����������һ��
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
            // �����ȡȫ����Ƭ
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
    /// ��ȡ��UI��ָ����Ƭģ�͵���������
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AvailableCardInfo GetCardModelInfo(FoodNameTypeMap type)
    {
        if (availableCardDict.ContainsKey(type))
            return availableCardDict[type].GetAvailableCardInfo();
        else
            Debug.Log("δ�ҵ���Ӧ��Ƭģ�ͣ�");
        return null;
    }

    /// <summary>
    /// ��ȡ��UI��ָ����Ƭģ�͵ľ���λ��
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Vector3 GetCardModelPosition(FoodNameTypeMap type)
    {
        if (availableCardDict.ContainsKey(type))
            return availableCardDict[type].transform.position;
        else
            Debug.Log("δ�ҵ���Ӧ��Ƭģ�ͣ�");
        return Vector3.zero;
    }

    /// <summary>
    /// ���ÿ�Ƭģ�͵�ѡ��״̬
    /// </summary>
    /// <param name="type"></param>
    /// <param name="selected"></param>
    public void SetCardModelSelect(FoodNameTypeMap type, bool selected)
    {
        if (availableCardDict.ContainsKey(type))
            availableCardDict[type].SetSelected(selected);
        else
            Debug.Log("δ�ҵ���Ӧ��Ƭģ�ͣ�");
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
