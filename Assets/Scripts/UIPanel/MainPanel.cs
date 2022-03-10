//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using DG.Tweening;

//public class MainPanel : BasePanel
//{
//    // ���ĳ����Դ��������Ϸ��ֻ�õ�һ�Σ���ֱ�ӻ�ȡ����ͨ�������ķ�ʽ��ȡ
//    private Animator carrotAnimator;
//    private Transform monsterTrans;
//    private Transform cloudTrans;
//    private Tween[] mainPanelTween; //0.�� 1.��
//    private Tween ExitTween;// �뿪��ҳ���еĶ���

//    protected override void Awake()
//    {
//        Debug.Log("MainPanel Awake!");
//        base.Awake();
//        // ��ȡ��Ա����
//        transform.SetSiblingIndex(8);
//        carrotAnimator = transform.Find("Emp_Carrot").GetComponent<Animator>();
//        carrotAnimator.Play("CarrotGrow");
//        monsterTrans = transform.Find("Img_Monster");
//        cloudTrans = transform.Find("Img_Cloud");

//        // ���������ƶ���
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

//    //UI��������
//    private void PlayUITween()
//    {
//        //���������Ч�����ƶ��������ѭ��Ʈ��Ч����
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

//    //����״̬�л��ķ���
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
