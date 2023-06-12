using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static StageInfoManager;
/// <summary>
/// ���½����
/// </summary>
namespace BigChapterPanel
{
    public class BigChapterPanel : BasePanel
    {
        private Transform Trans_UI;
        private Image Img_Map;
        private Image Img_Label;
        private Text Tex_Chapter;
        private Image Img_Rank;
        private Transform Trans_StageList;
        private List<Item> itemList = new List<Item>();
        private Button Btn_Last;
        private Button Btn_Next;
        private Button Btn_Exit;
        private Button Btn_Change;
        private Sprite[] RankSpriteArray;
        private BigChapter_Static bigChapter;

        protected override void Awake()
        {
            base.Awake();
            Trans_UI = transform.Find("UI");
            Img_Map = Trans_UI.Find("Img_Map").GetComponent<Image>();
            Img_Label = Trans_UI.Find("Img_Label").GetComponent<Image>();
            Tex_Chapter = Trans_UI.Find("Img_Chapter").Find("Tex_Chapter").GetComponent<Text>();
            Img_Rank = Trans_UI.Find("Img_Rank").Find("Rank").GetComponent<Image>();
            Trans_StageList = Trans_UI.Find("StageList");
            Btn_Last = Trans_UI.Find("Img_Chapter").Find("Btn_Last").GetComponent<Button>();
            Btn_Next = Trans_UI.Find("Img_Chapter").Find("Btn_Next").GetComponent<Button>();
            Btn_Exit = Trans_UI.Find("Btn_Exit").GetComponent<Button>();
            Btn_Change = Trans_UI.Find("Img_Rank").Find("Btn_Change").GetComponent<Button>();

            RankSpriteArray = new Sprite[4] {
            GameManager.Instance.GetSprite("UI/Difficulty/Easy"),
            GameManager.Instance.GetSprite("UI/Difficulty/Normal"),
            GameManager.Instance.GetSprite("UI/Difficulty/Hard"),
            GameManager.Instance.GetSprite("UI/Difficulty/Lunatic")
        };
            bigChapter = StageInfoManager.GetBigChapter("Mainline"); // ����Ĭ��ֵ
        }

        public override void InitPanel()
        {
            base.InitPanel();
            EnterLastAvailablePage();
            UpdateDisplay();

            // ��ť
            {
                Btn_Last.onClick.RemoveAllListeners();
                Btn_Last.onClick.AddListener(delegate {
                    bigChapter.currentIndex--;
                    UpdateDisplay();
                });

                Btn_Next.onClick.RemoveAllListeners();
                Btn_Next.onClick.AddListener(delegate {
                    bigChapter.currentIndex++;
                    UpdateDisplay();
                });

                Btn_Exit.onClick.RemoveAllListeners();
                Btn_Exit.onClick.AddListener(delegate { mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel].ExitPanel(); });

                Btn_Change.onClick.RemoveAllListeners();
                Btn_Change.onClick.AddListener(delegate { mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].EnterPanel(); });
            }
        }

        #region �������õķ���
        public void SetBigChapter(string bigChapterId)
        {
            bigChapter = StageInfoManager.GetBigChapter(bigChapterId); // ����Ĭ��ֵ
            UpdateDisplay();
        }
        #endregion

        #region ˽�з���
        /// <summary>
        /// ���»�����ʾ������
        /// </summary>
        private void UpdateDisplay()
        {
            Chapter_Static currentChapter = bigChapter.chapterList[bigChapter.currentIndex];
            // ������ͷ
            Img_Label.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Label_" + bigChapter.id);
            // ���°�ť�ɵ�����
            if (bigChapter.currentIndex <= 0)
                Btn_Last.interactable = false;
            else
                Btn_Last.interactable = true;
            if (bigChapter.currentIndex >= bigChapter.chapterList.Count - 1)
                Btn_Next.interactable = false;
            else
                Btn_Next.interactable = true;

            Img_Map.sprite = GameManager.Instance.GetSprite(currentChapter.bgPath);
            Tex_Chapter.text = currentChapter.name;
            // ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
            UpdateItemListAndItsDisplay();
            // �����Ѷ���ʾ
            Img_Rank.sprite = RankSpriteArray[PlayerData.GetInstance().GetDifficult()];
        }

        /// <summary>
        /// ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
        /// </summary>
        private void UpdateItemListAndItsDisplay()
        {
            Chapter_Static currentChapter = bigChapter.chapterList[bigChapter.currentIndex];

            foreach (var item in itemList)
            {
                item.ExecuteRecycle();
            }
            itemList.Clear();

            // ���ݵ�ǰ���½���Ϣ����ȡ�ؿ�����Ҵ浵����
            foreach (var static_info in currentChapter.stageList)
            {
                StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(static_info.id);
                BaseStage.StageInfo info = StageInfoManager.GetDetailInfo(static_info.id);
                Item item = Item.GetInstance(local_info.rank, !ConfigManager.IsDeveloperMode() && !local_info.isUnlocked, static_info.id + " " + info.name, static_info.GetUnLockCondition());
                itemList.Add(item);
                item.AddOnClickAction(delegate
                {
                    PlayerData data = PlayerData.GetInstance();
                    data.SetCurrentStageInfo(info);
                    data.SetCurrentStageID(static_info.id);
                    mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
                });
                item.transform.SetParent(Trans_StageList);
                item.transform.localScale = Vector2.one;
            }
        }


        /// <summary>
        /// �������һ�����Խ��Ľ��棨һ���Ǹ�����ҵ�ǰ���ȶ�����
        /// </summary>
        private void EnterLastAvailablePage()
        {
            //int level = PlayerData.GetInstance().GetLevel();
            //if (level <= 7)
            //    bigChapter.currentIndex = 0;
            //else if(level <= 14)
            //    bigChapter.currentIndex = 1;
            bigChapter.currentIndex = 0;
        }
        #endregion
    }

}
