using System.Collections.Generic;

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
        private Text Tex_Cooper;
        private Text Tex_Silver;
        private Text Tex_Gold;
        private Text Tex_Perfect;
        private Text Tex_ChapterIntroduce;
        private Sprite[] RankSpriteArray;
        private BigChapter_Local bigChapterLocal;
        private BigChapter_Static bigChapterStatic;

        protected override void Awake()
        {
            base.Awake();
            Trans_UI = transform.Find("UI");
            Img_Map = Trans_UI.Find("Content2").Find("Img_Map").GetComponent<Image>();
            Img_Label = Trans_UI.Find("Img_Label").GetComponent<Image>();
            Tex_Chapter = Trans_UI.Find("Content1").Find("Img_Chapter").Find("Tex_Chapter").GetComponent<Text>();
            Img_Rank = Trans_UI.Find("Content1").Find("Img_Rank").Find("Rank").GetComponent<Image>();
            Trans_StageList = Trans_UI.Find("StageList");

            // ���1
            {
                Transform trans = Trans_UI.Find("Content1").transform;
                Btn_Last = trans.Find("Img_Chapter").Find("Btn_Last").GetComponent<Button>();
                Btn_Next = trans.Find("Img_Chapter").Find("Btn_Next").GetComponent<Button>();
                Btn_Change = trans.Find("Img_Rank").Find("Btn_Change").GetComponent<Button>();
                Tex_Cooper = trans.Find("Img_Cooper").Find("Content").Find("Text").GetComponent<Text>();
                Tex_Silver = trans.Find("Img_Silver").Find("Content").Find("Text").GetComponent<Text>();
                Tex_Gold = trans.Find("Img_Gold").Find("Content").Find("Text").GetComponent<Text>();
                Tex_Perfect = trans.Find("Img_Perfect").Find("Content").Find("Text").GetComponent<Text>();
            }

            Btn_Exit = Trans_UI.Find("Btn_Exit").GetComponent<Button>();
            Tex_ChapterIntroduce = Trans_UI.Find("Content3").Find("Content").Find("Text").GetComponent<Text>();

            RankSpriteArray = new Sprite[4] {
            GameManager.Instance.GetSprite("UI/Difficulty/Easy"),
            GameManager.Instance.GetSprite("UI/Difficulty/Normal"),
            GameManager.Instance.GetSprite("UI/Difficulty/Hard"),
            GameManager.Instance.GetSprite("UI/Difficulty/Lunatic")
            };
            bigChapterStatic = StageInfoManager.GetBigChapterStatic("Mainline"); // ����Ĭ��ֵ
            bigChapterLocal = StageInfoManager.GetBigChapterLocal("Mainline"); // ����Ĭ��ֵ
        }

        public override void InitPanel()
        {
            base.InitPanel();
            UpdateDisplay();

            // ��ť
            {
                Btn_Last.onClick.RemoveAllListeners();
                Btn_Last.onClick.AddListener(delegate {
                    bigChapterLocal.currentIndex--;
                    UpdateDisplay();
                });

                Btn_Next.onClick.RemoveAllListeners();
                Btn_Next.onClick.AddListener(delegate {
                    bigChapterLocal.currentIndex++;
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
            bigChapterStatic = StageInfoManager.GetBigChapterStatic(bigChapterId); 
            bigChapterLocal = StageInfoManager.GetBigChapterLocal(bigChapterId); 
            UpdateDisplay();
        }
        #endregion

        #region ˽�з���
        /// <summary>
        /// ���»�����ʾ������
        /// </summary>
        private void UpdateDisplay()
        {
            bigChapterLocal.currentIndex = Mathf.Max(0, Mathf.Min(bigChapterStatic.chapterList.Count - 1, bigChapterLocal.currentIndex));
            StageInfoManager.Save();
            Chapter_Static currentChapter = bigChapterStatic.chapterList[bigChapterLocal.currentIndex];
            // ������ͷ
            Img_Label.sprite = GameManager.Instance.GetSprite("UI/BigChapterPanel/Label_" + bigChapterStatic.id);
            // ���°�ť�ɵ�����
            if (bigChapterLocal.currentIndex <= 0)
                Btn_Last.interactable = false;
            else
                Btn_Last.interactable = true;
            if (bigChapterLocal.currentIndex >= bigChapterStatic.chapterList.Count - 1)
                Btn_Next.interactable = false;
            else
                Btn_Next.interactable = true;

            Img_Map.sprite = GameManager.Instance.GetSprite(currentChapter.bgPath);
            Tex_Chapter.text = currentChapter.name;
            // ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
            UpdateItemListAndItsDisplay();
            // �����Ѷ���ʾ
            Img_Rank.sprite = RankSpriteArray[PlayerData.GetInstance().GetDifficult()];
            // ���½��ƻ�ȡ����
            UpdateMedal();
            // �����½ڽ���
            Tex_ChapterIntroduce.text = currentChapter.introduce;
        }

        /// <summary>
        /// ���¹ؿ�ѡ������Ϣ�Լ�����ʾ����
        /// </summary>
        private void UpdateItemListAndItsDisplay()
        {
            Chapter_Static currentChapter = bigChapterStatic.chapterList[bigChapterLocal.currentIndex];

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
                Item item = Item.GetInstance(local_info.rank, !ConfigManager.IsDeveloperMode() && !local_info.isUnlocked, static_info.id + " " + info.name, static_info.GetUnLockCondition(), local_info.diffRate, local_info.cardLevelArray, local_info.cardCountArray);
                itemList.Add(item);
                item.AddOnClickAction(delegate
                {
                    // ͨ�����½ڽӿ��������Ķ�̬�ؿ���Ϣ�ǿ�����id�ģ���Ϊ�б��ش浵����
                    {
                        PlayerData data = PlayerData.GetInstance();
                        PlayerData.StageInfo_Dynamic info_dynamic = new PlayerData.StageInfo_Dynamic();
                        info_dynamic.info = info;
                        info_dynamic.id = static_info.id;
                        data.SetCurrentDynamicStageInfo(info_dynamic);
                    }
                    mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
                });
                item.transform.SetParent(Trans_StageList);
                item.transform.localScale = Vector2.one;
            }
        }

        /// <summary>
        /// ���»�ȡ������
        /// </summary>
        private void UpdateMedal()
        {
            Chapter_Static currentChapter = bigChapterStatic.chapterList[bigChapterLocal.currentIndex];
            int totalCount = 0;
            int cooperCount = 0;
            int silverCount = 0;
            int goldCount = 0;
            int perfectCount = 0;
            foreach (var static_info in currentChapter.stageList)
            {
                totalCount++;
                StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(static_info.id);
                switch (local_info.rank)
                {
                    case 3:
                        cooperCount++;
                        silverCount++;
                        goldCount++;
                        perfectCount++;
                        break;
                    case 2:
                        cooperCount++;
                        silverCount++;
                        goldCount++;
                        break;
                    case 1:
                        cooperCount++;
                        silverCount++;
                        break;
                    case 0:
                        cooperCount++;
                        break;
                    default:
                        break;
                }
            }
            Tex_Cooper.text = cooperCount + "/" + totalCount;
            Tex_Silver.text = silverCount + "/" + totalCount;
            Tex_Gold.text = goldCount + "/" + totalCount;
            Tex_Perfect.text = perfectCount + "/" + totalCount;
        }

        #endregion
    }

}
