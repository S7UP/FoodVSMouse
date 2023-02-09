using UnityEngine;
/// <summary>
/// 负责拖拽放卡时的显示处理
/// </summary>
public class CardModel : MonoBehaviour, ICanvasRaycastFilter
{
    public enum DisplayMode
    {
        SetCharacter = 0,
        SetCard = 1,
        RemoveCard = 2,
    }

    private DisplayMode displayMode; // 显示模式
    private SpriteRenderer mSpr_CardModel; // 跟随鼠标的卡片模型
    private SpriteRenderer mSpr_Virtual; // 卡片预建造虚像

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
    /// 每当被激活时，应当是选卡成功了，因此要把自身的图标置为被选卡片建造器的图标
    /// </summary>
    private void OnCardSelected()
    {
        UpdatePosition();
        // 获取卡片建造器上显示卡片的图标
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
        // 设置图标
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
        // 卡片模型位置跟随鼠标
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position); //将对象坐标换成屏幕坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = pos.z;
        mSpr_CardModel.transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //将赋值了Z的鼠标屏幕坐标转世界坐标的同时赋值给对象
        // 卡片预建造位置更新（取当前鼠标悬停格）
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
    /// 不遮挡射线
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="eventCamera"></param>
    /// <returns></returns>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return false;
    }
}
