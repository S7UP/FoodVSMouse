using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 主线任务中的关卡选项条
/// </summary>
namespace LocalStagePanel
{
    public class Item : MonoBehaviour
    {
        private static Sprite[] Spr_Label;

        private Button Btn;
        private Text Tex_Name;
        private Image Img_Item;

        public void Awake()
        {
            if (Spr_Label == null)
            {
                Spr_Label = new Sprite[6];
                Spr_Label[0] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Easy");
                Spr_Label[1] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Normal");
                Spr_Label[2] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Hard");
                Spr_Label[3] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Lunatic");
                Spr_Label[4] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Ultra");
                Spr_Label[5] = GameManager.Instance.GetSprite("UI/BigChapterPanel/StageLabel/Unknow");
            }

            Btn = GetComponent<Button>();
            Img_Item = transform.Find("Btn").GetComponent<Image>();
            Tex_Name = transform.Find("Btn").Find("Tex_Name").GetComponent<Text>();
        }

        private void Initial()
        {
            Btn.onClick.RemoveAllListeners();
        }

        public void AddOnClickAction(UnityAction call)
        {
            Btn.onClick.AddListener(call);
        }

        public void RemoveOnClickAction(UnityAction call)
        {
            Btn.onClick.RemoveListener(call);
        }

        public static Item GetInstance(string name, int diff)
        {
            Item item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "LocalStagePanel/Item").GetComponent<Item>();
            item.Initial();
            item.Tex_Name.text = name;
            diff = Mathf.Max(0, Mathf.Min(diff, Spr_Label.Length - 1));
            item.Img_Item.sprite = Spr_Label[diff];

            return item;
        }


        public void ExecuteRecycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "LocalStagePanel/Item", gameObject);
        }
    }

}
