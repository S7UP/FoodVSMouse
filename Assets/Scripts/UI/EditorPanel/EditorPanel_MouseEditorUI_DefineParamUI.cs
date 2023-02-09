using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑器面板-老鼠编辑面板-自定义参数面板
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
    private Dictionary<string, List<float>> ParamDict; // 参数字典引用（来自当前EnemyGroup）

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
        // 把参数字典的内容显示到UI上
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
        // 保证默认参数名不重名
        int index = itemList.Count;
        string key = "Param" + index;
        while (ParamDict.ContainsKey(key))
        {
            index++;
            key = "Param" + index;
        }

        List<float> list = new List<float>();
        // 先加入字典
        ParamDict.Add(key, list);
        // 然后生成用于显示的实体
        AddItemToUI(key, list);

        UpdateContentRect();
    }

    /// <summary>
    /// 生成用于显示的实体（一个数组）
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
            ParamDict.Remove(item.ParamName); // 字典同步删除
            item.ExecuteRecycle();
            UpdateContentRect();
        }
    }

    /// <summary>
    /// 修改某个参数名
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns>若返回真，则更新成功；否则是不符合某个条件，无事发生；</returns>
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
    /// 根据子项情况更新Content的大小
    /// </summary>
    private void UpdateContentRect()
    {
        RectTrans_Content.sizeDelta = new Vector2(RectTrans_Content.sizeDelta.x, itemList.Count * Item_height + Btn_height);
    }

    /// <summary>
    /// 设置参数字典引用
    /// </summary>
    public void SetParamDict(Dictionary<string, List<float>> ParamDict)
    {
        this.ParamDict = ParamDict;
    }
}
