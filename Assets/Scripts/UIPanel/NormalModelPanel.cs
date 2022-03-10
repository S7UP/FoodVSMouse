//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
///// <summary>
///// ��Ϸ����UI���
///// </summary>
//public class NormalModelPanel : BasePanel
//{
//    // ���Ƶ�ҳ��
//    private GameObject topPageGo;
//    private GameObject gameOverPageGo;
//    private GameObject gameWinPageGo;
//    private GameObject menuPageGo;
//    private GameObject img_FinalWave;
//    private GameObject img_StartGame;
//    private GameObject prizePageGo;

//    public int totalRound;

//    // ����
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

//    // ���ſ�ʼ��Ϸ����ʱ��Ч
//    private void PlayAudio()
//    {

//    }

//    // ��ʼ��Ϸ
//    private void StartGame()
//    {
//        gameController.StartGame();
//    }

//    /// <summary>
//    /// ����崦���йصķ���
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
//    /// ҳ����ʾ���صķ���
//    /// </summary>
//    public void ShowPrizePage()
//    {
//        prizePageGo.SetActive(true);
//    }

//    public void ClosePrizePage()
//    {
//        prizePageGo.SetActive(false);
//    }

//    // �˵�ҳ��
//    public void ShowMenuPage()
//    {
//        menuPageGo.SetActive(true);
//    }

//    public void CloseMenuPage()
//    {
//        menuPageGo.SetActive(false);
//    }

//    // ʤ��ҳ��
//    public void ShowGameWinPage()
//    {
//        Stage stage = GameManager.Instance.playerManager.unLockedNormalModelLevelList[gameController.currentStage.mLevelID-1+(gameController.currentStage.mBigLevelID)*5];
//        // ���߻��¸���
//        if (gameController.IfAllClear())
//        {
//            stage.mAllClear = true;
//        }
//        // �ܲ����¸���
//        int carrotState = gameController.GetCarrotState();
//        if (carrotState != 0)
//        {
//            if (stage.mCarrotState > carrotState)
//            {
//                stage.mCarrotState = carrotState;
//            }
//        }
//        // ������һ���ؿ�
//        // �������һ���Ҳ������عؿ����ܽ�����һ��
//        if (gameController.currentStage.mLevelID % 5 != 0 && (gameController.currentStage.mLevelID - 1 + (gameController.currentStage.mBigLevelID) * 5)<GameManager.Instance.playerManager.unLockedNormalModelLevelList.Count)
//        {
//            GameManager.Instance.playerManager.unLockedNormalModelLevelList[gameController.currentStage.mLevelID + (gameController.currentStage.mBigLevelID) * 5].unLocked = true;
//        }
//        UpdatePlayerManagerData();
//        gameWinPageGo.SetActive(true);
//        gameController.gameOver = false;
//        GameManager.Instance.playerManager.adventrueModelNum++;
//    }

//    // ʧ��ҳ��
//    public void ShowGameOverPage()
//    {
//        UpdatePlayerManagerData();
//        gameOverPageGo.SetActive(true);
//        gameController.gameOver = false;
//    }

//    /// <summary>
//    /// ��UI��ʾ�йصķ���
//    /// </summary>
     
//    // ���»غϵ���ʾ�ı�
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

//    // ���һ����UI
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
//    /// ��ؿ������йصķ���
//    /// </summary>
    
//    // ���»�������
//    private void UpdatePlayerManagerData()
//    {
//        GameManager.Instance.playerManager.coin += gameController.coin;
//        GameManager.Instance.playerManager.killMonsterNum += gameController.killMonsterTotalNum;
//        GameManager.Instance.playerManager.clearItemNum += gameController.clearItemNum;
//    }

//    // ����
//    public void Replay()
//    {
//        UpdatePlayerManagerData();
//        mUIFacade.ChangeSceneState(new NormalModelSceneState(mUIFacade));
//        gameController.gameOver = false;
//        Invoke("ResetGame", 2);
//    }

//    // ���õ�ǰ�ؿ���Ϸ
//    private void ResetGame()
//    {
//        SceneManager.LoadScene(4);
//        ResetUI();
//        gameObject.SetActive(true);
//    }

//    // ����ҳ��UI��ʾ״̬
//    public void ResetUI()
//    {
//        gameOverPageGo.SetActive(false);
//        gameWinPageGo.SetActive(false);
//        menuPageGo.SetActive(false);
//        gameObject.SetActive(false);
//    }

//    // ѡ�������ؿ�
//    public void ChooseOtherLevel()
//    {
//        gameController.gameOver = false;
//        UpdatePlayerManagerData();
//        Invoke("ToOtherScene", 2);
//        mUIFacade.ChangeSceneState(new NormalGameOptionSceneState(mUIFacade));
//    }

//    // ���عؿ�ѡ�񳡾�
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
