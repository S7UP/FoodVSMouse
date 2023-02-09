using UnityEngine;
using UnityEngine.UI;

public class TextArea : MonoBehaviour
{
    private static TextArea _Instance;
    public static TextArea Instance { get { if (_Instance == null) return GetInstance(); else return _Instance; } }

    private RectTransform RectTrans_TextArea;
    private RectTransform RectTrans_Text;
    private Text mText;

    public void Awake()
    {
        RectTrans_TextArea = GetComponent<RectTransform>();
        RectTrans_Text = transform.Find("Text").GetComponent<RectTransform>();
        mText = RectTrans_Text.GetComponent<Text>();
    }

    public void SetText(string s)
    {
        mText.text = s;
        // ����string, �Զ��仯�߶�
        int countPerRow = Mathf.FloorToInt(RectTrans_Text.sizeDelta.x / mText.fontSize); // �ȼ���ÿ�������ɶ�����
        // Ȼ������ж�����
        char[] c_arr = s.ToCharArray();
        int rowCount = 2;
        int fontCount = 0; // ��ǰ������
        for (int i = 0; i < c_arr.Length; i++)
        {
            fontCount++;
            if (fontCount >= countPerRow)
            {
                rowCount++;
                fontCount = 0;
            }
            else if(c_arr[i].Equals('\n'))
            {
                rowCount++;
                fontCount = 0;
            }
        }
        // ���ø߶�
        RectTrans_TextArea.sizeDelta = new Vector2(RectTrans_TextArea.sizeDelta.x, rowCount * mText.fontSize + 24);
    }

    public void SetLocalPosition(Transform masterTrans, Vector2 pos, Vector2 offsetRotate)
    {
        transform.SetParent(GameManager.Instance.GetUICanvas().transform);
        transform.localScale = Vector2.one;
        transform.position = (Vector2)masterTrans.position + (pos + new Vector2(offsetRotate.x * RectTrans_TextArea.sizeDelta.x, offsetRotate.y * RectTrans_TextArea.sizeDelta.y))/200;
    }

    public static void ExecuteRecycle()
    {
        if(_Instance !=null)
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "TextArea", _Instance.gameObject);
        _Instance = null;
    }

    private static TextArea GetInstance()
    {
        _Instance = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "TextArea").GetComponent<TextArea>();
        return _Instance;
    }
}
