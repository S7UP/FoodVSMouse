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
    /// 关卡信息
    /// </summary>
    [System.Serializable]
    public struct StageInfo
    {
        public string name; // 关卡名
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

    ///// <summary>
    ///// 暂停关卡
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
    ///// 恢复
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

    public static void Save(StageInfo stageInfo)
    {
        Debug.Log("开始存档关卡信息！");
        JsonManager.Save(stageInfo, "Stage/" + stageInfo.name);
        Debug.Log("关卡信息存档完成！");
    }

    /// <summary>
    /// 关卡模版，测试用
    /// </summary>
    public static void DemoSave()
    {
        List<BaseEnemyGroup> enemyGroupList = new List<BaseEnemyGroup>();
        // 每轮有一组老鼠，一组老鼠有三只
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


        // 搞个六轮
        List<BaseRound.RoundInfo> roundInfos = new List<BaseRound.RoundInfo>();
        for (int i = 0; i < 6; i++)
        {
            BaseRound.RoundInfo roundInfo = new BaseRound.RoundInfo
            {
                name = "第"+(i+1)+"轮",
                info = "",
                roundInfoList = null, // 自身的小轮
                baseEnemyGroupList = enemyGroupList, // 组
                interval = 1, // 组与组之间的间隔
                endTime = 240 // 结尾处等待时间
            };
            roundInfos.Add(roundInfo);
        }


        List<List<int>> apartList = new List<List<int>>();
        for (int i = 0; i < 2; i++)
        {
            apartList.Add(new List<int>());
        }
        // 添加1267路至组1
        apartList[0].Add(0);
        apartList[0].Add(1);
        apartList[0].Add(5);
        apartList[0].Add(6);
        // 添加345路至组2
        apartList[1].Add(2);
        apartList[1].Add(3);
        apartList[1].Add(4);

        StageInfo stageInfo = new StageInfo()
        {
            name = "test关卡",
            roundInfoList = roundInfos,
            waveIndexList = new List<int> { 1, 2 },
            defaultMode = StageMode.HalfRandom,
            apartList = apartList,
            perpareTime = 180
        };

        Debug.Log("开始存档关卡信息！");
        JsonManager.Save(stageInfo, "Stage/test");
        Debug.Log("关卡信息存档完成！");
    }

    /// <summary>
    /// 从本地中读取关卡信息
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
