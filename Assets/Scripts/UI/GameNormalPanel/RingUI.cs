using UnityEngine;
using UnityEngine.UI;

namespace GameNormalPanel_UI
{
    public class RingUI : BaseUI
    {
        private Image Bg1;
        private Image Bg2;
        private Image Img_Ring;
        private Image Img_Icon;

        private void Awake()
        {
            Bg1 = transform.Find("Bg1").GetComponent<Image>();
            Bg2 = transform.Find("Bg2").GetComponent<Image>();
            Img_Ring = transform.Find("Ring").GetComponent<Image>();
            Img_Icon = transform.Find("Icon").GetComponent<Image>();
        }


        protected override void O_MInit()
        {
            Img_Ring.fillAmount = 1;
            Img_Ring.color = new Color(1, 1, 1, 1);
        }

        protected override void O_MUpdate()
        {
            
        }

        protected override void O_MPause()
        {
           
        }

        protected override void O_MPauseUpdate()
        {
            
        }

        protected override void O_MResume()
        {
            
        }

        protected override void O_Hide()
        {
            Bg1.enabled = false;
            Bg2.enabled = false;
            Img_Ring.enabled = false;
            Img_Icon.enabled = false;
        }

        protected override void O_Show()
        {
            Bg1.enabled = true;
            Bg2.enabled = true;
            Img_Ring.enabled = true;
            Img_Icon.enabled = true;
        }

        protected override void O_Recycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "GameNormalPanel/RingUI", gameObject);
        }

        #region 供外界调用的方法
        public static RingUI GetInstance(Vector2 size)
        {
            RingUI r = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "GameNormalPanel/RingUI").GetComponent<RingUI>();
            r.MInit();
            r.transform.localScale = size;
            return r;
        }

        /// <summary>
        /// 设置环形百分比
        /// </summary>
        /// <param name="percent"></param>
        public void SetPercent(float percent)
        {
            Img_Ring.fillAmount = percent;
        }

        public float GetPercent()
        {
            return Img_Ring.fillAmount;
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            Img_Icon.sprite = sprite;
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            Img_Ring.color = color;
        }
        #endregion
    }
}

