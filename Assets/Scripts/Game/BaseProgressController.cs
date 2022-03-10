using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProgressController : MonoBehaviour, IBaseProgressController, IGameControllerMember
{
    public List<BaseProgressBar> mProgressBarList; // �ں����ֽ������ļ���
    public RoundProgressBar mRoundProgressBar; // ��������������
    public BossHpBar mBossHpBar;

    private GameObject Emp_BossHpBar;
    private GameObject Emp_RoundProgressBar;

    private void Awake()
    {
        mProgressBarList = new List<BaseProgressBar>();
        Emp_BossHpBar = transform.Find("Emp_BossHpBar").gameObject;
        Emp_RoundProgressBar = transform.Find("Emp_RoundProgressBar").gameObject;
    }

    public bool IsEnd()
    {
        throw new System.NotImplementedException();
    }

    public void MInit()
    {
        mProgressBarList.Clear();
        // ��ȡ�����������ű�
        mRoundProgressBar = Emp_RoundProgressBar.GetComponent<RoundProgressBar>();
        mProgressBarList.Add(mRoundProgressBar);
        // ��ȡBOSSѪ���ű�
        mBossHpBar = Emp_BossHpBar.GetComponent<BossHpBar>();
        mProgressBarList.Add(mBossHpBar);

        // ��ʼ�����нű�����
        for (int i = 0; i < mProgressBarList.Count; i++)
        {
            mProgressBarList[i].PInit();
        }
    }

    public void MUpdate()
    {
        // ��ʼ�����нű�����
        for (int i = 0; i < mProgressBarList.Count; i++)
        {
            mProgressBarList[i].PUpdate();
        }
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }
}
