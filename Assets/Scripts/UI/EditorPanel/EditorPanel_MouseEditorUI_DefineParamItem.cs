using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �༭�����-����༭���-�Զ���������-�Զ��������
/// </summary>
public class EditorPanel_MouseEditorUI_DefineParamItem : MonoBehaviour
{
    private InputField Inf_ParamName;
    private Button Btn_Del;
    private InputField Inf_ParamList;

    private EditorPanel_MouseEditorUI_DefineParamUI masterUI;
    public string ParamName; // key
    private List<float> valueList; // ����

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
            // �����ַ���
            string s = Inf_ParamList.text;
            string[] sList = s.Split(',');
            List<float> fList = new List<float>();
            // ÿ�ζ�ת�����֣���������Ƿ�������ԭ
            foreach (var item in sList)
            {
                float f;
                if(float.TryParse(item, out f))
                {
                    fList.Add(f);
                }
                else
                {
                    // �ߵ�����˵�������Ƿ�������
                    Inf_ParamList.text = ValueListToString();
                    return;
                }
            }
            // ���������˵���ɹ��ˣ�Ȼ���ڱ���ԭList���õ�ǰ�����޸�List�������
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
    /// ������ת�ַ�����ʽ��Ȼ������������
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
