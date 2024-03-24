namespace EditorPanel
{
    using UnityEngine;
    using UnityEngine.UI;
    /// <summary>
    /// 编辑器修改卡片配置使用模型
    /// </summary>
    public class ConfigCardModel : MonoBehaviour
    {
        private ConfigCardUI mConfigCardUI;
        private Button button;
        private Image Img_Display;
        private Text Tex_Cost;
        private Image Img_Rank;
        private Image Img_Mask;
        private Toggle toggle;
        public bool isUpdate;

        private int type;
        private int shape;
        private int rank;
        new private string name;

        private void Awake()
        {
            button = GetComponent<Button>();
            Img_Display = transform.Find("Img_Display").GetComponent<Image>();
            Tex_Cost = transform.Find("Tex_Cost").GetComponent<Text>();
            Img_Rank = transform.Find("Img_Rank").GetComponent<Image>();
            Img_Mask = transform.Find("Img_Mask").GetComponent<Image>();
            toggle = transform.Find("Toggle").GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {
                Img_Mask.gameObject.SetActive(!IsSelected());
                if (isUpdate)
                    return;
                if (IsSelected())
                {
                    // 被选中则通知父级UI在关卡信息模型中添加此项
                    mConfigCardUI.AddAvailableCardInfo(type, shape, rank);
                }
                else
                {
                    // 取消选中则通知父级UI在关卡信息模型中移除此项
                    mConfigCardUI.RemoveAvailableCardInfo(type);
                }
            });
        }

        public void Initial()
        {
            button.onClick.RemoveAllListeners();
            type = 0;
            shape = 0;
            rank = 0;
            name = "unknow";
            // 因为要动toggle的值但又不能触发监听所以需要作保护
            isUpdate = true;
            toggle.isOn = false;
            isUpdate = false;
        }

        public void SetInfo(int type, int shape, int rank)
        {
            SetCardTypeAndShape(type, shape);
            SetRank(rank);
        }

        public void SetCardTypeAndShape(int cardType, int cardShape)
        {
            type = cardType;
            shape = cardShape;
            Img_Display.sprite = GameManager.Instance.GetSprite("Food/" + cardType + "/" + cardShape + "/display");
            SetCost((float)GameManager.Instance.attributeManager.GetCardBuilderAttribute(type, shape).GetCost(rank));
            SetName(GameManager.Instance.attributeManager.GetFoodUnitAttribute(type, shape).baseAttrbute.name);
        }

        public void SetRank(int rank)
        {
            this.rank = rank;
            Img_Rank.sprite = GameManager.Instance.GetSprite("UI/Rank2/" + rank);
            SetName(GameManager.Instance.attributeManager.GetFoodUnitAttribute(type, shape).baseAttrbute.name);
        }

        public void SetSelected(bool isOn)
        {
            toggle.isOn = isOn;
        }

        public bool IsSelected()
        {
            return toggle.isOn;
        }

        public void AddListener(UnityEngine.Events.UnityAction call)
        {
            button.onClick.AddListener(call);
        }

        public FoodNameTypeMap GetFoodType()
        {
            return (FoodNameTypeMap)type;
        }

        public int GetShape()
        {
            return shape;
        }

        public int GetRank()
        {
            return rank;
        }

        public string GetName()
        {
            return name;
        }

        private void SetCost(float cost)
        {
            Tex_Cost.text = "" + cost;
        }

        private void SetName(string name)
        {
            this.name = name;
        }

        public void SetConfigCardUI(ConfigCardUI u)
        {
            mConfigCardUI = u;
        }
    }

}