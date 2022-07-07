using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BaseStage : MonoBehaviour
{
    /// <summary>
    /// �ؿ�ģʽ
    /// </summary>
    public enum StageMode
    {
        HalfRandom, // �����ģʽ
        Fixed // �̶�ģʽ
    }

    /// <summary>
    /// �ؿ���Ϣ
    /// </summary>
    [System.Serializable]
    public struct StageInfo
    {
        public string name; // �ؿ���
        public List<BaseRound.RoundInfo> roundInfoList; // �ؿ�������
        public List<int> waveIndexList; // һ�󲨱�־�±��
        public StageMode defaultMode; // Ĭ�Ϲؿ�ģʽ
        /// <summary>
        /// ��·����һ��List������֧���ڶ���List����ÿ����֧����·�ı��
        /// ���� {{1,2,6,7},{3,4,5}} ������������֧����һ��֧����1267·���ڶ���֧����345·
        /// �ñ���Ҫ���ˢ���飨BaseEnemyGroup���е�����һͬʵ��ˢ��
        /// </summary>
        public List<List<int>> apartList;
        public int perpareTime; // ����ǰ��׼��ʱ��

        /// <summary>
        /// ��ȡ�����ؿ�������
        /// </summary>
        /// <returns></returns>
        public BaseRound.RoundInfo GetTotalRoundInfo()
        {
            BaseRound.RoundInfo roundInfo = new BaseRound.RoundInfo();
            roundInfo.roundInfoList = roundInfoList;
            return roundInfo;
        }
    }


    // ����
    public StageInfo mStageInfo;
    public List<BaseRound> mRoundList;
    public BaseRound mCurrentRound;
    public int mCurrentRoundTimer;
    public int mCurrentRoundIndex;
    private List<int> apartRowOffsetList; // ��ǰ��·���Ӧ��ƫ����
    //private IEnumerator enumerator;
    //private Coroutine mCoroutine;

    /// <summary>
    /// ��ʼ�ؿ�
    /// </summary>
    public void StartStage()
    {
        //enumerator = Start();
        //if (mCoroutine != null)
        //{
        //    GameController.Instance.StopCoroutine(mCoroutine);
        //    mCoroutine = null;
        //}
        //mCoroutine = GameController.Instance.StartCoroutine(enumerator);
        //if (GameController.Instance.isPause)
        //    PauseStage();
        StartCoroutine(Execute());
    }

    /// <summary>
    /// ����һ���ȴ�����֡��IEnumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForIEnumerator(int time)
    {
        for (int i = 0; i < time; i++)
        {
            if (GameController.Instance.isPause)
                i--;
            yield return null;
        }
    }

    ///// <summary>
    ///// ��ͣ�ؿ�
    ///// </summary>
    //public void PauseStage()
    //{
    //    if (mCoroutine != null)
    //    {
    //        GameController.Instance.StopCoroutine(mCoroutine);
    //    }
    //    foreach (var item in mRoundList)
    //    {
    //        item.PauseRound();
    //    }
    //}

    ///// <summary>
    ///// �ָ�
    ///// </summary>
    //public void ResumeStage()
    //{
    //    if (mCoroutine != null)
    //    {
    //        GameController.Instance.StartCoroutine(enumerator);
    //    }
    //    foreach (var item in mRoundList)
    //    {
    //        item.ResumeRound();
    //    }
    //}

    /// <summary>
    /// �ؿ���ʼ
    /// </summary>
    public IEnumerator Execute()
    {
        GameController.Instance.mProgressController.mRoundProgressBar.SetTotalRoundCount(mRoundList.Count);
        GameController.Instance.mProgressController.mRoundProgressBar.UpdateRoundCountUI(0);
        mCurrentRoundIndex = -1;
        mCurrentRoundTimer = 0;
        Debug.Log("��Ϸ��ʼ�ˣ������ǵ�"+GameController.Instance.GetCurrentStageFrame()+"֡");
        yield return StartCoroutine(WaitForIEnumerator(mStageInfo.perpareTime));
        //yield return GameController.Instance.Wait(mStageInfo.perpareTime);
        Debug.Log("��ʼ�����ˣ������ǵ�" + GameController.Instance.GetCurrentStageFrame() + "֡");
        for (int i = 0; i < mRoundList.Count; i++)
        {
            mCurrentRound = mRoundList[i];
            GameController.Instance.mProgressController.mRoundProgressBar.UpdateRoundCountUI(i + 1);
            mCurrentRoundTimer = 0;
            mCurrentRoundIndex = i;
            if (mStageInfo.defaultMode.Equals(StageMode.HalfRandom))
            {
                for (int j = 0; j < mStageInfo.apartList.Count; j++)
                {
                    apartRowOffsetList[j] = Random.Range(0, mStageInfo.apartList[j].Count); // ע�⣬��������ʱ���������ֵ
                }
            }
            //yield return GameController.Instance.StartCoroutine(mCurrentRound.Execute());
            yield return StartCoroutine(mCurrentRound.Execute());
        }
        Debug.Log("������ϣ�");
        //enumerator = null;
    }

    /// <summary>
    /// ����ؿ���Ϣ������
    /// </summary>
    public void Save()
    {
        Save(mStageInfo);
    }

    public static void Save(StageInfo stageInfo)
    {
        Debug.Log("��ʼ�浵�ؿ���Ϣ��");
        JsonManager.Save(stageInfo, "Stage/" + stageInfo.name);
        Debug.Log("�ؿ���Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// �ؿ�ģ�棬������
    /// </summary>
    public static void DemoSave()
    {
        List<BaseEnemyGroup> enemyGroupList = new List<BaseEnemyGroup>();
        // ÿ����һ������һ����������ֻ
        BaseEnemyGroup enemyGroup = new BaseEnemyGroup();
        enemyGroup.Init(
            0, // apartIndex
            0, // startIndex
            new BaseEnemyGroup.EnemyInfo()
            {
                type = 0,
                shape = 0
            },
            3 // mouse count
            );
        enemyGroupList.Add(enemyGroup);


        // �������
        List<BaseRound.RoundInfo> roundInfos = new List<BaseRound.RoundInfo>();
        for (int i = 0; i < 6; i++)
        {
            BaseRound.RoundInfo roundInfo = new BaseRound.RoundInfo
            {
                name = "��"+(i+1)+"��",
                info = "",
                roundInfoList = null, // �����С��
                baseEnemyGroupList = enemyGroupList, // ��
                interval = 1, // ������֮��ļ��
                endTime = 240 // ��β���ȴ�ʱ��
            };
            roundInfos.Add(roundInfo);
        }


        List<List<int>> apartList = new List<List<int>>();
        for (int i = 0; i < 2; i++)
        {
            apartList.Add(new List<int>());
        }
        // ���1267·����1
        apartList[0].Add(0);
        apartList[0].Add(1);
        apartList[0].Add(5);
        apartList[0].Add(6);
        // ���345·����2
        apartList[1].Add(2);
        apartList[1].Add(3);
        apartList[1].Add(4);

        StageInfo stageInfo = new StageInfo()
        {
            name = "test�ؿ�",
            roundInfoList = roundInfos,
            waveIndexList = new List<int> { 1, 2 },
            defaultMode = StageMode.HalfRandom,
            apartList = apartList,
            perpareTime = 180
        };

        Debug.Log("��ʼ�浵�ؿ���Ϣ��");
        JsonManager.Save(stageInfo, "Stage/test");
        Debug.Log("�ؿ���Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// �ӱ����ж�ȡ�ؿ���Ϣ
    /// </summary>
    public void DemoLoad()
    {
        mStageInfo = Load(Test.TestStageName);
    }

    public static StageInfo Load(string path)
    {
        return JsonManager.Load<StageInfo>("Stage/" + path);
    }

    /// <summary>
    /// ���ݴӱ��ض�ȡ��StageInfo����ʼ�����ر���
    /// </summary>
    public virtual void Init()
    {
        //enumerator = null;
        //mCoroutine = null;
        mCurrentRound = null;
        mCurrentRoundTimer = 0;
        mCurrentRoundIndex = -1;
        // ͨ��RoundInfo������BaseRoundʵ��
        mRoundList = new List<BaseRound>();
        if (mStageInfo.roundInfoList != null)
        {
            for (int i = 0; i < mStageInfo.roundInfoList.Count; i++)
            {
                BaseRound round = new BaseRound();
                round.Init(mStageInfo.roundInfoList[i]);
                mRoundList.Add(round);
            }
            mCurrentRoundIndex = -1;
            mCurrentRound = null;
        }
        else
        {
            Debug.LogWarning("�޷��ҵ���ǰ�ؿ���RoundInfo��Ϣ��");
        }

        // ��ƫ������ʼ��
        apartRowOffsetList = new List<int>();
        for (int i = 0; i < mStageInfo.apartList.Count; i++)
        {
            apartRowOffsetList.Add(0);
        }
    }

    /// <summary>
    /// ��ȡ��ǰ��
    /// </summary>
    /// <returns></returns>
    public BaseRound GetCurrentRound()
    {
        return mCurrentRound;
    }

    /// <summary>
    /// ��ȡ��ǰ�ؿ��Ľ���
    /// </summary>
    /// <returns></returns>
    public float GetCurrentProgress()
    {
        float per = 1.0f / (GetTotalRoundCount() + 1);
        if (GetCurrentRoundIndex() == -1)
        {
            // ���Ϊ׼���ִ�
            return per * Mathf.Min(1.0f, (float)mCurrentRoundTimer / mStageInfo.perpareTime);
        }
        else
        {
            return (GetCurrentRoundIndex() + 1 + Mathf.Min(1.0f, (float)mCurrentRoundTimer / mStageInfo.roundInfoList[GetCurrentRoundIndex()].GetTotalTimer())) * per;
        }
    }

    /// <summary>
    /// ��ȡ��ǰ�����±�
    /// </summary>
    /// <returns>-1Ϊ׼���׶Σ�0Ϊ��һ�֣��Դ�����</returns>
    public int GetCurrentRoundIndex()
    {
        return mCurrentRoundIndex;
    }

    /// <summary>
    /// ��ȡ������
    /// </summary>
    /// <returns></returns>
    public int GetTotalRoundCount()
    {
        return mStageInfo.roundInfoList.Count;
    }

    public string GetName()
    {
        return mStageInfo.name;
    }

    public List<List<int>> GetApartList()
    {
        return mStageInfo.apartList;
    }

    public virtual void Update()
    {
        if (GameController.Instance.isPause)
            return;
        mCurrentRoundTimer++;
        // ������ǰ������������UI
        GameController.Instance.mProgressController.mRoundProgressBar.SetCurrentProgress(GetCurrentProgress());
    }

    public virtual bool WinCondition()
    {
        return false;
    }

    public virtual bool LossCondition()
    {
        return false;
    }

    /// <summary>
    /// ��ȡ�ض���·�鵱ǰ����ƫ����
    /// </summary>
    /// <param name="apartIndex"></param>
    /// <returns></returns>
    public int GetApartRowOffsetByIndex(int apartIndex)
    {
        return apartRowOffsetList[apartIndex];
    }
}
