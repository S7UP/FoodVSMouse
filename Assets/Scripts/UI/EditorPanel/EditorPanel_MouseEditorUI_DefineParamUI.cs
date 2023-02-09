using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �༭�����-����༭���-�Զ���������
/// </summary>
public class EditorPanel_MouseEditorUI_DefineParamUI : MonoBehaviour
{
    private Transform Trans_Content;
    private RectTransform RectTrans_Content;
    private Button Btn_Add;
    private Button Btn_Exit;

    private float Item_height = 55;
    private float Btn_height = 32;

    private List<EditorPanel_MouseEditorUI_DefineParamItem> itemList = new List<EditorPanel_MouseEditorUI_DefineParamItem>();
    private Dictionary<string, List<float>> ParamDict; // �����ֵ����ã����Ե�ǰEnemyGroup��

    private void Awake()
    {
        Trans_Content = transform.Find("MainUI").Find("Img_center").Find("Emp_Container").Find("Scr").Find("Viewport").Find("Content");
        RectTrans_Content = Trans_Content.GetComponent<RectTransform>();
        Btn_Add = Trans_Content.transform.Find("Btn_Add").GetComponent<Button>();
        Btn_Exit = transform.Find("Btn_Exit").GetComponent<Button>();
    }

    public void Initial()
    {
        foreach (var item in itemList)
        {
            item.ExecuteRecycle();
        }
        itemList.Clear();
        // �Ѳ����ֵ��������ʾ��UI��
        foreach (var keyValuePair in ParamDict)
        {
            AddItemToUI(keyValuePair.Key, keyValuePair.Value);
        }

        Btn_Add.onClick.RemoveAllListeners();
        Btn_Add.onClick.AddListener(delegate {
            AddItem();
        });

        Btn_Exit.onClick.RemoveAllListeners();
        Btn_Exit.onClick.AddListener(delegate {
            Hide();
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Initial();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddItem()
    {
        // ��֤Ĭ�ϲ�����������
        int index = itemList.Count;
        string key = "Param" + index;
        while (ParamDict.ContainsKey(key))
        {
            index++;
            key = "Param" + index;
        }

        List<float> list = new List<float>();
        // �ȼ����ֵ�
        ParamDict.Add(key, list);
        // Ȼ������������ʾ��ʵ��
        AddItemToUI(key, list);

        UpdateContentRect();
    }

    /// <summary>
    /// ����������ʾ��ʵ�壨һ�����飩
    /// </summary>
    /// <param name="key"></param>
    /// <param name="list"></param>
    private void AddItemToUI(string key, List<float> list)
    {
        EditorPanel_MouseEditorUI_DefineParamItem item = EditorPanel_MouseEditorUI_DefineParamItem.GetInstance(this, key, list);
        itemList.Add(item);
        item.transform.SetParent(Trans_Content);
        item.transform.localScale = Vector2.one;
        item.transform.SetAsLastSibling();
        Btn_Add.transform.SetAsLastSibling();
    }

    public void RemoveItem(EditorPanel_MouseEditorUI_DefineParamItem item)
    {
        if (itemList.Contains(item))
        {
            itemList.Remove(item);
            ParamDict.Remove(item.ParamName); // �ֵ�ͬ��ɾ��
            item.ExecuteRecycle();
            UpdateContentRect();
        }
    }

    /// <summary>
    /// �޸�ĳ��������
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns>�������棬����³ɹ��������ǲ�����ĳ�����������·�����</returns>
    public bool ModifyParamName(string oldName, string newName)
    {
        if (!ParamDict.ContainsKey(oldName) || ParamDict.ContainsKey(newName))
            return false;
        
        List<float> list = ParamDict[oldName];
        ParamDict.Remove(oldName);
        ParamDict.Add(newName, list);
        return true;
    }

    /// <summary>
    /// ���������������Content�Ĵ�С
    /// </summary>
    private void UpdateContentRect()
    {
        RectTrans_Content.sizeDelta = new Vector2(RectTrans_Content.sizeDelta.x, itemList.Count * Item_height + Btn_height);
    }

    /// <summary>
    /// ���ò����ֵ�����
    /// </summary>
    public void SetParamDict(Dictionary<string, List<float>> ParamDict)
    {
        this.ParamDict = ParamDict;
    }
}
