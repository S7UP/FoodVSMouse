using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static BaseRound;
using static BaseStage;

public class BaseStage
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
    public int mCurrentRoundIndex;

    /// <summary>
    /// �ؿ���ʼ
    /// </summary>
    public IEnumerator Start()
    {
        Debug.Log("��Ϸ��ʼ�ˣ������ǵ�"+GameController.Instance.GetCurrentStageFrame()+"֡");
        yield return GameController.Instance.StartCoroutine(GameController.Instance.WaitForIEnumerator(mStageInfo.perpareTime));
        Debug.Log("��ʼ�����ˣ������ǵ�" + GameController.Instance.GetCurrentStageFrame() + "֡");
        for (int i = 0; i < mRoundList.Count; i++)
        {
            yield return GameController.Instance.StartCoroutine(mRoundList[i].Execute());
        }
        Debug.Log("������ϣ�");
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
        //mStageInfo = Load("test�ؿ�2");
        mStageInfo = Load("singleMouseText");
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
            mCurrentRoundIndex = 0;
            mCurrentRound = mRoundList[mCurrentRoundIndex];
        }
        else
        {
            Debug.LogWarning("�޷��ҵ���ǰ�ؿ���RoundInfo��Ϣ��");
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

    }

    public virtual bool WinCondition()
    {
        return false;
    }

    public virtual bool LossCondition()
    {
        return false;
    }
}
