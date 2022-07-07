using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class BaseRound
{
    [System.Serializable]
    public class RoundInfo
    {
        public string name; // ��������
        public string info; // ���ֱ�ע
        public List<RoundInfo> roundInfoList; // �����С��
        public List<BaseEnemyGroup> baseEnemyGroupList; // ��
        public int interval; // ������֮��ļ��
        public int endTime; // ��β���ȴ�ʱ��

        /// <summary>
        /// ��ȡ�����ܳ���ʱ��
        /// </summary>
        /// <returns></returns>
        public int GetTotalTimer()
        {
            int t = baseEnemyGroupList.Count*interval + endTime;
            foreach (var item in roundInfoList)
            {
                t += item.GetTotalTimer();
            }
            return t;
        }
    }

    public RoundInfo mRoundInfo;
    public List<BaseRound> mRoundList;
    public int mCurrentIndex;
    private BaseEnemyGroup mCurrentEnemyGroup; // ��ǰ����ִ�е���
    //private IEnumerator currentEnumerator;

    /// <summary>
    /// ����RoundInfo��ʼ��
    /// </summary>
    public void Init(RoundInfo roundInfo)
    {
        mRoundInfo = roundInfo;
        mRoundList = new List<BaseRound>();
        if (roundInfo.roundInfoList != null)
        {
            for (int i = 0; i < roundInfo.roundInfoList.Count; i++)
            {
                BaseRound round = new BaseRound();
                round.Init(roundInfo.roundInfoList[i]);
                mRoundList.Add(round);
            }
        }
        mCurrentIndex = 0;
        mCurrentEnemyGroup = null;
    }

    // ʹ��Э��ִ�б��ֵĳ����߼�
    public IEnumerator Execute()
    {
        // ��ִ���Լ���ˢ���������
        Debug.Log("��ǰ��Ϊ��"+mRoundInfo.name+"����ʼˢ�֣�");
        for (int i = 0; i < mRoundInfo.baseEnemyGroupList.Count; i++)
        {
            // ��ȡ��ǰ��
            mCurrentEnemyGroup = mRoundInfo.baseEnemyGroupList[i];
            // ��ȡ����ʵ��ˢ��λ�ñ�
            BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
            // ֪ͨGameControllerˢ����
            CreateEnemies(realEnemyList);

            // �������֡��ˢ����һ��
            yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.interval));
            //yield return GameController.Instance.Wait(mRoundInfo.interval);
        }

        //int waitTime = 0;
        //for (int i = 0; i < mRoundInfo.endTime; i++)
        //{
        //    waitTime++;
        //    yield return null;
        //}
        

        // ִ�����ִ���������ֵ�ˢ��������
        for (int i = 0; i < mRoundList.Count; i++)
        {
            BaseRound round = mRoundList[i];
            //yield return GameController.Instance.StartCoroutine(round.Execute());
            yield return GameController.Instance.mCurrentStage.StartCoroutine(round.Execute());
        }

        yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.endTime));

        Debug.Log("����ˢ����ɣ�");

        //Debug.Log("�����ѽ�����waitTime="+waitTime);
        //currentEnumerator = null;
    }

    /// <summary>
    /// ����ʵ��ˢ��λ�ñ���ˢ��
    /// </summary>
    private void CreateEnemies(BaseEnemyGroup.RealEnemyList realEnemyList)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            GameController.Instance.CreateMouseUnit(realEnemyList.Get(i), realEnemyList.enemyInfo);
        }
    }


    /// <summary>
    /// ��ȡ�������������ڵ�ǰ��
    /// </summary>
    public BaseRound GetCurrentRound()
    {
        return mRoundList[mCurrentIndex];
    }

    /// <summary>
    /// ��ȡҶ�ӽڵ㣨��С�����ڵ㣬���ݹ鵽�����ٵݹ���Ǹ��ڵ㣩
    /// </summary>
    /// <returns></returns>
    public BaseRound GetCurrentLeafRound()
    {
        BaseRound round = this;
        while (true)
        {
            if(round.mRoundList==null || round.mRoundList.Count <= 0 || mCurrentIndex >= round.mRoundList.Count)
            {
                return round;
            }
            round = GetCurrentRound();
        }
    }

    /// <summary>
    /// ��ȡ�ȴ����
    /// </summary>
    /// <returns></returns>
    public int GetInterval()
    {
        return mRoundInfo.interval;
    }

    /// <summary>
    /// ��ȡ��β�ȴ�ʱ��
    /// </summary>
    /// <returns></returns>
    public int GetEndTime()
    {
        return mRoundInfo.endTime;
    }

    /// <summary>
    /// ��ȡ�����ܳ���ʱ��
    /// </summary>
    /// <returns></returns>
    public int GetTotalTimer()
    {
        return mRoundInfo.GetTotalTimer();
    }

    /// <summary>
    /// ��ȡһ���Ѿ���ʼ����Ĭ��ֵ��RoundInfo
    /// </summary>
    /// <returns></returns>
    public static RoundInfo GetInitalRoundInfo()
    {
        return new BaseRound.RoundInfo()
        {
            name = "unknow name",
            info = "",
            roundInfoList = new List<RoundInfo>(),
            baseEnemyGroupList = new List<BaseEnemyGroup>(),
            interval = 0,
            endTime = 0
        };
    }
}
