using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �������ս������UI������
/// </summary>

public class GameNormalPanel : MonoBehaviour, IBasePanel
{
    public const string Path = "Pictures/UI/"; // �������Դ���·��

    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList; // ����UI������
    public GameObject mCardBuilderList2;  // ����UI������
    public CardModel mCardModel; // ��Ƭģ�ͣ�ѡ��ʱ������������֣�

    private void Awake()
    {
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        mCardBuilderList2 = mCardControllerUI.transform.Find("CardBuilderList2").gameObject;
        mCardModel = mCardControllerUI.transform.Find("Emp_CardModel").gameObject.GetComponent<CardModel>();
    }

    // ���Ϸ�UI������һ������
    public void AddCardSlot(BaseCardBuilder CardBuilder)
    {
        if (mCardBuilderList.transform.childCount < 13)
        {
            CardBuilder.SetIndex(mCardBuilderList.transform.childCount);
            //CardBuilder.SetCardIndex(0);
            CardBuilder.transform.SetParent(mCardBuilderList.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent������scale�ĵ�1�����������100��
        }
        else if(mCardBuilderList2.transform.childCount < 5)
        {
            CardBuilder.SetIndex(13 + mCardBuilderList2.transform.childCount);
            //CardBuilder.SetCardIndex(0);
            CardBuilder.transform.SetParent(mCardBuilderList2.transform);
            CardBuilder.transform.localScale = Vector3.one; // SetParent������scale�ĵ�1�����������100��
        }
        else
        {
            Debug.LogWarning("Я����Ƭ�ѳ���18�ţ������࿨Ƭ�۲���Ч");
        }
    }

    /// <summary>
    /// ���뿨Ƭ����ģʽ����ĳ����Ƭ���������ɹ�ѡȡʱ����
    /// </summary>
    public void EnterConstructMode()
    {
        mCardModel.gameObject.SetActive(true);
    }

    /// <summary>
    /// �뿪��Ƭ����ģʽ����Ƭ����ɹ�����ȡ������ʱ����
    /// </summary>
    public void ExitConstructMode()
    {
        mCardModel.gameObject.SetActive(false);
    }

    public void InitPanel()
    {
        // throw new System.NotImplementedException();
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
}
