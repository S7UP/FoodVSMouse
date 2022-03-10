using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ������ק�ſ�ʱ����ʾ����
/// </summary>
public class CardModel : MonoBehaviour
{
    private GameObject mImg_CardModel; // �������Ŀ�Ƭģ��
    private GameObject mImg_Virtual; // ��ƬԤ��������

    private void Awake()
    {
        mImg_CardModel = transform.Find("Img_CardModel").gameObject;
        mImg_Virtual = transform.Find("Img_Virtual").gameObject;
    }

    /// <summary>
    /// ÿ��������ʱ��Ӧ����ѡ���ɹ��ˣ����Ҫ�������ͼ����Ϊ��ѡ��Ƭ��������ͼ��
    /// </summary>
    private void OnEnable()
    {
        UpdatePosition();
        // ��ȡ��Ƭ����������ʾ��Ƭ��ͼ��
        Image img = GameController.Instance.mCardController.GetSelectCardBuilder().mImg_Card.GetComponent<Image>();
        // ����ͼ��
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
        // ��Ƭģ��λ�ø������
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position); //���������껻����Ļ����
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = pos.z;
        mImg_CardModel.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //����ֵ��Z�������Ļ����ת���������ͬʱ��ֵ������
        // ��ƬԤ����λ�ø��£�ȡ��ǰ�����ͣ��
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
