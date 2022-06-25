using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ������ק�ſ�ʱ����ʾ����
/// </summary>
public class CardModel : MonoBehaviour
{
    public enum DisplayMode
    {
        SetCharacter = 0,
        SetCard = 1,
        RemoveCard = 2,
    }

    private DisplayMode displayMode; // ��ʾģʽ
    private GameObject mImg_CardModel; // �������Ŀ�Ƭģ��
    private GameObject mImg_Virtual; // ��ƬԤ��������

    private void Awake()
    {
        mImg_CardModel = transform.Find("Img_CardModel").gameObject;
        mImg_Virtual = transform.Find("Img_Virtual").gameObject;
    }

    public void ShowCardModel(DisplayMode displayMode)
    {
        this.displayMode = displayMode;
        gameObject.SetActive(true);
    }

    public void HideCardModel()
    {
        gameObject.SetActive(false);
    }

    public DisplayMode GetDisplayMode()
    {
        return displayMode;
    }

    /// <summary>
    /// ÿ��������ʱ��Ӧ����ѡ���ɹ��ˣ����Ҫ�������ͼ����Ϊ��ѡ��Ƭ��������ͼ��
    /// </summary>
    private void OnEnable()
    {
        UpdatePosition();
        // ��ȡ��Ƭ����������ʾ��Ƭ��ͼ��
        Sprite sprite = null;
        switch (displayMode)
        {
            case DisplayMode.SetCharacter:
                sprite = GameController.Instance.mCharacterController.mCurrentCharacter.GetSpriteRender().GetComponent<SpriteRenderer>().sprite;
                UpdateSize(Vector3.one);
                break;
            case DisplayMode.SetCard:
                sprite = GameController.Instance.mCardController.GetSelectCardBuilder().mImg_Card.GetComponent<Image>().sprite;
                UpdateSize(Vector3.one/2);
                break;
            case DisplayMode.RemoveCard:
                break;
            default:
                break;
        }
        // ����ͼ��
        mImg_CardModel.GetComponent<Image>().sprite = sprite;
        mImg_Virtual.GetComponent<Image>().sprite = sprite;
        mImg_CardModel.GetComponent<Image>().SetNativeSize();
        mImg_Virtual.GetComponent<Image>().SetNativeSize();
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
            mImg_Virtual.transform.position = overGrid.GetUnitInPosition(Vector2.zero);
        }
        else
        {
            mImg_Virtual.SetActive(false);
        }
    }

    private void UpdateSize(Vector3 scale)
    {
        mImg_CardModel.transform.localScale = scale;
        mImg_Virtual.transform.localScale = scale;
    }
}
