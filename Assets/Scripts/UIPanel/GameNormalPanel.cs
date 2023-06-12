using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// �������ս������UI������
/// </summary>

public class GameNormalPanel : BasePanel
{
    public static GameNormalPanel Instance;

    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList; // ����UI������
    public GameObject mCardBuilderList2;  // ����UI������
    public CardModel mCardModel; // ��Ƭģ�ͣ�ѡ��ʱ������������֣�
    public Transform mShovelSlot2Trans; // ���ӶѷŴ�
    public ShovelModel mShovelModel; // ����ģ��
    public Text Tex_Pause; // ��ͣ�ı�
    public GameObject MenuUI; // �˵�
    public GameObject Img_Hp; // Ѫ��ʵ��
    private Text Tex_Hp; // Ѫ���ı�
    private GameObject WinPanel;
    private Animator Ani_Win;
    private GameObject LosePanel;
    private Animator Ani_Lose;
    private GameObject Img_BigWave;
    private Animator Ani_BigWave;
    private GameObject PauseUI;
    private GameObject PutCharacterUI;
    // private GameObject Prefab_Tex_DamageNumber;
    private InfoUI_GameNormalPanel InfoUI;
    private Transform Tran_JewelSkillUI;
    private JewelSkill_GameNormalPanel[] JewelSkillArray;
    private Text Tex_ExpTips; // ����ֵ�ı�

    private Image Img_Rank;
    private Text Tex_Rank;
    private Text Tex_StageName;
    private Text Tex_PlayTime;
    private Text Tex_TotalEnergy;
    private Text Tex_PauseCount;

    private RectTransform RectTrans_BGM;
    private Text Tex_BGM;
    private Tweener Tweener_BGM;

    public bool IsInCharacterConstructMode; // �Ƿ��ڽ�ɫ����ģʽ


    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        DontDestroyOnLoad(gameObject); // �г���ʱ������
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mShovelSlot2Trans = mCardControllerUI.transform.Find("ShovelSlot2");
        mShovelModel = mCardControllerUI.transform.Find("Emp_ShovelModel").gameObject.GetComponent<ShovelModel>();
        Tex_Pause = transform.Find("Btn_Pause").Find("Text").GetComponent<Text>();
        MenuUI = transform.Find("MenuUI").gameObject;
        Img_Hp = transform.Find("Img_Hp").gameObject;
        Tex_Hp = transform.Find("Img_Hp").Find("Text").GetComponent<Text>();
        //mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ���ӹ�λ
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
        Img_Rank = transform.Find("Img_Rank").Find("Rank").GetComponent<Image>();
        Tex_Rank = transform.Find("Img_Rank").Find("Text").GetComponent<Text>();
        Tex_StageName = transform.Find("LeftBottomInfo").Find("StageName_Label").Find("Text").GetComponent<Text>();
        Tex_PlayTime = transform.Find("LeftBottomInfo").Find("Time_Label").Find("Text").GetComponent<Text>();
        Tex_TotalEnergy = transform.Find("LeftBottomInfo").Find("TotalEnergy_Label").Find("Text").GetComponent<Text>();
        Tex_PauseCount = transform.Find("LeftBottomInfo").Find("PauseCount_Label").Find("Text").GetComponent<Text>();

        RectTrans_BGM = transform.Find("BGM").GetComponent<RectTransform>();
        Tex_BGM = RectTrans_BGM.Find("Text").GetComponent<Text>();
        Tweener_BGM = RectTrans_BGM.DOAnchorPosY(42, 2);
        Tweener_BGM.SetAutoKill(false);
        Tweener_BGM.Pause();
    }

    public override void InitPanel()
    {
        base.EnterPanel();
        // ֹͣ����BGM
        GameManager.Instance.audioSourceManager.StopAllMusic();
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ���ӹ�λ
        Tex_Pause.text = "��ͣ(Space)";
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
        // ������GameController��ʼ������
        GameController.Instance.SetStart();
    }

    public override void EnterPanel()
    {

    }

    /// <summary>
    /// ��GameController�еĳ�ʼ������
    /// </summary>
    public void InitInGameController()
    {
        //mCardModel = GameObject.Find("Emp_CardModel").GetComponent<CardModel>();
        mCardModel = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Emp_CardModel").GetComponent<CardModel>();
        mCardModel.transform.SetParent(GameController.Instance.transform);
        mCardModel.transform.localScale = Vector2.one;
        mCardModel.HideCardModel(); // ���ؿ�Ƭģ��
        ExitCharacterConstructMode();
        ExitCardConstructMode();
        ExitCardRemoveMode();
        // ��ȡ���ѡ��ı�ʯ�����϶�Ӧ�ļ���ͼ��
        PlayerData data = PlayerData.GetInstance();
        for (int i = 0; i < 3; i++)
        {
            int type = data.GetJewel(i);
            JewelSkillArray[i].MInit(type, GameController.Instance.mJewelSkillArray[i]);
        }
        // ��ȡ��Ϸ��ǰ�ѶȲ�����ʱ��
        Tex_Rank.gameObject.SetActive(false);
        Img_Rank.gameObject.SetActive(true);
        switch (data.GetDifficult())
        {
            case 1: Img_Rank.sprite = GameManager.Instance.GetSprite("UI/Difficulty/Normal");break;
            case 2: Img_Rank.sprite = GameManager.Instance.GetSprite("UI/Difficulty/Hard"); break;
            case 3: { 
                    if(data.GetRankRate() > 1)
                    {
                        Img_Rank.gameObject.SetActive(false);
                        Tex_Rank.gameObject.SetActive(true);
                        Tex_Rank.text = (Mathf.FloorToInt(data.GetRankRate() * 100)).ToString() + "%";
                    }
                    else
                    {
                        Img_Rank.sprite = GameManager.Instance.GetSprite("UI/Difficulty/Lunatic");
                    }
                    break;
                } 
            default:
                Img_Rank.sprite = GameManager.Instance.GetSprite("UI/Difficulty/Easy");
                break;
        }
        Tex_StageName.text = "unknow";
        Tex_PlayTime.text = "00:00:00.00";
        Tex_TotalEnergy.text = "--";
        Tex_PauseCount.text = "0";
    }

    /// <summary>
    /// ����ͣ��ť�����ʱ
    /// </summary>
    public void OnPauseButtonClick()
    {
        // ��������ý׶ε���ͣ��û���κε��õ�
        if (IsInCharacterConstructMode || MenuUI.activeInHierarchy)
            return;

        // �Ȼ�ȡĿǰ����ͣ���Ƿ���ͣ״̬
        if (GameController.Instance.isPause)
        {
            // �������ͣ״̬��������ͣ
            GameController.Instance.Resume();
            // ��������
            Tex_Pause.text = "��ͣ(Space)";
            // �Ƴ�����
            PauseUI.SetActive(false);
        }
        else
        {
            // ����Ƿ���ͣ״̬������ͣ
            GameController.Instance.Pause();
            // ��������
            Tex_Pause.text = "�����ͣ";
            // ��ʾ����
            PauseUI.SetActive(true);
        }
    }

    /// <summary>
    /// ���˵���ť�����ʱ
    /// </summary>
    public void OnMenuButtonClick()
    {
        PauseUI.SetActive(false);
        SetMenuEnable(true);
    }

    /// <summary>
    /// ��������Ϸ��ť�����ʱ
    /// </summary>
    public void OnReturnToGameClick()
    {
        SetMenuEnable(false);
    }

    /// <summary>
    /// �����³��Ա����ʱ��ʤ����ʧ�ܽ���ҳ������³��ԣ�
    /// </summary>
    public void OnRetryClick()
    {
        InitPanel();
        GameController.Instance.Restart();
    }

    /// <summary>
    /// �����³��Ա����ʱ����Ϸ��δ�������ǴӲ˵�����������³��Եģ�
    /// </summary>
    public void OnClickRetryWhenNoEnd()
    {
        // �ư�Ҳ�Ӿ���ֵ��
        PlayerData.GetInstance().AddExp(GameController.Instance.GetDefaultExpReward());
        OnRetryClick();
    }

    /// <summary>
    /// ������ѡ�����ñ����ʱ
    /// </summary>
    public void OnReturnToSelcetClick()
    {
        // GameController.Instance.RecycleAndDestoryAllInstance();
        // GameManager.Instance.EnterSelectScene();
        GameManager.Instance.EnterTownScene();
    }

    /// <summary>
    /// ������ѡ�����ñ����ʱ����Ϸ��δ�������ǴӲ˵�����������³��Եģ�
    /// </summary>
    public void OnReturnToSelcetClickWhenNoEnd()
    {
        // �ư�Ҳ�Ӿ���ֵ��
        PlayerData.GetInstance().AddExp(GameController.Instance.GetDefaultExpReward());
        OnReturnToSelcetClick();
    }

    /// <summary>
    /// ���������˵������ʱ
    /// </summary>
    public void OnReturnToMainClick()
    {
        // GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterMainScene();
    }

    /// <summary>
    /// ���������˵������ʱ����Ϸ��δ�������ǴӲ˵�����������³��Եģ�
    /// </summary>
    public void OnReturnToMainClickWhenNoEnd()
    {
        // �ư�Ҳ�Ӿ���ֵ��
        PlayerData.GetInstance().AddExp(GameController.Instance.GetDefaultExpReward());
        OnReturnToMainClick();
    }

    /// <summary>
    /// ���ò˵���ʾ
    /// </summary>
    private void SetMenuEnable(bool enable)
    {
        // ��������ý׶β�ִ�к����߼�
        if (IsInCharacterConstructMode)
            return;

        MenuUI.SetActive(enable);

        if (enable)
            GameController.Instance.Pause();
        else
            GameController.Instance.Resume();
    }

    /// <summary>
    /// ������п���
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

    // ���Ϸ�UI������һ������
    public bool AddCardSlot(BaseCardBuilder CardBuilder)
    {
        if (mCardBuilderList.transform.childCount < 13)
        {
            CardBuilder.SetIndex(mCardBuilderList.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent������scale�ĵ�1�����������100��
            return true;
        }
        else if(mCardBuilderList2.transform.childCount < 5)
        {
            CardBuilder.SetIndex(13 + mCardBuilderList2.transform.childCount);
            CardBuilder.transform.SetParent(mCardBuilderList2.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent������scale�ĵ�1�����������100��
            return true;
        }
        else
        {
            Debug.LogWarning("Я����Ƭ�ѳ���18�ţ������࿨Ƭ�۲���Ч");
            return false;
        }
    }

    /// <summary>
    /// �����ɫ����ģʽ
    /// </summary>
    public void EnterCharacterConstructMode()
    {
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCharacter);
        Img_Hp.SetActive(false);
        IsInCharacterConstructMode = true;
        PutCharacterUI.SetActive(true);
    }

    /// <summary>
    /// �뿪��ɫ����ģʽ
    /// </summary>
    public void ExitCharacterConstructMode()
    {
        mCardModel.HideCardModel();
        Img_Hp.SetActive(true);
        IsInCharacterConstructMode = false;
        PutCharacterUI.SetActive(false);
    }

    /// <summary>
    /// ���뿨Ƭ����ģʽ����ĳ����Ƭ���������ɹ�ѡȡʱ����
    /// </summary>
    public void EnterCardConstructMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mCardModel.ShowCardModel(CardModel.DisplayMode.SetCard);
    }

    /// <summary>
    /// �뿪��Ƭ����ģʽ����Ƭ����ɹ�����ȡ������ʱ����
    /// </summary>
    public void ExitCardConstructMode()
    {
        mCardModel.HideCardModel();
    }

    /// <summary>
    /// ���뿨Ƭ�Ƴ�ģʽ
    /// </summary>
    public void EnterCardRemoveMode()
    {
        if (IsInCharacterConstructMode)
            return;
        mShovelModel.ShowModel();
    }

    /// <summary>
    /// �뿪��Ƭ�Ƴ�ģʽ
    /// </summary>
    public void ExitCardRemoveMode()
    {
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ��λ
        mShovelModel.HideModel();
    }

    /// <summary>
    /// �����Ӳ�λ�������������
    /// </summary>
    public void OnShovelSlotTrigger()
    {
        if (IsInCharacterConstructMode)
            return;
        GameController.Instance.mCardController.SelectShovel();
    }



    /// <summary>
    /// ��������HP�ı���λ��
    /// </summary>
    /// <param name="hp"></param>
    public void SetCharacterHpTextAndPosition(float hp, Vector3 position)
    {
        Img_Hp.transform.position = position;
        Tex_Hp.text = "HP:" + hp;
    }

    /// <summary>
    /// ����ʤ���������
    /// </summary>
    public void EnterWinPanel()
    {
        //InitPanel();
        WinPanel.SetActive(true);
        Tex_ExpTips.gameObject.SetActive(true);
        Ani_Win.Play("win", 0, 0);
    }

    /// <summary>
    /// ����ʧ�ܽ������
    /// </summary>
    public void EnterLosePanel()
    {
        //InitPanel();
        LosePanel.SetActive(true);
        Tex_ExpTips.gameObject.SetActive(true);
        Ani_Win.Play("lose", 0, 0);
    }

    /// <summary>
    /// ��ʾһ��
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
    /// ��ʾ��ʳ��Ϣ
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayFoodInfo(BaseUnit unit)
    {
        InfoUI.DisplayFoodInfo(unit);
    }

    /// <summary>
    /// ��ʾ������Ϣ
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayMouseInfo(BaseUnit unit)
    {
        InfoUI.DisplayMouseInfo(unit);
    }

    /// <summary>
    /// ��ʾ������Ϣ
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayCharacterInfo(BaseUnit unit)
    {
        InfoUI.DisplayCharacterInfo(unit);
    }

    /// <summary>
    /// ����ͨ��ʱ�ľ���ֵ��ʾ
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
        if (InfoUI.isActiveAndEnabled)
        {
            InfoUI.MUpdate();
        }
        foreach (var item in JewelSkillArray)
        {
            item.MUpdate();
        }
        // ����ʱ����
        if (GameController.Instance != null)
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
            Tex_StageName.text = GameController.Instance.mCurrentStage.mStageInfo.name;
            Tex_PlayTime.text = hh + ":" + mm + ":" + ss + "." + msms;
            Tex_TotalEnergy.text = GameController.Instance.mCostController.totalFire.ToString("#0");
            Tex_PauseCount.text = GameController.Instance.pauseCount.ToString();
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
            Tex_BGM.text = "BGM��" + info.displayName + "    Author��" + info.author;
            RectTrans_BGM.sizeDelta = new Vector2(Tex_BGM.text.Length * Tex_BGM.fontSize + 40, RectTrans_BGM.sizeDelta.y);
            Tweener_BGM.PlayForward();
            StartCoroutine(ShowBGM());
        }
    }

    private IEnumerator ShowBGM()
    {
        while (Tweener_BGM.IsPlaying())
            yield return null;
        // Ȼ�������
        for (int i = 0; i < 120; i++)
            yield return null;
        Tweener_BGM.PlayBackwards();
    }
}
