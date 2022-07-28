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
    /// �½ڹؿ���������Ϣ���ºš������š��غţ�
    /// </summary>
    [System.Serializable]
    public struct ChapterStageValue
    {
        public int chapterIndex;
        public int sceneIndex;
        public int stageIndex;
    }

    /// <summary>
    /// �ؿ���Ϣ
    /// </summary>
    [System.Serializable]
    public class StageInfo
    {
        public string name; // �ؿ���
        public string background; // �ؿ���������
        public string illustrate; // �ؿ�����˵��
        public string additionalNotes; // �ؿ�����˵��
        public int chapterIndex;
        public int sceneIndex;
        public int stageIndex;
        public int startCost; // ��ʼ����
        public bool isEnableTimeLimit; // �Ƿ�����ʱ������
        public int totalSeconds; // �ؿ���ʱ�䣨�룩
        public bool isEnableCardLimit; // �Ƿ����ÿ�Ƭ����
        public List<AvailableCardInfo> availableCardInfoList; // ��ѡ�õĿ�Ƭ��Ϣ��
        public bool isEnableCardCount; // �Ƿ����ÿ�Ƭ��������
        public int cardCount; // ��Ƭ����

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

        /// <summary>
        /// ��������������ѡ�ÿ���
        /// </summary>
        public void SortAvailableCardInfoList()
        {
            for (int i = 0; i < availableCardInfoList.Count; i++)
            {
                AvailableCardInfo a = availableCardInfoList[i];
                for (int j = i+1; j < availableCardInfoList.Count; j++)
                {
                    AvailableCardInfo b = availableCardInfoList[j];
                    if (a.type > b.type)
                    {
                        availableCardInfoList[i] = b;
                        availableCardInfoList[j] = a;
                        a = b;
                    }
                }
            }
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

    /// <summary>
    /// �浵�ؿ���Ϣ
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Save(StageInfo stageInfo)
    {
        Debug.Log("��ʼ�浵�ؿ���Ϣ��");
        JsonManager.Save(stageInfo, "Stage/Chapter/"+stageInfo.chapterIndex+"/"+stageInfo.sceneIndex+"/"+stageInfo.stageIndex);
        Debug.Log("�ؿ���Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ɾ��ĳ���ؿ���Ϣ
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Delete(StageInfo stageInfo)
    {
        Debug.Log("��ʼɾ���ؿ���Ϣ��");
        JsonManager.Delete("Stage/Chapter/" + stageInfo.chapterIndex + "/" + stageInfo.sceneIndex + "/" + stageInfo.stageIndex);
        Debug.Log("�ؿ���Ϣɾ���ɹ���");
    }

    public void Load()
    {
        ChapterStageValue values = GameManager.Instance.playerData.GetCurrentChapterStageValue();
        mStageInfo = Load("Chapter/"+ values.chapterIndex + "/" + values.sceneIndex + "/" + values.stageIndex);
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
