using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
namespace UIPanel.StageConfigPanel
{
    public class SelectTagUI : MonoBehaviour, IGameControllerMember
    {
        public ScrollRect Scr_TagArea;
        public RectTransform Rect_Content;
        public GameObject DisableMask;
        public Text Tex_DisableCondition;

        private List<TagArray> tagArrayList = new List<TagArray>();
        private float width = 0;
        
        private void Awake()
        {
            //DisableMask = transform.Find("DisableMask").gameObject;
            //Tex_DisableCondition = DisableMask.transform.Find("Text").GetComponent<Text>();
        }

        public void MInit()
        {
            tagArrayList.Clear();
            width = 0;
            DisableMask.SetActive(false);
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

        public void ShowDisableMask(string reason)
        {
            DisableMask.SetActive(true);
            Tex_DisableCondition.text = reason;
        }

        public void HideDisableMask()
        {
            DisableMask.SetActive(false);
        }
        #endregion
    }
}
