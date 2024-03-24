using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 负责管理战斗场景UI面板的类
/// </summary>

public class GameNormalPanel : BasePanel
{
    public static GameNormalPanel Instance;

    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList; // 卡槽UI（横向）
    public GameObject mCardBuilderList2;  // 卡槽UI（纵向）
    public CardModel mCardModel; // 卡片模型（选卡时会跟随鼠标的那种）
    public Transform mShovelSlot2Trans; // 铲子堆放处
    public ShovelModel mShovelModel; // 铲子模型
    public Text Tex_Pause; // 暂停文本
    public GameObject MenuUI; // 菜单
    public GameObject Img_Hp; // 血量实例
    private Text Tex_Hp; // 血量文本
    private GameObject WinPanel;
    private Animator Ani_Win;
    private GameObject LosePanel;
    private Animator Ani_Lose;
    private GameObject Img_BigWave;
    private Animator Ani_BigWave;
    private GameObject PauseUI;
    private GameObject PutCharacterUI;
    private InfoUI_GameNormalPanel InfoUI;
    private Transform Tran_JewelSkillUI;
    private JewelSkill_GameNormalPanel[] JewelSkillArray;
    private Text Tex_ExpTips; // 经验值文本

    private Image Img_Rank;
    private Text Tex_Rank;
    private Text Tex_StageName;
    private Text Tex_PlayTime;
    private Text Tex_RealPlayTime;
    private Text Tex_TotalEnergy;
    private Text Tex_PauseCount;
    private GameObject NoLimit_Go;

    private RectTransform RectTrans_BGM;
    private Text Tex_BGM;
    private Tweener Tweener_BGM;

    public bool IsInCharacterConstructMode; // 是否在角色放置模式

    private List<BaseUI> uiList = new List<BaseUI>(); // 附加ui表
    private Transform Trans_BottomLayer;

    private float startRealTime; // 起始记录的实际时间


    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        DontDestroyOnLoad(gameObject); // 切场景时不销毁
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mShovelSlot2Trans = mCardControllerUI.transform.Find("ShovelSlot2");
        mShovelModel = mCardControllerUI.transform.Find("Emp_ShovelModel").gameObject.GetComponent<ShovelModel>();
        Tex_Pause = transform.Find("Btn_Pause").Find("Text").GetComponent<Text>();
        MenuUI = transform.Find("MenuUI").gameObject;
        Img_Hp = transform.Find("Img_Hp").gameObject;
        Tex_Hp = transform.Find("Img_Hp").Find("Text").GetComponent<Text>();
        //mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 铲子归位
        WinPanel = transform.Find("WinPanel").gameObject;
        Ani_Win = WinPanel.transform.Find("Img_Win").GetComponent<Animator>();
        LosePanel = transform.Find("LosePanel").gameObject;
        Ani_Lose = LosePanel.transform.Find("Img_Lose").GetComponent<Animator>();
        Img_BigWave = transform.Find("Img_BigWave").gameObject;
        Ani_BigWave = Img_BigWave.GetComponent<Animator>();
        PauseUI = transform.Find("PauseUI").gameObject;
        PutCharacterUI = transform.Find("PutCharacterUI").gameObject;
        InfoUI = transform.Find("InfoUI").GetComponent<InfoUI_GameNormalPanel>();
        // Prefab_Tex_DamageNumber = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "GameNormalPanel/Tex_DamageNumber");
        Tran_JewelSkillUI = transform.Find("JewelSkillUI").transform;
        JewelSkillArray = new JewelSkill_GameNormalPanel[3] {
            Tran_JewelSkillUI.Find("Btn_Skill1").GetComponent<JewelSkill_GameNormalPanel>(),
            Tran_JewelSkillUI.Find("Btn_Skill2").GetComponent<JewelSkill_GameNormalPanel>(),
            Tran_JewelSkillUI.Find("Btn_Skill3").GetComponent<JewelSkill_GameNormalPanel>()
        };
        Tex_ExpTips = transform.Find("ExpTips").GetComponent<Text>();
        Img_Rank = transform.Find("LeftBottomInfo").Find("Rank_Label").Find("Rank").GetComponent<Image>();
        Tex_Rank = transform.Find("LeftBottomInfo").Find("Rank_Label").Find("Text").GetComponent<Text>();
        Tex_StageName = transform.Find("LeftBottomInfo").Find("StageName_Label").Find("Text").GetComponent<Text>();
        Tex_PlayTime = transform.Find("LeftBottomInfo").Find("Time_Label").Find("Text").GetComponent<Text>();
        Tex_RealPlayTime = transform.Find("LeftBottomInfo").Find("RealTime_Label").Find("Text").GetComponent<Text>();
        Tex_TotalEnergy = transform.Find("LeftBottomInfo").Find("TotalEnergy_Label").Find("Text").GetComponent<Text>();
        Tex_PauseCount = transform.Find("LeftBottomInfo").Find("PauseCount_Label").Find("Text").GetComponent<Text>();
        NoLimit_Go = transform.Find("LeftBottomInfo").Find("NoLimit_Go").gameObject;
        {
            RectTransform rect = NoLimit_Go.GetComponent<RectTransform>();
            // 鼠标进入的触发器
            {
                EventTrigger trigger = NoLimit_Go.GetComponent<EventTrigger>();
                EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
                e.AddListener(delegate {
                    TextArea.Instance.SetText("此模式下胜利不会记录任何成绩。");
                    TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.rect.width, 0), new Vector2(2, 0.5f));
                    // TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.rect.width / 2, 0), new Vector2(0, 0));
                });
                trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = e });
            }
            // 鼠标退出的触发器
            {
                EventTrigger trigger = NoLimit_Go.GetComponent<EventTrigger>();
                EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
                e.AddListener(delegate {
                    TextArea.ExecuteRecycle();
                });
                trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit, callback = e });
            }
        }


        RectTrans_BGM = transform.Find("BGM").GetComponent<RectTransform>();
        Tex_BGM = RectTrans_BGM.Find("Text").GetComponent<Text>();
        Tweener_BGM = RectTrans_BGM.DOAnchorPosY(42, 2);
        Tweener_BGM.SetAutoKill(false);
        Tweener_BGM.Pause();

        Trans_BottomLayer = transform.Find("BottomLayer");
    }

    public override void InitPanel()
    {
        base.EnterPanel();
        // 停止所有BGM
        GameManager.Instance.audioSourceController.StopAllMusic();
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 铲子归位
        Tex_Pause.text = "暂停(Space)";
        MenuUI.SetActive(false);
        Img_Hp.SetActive(false);
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        Img_BigWave.SetActive(false);
        PauseUI.SetActive(false);
        PutCharacterUI.SetActive(false);
        IsInCharacterConstructMode = false;
        InfoUI.gameObject.SetActive(false);
        Tex_ExpTips.gameObject.SetActive(false);

        foreach (var ui in uiList)
            ui.MDestory();
        uiList.Clear();
        // 可以让GameController开始工作了
        GameController.Instance.SetStart();
    }

    public override void EnterPanel()
    {

    }

    /// <summary>
    /// 在GameController中的初始化方法
    /// </summary>
    public void InitInGameController()
    {
        //mCardModel = GameObject.Find("Emp_CardModel").GetComponent<CardModel>();
        mCardModel = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_CardModel").GetComponent<CardModel>();
        mCardModel.transform.SetParent(GameController.Instance.transform);
        mCardModel.transform.localScale = Vector2.one;
        mCardModel.HideCardModel(); // 隐藏卡片模型
        ExitCharacterConstructMode();
        ExitCardConstructMode();
        ExitCardRemoveMode();
        // 读取玩家选择的宝石，安上对应的技能图标
        PlayerData data = PlayerData.GetInstance();
        PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
        BaseStage.StageInfo info = info_dynamic.info;
        for (int i = 0; i < 3; i++)
        {
            int type = data.GetJewel(i);
            if (i + 1 > info.jewelCount && info.isEnableJewelCount && !info_dynamic.isNoLimit)
                type = -1; // -1表示的是锁定
            JewelSkillArray[i].MInit(type, GameController.Instance.mJewelSkillArray[i]);
        }
        // 读取游戏当前难度并重置时间
        Tex_Rank.gameObject.SetActive(false);
        Img_Rank.gameObject.SetActive(true);
        Img_Rank.gameObject.SetActive(false);
        Tex_Rank.gameObject.SetActive(true);
        Tex_Rank.text = (Mathf.FloorToInt(data.GetRankRate() * 100)).ToString() + "%";

        Tex_StageName.text = "unknow";
        Tex_PlayTime.text = "00:00:00.00";
        Tex_RealPlayTime.text = "00:00:00.00";
        Tex_TotalEnergy.text = "--";
        if(info.pauseCount > -1)
            Tex_PauseCount.text = "0/"+info.pauseCount;
        else
            Tex_PauseCount.text = "0/∞";
        // 是否解限
        NoLimit_Go.SetActive(info_dynamic.isNoLimit);
    }

    /// <summary>
    /// 当暂停按钮被点击时
    /// </summary>
    public void OnPauseButtonClick()
    {
        // 在人物放置阶段点暂停 或者 游戏结束时暂停 是没有任何吊用的
        if (IsInCharacterConstructMode || MenuUI.activeInHierarchy || GameController.Instance.isOver)
            return;

        // 先获取目前是暂停还是非暂停状态
        if (GameController.Instance.isPause)
        {
            // 如果是暂停状态，则解除暂停
            GameController.Instance.Resume();
            // 设置文字
            Tex_Pause.text = "暂停(Space)";
            // 移除遮罩
            PauseUI.SetActive(false);
        }
        else
        {
            // 如果是非暂停状态，则需要判断一下可否暂停
            PlayerData data = PlayerData.GetInstance();
            PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
            BaseStage.StageInfo info = info_dynamic.info;

            if(info_dynamic.isNoLimit || info.pauseCount <= -1 || GameController.Instance.pauseCount < info.pauseCount)
            {
                GameController.Instance.Pause();
                // 设置文字
                Tex_Pause.text = "解除暂停";
                // 显示遮罩
                PauseUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 当菜单按钮被点击时
    /// </summary>
    public void OnMenuButtonClick()
    {
        PauseUI.SetActive(false);
        SetMenuEnable(true);
    }

    /// <summary>
    /// 当返回游戏按钮被点击时
    /// </summary>
    public void OnReturnToGameClick()
    {
        SetMenuEnable(false);
    }

    /// <summary>
    /// 当重新尝试被点击时（胜利或失败结算页面的重新尝试）
    /// </summary>
    public void OnRetryClick()
    {
        InitPanel();
        GameController.Instance.Restart();
    }

    /// <summary>
    /// 当游戏非正常结束时（胜利和失败以外的中断方式）的奖励结算
    /// </summary>
    private void RewardWhenGameInterrupted()
    {
        PlayerData data = PlayerData.GetInstance();
        data.AddExp(GameController.Instance.GetDefaultExpReward()); // 加经验值
        data.playTime += GetRealTime();
        // 如果是正式收录的关，还需要更新用时记录
        PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
        if (info_dynamic != null && info_dynamic.id != null)
        {
            string id = info_dynamic.id;
            StageInfoManager.StageInfo_Local local_info = StageInfoManager.GetLocalStageInfo(id);
            // 总用时记录
            local_info.totalPlayTime += GetRealTime();
        }
    }

    /// <summary>
    /// 当重新尝试被点击时（游戏还未结束，是从菜单主动点击重新尝试的）
    /// </summary>
    public void OnClickRetryWhenNoEnd()
    {
        // 推把也加经验值的
        RewardWhenGameInterrupted();
        OnRetryClick();
    }

    /// <summary>
    /// 当返回选择配置被点击时
    /// </summary>
    public void OnReturnToSelcetClick()
    {
        GameManager.Instance.EnterTownScene();
    }

    /// <summary>
    /// 当返回选择配置被点击时（游戏还未结束，是从菜单主动点击重新尝试的）
    /// </summary>
    public void OnReturnToSelcetClickWhenNoEnd()
    {
        // 推把也加经验值的
        RewardWhenGameInterrupted();
        OnReturnToSelcetClick();
    }

    /// <summary>
    /// 当返回主菜单被点击时
    /// </summary>
    public void OnReturnToMainClick()
    {
        // GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterMainScene();
    }

    /// <summary>
    /// 当返回主菜单被点击时（游戏还未结束，是从菜单主动点击重新尝试的）
    /// </summary>
    public void OnReturnToMainClickWhenNoEnd()
    {
        // 推把也加经验值的
        RewardWhenGameInterrupted();
        OnReturnToMainClick();
    }

    /// <summary>
    /// 设置菜单显示
    /// </summary>
    private void SetMenuEnable(bool enable)
    {
        // 在人物放置阶段不执行后续逻辑
        if (IsInCharacterConstructMode)
            return;

        MenuUI.SetActive(enable);

        if (enable)
        {
            // 如果是非暂停状态，则需要判断一下可否暂停
            PlayerData data = PlayerData.GetInstance();
            PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
            BaseStage.StageInfo info = info_dynamic.info;

            if (!GameController.Instance.isPause && (info_dynamic.isNoLimit || info.pauseCount <= -1 || GameController.Instance.pauseCount < info.pauseCount))
                GameController.Instance.Pause();
        }
        else if(GameController.Instance.isPause)
        {
            GameController.Instance.Resume();
        }
            
    }

    /// <summary>
    /// 清除所有卡槽
    /// </summary>
    public void ClearAllSlot()
    {
        List<BaseCardBuilder> list = new List<BaseCardBuilder>();
        for (int i = 0; i < mCardBuilderList.transform.childCount; i++)
        {
            list.Add(mCardBuilderList.transform.GetChild(i).GetComponent<BaseCardBuilder>());
        }
        for (int i = 0; i < mCardBuilderList2.transform.childCount; i++)
        {
            list.Add(mCardBuilderList2.transform.GetChild(i).GetComponent<BaseCardBuilder>());
        }
        foreach (var item in list)
        {
            item.Recycle();
        }
    }

    // 在上方UI中增加一个卡槽
    public bool AddCardSlot(BaseCardBuilder CardBuilder)
    {
        if (mCardBuilderList.transform.childCount < 13)
        {
            CardBuilder.SetIndex(mCardBuilderList.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
            return true;
        }
        else if(mCardBuilderList2.transform.childCount < 5)
        {
            CardBuilder.SetIndex(13 + mCardBuilderList2.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList2.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent完后请把scale改到1，否则会扩大100倍
            return true;
        }
        else
        {
            Debug.LogWarning("携带卡片已超过18张！！多余卡片槽不生效");
            return false;
        }
    }

    /// <summary>
    /// 进入角色放置模式
    /// </summary>
    public void EnterCharacterConstructMode()
    {
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCharacter);
        Img_Hp.SetActive(false);
        IsInCharacterConstructMode = true;
        PutCharacterUI.SetActive(true);
    }

    /// <summary>
    /// 离开角色放置模式
    /// </summary>
    public void ExitCharacterConstructMode()
    {
        mCardModel.HideCardModel();
        Img_Hp.SetActive(true);
        IsInCharacterConstructMode = false;
        PutCharacterUI.SetActive(false);
    }

    /// <summary>
    /// 进入卡片放置模式，由某个卡片建造器被成功选取时触发
    /// </summary>
    public void EnterCardConstructMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCard);
    }

    /// <summary>
    /// 离开卡片放置模式，卡片建造成功或者取消建造时触发
    /// </summary>
    public void ExitCardConstructMode()
    {
        mCardModel.HideCardModel();
    }

    /// <summary>
    /// 进入卡片移除模式
    /// </summary>
    public void EnterCardRemoveMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mShovelModel.ShowModel();
    }

    /// <summary>
    /// 离开卡片移除模式
    /// </summary>
    public void ExitCardRemoveMode()
    {
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // 归位
        mShovelModel.HideModel();
    }

    /// <summary>
    /// 当铲子槽位被点击（触发）
    /// </summary>
    public void OnShovelSlotTrigger()
    {
        if (IsInCharacterConstructMode)
            return;
        GameController.Instance.mCardController.SelectShovel();
    }



    /// <summary>
    /// 设置人物HP文本与位置
    /// </summary>
    /// <param name="hp"></param>
    public void SetCharacterHpTextAndPosition(float hp, Vector3 position)
    {
        Img_Hp.transform.position = position;
        Tex_Hp.text = "HP:" + Mathf.FloorToInt(hp);
    }

    /// <summary>
    /// 进入胜利结算界面
    /// </summary>
    public void EnterWinPanel()
    {
        //InitPanel();
        WinPanel.SetActive(true);
        Tex_ExpTips.gameObject.SetActive(true);
        Ani_Win.Play("win", 0, 0);
    }

    /// <summary>
    /// 进入失败结算界面
    /// </summary>
    public void EnterLosePanel()
    {
        //InitPanel();
        LosePanel.SetActive(true);
        Tex_ExpTips.gameObject.SetActive(true);
        Ani_Win.Play("lose", 0, 0);
    }

    /// <summary>
    /// 显示一大波
    /// </summary>
    public void ShowBigWave(bool isLastWave)
    {
        Img_BigWave.SetActive(true);
        Sprite s;
        if (isLastWave)
        {
            s = GameManager.Instance.GetSprite("UI/LastWave");
            Img_BigWave.GetComponent<Image>().sprite = s;
            Img_BigWave.GetComponent<Image>().SetNativeSize();
            Ani_BigWave.Play("LastWave", -1, 0);
        }
        else
        {
            s = GameManager.Instance.GetSprite("UI/BigWave");
            Img_BigWave.GetComponent<Image>().sprite = s;
            Img_BigWave.GetComponent<Image>().SetNativeSize();
            Ani_BigWave.Play("BigWave", -1, 0);
        }
    }

    /// <summary>
    /// 显示美食信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayFoodInfo(BaseUnit unit)
    {
        InfoUI.DisplayFoodInfo(unit);
    }

    /// <summary>
    /// 显示老鼠信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayMouseInfo(BaseUnit unit)
    {
        InfoUI.DisplayMouseInfo(unit);
    }

    /// <summary>
    /// 显示人物信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayCharacterInfo(BaseUnit unit)
    {
        InfoUI.DisplayCharacterInfo(unit);
    }

    /// <summary>
    /// 设置通关时的经验值提示
    /// </summary>
    /// <param name="s"></param>
    public void SetExpTips(string s)
    {
        Tex_ExpTips.text = s;
    }

    public override void ExitPanel()
    {
        base.ExitPanel();
    }

    public override void UpdatePanel()
    {
        PlayerData data = PlayerData.GetInstance();
        PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
        BaseStage.StageInfo info = info_dynamic.info;

        if (InfoUI.isActiveAndEnabled)
        {
            InfoUI.MUpdate();
        }
        foreach (var item in JewelSkillArray)
        {
            item.MUpdate();
        }
        // 更新时间轴
        if (GameController.Instance != null)
        {
            Tex_StageName.text = GameController.Instance.mCurrentStage.mStageInfo.name;
            // 游戏时间
            {
                int frame = GameController.Instance.GetCurrentStageFrame();
                int hour = frame / 216000;
                frame = frame % 216000;
                int min = frame / 3600;
                frame = frame % 3600;
                int sec = frame / 60;
                frame = frame % 60;
                int ms = 100 * frame / 60;

                string hh = (hour >= 10 ? hour.ToString() : "0" + hour.ToString());
                string mm = (min >= 10 ? min.ToString() : "0" + min.ToString());
                string ss = (sec >= 10 ? sec.ToString() : "0" + sec.ToString());
                string msms = (ms >= 10 ? ms.ToString() : "0" + ms.ToString());

                Tex_PlayTime.text = hh + ":" + mm + ":" + ss + "." + msms;
            }
            // 实际时间
            if(!GameController.Instance.isOver){
                if (IsInCharacterConstructMode)
                    startRealTime = Time.time;
                float real_time = GetRealTime();
                int hour = Mathf.FloorToInt(real_time / 3600);
                real_time = real_time % 3600;
                int min = Mathf.FloorToInt(real_time / 60);
                real_time = real_time % 60;
                int sec = Mathf.FloorToInt(real_time);
                real_time = real_time - sec;
                int ms = Mathf.FloorToInt(100 * real_time);

                string hh = (hour >= 10 ? hour.ToString() : "0" + hour.ToString());
                string mm = (min >= 10 ? min.ToString() : "0" + min.ToString());
                string ss = (sec >= 10 ? sec.ToString() : "0" + sec.ToString());
                string msms = (ms >= 10 ? ms.ToString() : "0" + ms.ToString());

                Tex_RealPlayTime.text = hh + ":" + mm + ":" + ss + "." + msms;
            }
            
            Tex_TotalEnergy.text = GameController.Instance.mCostController.totalFire.ToString("#0");
            if(!info_dynamic.isNoLimit && info.pauseCount > -1)
                Tex_PauseCount.text = GameController.Instance.pauseCount.ToString() + "/" + info.pauseCount;
            else
                Tex_PauseCount.text = GameController.Instance.pauseCount.ToString() + "/∞";
        }
        // 更新附加小ui组件
        List<BaseUI> delList = new List<BaseUI>();
        foreach (var ui in uiList)
        {
            if (ui.IsValid())
                ui.MUpdate();
            else
                delList.Add(ui);
        }
        foreach (var ui in delList)
        {
            uiList.Remove(ui);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Destory GameNormalPanel");
    }

    public void OnEnterShovelModel()
    {
        mShovelModel.OnPointerEnter();
    }

    public void OnExitShovelModel()
    {
        mShovelModel.OnPointerExit();
    }

    public void ShowBGM(MusicInfo info)
    {
        if(info != null)
        {
            Tex_BGM.text = "BGM：" + info.displayName + "    Author：" + info.author;
            // RectTrans_BGM.sizeDelta = new Vector2(Tex_BGM.text.Length * Tex_BGM.fontSize + 40, RectTrans_BGM.sizeDelta.y);
            Tweener_BGM.PlayForward();
            StartCoroutine(ShowBGM());
        }
    }

    private IEnumerator ShowBGM()
    {
        while (Tweener_BGM.IsPlaying())
            yield return null;
        // 然后等两秒
        for (int i = 0; i < 120; i++)
            yield return null;
        Tweener_BGM.PlayBackwards();
    }

    /// <summary>
    /// 添加小ui
    /// </summary>
    public void AddUI(BaseUI ui)
    {
        Vector2 size = ui.transform.localScale;
        uiList.Add(ui);
        ui.transform.SetParent(Trans_BottomLayer);
        ui.transform.localScale = size;
    }

    /// <summary>
    /// 获取当前关实际用时
    /// </summary>
    /// <returns></returns>
    public float GetRealTime()
    {
        return Time.time - startRealTime;
    }
}
