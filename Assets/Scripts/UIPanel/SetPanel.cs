//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using DG.Tweening;
//using UnityEngine.UI;

//public class SetPanel : BasePanel
//{
//    private GameObject optionPageGo;
//    private GameObject statisticsPageGo;
//    private GameObject producerPageGo;
//    private GameObject panel_ResetGo;
//    private Tween setPanelTween;
//    private bool playBGMusic = true;
//    private bool playEffectMusic = true;
//    public Sprite[] btnSprites; //0.音效开 1.音效关 2.BGM开 3.BGM关
//    private Image Img_Btn_EffectAudio;
//    private Image Img_Btn_BGAudio;
//    public Text[] statisticesTexts;

//    protected override void Awake()
//    {
//        base.Awake();
//        setPanelTween = transform.DOLocalMoveX(0, 0.5f);
//        setPanelTween.SetAutoKill(false);
//        setPanelTween.Pause();
//        optionPageGo = transform.Find("OptionPage").gameObject;
//        statisticsPageGo = transform.Find("StatisticsPage").gameObject;
//        producerPageGo = transform.Find("ProducerPage").gameObject;
//        Img_Btn_EffectAudio = optionPageGo.transform.Find("Btn_EffectAudio").GetComponent<Image>();
//        Img_Btn_BGAudio = optionPageGo.transform.Find("Btn_BGAudio").GetComponent<Image>();
//        panel_ResetGo = transform.Find("Panel_Reset").gameObject;
//        // InitPanel();
//    }

//    public override void InitPanel()
//    {
//        transform.localPosition = new Vector3(-1920, 0, 0);
//        transform.SetSiblingIndex(5);
//    }

//    // 显示页面的方法
//    public void ShowOptionPage()
//    {
//        optionPageGo.SetActive(true);
//        statisticsPageGo.SetActive(false);
//        producerPageGo.SetActive(false);
//    }

//    public void ShowStatisticsPage()
//    {
//        optionPageGo.SetActive(false);
//        statisticsPageGo.SetActive(true);
//        producerPageGo.SetActive(false);
//        ShowStatistics();
//    }

//    public void ShowProducerPage()
//    {
//        optionPageGo.SetActive(false);
//        statisticsPageGo.SetActive(false);
//        producerPageGo.SetActive(true);
//    }

//    // 进入退出页面的方法
//    public override void EnterPanel()
//    {
//        ShowOptionPage();
//        MoveToCenter();
//    }

//    public override void ExitPanel()
//    {
//        setPanelTween.PlayBackwards();
//        mUIFacade.currentScenePanelDict[StringManager.MainPanel].EnterPanel();
//        InitPanel();
//    }

//    public void MoveToCenter()
//    {
//        setPanelTween.PlayForward();
//    }

//    // 音乐处理
//    public void CloseOrOpenBGMusic()
//    {
//        playBGMusic = !playBGMusic;
//        mUIFacade.CloseOrOpenBGMusic();
//        if (playBGMusic)
//        {
//            Img_Btn_BGAudio.sprite = btnSprites[2];
//        }
//        else
//        {
//            Img_Btn_BGAudio.sprite = btnSprites[3];
//        }
//    }

//    public void CloseOrOpeanEffectMusic()
//    {
//        playEffectMusic = !playEffectMusic;
//        mUIFacade.CloseOrOpenEffectMusic();
//        if (playEffectMusic)
//        {
//            Img_Btn_EffectAudio.sprite = btnSprites[0];
//        }
//        else
//        {
//            Img_Btn_EffectAudio.sprite = btnSprites[1];
//        }
//    }

//    // 数据显示
//    public void ShowStatistics()
//    {
//        PlayerManager playerManager = mUIFacade.mPlayerManager;
//        statisticesTexts[0].text = playerManager.adventrueModelNum.ToString();
//        statisticesTexts[1].text = playerManager.burriedLevelNum.ToString();
//        statisticesTexts[2].text = playerManager.bossModelNum.ToString();
//        statisticesTexts[3].text = playerManager.coin.ToString();
//        statisticesTexts[4].text = playerManager.killMonsterNum.ToString();
//        statisticesTexts[5].text = playerManager.killBossNum.ToString();
//        statisticesTexts[6].text = playerManager.clearItemNum.ToString();
//    }

//    // 重置游戏
//    public void ResetGame()
//    {
        
//    }

//    public void ShowResetPanel()
//    {
//        panel_ResetGo.SetActive(true);
//    }

//    public void CloseResetPanel()
//    {
//        panel_ResetGo.SetActive(false);

//    }
//}
