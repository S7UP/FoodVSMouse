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
    public GameObject Mask; // ��ɫ����
    public GameObject MenuUI; // �˵�

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // �г���ʱ������
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mShovelSlot2Trans = mCardControllerUI.transform.Find("ShovelSlot2");
        mShovelModel = mCardControllerUI.transform.Find("Emp_ShovelModel").gameObject.GetComponent<ShovelModel>();
        Tex_Pause = transform.Find("Btn_Pause").Find("Text").GetComponent<Text>();
        Mask = transform.Find("Mask").gameObject;
        MenuUI = transform.Find("MenuUI").gameObject;
        //mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ���ӹ�λ
    }

    public void InitPanel()
    {
        Debug.Log("InitPanel()");
        mShovelModel.transform.position = mShovelSlot2Trans.transform.position; // ���ӹ�λ
        Tex_Pause.text = "��ͣ��Ϸ";
        SetMenuEnable(false);
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
        // �Ȼ�ȡĿǰ����ͣ���Ƿ���ͣ״̬
        if (GameController.Instance.isPause)
        {
            // �������ͣ״̬��������ͣ
            GameController.Instance.Resume();
            // ��������
            Tex_Pause.text = "��ͣ��Ϸ";
        }
        else
        {
            // ����Ƿ���ͣ״̬������ͣ
            GameController.Instance.Pause();
            // ��������
            Tex_Pause.text = "�����ͣ";
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
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// ���������˵������ʱ
    /// </summary>
    public void OnReturnToMainClick()
    {
        GameManager.Instance.EnterMainScene();
    }

    /// <summary>
    /// ���ò˵���ʾ
    /// </summary>
    private void SetMenuEnable(bool enable)
    {
        Mask.SetActive(enable);
        MenuUI.SetActive(enable);
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
    }

    /// <summary>
    /// �뿪��ɫ����ģʽ
    /// </summary>
    public void ExitCharacterConstructMode()
    {
        mCardModel.HideCardModel();
    }

    /// <summary>
    /// ���뿨Ƭ����ģʽ����ĳ����Ƭ���������ɹ�ѡȡʱ����
    /// </summary>
    public void EnterCardConstructMode()
    {
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
        GameController.Instance.mCardController.SelectShovel();
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
        throw new System.NotImplementedException();
    }

    private void OnDestroy()
    {
        Debug.Log("Destory GameNormalPanel");
    }
}
