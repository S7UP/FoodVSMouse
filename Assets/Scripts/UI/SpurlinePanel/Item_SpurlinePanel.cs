using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 支线任务中的关卡选项条
/// </summary>
public class Item_SpurlinePanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Item;
    private Image Img_Medal;
    private Text Tex_Name;
    private Text Tex_Condition;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        Img_Item = GetComponent<Image>();
        Img_Medal = transform.Find("Image").GetComponent<Image>();
        Tex_Name = transform.Find("Tex_Name").GetComponent<Text>();
        Tex_Condition = transform.Find("Tex_Condition").GetComponent<Text>();
    }

    private void Initial()
    {
        Btn.onClick.RemoveAllListeners();
    }

    public static Item_SpurlinePanel GetInstance(UnityAction call, int rank, bool isLock, string name, string condition)
    {
        Item_SpurlinePanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SpurlinePanel/Item").GetComponent<Item_SpurlinePanel>();
        item.Initial();
        item.Btn.onClick.AddListener(call);
        if (isLock)
        {
            item.Img_Medal.gameObject.SetActive(true);
            item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/MainlinePanel/Lock");
            item.Img_Item.sprite = GameManager.Instance.GetSprite("UI/MainlinePanel/Item_Locked");
            item.Tex_Condition.gameObject.SetActive(true);
            item.Btn.interactable = false;
        }
        else
        {
            item.Tex_Condition.gameObject.SetActive(false);
            item.Img_Item.sprite = GameManager.Instance.GetSprite("UI/MainlinePanel/Item");
            if (rank > -1)
            {
                item.Img_Medal.gameObject.SetActive(true);
                item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/MainlinePanel/Medals" + rank);
            }
            else
            {
                item.Img_Medal.gameObject.SetActive(false);
            }
            item.Btn.interactable = true;
        }
        item.Img_Medal.SetNativeSize();
        item.Tex_Name.text = name;
        item.Tex_Condition.text = condition;
        return item;
    }


    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SpurlinePanel/Item", gameObject);
    }
}
