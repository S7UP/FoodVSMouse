using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 负责拖拽放卡时的显示处理
/// </summary>
public class CardModel : MonoBehaviour
{
    private GameObject mImg_CardModel; // 跟随鼠标的卡片模型
    private GameObject mImg_Virtual; // 卡片预建造虚像

    private void Awake()
    {
        mImg_CardModel = transform.Find("Img_CardModel").gameObject;
        mImg_Virtual = transform.Find("Img_Virtual").gameObject;
    }

    /// <summary>
    /// 每当被激活时，应当是选卡成功了，因此要把自身的图标置为被选卡片建造器的图标
    /// </summary>
    private void OnEnable()
    {
        UpdatePosition();
        // 获取卡片建造器上显示卡片的图标
        Image img = GameController.Instance.mCardController.GetSelectCardBuilder().mImg_Card.GetComponent<Image>();
        // 设置图标
        mImg_CardModel.GetComponent<Image>().sprite = img.sprite;
        mImg_Virtual.GetComponent<Image>().sprite = img.sprite;
    }

    private void OnDisable()
    {
        
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

    private void UpdatePosition()
    {
        // 卡片模型位置跟随鼠标
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position); //将对象坐标换成屏幕坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = pos.z;
        mImg_CardModel.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //将赋值了Z的鼠标屏幕坐标转世界坐标的同时赋值给对象
        // 卡片预建造位置更新（取当前鼠标悬停格）
        BaseGrid overGrid = GameController.Instance.GetOverGrid();
        if(overGrid != null)
        {
            mImg_Virtual.SetActive(true);
            mImg_Virtual.transform.position = overGrid.transform.position;
        }
        else
        {
            mImg_Virtual.SetActive(false);
        }
    }
}
