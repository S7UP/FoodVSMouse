//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using DG.Tweening;

//public class MainPanel : BasePanel
//{
//    // 如果某个资源在整个游戏中只用到一次，就直接获取，不通过工厂的方式获取
//    private Animator carrotAnimator;
//    private Transform monsterTrans;
//    private Transform cloudTrans;
//    private Tween[] mainPanelTween; //0.右 1.左
//    private Tween ExitTween;// 离开主页运行的动画

//    protected override void Awake()
//    {
//        Debug.Log("MainPanel Awake!");
//        base.Awake();
//        // 获取成员变量
//        transform.SetSiblingIndex(8);
//        carrotAnimator = transform.Find("Emp_Carrot").GetComponent<Animator>();
//        carrotAnimator.Play("CarrotGrow");
//        monsterTrans = transform.Find("Img_Monster");
//        cloudTrans = transform.Find("Img_Cloud");

//        // 设置左右移动画
//        mainPanelTween = new Tween[2];
//        mainPanelTween[0] = transform.DOLocalMoveX(1920, 0.5f);
//        mainPanelTween[0].SetAutoKill(false);
//        mainPanelTween[0].Pause();

//        mainPanelTween[1] = transform.DOLocalMoveX(-1920, 0.5f);
//        mainPanelTween[1].SetAutoKill(false);
//        mainPanelTween[1].Pause();

//        PlayUITween();
//    }

//    public override void EnterPanel()
//    {
//        transform.SetSiblingIndex(8);
//        carrotAnimator.Play("CarrotGrow");
//        if (ExitTween != null)
//        {
//            ExitTween.PlayBackwards();
//        }
//        cloudTrans.gameObject.SetActive(true);
//    }

//    public override void ExitPanel()
//    {
//        ExitTween.PlayForward();
//        cloudTrans.gameObject.SetActive(false);
//    }

//    //UI动画播放
//    private void PlayUITween()
//    {
//        //（鸟怪悬浮效果与云朵从左向右循环飘的效果）
//        monsterTrans.DOLocalMoveY(600, 1.5f).SetLoops(-1, LoopType.Yoyo);
//        cloudTrans.DOLocalMoveX(1300, 8f).SetLoops(-1, LoopType.Restart);
//    }

//    public void MoveToRight()
//    {
//        ExitTween = mainPanelTween[0];
//        mUIFacade.currentScenePanelDict[StringManager.SetPanel].EnterPanel();
//    }

//    public void MoveToLeft()
//    {
//        ExitTween = mainPanelTween[1];
//        mUIFacade.currentScenePanelDict[StringManager.HelpPanel].EnterPanel();
//    }

//    //场景状态切换的方法
//    public void ToNormalModelScene()
//    {
//        mUIFacade.currentScenePanelDict[StringManager.GameLoadPanel].EnterPanel();
//        mUIFacade.ChangeSceneState(new NormalGameOptionSceneState(mUIFacade));
//    }

//    public void ToBossModelScene()
//    {
//        mUIFacade.currentScenePanelDict[StringManager.GameLoadPanel].EnterPanel();
//        mUIFacade.ChangeSceneState(new BossGameOptionSceneState(mUIFacade));
//    }

//    public void ToMonsterNest()
//    {
//        mUIFacade.currentScenePanelDict[StringManager.GameLoadPanel].EnterPanel();
//        mUIFacade.ChangeSceneState(new MonsterNestSceneState(mUIFacade));
//    }

//    public void ExitGame()
//    {
//        Application.Quit();
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
