using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
namespace UIPanel.StageConfigPanel
{
    public class SettlementUI : MonoBehaviour, IGameControllerMember
    {
        private RectTransform Rect_Content;
        private Text Tex_RankRate;

        private float height;
        private List<LimitItem> itemList = new List<LimitItem>();

        private void Awake()
        {
            Rect_Content = transform.Find("Emp_LimitList").Find("Scr").Find("Viewport").Find("Content").GetComponent<RectTransform>();
            Tex_RankRate = transform.Find("Emp_Rank").Find("Img_RankRate").Find("Text").GetComponent<Text>();
        }

        public void MInit()
        {
            itemList.Clear();
            height = 0;
            Tex_RankRate.text = "100%";
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
            foreach (var item in itemList)
            {
                item.MDestory();
            }
            itemList.Clear();
        }

        #region 供外界使用的方法
        public void AddLimitItem(LimitItem item)
        {
            itemList.Add(item);
            item.transform.SetParent(Rect_Content);
            item.transform.localScale = Vector2.one;
            height += item.GetComponent<RectTransform>().sizeDelta.y + 5;
            Rect_Content.sizeDelta = new Vector2(Rect_Content.sizeDelta.x, height);
        }

        public void RemoveLimitItem(LimitItem item)
        {
            if (itemList.Remove(item))
            {
                item.MDestory();
                height -= item.GetComponent<RectTransform>().sizeDelta.y + 5;
                Rect_Content.sizeDelta = new Vector2(Rect_Content.sizeDelta.x, height);
            }
        }

        public void SetRankRateText(string text)
        {
            Tex_RankRate.text = text;
        }
        #endregion

    }
}
