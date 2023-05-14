using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
namespace UIPanel.StageConfigPanel
{
    public class SelectTagUI : MonoBehaviour, IGameControllerMember
    {
        public ScrollRect Scr_TagArea;
        public RectTransform Rect_Content;
        private List<TagArray> tagArrayList = new List<TagArray>();
        private float width = 0;
        
        private void Awake()
        {
            // Scr_TagArea = transform.Find("Scr").GetComponent<ScrollRect>();
            // Rect_Content = Scr_TagArea.content.GetComponent<RectTransform>();
        }

        public void MInit()
        {
            tagArrayList.Clear();
            width = 0;
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
            foreach (var tagArray in tagArrayList)
            {
                tagArray.MDestory();
            }
            tagArrayList.Clear();
        }

        #region 供外界使用的方法
        public void AddTagArray(TagArray tagArray)
        {
            tagArrayList.Add(tagArray);
            tagArray.transform.SetParent(Scr_TagArea.content);
            tagArray.transform.localScale = Vector2.one;
            width += tagArray.GetComponent<RectTransform>().sizeDelta.x + 5;
            Rect_Content.sizeDelta = new Vector2(width, Rect_Content.sizeDelta.y);
        }
        #endregion
    }
}
