//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class GameNormalBigLevelPanel : BasePanel
//{
//    public Transform bigLevelContentTrans; // ������ͼ��content
//    public int bigLevelPageCount; // ��ؿ�����
//    private SlideScrollView slideScrollView;
//    private PlayerManager playerManager;
//    private Transform[] bigLevelPage; // ÿ����ذ�Ť������������Ǹ��飩

//    private bool hasRigisterEvent;

//    protected override void Awake()
//    {
//        base.Awake();
//        playerManager = mUIFacade.mPlayerManager;
//        bigLevelPage = new Transform[bigLevelPageCount];
//        slideScrollView = transform.Find("Scroll View").GetComponent<SlideScrollView>();
//        // ��ʾ��ؿ���Ϣ
//        for(int i = 0; i < bigLevelPageCount; i++)
//        {
//            bigLevelPage[i] = bigLevelContentTrans.GetChild(i);
//            ShowBigLevelState(
//                playerManager.unLockedNormalModelBigLevelList[i],
//                playerManager.unLockedNormalModelLevelNum[i],
//                5,
//                bigLevelPage[i],
//                //i+1);
//        }
//    }

//    private void OnEnable()
//    {
//        // ��ʾ��ؿ���Ϣ(ÿ�μ���ʱ��Ҫˢ��һ�´�ؿ���Ϣ��ʾ��
//        for (int i = 0; i < bigLevelPageCount; i++)
//        {
//            bigLevelPage[i] = bigLevelContentTrans.GetChild(i);
//            ShowBigLevelState(
//                playerManager.unLockedNormalModelBigLevelList[i],
//                playerManager.unLockedNormalModelLevelNum[i],
//                5,
//                bigLevelPage[i],
//                i + 1);
//        }
//    }

//    // �����˳����
//    public override void EnterPanel()
//    {
//        base.EnterPanel();
//        slideScrollView.Init();
//        gameObject.SetActive(true);
//    }

//    public override void ExitPanel()
//    {
//        base.ExitPanel();
//        gameObject.SetActive(false);
//    }

//    // ��ʾ��ؿ���Ϣ
//    public void ShowBigLevelState(bool unLocked, int unLockedLevelNum, int totalNum, Transform theBigLevelButtonTrans, int bigLevelID)
//    {
//        if (unLocked) //����״̬
//        {
//            theBigLevelButtonTrans.Find("Img_Lock").gameObject.SetActive(false);
//            theBigLevelButtonTrans.Find("Img_Page").gameObject.SetActive(true);
//            theBigLevelButtonTrans.Find("Img_Page").Find("Tex_Page").GetComponent<Text>().text = unLockedLevelNum.ToString() + "/" + totalNum.ToString();
//            Button theBigLevelButtonCom = theBigLevelButtonTrans.GetComponent<Button>();
//            theBigLevelButtonCom.interactable = true;
//            if (!hasRigisterEvent) // ��ֹ���ע�ᰴŤ�¼�
//            {
//                theBigLevelButtonCom.onClick.AddListener(() =>
//                {
//                    // �뿪��ؿ�ҳ��
//                    mUIFacade.currentScenePanelDict[StringManager.GameNormalBigLevelPanel].ExitPanel();
//                    // ����С�ؿ�
//                    GameNormalLevelPanel gameNormalLevelPanel = mUIFacade.currentScenePanelDict[StringManager.GameNormalLevelPanel] as GameNormalLevelPanel;
//                    gameNormalLevelPanel.ToThisPanel(bigLevelID);
//                    // ��������ҳ��
//                    GameNormalOptionPanel gameNormalOptionPanel = mUIFacade.currentScenePanelDict[StringManager.GameNormalOptionPanel] as GameNormalOptionPanel;
//                    gameNormalOptionPanel.isInBigLevelPanel = false;
//                });
//                hasRigisterEvent = true;
//            }
//        }
//        else //δ����
//        {
//            theBigLevelButtonTrans.Find("Img_Lock").gameObject.SetActive(true);
//            theBigLevelButtonTrans.Find("Img_Page").gameObject.SetActive(false);
//            theBigLevelButtonTrans.GetComponent<Button>().interactable = false; //���ö�ӦButtonΪ���ɵ��״̬

//        }
//    }

//    //��ҳ��ť����
//    public void ToNextPage()
//    {
//        slideScrollView.ToNextPage();
//    }

//    public void ToLastPage()
//    {
//        slideScrollView.ToLastPage();
//    }
//}
