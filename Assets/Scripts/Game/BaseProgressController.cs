using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProgressController : MonoBehaviour, IBaseProgressController, IGameControllerMember
{
    public List<BaseProgressBar> mProgressBarList; // 内含各种进度条的集合
    public RoundProgressBar mRoundProgressBar; // 轮数进度条引用
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
        // 获取轮数进度条脚本
        mRoundProgressBar = Emp_RoundProgressBar.GetComponent<RoundProgressBar>();
        mProgressBarList.Add(mRoundProgressBar);
        // 获取BOSS血条脚本
        mBossHpBar = Emp_BossHpBar.GetComponent<BossHpBar>();
        mProgressBarList.Add(mBossHpBar);

        // 初始化所有脚本内容
        for (int i = 0; i < mProgressBarList.Count; i++)
        {
            mProgressBarList[i].PInit();
        }
    }

    public void MUpdate()
    {
        // 初始化所有脚本内容
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
