using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BaseStage : MonoBehaviour
{
    /// <summary>
    /// 关卡模式
    /// </summary>
    public enum StageMode
    {
        HalfRandom, // 半随机模式
        Fixed // 固定模式
    }

    /// <summary>
    /// 章节关卡的数据信息（章号、场景号、关号）
    /// </summary>
    [System.Serializable]
    public struct ChapterStageValue
    {
        public int chapterIndex;
        public int sceneIndex;
        public int stageIndex;
    }

    /// <summary>
    /// 关卡信息
    /// </summary>
    [System.Serializable]
    public class StageInfo
    {
        public string name; // 关卡名
        public string background; // 关卡背景描述
        public string illustrate; // 关卡机制说明
        public string additionalNotes; // 关卡附加说明
        public int chapterIndex;
        public int sceneIndex;
        public int stageIndex;
        public int startCost; // 初始费用
        public bool isEnableTimeLimit; // 是否启用时间限制
        public int totalSeconds; // 关卡总时间（秒）
        public bool isEnableCardLimit; // 是否启用卡片限制
        public List<AvailableCardInfo> availableCardInfoList; // 可选用的卡片信息表
        public bool isEnableCardCount; // 是否启用卡片数量限制
        public int cardCount; // 卡片数量

        public List<BaseRound.RoundInfo> roundInfoList; // 关卡轮数表
        public List<int> waveIndexList; // 一大波标志下标表
        public StageMode defaultMode; // 默认关卡模式
        /// <summary>
        /// 分路表，第一层List表明分支，第二层List表明每个分支包含路的编号
        /// 例如 {{1,2,6,7},{3,4,5}} 表明有两个分支，第一分支包含1267路，第二分支包含345路
        /// 该表需要配合刷怪组（BaseEnemyGroup）中的内容一同实现刷怪
        /// </summary>
        public List<List<int>> apartList;
        public int perpareTime; // 出怪前的准备时间
        

        /// <summary>
        /// 获取整个关卡轮数表
        /// </summary>
        /// <returns></returns>
        public BaseRound.RoundInfo GetTotalRoundInfo()
        {
            BaseRound.RoundInfo roundInfo = new BaseRound.RoundInfo();
            roundInfo.roundInfoList = roundInfoList;
            return roundInfo;
        }

        /// <summary>
        /// 按编号升序排序可选用卡组
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


    // 变量
    public StageInfo mStageInfo;
    public List<BaseRound> mRoundList;
    public BaseRound mCurrentRound;
    public int mCurrentRoundTimer;
    public int mCurrentRoundIndex;
    private List<int> apartRowOffsetList; // 当前分路组对应行偏移量
    //private IEnumerator enumerator;
    //private Coroutine mCoroutine;

    /// <summary>
    /// 开始关卡
    /// </summary>
    public void StartStage()
    {
        StartCoroutine(Execute());
    }

    /// <summary>
    /// 产生一个等待若干帧的IEnumerator
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
    /// 关卡开始
    /// </summary>
    public IEnumerator Execute()
    {
        GameController.Instance.mProgressController.mRoundProgressBar.SetTotalRoundCount(mRoundList.Count);
        GameController.Instance.mProgressController.mRoundProgressBar.UpdateRoundCountUI(0);
        mCurrentRoundIndex = -1;
        mCurrentRoundTimer = 0;
        Debug.Log("游戏开始了！现在是第"+GameController.Instance.GetCurrentStageFrame()+"帧");
        yield return StartCoroutine(WaitForIEnumerator(mStageInfo.perpareTime));
        //yield return GameController.Instance.Wait(mStageInfo.perpareTime);
        Debug.Log("开始出怪了！现在是第" + GameController.Instance.GetCurrentStageFrame() + "帧");
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
                    apartRowOffsetList[j] = Random.Range(0, mStageInfo.apartList[j].Count); // 注意，生成整形时不包括最大值
                }
            }
            //yield return GameController.Instance.StartCoroutine(mCurrentRound.Execute());
            yield return StartCoroutine(mCurrentRound.Execute());
        }
        Debug.Log("出怪完毕！");
        //enumerator = null;
    }

    /// <summary>
    /// 保存关卡信息至本地
    /// </summary>
    public void Save()
    {
        Save(mStageInfo);
    }

    /// <summary>
    /// 存档关卡信息
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Save(StageInfo stageInfo)
    {
        Debug.Log("开始存档关卡信息！");
        JsonManager.Save(stageInfo, "Stage/Chapter/"+stageInfo.chapterIndex+"/"+stageInfo.sceneIndex+"/"+stageInfo.stageIndex);
        Debug.Log("关卡信息存档完成！");
    }

    /// <summary>
    /// 删除某个关卡信息
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Delete(StageInfo stageInfo)
    {
        Debug.Log("开始删除关卡信息！");
        JsonManager.Delete("Stage/Chapter/" + stageInfo.chapterIndex + "/" + stageInfo.sceneIndex + "/" + stageInfo.stageIndex);
        Debug.Log("关卡信息删除成功！");
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
    /// 根据从本地读取的StageInfo来初始化本关变量
    /// </summary>
    public virtual void Init()
    {
        //enumerator = null;
        //mCoroutine = null;
        mCurrentRound = null;
        mCurrentRoundTimer = 0;
        mCurrentRoundIndex = -1;
        // 通过RoundInfo来创建BaseRound实体
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
            Debug.LogWarning("无法找到当前关卡的RoundInfo信息！");
        }

        // 行偏移量初始化
        apartRowOffsetList = new List<int>();
        for (int i = 0; i < mStageInfo.apartList.Count; i++)
        {
            apartRowOffsetList.Add(0);
        }
    }

    /// <summary>
    /// 获取当前轮
    /// </summary>
    /// <returns></returns>
    public BaseRound GetCurrentRound()
    {
        return mCurrentRound;
    }

    /// <summary>
    /// 获取当前关卡的进度
    /// </summary>
    /// <returns></returns>
    public float GetCurrentProgress()
    {
        float per = 1.0f / (GetTotalRoundCount() + 1);
        if (GetCurrentRoundIndex() == -1)
        {
            // 如果为准备轮次
            return per * Mathf.Min(1.0f, (float)mCurrentRoundTimer / mStageInfo.perpareTime);
        }
        else
        {
            return (GetCurrentRoundIndex() + 1 + Mathf.Min(1.0f, (float)mCurrentRoundTimer / mStageInfo.roundInfoList[GetCurrentRoundIndex()].GetTotalTimer())) * per;
        }
    }

    /// <summary>
    /// 获取当前轮数下标
    /// </summary>
    /// <returns>-1为准备阶段，0为第一轮，以此类推</returns>
    public int GetCurrentRoundIndex()
    {
        return mCurrentRoundIndex;
    }

    /// <summary>
    /// 获取总轮数
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
        // 操作当前轮数进度条的UI
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
    /// 获取特定分路组当前的行偏移量
    /// </summary>
    /// <param name="apartIndex"></param>
    /// <returns></returns>
    public int GetApartRowOffsetByIndex(int apartIndex)
    {
        return apartRowOffsetList[apartIndex];
    }
}
