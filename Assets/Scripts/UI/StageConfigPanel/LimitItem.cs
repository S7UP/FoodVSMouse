using UnityEngine;
using UnityEngine.UI;
namespace UIPanel.StageConfigPanel
{
    public class LimitItem : MonoBehaviour, IGameControllerMember
    {
        private Image Img;
        private Text Text;
        private RectTransform Rect_Text;
        private RectTransform Rect;

        private void Awake()
        {
            Img = transform.Find("Image").GetComponent<Image>();
            Text = transform.Find("Text").GetComponent<Text>();
            Rect_Text = Text.GetComponent<RectTransform>();
            Rect = GetComponent<RectTransform>();
        }

        public void MInit()
        {
            Img.sprite = null;
            Text.text = "";
        }
        public void MUpdate()
        {

        }
        public void MPause()
        {

        }

        public void MPauseUpdate()
        {

        }

        public void MResume()
        {

        }
        public void MDestory()
        {
            ExecuteRecycle();
        }


        #region 供外界使用的方法
        public void ChangeSprite(Sprite sprite)
        {
            Img.sprite = sprite;
        }

        /// <summary>
        /// 设置文本内容的同时自适应高度
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            float fontSize = Text.fontSize;
            float width = Rect_Text.rect.width;
            int count = Mathf.FloorToInt(width / fontSize);
            int row = 1 + text.Length / count;
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, row*(fontSize+2));
            Text.text = text;
        }
        #endregion

        public static LimitItem GetInstance()
        {
            LimitItem item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/LimitItem").GetComponent<LimitItem>();
            item.MInit();
            return item;
        }

        private void ExecuteRecycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/LimitItem", gameObject);
        }
    }
}

