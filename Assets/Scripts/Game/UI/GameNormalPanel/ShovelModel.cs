using UnityEngine;
/// <summary>
/// ����ģ��
/// </summary>
public class ShovelModel : MonoBehaviour, ICanvasRaycastFilter
{
    private GameObject mImg_Model; // ��������ģ��
    private GameObject mImg_Virtual; // ����Ԥ��������
    private bool isUse; // �Ƿ�ʹ��

    private void Awake()
    {
        mImg_Model = transform.Find("Img_Model").gameObject;
        mImg_Virtual = transform.Find("Img_Virtual").gameObject;

        HideModel();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    public void ShowModel()
    {
        //gameObject.SetActive(true);
        isUse = true;
    }

    public void HideModel()
    {
        //gameObject.SetActive(false);
        isUse = false;
        // ����ԭ
        mImg_Model.transform.localPosition = Vector3.zero;
        mImg_Virtual.transform.localPosition = Vector3.zero;
        mImg_Virtual.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (isUse)
        {
            // ����ģ��λ�ø������
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position); //���������껻����Ļ����
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = pos.z;
            mImg_Model.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //����ֵ��Z�������Ļ����ת���������ͬʱ��ֵ������
                                                                                           // ����Ԥ����λ�ø��£�ȡ��ǰ�����ͣ��
            BaseGrid overGrid = GameController.Instance.GetOverGrid();
            if (overGrid != null)
            {
                mImg_Virtual.SetActive(true);
                mImg_Virtual.transform.position = overGrid.GetUnitInPosition(Vector2.zero);
            }
            else
            {
                mImg_Virtual.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ���ڵ�����
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return false;
    }
}
