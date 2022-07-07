using UnityEngine;
/// <summary>
/// 铲子模型
/// </summary>
public class ShovelModel : MonoBehaviour, ICanvasRaycastFilter
{
    private GameObject mImg_Model; // 跟随鼠标的模型
    private GameObject mImg_Virtual; // 铲子预铲除虚像
    private bool isUse; // 是否使用

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
        // 虚像复原
        mImg_Model.transform.localPosition = Vector3.zero;
        mImg_Virtual.transform.localPosition = Vector3.zero;
        mImg_Virtual.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (isUse)
        {
            // 铲子模型位置跟随鼠标
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position); //将对象坐标换成屏幕坐标
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = pos.z;
            mImg_Model.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //将赋值了Z的鼠标屏幕坐标转世界坐标的同时赋值给对象
                                                                                           // 铲子预铲除位置更新（取当前鼠标悬停格）
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
    /// 不遮挡射线
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return false;
    }
}
