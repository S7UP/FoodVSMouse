using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class BaseRound
{
    public enum RoundMode
    {
        Fixed = 0, // �̶�ģʽ
        HalfRandom = 1, // �����ģʽ
    }

    [System.Serializable]
    public class RoundInfo
    {
        public string name; // ��������
        public string info; // ���ֱ�ע
        public List<RoundInfo> roundInfoList; // �����С��
        public List<BaseEnemyGroup> baseEnemyGroupList; // ��
        public int interval; // ������֮��ļ��
        public int endTime; // ��β���ȴ�ʱ��
        public RoundMode roundMode; // ��ǰ��ˢ��ģʽ
        public bool isBossRound; // �Ƿ�ΪBOSS�ִ�
        public List<BaseEnemyGroup> bossList; // BOSS�б�

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

        /// <summary>
        /// ��ȡ�����е�BOSS�������������֣�
        /// </summary>
        /// <returns></returns>
        public int GetBossCount()
        {
            int count = 0;
            foreach (var item in roundInfoList)
            {
                count += item.GetBossCount();
            }
            if (isBossRound && bossList != null)
                count += bossList.Count;
            return count;
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
        // ������ƫ����
        if (mRoundInfo.roundMode.Equals(RoundMode.Fixed))
            GameController.Instance.mCurrentStage.PushZeroToRowOffsetStack();
        else if(mRoundInfo.roundMode.Equals(RoundMode.HalfRandom))
            GameController.Instance.mCurrentStage.PushRandomToRowOffsetStack();

        // ��ִ���Լ���ˢ���������
        if (mRoundInfo.isBossRound)
        {
            // �����BOSS�ִ�
            Debug.Log("��ǰ��ΪBOSS�ִΣ��֣�" + mRoundInfo.name + "����ʼˢ�֣�");
            for (int i = 0; i < mRoundInfo.bossList.Count; i++)
            {
                // ��ȡ��ǰ��
                mCurrentEnemyGroup = mRoundInfo.bossList[i];
                // ��ȡ����ʵ��ˢ��λ�ñ�
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // ֪ͨGameControllerˢ����
                CreateBoss(realEnemyList, mCurrentEnemyGroup.mHp);
                // û�м����BOSS��ͬʱˢ�µ�
            }

            // ִ�����ִ���������ֵ�ˢ��������
            // ��BOSS�ִ��У�������˳��ִ��һ��֮������ѭ�����ֵ����һ��С��ֱ��BOSS����
            if (mRoundList.Count > 0)
            {
                int j = 0;
                while (GameController.Instance.IsHasBoss())
                {
                    BaseRound round = mRoundList[j];
                    yield return GameController.Instance.mCurrentStage.StartCoroutine(round.Execute());
                    if (j < mRoundList.Count - 1)
                        j++;
                }
            }
            else
            {
                // ���û�ж���С�֣����Ϊÿ����һ��BOSS�Ƿ���ڣ��������������һ�׶�
                while (GameController.Instance.IsHasBoss())
                {
                    yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(60));
                }
            }

        }
        else
        {
            // С���ִ�
            Debug.Log("��ǰ��Ϊ��" + mRoundInfo.name + "����ʼˢ�֣�");
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
            }

            // ִ�����ִ���������ֵ�ˢ��������
            for (int i = 0; i < mRoundList.Count; i++)
            {
                BaseRound round = mRoundList[i];
                yield return GameController.Instance.mCurrentStage.StartCoroutine(round.Execute());
            }
        }

        yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.endTime));

        GameController.Instance.mCurrentStage.PopRowOffsetStack();

        Debug.Log("����ˢ����ɣ�");
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
    /// ����ʵ��ˢ��λ�ñ���ˢBOSS
    /// </summary>
    /// <param name="realEnemyList"></param>
    private void CreateBoss(BaseEnemyGroup.RealEnemyList realEnemyList, float hp)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            // GameController.Instance.CreateMouseUnit(realEnemyList.Get(i), realEnemyList.enemyInfo);
            BossUnit b = GameController.Instance.CreateBossUnit(realEnemyList.Get(i), realEnemyList.enemyInfo, hp, -1);
            
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
