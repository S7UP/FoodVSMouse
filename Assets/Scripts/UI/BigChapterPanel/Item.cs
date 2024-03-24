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
        private static Sprite[] Spr_Label;

        private Button Btn;
        private Image Img_Item;
        private Image Img_Medal;
        private Text Tex_Name;
        private GameObject RankRate;
        private Text Tex_Rank;
        private GameObject Lowest;
        private Text[] Tex_LowestNumArray;
        private Image[] Img_LowestLevelArray;
        
        private string unlockCondition;

        public void Awake()
        {
            if(Spr_Label == null)
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
            Img_Medal = transform.Find("Img_Medal").GetComponent<Image>();
            Tex_Name = transform.Find("Btn").Find("Tex_Name").GetComponent<Text>();
            RankRate = transform.Find("Img_RankRate").gameObject;
            Tex_Rank = RankRate.transform.Find("Tex_Rank").GetComponent<Text>();
            Lowest = transform.Find("Img_Lowest").gameObject;
            Tex_LowestNumArray = new Text[2];
            Img_LowestLevelArray = new Image[2];
            for (int i = 0; i < Tex_LowestNumArray.Length; i++)
            {
                Tex_LowestNumArray[i] = Lowest.transform.Find("Tex_Level"+i+"Num").GetComponent<Text>();
                Img_LowestLevelArray[i] = Lowest.transform.Find("Img_Level"+i).GetComponent<Image>();
            }
        }

        private void Initial()
        {
            Btn.onClick.RemoveAllListeners();
            unlockCondition = null;
            RankRate.gameObject.SetActive(false);
            Lowest.gameObject.SetActive(false);
        }

        public void AddOnClickAction(UnityAction call)
        {
            Btn.onClick.AddListener(call);
        }

        public void RemoveOnClickAction(UnityAction call)
        {
            Btn.onClick.RemoveListener(call);
        }

        public static Item GetInstance(int rank, bool isLock, string name, int diff, string unlockCondition, float rankRate, int[] cardLevelArray, int[] cardCountArray)
        {
            Item item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "BigChapterPanel/Item").GetComponent<Item>();
            item.Initial();
            diff = Mathf.Max(0, Mathf.Min(diff, Spr_Label.Length - 1));
            item.Img_Item.sprite = Spr_Label[diff];
            if (isLock)
            {
                item.Btn.interactable = false;
                item.unlockCondition = unlockCondition;
                item.Img_Medal.gameObject.SetActive(true);
                item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Lock");
            }
            else
            {
                item.Btn.interactable = true;
                if (rank > -1)
                {
                    item.Img_Medal.gameObject.SetActive(true);
                    item.Img_Medal.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Medals3");
                    item.RankRate.SetActive(true);
                    item.Tex_Rank.text = Mathf.FloorToInt(rankRate * 100) + "%难度";

                    if (cardCountArray[0] > 0 && cardLevelArray[0] > -1)
                    {
                        item.Lowest.SetActive(true);
                        item.Tex_LowestNumArray[0].text = cardCountArray[0].ToString();
                        item.Img_LowestLevelArray[0].sprite = GameManager.Instance.GetSprite("UI/Rank2/" + cardLevelArray[0]);

                        if (cardCountArray[1] > 0 && cardLevelArray[1] > -1)
                        {
                            item.Tex_LowestNumArray[1].gameObject.SetActive(true);
                            item.Tex_LowestNumArray[1].text = cardCountArray[1].ToString();
                            item.Img_LowestLevelArray[1].gameObject.SetActive(true);
                            item.Img_LowestLevelArray[1].sprite = GameManager.Instance.GetSprite("UI/Rank2/" + cardLevelArray[1]);
                        }
                        else
                        {
                            item.Tex_LowestNumArray[1].gameObject.SetActive(false);
                            item.Img_LowestLevelArray[1].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    // 未通关是不显示这些东西的
                    item.Img_Medal.gameObject.SetActive(false);
                }
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
