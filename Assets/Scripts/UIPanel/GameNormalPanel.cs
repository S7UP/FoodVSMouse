using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �������ս������UI������
/// </summary>

public class GameNormalPanel : MonoBehaviour, IBasePanel
{
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


    public bool IsInCharacterConstructMode; // �Ƿ��ڽ�ɫ����ģʽ

    private void Awake()
    {
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
        // Prefab_Tex_DamageNumber = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "GameNormalPanel/Tex_DamageNumber");
    }

    public void InitPanel()
    {
        // ֹͣ����BGM
        GameManager.Instance.audioSourceManager.StopAllMusic();
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ���ӹ�λ
        Tex_Pause.text = "��ͣ��Ϸ";
        MenuUI.SetActive(false);
        Img_Hp.SetActive(false);
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        Img_BigWave.SetActive(false);
        PauseUI.SetActive(false);
        PutCharacterUI.SetActive(false);
        IsInCharacterConstructMode = false;
    }

    /// <summary>
    /// ��GameController�еĳ�ʼ������
    /// </summary>
    public void InitInGameController()
    {
        mCardModel = GameObject.Find("Emp_CardModel").GetComponent<CardModel>();
        mCardModel.HideCardModel(); // ���ؿ�Ƭģ��
        ExitCharacterConstructMode();
        ExitCardConstructMode();
        ExitCardRemoveMode();
    }

    /// <summary>
    /// ����ͣ��ť�����ʱ
    /// </summary>
    public void OnPauseButtonClick()
    {
        // ��������ý׶ε���ͣ��û���κε��õ�
        if (IsInCharacterConstructMode)
            return;

        // �Ȼ�ȡĿǰ����ͣ���Ƿ���ͣ״̬
        if (GameController.Instance.isPause)
        {
            // �������ͣ״̬��������ͣ
            GameController.Instance.Resume();
            // ��������
            Tex_Pause.text = "�� ͣ";
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
    /// �����³��Ա����ʱ
    /// </summary>
    public void OnRetryClick()
    {
        InitPanel();
        GameController.Instance.Restart();
    }

    /// <summary>
    /// ������ѡ�����ñ����ʱ
    /// </summary>
    public void OnReturnToSelcetClick()
    {
        GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// ���������˵������ʱ
    /// </summary>
    public void OnReturnToMainClick()
    {
        GameController.Instance.RecycleAndDestoryAllInstance();
        GameManager.Instance.EnterMainScene();
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
        InitPanel();
        WinPanel.SetActive(true);
        Ani_Win.Play("win", 0, 0);
    }

    /// <summary>
    /// ����ʧ�ܽ������
    /// </summary>
    public void EnterLosePanel()
    {
        InitPanel();
        LosePanel.SetActive(true);
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
            Ani_BigWave.Play("LastWave");
        }
        else
        {
            s = GameManager.Instance.GetSprite("UI/BigWave");
            Img_BigWave.GetComponent<Image>().sprite = s;
            Img_BigWave.GetComponent<Image>().SetNativeSize();
            Ani_BigWave.Play("BigWave");
        }
            
    }

    public void EnterPanel()
    {
        throw new System.NotImplementedException();
    }

    public void ExitPanel()
    {
        throw new System.NotImplementedException();
    }

    public void UpdatePanel()
    {
        
    }

    private void OnDestroy()
    {
        Debug.Log("Destory GameNormalPanel");
    }
}
