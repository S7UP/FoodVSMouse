using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 主线任务中的关卡选项条
/// </summary>
namespace BigChapterPanel
{
    public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Button Btn;
        private Image Img_Item;
        private Image Img_Medal;
        private Text Tex_Name;
        private string unlockCondition;

        public void Awake()
        {
            Btn = GetComponent<Button>();
            Img_Item = GetComponent<Image>();
            Img_Medal = transform.Find("Image").GetComponent<Image>();
            Tex_Name = transform.Find("Tex_Name").GetComponent<Text>();
        }

        private void Initial()
        {
            Btn.onClick.RemoveAllListeners();
            unlockCondition = null;
        }

        public void AddOnClickAction(UnityAction call)
        {
            Btn.onClick.AddListener(call);
        }

        public void RemoveOnClickAction(UnityAction call)
        {
            Btn.onClick.RemoveListener(call);
        }

        public static Item GetInstance(int rank, bool isLock, string name, string unlockCondition)
        {
            Item item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "BigChapterPanel/Item").GetComponent<Item>();
            item.Initial();
            if (isLock)
            {
                item.unlockCondition = unlockCondition;
                item.Img_Medal.gameObject.SetActive(true);
                item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Lock");
                item.Img_Item.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Item_Locked");
                item.Btn.interactable = false;
            }
            else
            {
                item.Img_Item.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Item");
                if (rank > -1)
                {
                    item.Img_Medal.gameObject.SetActive(true);
                    item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Medals" + rank);
                }
                else
                {
                    item.Img_Medal.gameObject.SetActive(false);
                }
                item.Btn.interactable = true;
            }
            item.Img_Medal.SetNativeSize();
            item.Tex_Name.text = name;
            return item;
        }


        public void ExecuteRecycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "BigChapterPanel/Item", gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (unlockCondition != null)
            {
                TextArea.Instance.SetText(unlockCondition);
                TextArea.Instance.SetLocalPosition(transform, new Vector2(-200, -30), new Vector2(1, -1));
            }

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (unlockCondition != null)
                TextArea.ExecuteRecycle();
        }
    }

}
