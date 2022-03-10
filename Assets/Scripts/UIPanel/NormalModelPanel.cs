//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
///// <summary>
///// 游戏场景UI面板
///// </summary>
//public class NormalModelPanel : BasePanel
//{
//    // 控制的页面
//    private GameObject topPageGo;
//    private GameObject gameOverPageGo;
//    private GameObject gameWinPageGo;
//    private GameObject menuPageGo;
//    private GameObject img_FinalWave;
//    private GameObject img_StartGame;
//    private GameObject prizePageGo;

//    public int totalRound;

//    // 引用
//    public TopPage topPage;
//    private GameController gameController;

//    protected override void Awake()
//    {
//        base.Awake();
//        gameController = GameController.Instance;
//        transform.SetSiblingIndex(1);
//        topPageGo = transform.Find("Img_TopPage").gameObject;
//        gameOverPageGo = transform.Find("GameOverPage").gameObject;
//        gameWinPageGo = transform.Find("GameWinPage").gameObject;
//        menuPageGo = transform.Find("MenuPage").gameObject;
//        prizePageGo = transform.Find("PrizePage").gameObject;
//        img_FinalWave = transform.Find("Img_FinalWave").gameObject;
//        img_StartGame = transform.Find("StartUI").gameObject;
//        topPage = topPageGo.GetComponent<TopPage>();
//    }

//    private void OnEnable()
//    {
//        img_StartGame.SetActive(true);
//        InvokeRepeating("PlayAudio", 0, 1);
//        Invoke("StartGame", 3);
//    }

//    // 播放开始游戏倒计时音效
//    private void PlayAudio()
//    {

//    }

//    // 开始游戏
//    private void StartGame()
//    {
//        gameController.StartGame();
//    }

//    /// <summary>
//    /// 与面板处理有关的方法
//    /// </summary>
//    public override void EnterPanel()
//    {
//        base.EnterPanel();
//        totalRound = gameController.currentStage.mTotalRound;
//        topPageGo.SetActive(true);
//    }

//    public override void UpdatePanel()
//    {
//        base.UpdatePanel();
//        topPage.UpdateRoundText();
//        topPage.UpdateCoinText();
//    }

//    /// <summary>
//    /// 页面显示隐藏的方法
//    /// </summary>
//    public void ShowPrizePage()
//    {
//        prizePageGo.SetActive(true);
//    }

//    public void ClosePrizePage()
//    {
//        prizePageGo.SetActive(false);
//    }

//    // 菜单页面
//    public void ShowMenuPage()
//    {
//        menuPageGo.SetActive(true);
//    }

//    public void CloseMenuPage()
//    {
//        menuPageGo.SetActive(false);
//    }

//    // 胜利页面
//    public void ShowGameWinPage()
//    {
//        Stage stage = GameManager.Instance.playerManager.unLockedNormalModelLevelList[gameController.currentStage.mLevelID-1+(gameController.currentStage.mBigLevelID)*5];
//        // 道具徽章更新
//        if (gameController.IfAllClear())
//        {
//            stage.mAllClear = true;
//        }
//        // 萝卜徽章更新
//        int carrotState = gameController.GetCarrotState();
//        if (carrotState != 0)
//        {
//            if (stage.mCarrotState > carrotState)
//            {
//                stage.mCarrotState = carrotState;
//            }
//        }
//        // 解锁下一个关卡
//        // 不是最后一关且不是隐藏关卡才能解锁下一关
//        if (gameController.currentStage.mLevelID % 5 != 0 && (gameController.currentStage.mLevelID - 1 + (gameController.currentStage.mBigLevelID) * 5)<GameManager.Instance.playerManager.unLockedNormalModelLevelList.Count)
//        {
//            GameManager.Instance.playerManager.unLockedNormalModelLevelList[gameController.currentStage.mLevelID + (gameController.currentStage.mBigLevelID) * 5].unLocked = true;
//        }
//        UpdatePlayerManagerData();
//        gameWinPageGo.SetActive(true);
//        gameController.gameOver = false;
//        GameManager.Instance.playerManager.adventrueModelNum++;
//    }

//    // 失败页面
//    public void ShowGameOverPage()
//    {
//        UpdatePlayerManagerData();
//        gameOverPageGo.SetActive(true);
//        gameController.gameOver = false;
//    }

//    /// <summary>
//    /// 与UI显示有关的方法
//    /// </summary>
     
//    // 更新回合的显示文本
//    public void ShowRoundText(Text roundText)
//    {
//        int roundNum = gameController.level.currentRound + 1;
//        string roundStr = "";
//        if(roundNum < 10)
//        {
//            roundStr = "0  " + roundNum.ToString();
//        }
//        else
//        {
//            roundStr = (roundNum / 10).ToString() + "  " + (roundNum % 10).ToString();
//        }
//        roundText.text = roundStr;
//    }

//    // 最后一波怪UI
//    public void ShowFinalWaveUI()
//    {
//        img_FinalWave.SetActive(true);
//        Invoke("CloseFinalWaveUI",1f);
//    }

//    private void CloseFinalWaveUI()
//    {
//        img_FinalWave.SetActive(false);
//    }

//    /// <summary>
//    /// 与关卡处理有关的方法
//    /// </summary>
    
//    // 更新基础数据
//    private void UpdatePlayerManagerData()
//    {
//        GameManager.Instance.playerManager.coin += gameController.coin;
//        GameManager.Instance.playerManager.killMonsterNum += gameController.killMonsterTotalNum;
//        GameManager.Instance.playerManager.clearItemNum += gameController.clearItemNum;
//    }

//    // 重玩
//    public void Replay()
//    {
//        UpdatePlayerManagerData();
//        mUIFacade.ChangeSceneState(new NormalModelSceneState(mUIFacade));
//        gameController.gameOver = false;
//        Invoke("ResetGame", 2);
//    }

//    // 重置当前关卡游戏
//    private void ResetGame()
//    {
//        SceneManager.LoadScene(4);
//        ResetUI();
//        gameObject.SetActive(true);
//    }

//    // 重置页面UI显示状态
//    public void ResetUI()
//    {
//        gameOverPageGo.SetActive(false);
//        gameWinPageGo.SetActive(false);
//        menuPageGo.SetActive(false);
//        gameObject.SetActive(false);
//    }

//    // 选择其他关卡
//    public void ChooseOtherLevel()
//    {
//        gameController.gameOver = false;
//        UpdatePlayerManagerData();
//        Invoke("ToOtherScene", 2);
//        mUIFacade.ChangeSceneState(new NormalGameOptionSceneState(mUIFacade));
//    }

//    // 返回关卡选择场景
//    public void ToOtherScene()
//    {
//        gameController.gameOver = false;
//        ResetUI();
//        SceneManager.LoadScene(2);
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
