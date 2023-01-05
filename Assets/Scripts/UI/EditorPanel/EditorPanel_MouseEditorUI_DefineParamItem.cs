using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑器面板-老鼠编辑面板-自定义参数面板-自定义参数项
/// </summary>
public class EditorPanel_MouseEditorUI_DefineParamItem : MonoBehaviour
{
    private InputField Inf_ParamName;
    private Button Btn_Del;
    private InputField Inf_ParamList;

    private EditorPanel_MouseEditorUI_DefineParamUI masterUI;
    public string ParamName; // key
    private List<float> valueList; // 引用

    public void Awake()
    {
        Inf_ParamName = transform.Find("Inf_ParamName").GetComponent<InputField>();
        Btn_Del = transform.Find("Btn_Del").GetComponent<Button>();
        Inf_ParamList = transform.Find("Inf_ParamList").GetComponent<InputField>();
    }

    private void MInit()
    {
        Btn_Del.onClick.RemoveAllListeners();
        Btn_Del.onClick.AddListener(delegate { masterUI.RemoveItem(this); });

        Inf_ParamName.onEndEdit.RemoveAllListeners();
        Inf_ParamName.text = ParamName;
        Inf_ParamName.onEndEdit.AddListener(delegate 
        { 
            if(masterUI.ModifyParamName(ParamName, Inf_ParamName.text))
                ParamName = Inf_ParamName.text;
            else
                Inf_ParamName.text = ParamName;
        });

        Inf_ParamList.onEndEdit.RemoveAllListeners();
        Inf_ParamList.text = ValueListToString();
        Inf_ParamList.onEndEdit.AddListener(delegate {
            // 解析字符串
            string s = Inf_ParamList.text;
            string[] sList = s.Split(',');
            List<float> fList = new List<float>();
            // 每段都转成数字，如果遇到非法输入则复原
            foreach (var item in sList)
            {
                float f;
                if(float.TryParse(item, out f))
                {
                    fList.Add(f);
                }
                else
                {
                    // 走到这里说明遇到非法输入了
                    Inf_ParamList.text = ValueListToString();
                    return;
                }
            }
            // 如果到这里说明成功了，然后，在保护原List引用的前提下修改List里的内容
            valueList.Clear();
            foreach (var item in fList)
            {
                valueList.Add(item);
            }
        });
    }

    private void MDestory()
    {
        masterUI = null;
        ParamName = default;
        valueList = default;
    }

    /// <summary>
    /// 参数表转字符串形式，然后输出到面板上
    /// </summary>
    /// <returns></returns>
    private string ValueListToString()
    {
        string s = "";
        for (int i = 0; i < valueList.Count; i++)
        {
            s += valueList[i].ToString();
            if (i < valueList.Count - 1)
                s += ",";
        }
        return s;
    }

    public static EditorPanel_MouseEditorUI_DefineParamItem GetInstance(EditorPanel_MouseEditorUI_DefineParamUI masterUI, string ParamName, List<float> valueList)
    {
        EditorPanel_MouseEditorUI_DefineParamItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "EditorPanel/EditorPanel_MouseEditorUI_DefineParamItem").GetComponent<EditorPanel_MouseEditorUI_DefineParamItem>();
        item.masterUI = masterUI;
        item.ParamName = ParamName;
        item.valueList = valueList;
        item.MInit();
        return item;
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "EditorPanel/EditorPanel_MouseEditorUI_DefineParamItem", gameObject);
        MDestory();
    }


}
