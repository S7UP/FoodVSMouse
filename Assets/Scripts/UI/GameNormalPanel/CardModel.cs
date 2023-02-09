using UnityEngine;
/// <summary>
/// ������ק�ſ�ʱ����ʾ����
/// </summary>
public class CardModel : MonoBehaviour, ICanvasRaycastFilter
{
    public enum DisplayMode
    {
        SetCharacter = 0,
        SetCard = 1,
        RemoveCard = 2,
    }

    private DisplayMode displayMode; // ��ʾģʽ
    private SpriteRenderer mSpr_CardModel; // �������Ŀ�Ƭģ��
    private SpriteRenderer mSpr_Virtual; // ��ƬԤ��������

    private void Awake()
    {
        mSpr_CardModel = transform.Find("Spr_Model").GetComponent<SpriteRenderer>();
        mSpr_Virtual = transform.Find("Spr_Virtual").GetComponent<SpriteRenderer>();
    }

    public void ShowCardModel(DisplayMode displayMode)
    {
        this.displayMode = displayMode;
        OnCardSelected();
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
    private void OnCardSelected()
    {
        UpdatePosition();
        // ��ȡ��Ƭ����������ʾ��Ƭ��ͼ��
        Sprite sprite = null;
        switch (displayMode)
        {
            case DisplayMode.SetCharacter:
                int type = GameManager.Instance.playerData.GetCharacter();
                sprite = GameManager.Instance.GetSprite("Character/" + type + "/1");
                UpdateSize(Vector3.one);
                break;
            case DisplayMode.SetCard:
                BaseCardBuilder builder = GameController.Instance.mCardController.GetSelectCardBuilder();
                sprite = GameManager.Instance.GetSprite("Food/" + builder.mType + "/" + builder.mShape + "/model");
                UpdateSize(Vector3.one);
                break;
            case DisplayMode.RemoveCard:
                break;
            default:
                break;
        }
        // ����ͼ��
        mSpr_CardModel.sprite = sprite;
        mSpr_Virtual.sprite = sprite;
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
        mSpr_CardModel.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //����ֵ��Z�������Ļ����ת���������ͬʱ��ֵ������
        // ��ƬԤ����λ�ø��£�ȡ��ǰ�����ͣ��
        if(GameController.Instance != null)
        {
            BaseGrid overGrid = GameController.Instance.GetOverGrid();
            if (overGrid != null)
            {
                mSpr_Virtual.gameObject.SetActive(true);
                mSpr_Virtual.gameObject.transform.position = overGrid.GetUnitInPosition(Vector2.zero);
            }
            else
            {
                mSpr_Virtual.gameObject.SetActive(false);
            }
        }

    }

    private void UpdateSize(Vector3 scale)
    {
        mSpr_CardModel.transform.localScale = scale;
        mSpr_Virtual.transform.localScale = scale;
    }

    /// <summary>
    /// ���ڵ�����
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return false;
    }
}
