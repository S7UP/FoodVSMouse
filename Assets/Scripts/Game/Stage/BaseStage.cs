using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static BaseRound;
using static BaseStage;

public class BaseStage
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
    public int mCurrentRoundIndex;

    /// <summary>
    /// 关卡开始
    /// </summary>
    public IEnumerator Start()
    {
        Debug.Log("游戏开始了！现在是第"+GameController.Instance.GetCurrentStageFrame()+"帧");
        yield return GameController.Instance.StartCoroutine(GameController.Instance.WaitForIEnumerator(mStageInfo.perpareTime));
        Debug.Log("开始出怪了！现在是第" + GameController.Instance.GetCurrentStageFrame() + "帧");
        for (int i = 0; i < mRoundList.Count; i++)
        {
            yield return GameController.Instance.StartCoroutine(mRoundList[i].Execute());
        }
        Debug.Log("出怪完毕！");
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
        //mStageInfo = Load("test关卡2");
        mStageInfo = Load("singleMouseText");
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
            mCurrentRoundIndex = 0;
            mCurrentRound = mRoundList[mCurrentRoundIndex];
        }
        else
        {
            Debug.LogWarning("无法找到当前关卡的RoundInfo信息！");
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
