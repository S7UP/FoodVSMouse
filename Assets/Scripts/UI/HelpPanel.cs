//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using DG.Tweening;
//using UnityEngine.SceneManagement;

//public class HelpPanel : BasePanel
//{
//    private GameObject helpPageGo;
//    private GameObject monsterPageGo;
//    private GameObject towerPageGo;
//    private SlideScrollView slideScrollView;
//    private SlideCanCoverScrollView slideCanCoverScrollView;
//    private Tween helpPanelTween;

//    protected override void Awake()
//    {
//        base.Awake();
//        helpPageGo = transform.Find("HelpPage").gameObject;
//        monsterPageGo = transform.Find("MonsterPage").gameObject;
//        towerPageGo = transform.Find("TowerPage").gameObject;
//        slideCanCoverScrollView = transform.Find("HelpPage").Find("Scroll View").GetComponent<SlideCanCoverScrollView>();
//        slideScrollView = transform.Find("TowerPage").Find("Scroll View").GetComponent<SlideScrollView>();
//        helpPanelTween = transform.DOLocalMoveX(0, 0.5f);
//        helpPanelTween.SetAutoKill(false);
//        helpPanelTween.Pause();


//    }

//    // ��ʾҳ��ķ���
//    public void ShowHelpPage()
//    {
//        helpPageGo.SetActive(true);
//        monsterPageGo.SetActive(false);
//        towerPageGo.SetActive(false);
//    }

//    public void ShowMonsterPage()
//    {
//        helpPageGo.SetActive(false);
//        monsterPageGo.SetActive(true);
//        towerPageGo.SetActive(false);
//    }

//    public void ShowTowerPage()
//    {
//        helpPageGo.SetActive(false);
//        monsterPageGo.SetActive(false);
//        towerPageGo.SetActive(true);
//    }

//    //�������ķ���
//    public override void InitPanel()
//    {
//        base.InitPanel();
//        transform.SetSiblingIndex(5);
//        slideScrollView.Init();
//        slideCanCoverScrollView.Init();
//        ShowHelpPage();

//        // ��������
//        if (transform.localPosition == Vector3.zero)
//        {
//            gameObject.SetActive(false);
//            helpPanelTween.PlayBackwards();
//        }
//        transform.localPosition = new Vector3(1920, 0, 0);
//    }

//    public override void EnterPanel()
//    {
//        base.EnterPanel();
//        gameObject.SetActive(true);
//        slideScrollView.Init();
//        slideCanCoverScrollView.Init();
//        MoveToCenter();

//    }

//    public override void ExitPanel()
//    {
//        base.ExitPanel();


//        if (mUIFacade.currentSceneState.GetType() == typeof(NormalGameOptionSceneState))
//        {
//            // ��ð��ģʽѡ�񳡾�
//            mUIFacade.ChangeSceneState(new MainSceneState(mUIFacade));
//            SceneManager.LoadScene(1);
//        }
//        else // �������������
//        {
//            helpPanelTween.PlayBackwards();
//            mUIFacade.currentScenePanelDict[StringManager.MainPanel].EnterPanel();
//        }
//    }

//    public void MoveToCenter()
//    {
//        helpPanelTween.PlayForward();
//    }
//}
