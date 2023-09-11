
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �ؿ���Ϣ����
/// </summary>
namespace UIPanel.StageConfigPanel
{
    public class StageInfoUI : MonoBehaviour
    {
        private GameObject Emp_StageInfo;
        private GameObject Emp_EnemyInfo;
        private MousePanel_StageConfigPanel mMousePanel;
        private ScrollRect Scr_EnemyList;
        private RectTransform RectTrans_MouseItemContent;
        private GameObject Scr_AddInfo;
        private Sprite btnSprite0; // �Ϸ���ť��
        private Sprite btnSprite1; // �Ϸ���ť��
        private Image[] imgList;
        private Image Img_Map;
        private Text Tex_MapName;
        private Text Tex_StageName;
        private Text Tex_TimeLimit;
        private Text Tex_JewelLimit;
        private ScrollRect Scr_Background;
        private ScrollRect Scr_Illustrate;
        private Text Tex_Background;
        private Text Tex_Illustrate;
        private Text Tex_AddInfo;

        private List<MouseItem_StageConfigPanel> mouseItemList = new List<MouseItem_StageConfigPanel>();
        private List<BossItem_StageConfigPanel> bossItemList = new List<BossItem_StageConfigPanel>();
        private MouseItem_StageConfigPanel currentMouseItem; // ��ǰѡ�е�����
        private BossItem_StageConfigPanel currentBossItem; // ��ǰѡ�е�BOSS

        private void Awake()
        {
            Emp_StageInfo = transform.Find("Emp_StageInfo").gameObject;
            Emp_EnemyInfo = transform.Find("Emp_EnemyInfo").gameObject;
            mMousePanel = Emp_EnemyInfo.transform.Find("MousePanel").GetComponent<MousePanel_StageConfigPanel>();
            Scr_EnemyList = Emp_EnemyInfo.transform.Find("Scr_EnemyList").GetComponent<ScrollRect>();
            RectTrans_MouseItemContent = Scr_EnemyList.content.GetComponent<RectTransform>();
            Scr_AddInfo = transform.Find("Scr_AddInfo").gameObject;

            btnSprite0 = GameManager.Instance.GetSprite("UI/SelectPanel/36");
            btnSprite1 = GameManager.Instance.GetSprite("UI/SelectPanel/33");

            imgList = new Image[3];
            imgList[0] = transform.Find("Emp_BtnList").Find("Button").GetComponent<Image>();
            imgList[1] = transform.Find("Emp_BtnList").Find("Button1").GetComponent<Image>();
            imgList[2] = transform.Find("Emp_BtnList").Find("Button2").GetComponent<Image>();

            Img_Map = Emp_StageInfo.transform.Find("Scr_Map").Find("Viewport").Find("Content").Find("Img_Map").GetComponent<Image>();
            Tex_MapName = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_MapName").GetComponent<Text>();
            Tex_StageName = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_StageName").GetComponent<Text>();
            Tex_TimeLimit = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_TimeLimit").GetComponent<Text>();
            Tex_JewelLimit = Emp_StageInfo.transform.Find("Img_TextArea").Find("Tex_JewelLimit").GetComponent<Text>();
            Scr_Background = Emp_StageInfo.transform.Find("Img_TextArea").Find("Scr_Background").GetComponent<ScrollRect>();
            Scr_Illustrate = Emp_StageInfo.transform.Find("Img_TextArea").Find("Scr_Illustrate").GetComponent<ScrollRect>();
            Tex_Background = Scr_Background.content.Find("Tex_Background").GetComponent<Text>();
            Tex_Illustrate = Scr_Illustrate.content.Find("Tex_Illustrate").GetComponent<Text>();
            Tex_AddInfo = Scr_AddInfo.transform.Find("Viewport").Find("Content").Find("Text").GetComponent<Text>();
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public void Initial()
        {
            // ����ʾ����
            Emp_StageInfo.gameObject.SetActive(false);
            HideEnemyInfoPanel();
            Scr_AddInfo.gameObject.SetActive(false);

            // ��ťȫ���ð�
            foreach (var img in imgList)
            {
                img.sprite = btnSprite0;
            }

            // Ĭ����ʾ��һ��
            Emp_StageInfo.gameObject.SetActive(true);
            imgList[0].sprite = btnSprite1;
        }

        /// <summary>
        /// ��ʾ�з���Ϣҳ��
        /// </summary>
        public void ShowEnemyInfoPanel()
        {
            mMousePanel.Initial();
            foreach (var item in mouseItemList)
            {
                item.ExecuteRecycle();
            }
            mouseItemList.Clear();
            foreach (var item in bossItemList)
            {
                item.ExecuteRecycle();
            }
            bossItemList.Clear();

            // ��ȡ��ǰ�ؿ���Ϣ
            BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
            if (info == null)
                return;
            // ͳ��С�ֲ��������
            {
                List<BaseEnemyGroup> list = BaseRound.GetAllEnemyList(new List<BaseEnemyGroup>(), info.roundInfoList);
                if (list != null)
                {
                    foreach (var g in list)
                    {
                        int type = g.mEnemyInfo.type;
                        int shape = g.mEnemyInfo.shape;
                        int num = g.mCount;
                        MouseItem_StageConfigPanel mouseItem = null;
                        mouseItem = MouseItem_StageConfigPanel.GetInstance(type, shape, num, delegate { SetCurrentMouseItem(mouseItem); });
                        mouseItem.transform.SetParent(Scr_EnemyList.content);
                        mouseItem.transform.localScale = Vector2.one;
                        mouseItemList.Add(mouseItem);
                    }
                    SetCurrentMouseItem(mouseItemList[0]);
                }
            }

            // ͳ��BOSS���������
            {
                List<BaseEnemyGroup> list = BaseRound.GetBossList(new List<BaseEnemyGroup>(), info.roundInfoList);
                if (list != null)
                {
                    foreach (var g in list)
                    {
                        BossItem_StageConfigPanel bossItem = null;
                        bossItem = BossItem_StageConfigPanel.GetInstance(g, delegate { SetCurrentBossItem(bossItem); });
                        bossItem.transform.SetParent(Scr_EnemyList.content);
                        bossItem.transform.localScale = Vector2.one;
                        bossItemList.Add(bossItem);
                    }
                }
            }

            // ����Ӧ�������ڵĸ߶�
            RectTrans_MouseItemContent.sizeDelta = new Vector2(RectTrans_MouseItemContent.sizeDelta.x, 10 + 90 * (1 + (mouseItemList.Count + bossItemList.Count) / 3));

            Emp_EnemyInfo.gameObject.SetActive(true);
        }

        /// <summary>
        /// ���صз���Ϣҳ��
        /// </summary>
        public void HideEnemyInfoPanel()
        {
            Emp_EnemyInfo.gameObject.SetActive(false);
        }

        /// <summary>
        /// ���õ�ǰѡ�е�����ͼ��
        /// </summary>
        /// <param name="mouseItem"></param>
        public void SetCurrentMouseItem(MouseItem_StageConfigPanel mouseItem)
        {
            currentMouseItem = mouseItem;
            currentBossItem = null;
            UpdateMousePanel();
        }

        /// <summary>
        /// ���õ�ǰѡ�е�Bossͼ��
        /// </summary>
        /// <param name="mouseItem"></param>
        public void SetCurrentBossItem(BossItem_StageConfigPanel bossItem)
        {
            currentBossItem = bossItem;
            currentMouseItem = null;
            UpdateMousePanel();
        }

        /// <summary>
        /// ����������Ϣ���
        /// </summary>
        private void UpdateMousePanel()
        {
            int type;
            int shape;
            if (currentMouseItem != null)
            {
                type = currentMouseItem.type;
                shape = currentMouseItem.shape;
                mMousePanel.Initial();
                mMousePanel.UpdateByMouseParam(type, shape);
            }
            else if (currentBossItem != null)
            {
                type = currentBossItem.enemyGroupInfo.mEnemyInfo.type;
                shape = currentBossItem.enemyGroupInfo.mEnemyInfo.shape;
                mMousePanel.Initial();
                mMousePanel.UpdateByBossParam(type, shape, currentBossItem.enemyGroupInfo.mHp);
            }


        }

        /// <summary>
        /// ������ʾ��Ϣ
        /// </summary>
        public void UpdateInfo()
        {
            // �ؿ��鱨
            BaseStage.StageInfo info = PlayerData.GetInstance().GetCurrentStageInfo();
            Img_Map.sprite = GameManager.Instance.GetSprite("Chapter/" + info.chapterIndex + "/" + info.sceneIndex + "/0");
            Tex_MapName.text = "��ͼ���ƣ�" + ChapterManager.GetSceneName((ChapterNameTypeMap)info.chapterIndex, info.sceneIndex);
            Tex_StageName.text = "�ؿ����ƣ�" + info.name;
            if (info.isEnableTimeLimit)
            {
                Tex_TimeLimit.gameObject.SetActive(true);
                Tex_TimeLimit.text = "ʱ�����ƣ�" + info.totalSeconds + "��";
            }
            else
            {
                Tex_TimeLimit.gameObject.SetActive(false);
            }
            if (info.isEnableJewelCount)
            {
                Tex_JewelLimit.gameObject.SetActive(true);
                Tex_JewelLimit.text = "��ʯ���ƣ�" + info.jewelCount + "��";
            }
            else
            {
                Tex_JewelLimit.gameObject.SetActive(false);
            }
            // �ؿ�����
            {
                Tex_Background.text = info.background;
                int countPerRow = Mathf.FloorToInt(Scr_Background.content.rect.width / Tex_Background.fontSize);
                int rowCount = Mathf.CeilToInt((float)Tex_Background.text.Length / countPerRow + 3); // ������Ҫ������
                foreach (var c in Tex_Background.text.ToCharArray())
                {
                    if (c.Equals('\n'))
                        rowCount++;
                }
                Scr_Background.content.sizeDelta = new Vector2(Scr_Background.content.sizeDelta.x, rowCount * Tex_Background.fontSize);
            }
            // �ؿ�����
            {
                Tex_Illustrate.text = info.illustrate;
                int countPerRow = Mathf.FloorToInt(Scr_Illustrate.content.rect.width / Tex_Illustrate.fontSize);
                int rowCount = Mathf.CeilToInt((float)Tex_Illustrate.text.Length / countPerRow + 3); // ������Ҫ������
                foreach (var c in Tex_Illustrate.text.ToCharArray())
                {
                    if (c.Equals('\n'))
                        rowCount++;
                }
                Scr_Illustrate.content.sizeDelta = new Vector2(Scr_Illustrate.content.sizeDelta.x, rowCount * Tex_Illustrate.fontSize);
            }

            // ����˵��
            Tex_AddInfo.text = info.additionalNotes;
        }

        /////////////////////////////////////���·����Ǳ�¶����ť�õ�/////////////////////////////////

        /// <summary>
        /// �ؿ��鱨��ť���
        /// </summary>
        public void OnStageInfoClick()
        {
            imgList[0].sprite = btnSprite1;
            imgList[1].sprite = btnSprite0;
            imgList[2].sprite = btnSprite0;
            Emp_StageInfo.gameObject.SetActive(true);
            HideEnemyInfoPanel();
            Scr_AddInfo.gameObject.SetActive(false);
        }

        /// <summary>
        /// �б��鱨��ť���
        /// </summary>
        public void OnEnemyInfoClick()
        {
            imgList[0].sprite = btnSprite0;
            imgList[1].sprite = btnSprite1;
            imgList[2].sprite = btnSprite0;
            Emp_StageInfo.gameObject.SetActive(false);
            ShowEnemyInfoPanel();
            Scr_AddInfo.gameObject.SetActive(false);
        }

        /// <summary>
        /// ����˵����ť���
        /// </summary>
        public void OnAddInfoClick()
        {
            imgList[0].sprite = btnSprite0;
            imgList[1].sprite = btnSprite0;
            imgList[2].sprite = btnSprite1;
            Emp_StageInfo.gameObject.SetActive(false);
            HideEnemyInfoPanel();
            Scr_AddInfo.gameObject.SetActive(true);
        }
    }

}
