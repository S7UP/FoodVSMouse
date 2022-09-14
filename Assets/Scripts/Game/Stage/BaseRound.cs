using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class BaseRound
{
    public enum RoundMode
    {
        Fixed = 0, // 固定模式
        HalfRandom = 1, // 半随机模式
    }

    [System.Serializable]
    public class RoundInfo
    {
        public string name; // 本轮名称
        public string info; // 本轮备注
        public List<RoundInfo> roundInfoList; // 自身的小轮
        public List<BaseEnemyGroup> baseEnemyGroupList; // 组
        public int interval; // 组与组之间的间隔
        public int endTime; // 结尾处等待时间
        public RoundMode roundMode; // 当前轮刷怪模式
        public bool isBossRound; // 是否为BOSS轮次
        public List<BaseEnemyGroup> bossList; // BOSS列表

        /// <summary>
        /// 获取本轮总持续时间
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
        /// 获取本轮中的BOSS数量（包括子轮）
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
    private BaseEnemyGroup mCurrentEnemyGroup; // 当前所在执行的组
    //private IEnumerator currentEnumerator;

    /// <summary>
    /// 根据RoundInfo初始化
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

    // 使用协程执行本轮的出怪逻辑
    public IEnumerator Execute()
    {
        // 设置行偏移量
        if (mRoundInfo.roundMode.Equals(RoundMode.Fixed))
            GameController.Instance.mCurrentStage.PushZeroToRowOffsetStack();
        else if(mRoundInfo.roundMode.Equals(RoundMode.HalfRandom))
            GameController.Instance.mCurrentStage.PushRandomToRowOffsetStack();

        // 先执行自己的刷怪组的内容
        if (mRoundInfo.isBossRound)
        {
            // 如果是BOSS轮次
            Debug.Log("当前轮为BOSS轮次，轮：" + mRoundInfo.name + "，开始刷怪！");
            for (int i = 0; i < mRoundInfo.bossList.Count; i++)
            {
                // 获取当前组
                mCurrentEnemyGroup = mRoundInfo.bossList[i];
                // 获取最终实际刷怪位置表
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // 通知GameController刷怪啦
                CreateBoss(realEnemyList, mCurrentEnemyGroup.mHp);
                // 没有间隔，BOSS是同时刷新的
            }

            // 执行完后执行自身子轮的刷怪组内容
            // 在BOSS轮次中，子轮先顺序执行一遍之后无限循环子轮的最后一个小轮直至BOSS死亡
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
                // 如果没有定义小轮，则改为每秒检测一次BOSS是否存在，不存在则进入下一阶段
                while (GameController.Instance.IsHasBoss())
                {
                    yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(60));
                }
            }

        }
        else
        {
            // 小怪轮次
            Debug.Log("当前轮为：" + mRoundInfo.name + "，开始刷怪！");
            for (int i = 0; i < mRoundInfo.baseEnemyGroupList.Count; i++)
            {
                // 获取当前组
                mCurrentEnemyGroup = mRoundInfo.baseEnemyGroupList[i];
                // 获取最终实际刷怪位置表
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // 通知GameController刷怪啦
                CreateEnemies(realEnemyList);

                // 间隔若干帧后刷新下一组
                yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.interval));
            }

            // 执行完后执行自身子轮的刷怪组内容
            for (int i = 0; i < mRoundList.Count; i++)
            {
                BaseRound round = mRoundList[i];
                yield return GameController.Instance.mCurrentStage.StartCoroutine(round.Execute());
            }
        }

        yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.endTime));

        GameController.Instance.mCurrentStage.PopRowOffsetStack();

        Debug.Log("本轮刷怪完成！");
    }

    /// <summary>
    /// 根据实际刷怪位置表来刷怪
    /// </summary>
    private void CreateEnemies(BaseEnemyGroup.RealEnemyList realEnemyList)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            GameController.Instance.CreateMouseUnit(realEnemyList.Get(i), realEnemyList.enemyInfo);
        }
    }

    /// <summary>
    /// 根据实际刷怪位置表来刷BOSS
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
    /// 获取自身子轮中所在当前轮
    /// </summary>
    public BaseRound GetCurrentRound()
    {
        return mRoundList[mCurrentIndex];
    }

    /// <summary>
    /// 获取叶子节点（最小轮数节点，即递归到不能再递归的那个节点）
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
    /// 获取等待间隔
    /// </summary>
    /// <returns></returns>
    public int GetInterval()
    {
        return mRoundInfo.interval;
    }

    /// <summary>
    /// 获取结尾等待时间
    /// </summary>
    /// <returns></returns>
    public int GetEndTime()
    {
        return mRoundInfo.endTime;
    }

    /// <summary>
    /// 获取本轮总持续时间
    /// </summary>
    /// <returns></returns>
    public int GetTotalTimer()
    {
        return mRoundInfo.GetTotalTimer();
    }

    /// <summary>
    /// 获取一个已经初始化好默认值的RoundInfo
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
