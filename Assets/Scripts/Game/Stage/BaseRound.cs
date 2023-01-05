using System;
using System.Collections;
using System.Collections.Generic;

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
        public bool isShowBigWaveRound; // 当前轮是否要提示为一大波老鼠
        public bool isBossRound; // 是否为BOSS轮次
        public List<BaseEnemyGroup> bossList; // BOSS列表
        public string musicRefenceName; // BGM的引用名


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

        /// <summary>
        /// 前序遍历当前轮
        /// </summary>
        /// <returns></returns>
        public List<RoundInfo> PreorderTraversal()
        {
            List<RoundInfo> list = new List<RoundInfo>();
            // 先加入自己（因为是先执行自己）
            list.Add(this);
            // 对每小轮都进行前序遍历，
            if (roundInfoList != null)
            {
                foreach (var roundInfo in roundInfoList)
                {
                    // 将结果集依次加入
                    foreach (var info in roundInfo.PreorderTraversal())
                    {
                        list.Add(info);
                    }
                }
            }
            return list;
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

        // 判断是否要显示一大波，以及，显示的内容
        if (mRoundInfo.isShowBigWaveRound)
        {
            bool isLastWave = GameController.Instance.mCurrentStage.IsNextBigWaveIsLastWave();
            GameController.Instance.mGameNormalPanel.ShowBigWave(isLastWave);
        }

        // 播放BGM
        GameManager.Instance.audioSourceManager.PlayBGMusic(mRoundInfo.musicRefenceName);

        // 先执行自己的刷怪组的内容
        if (mRoundInfo.isBossRound)
        {
            // 如果是BOSS轮次
            for (int i = 0; i < mRoundInfo.bossList.Count; i++)
            {
                // 获取当前组
                mCurrentEnemyGroup = mRoundInfo.bossList[i];
                // 获取最终实际刷怪位置表
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // 通知GameController刷怪啦
                CreateBoss(realEnemyList, mCurrentEnemyGroup.mHp, mCurrentEnemyGroup.GetEnemyAttribute());
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
            for (int i = 0; i < mRoundInfo.baseEnemyGroupList.Count; i++)
            {
                // 获取当前组
                mCurrentEnemyGroup = mRoundInfo.baseEnemyGroupList[i];
                // 获取最终实际刷怪位置表
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // 通知GameController刷怪啦
                CreateEnemies(realEnemyList, mCurrentEnemyGroup.GetEnemyAttribute());

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
    }

    /// <summary>
    /// 根据实际刷怪位置表来刷怪
    /// </summary>
    private void CreateEnemies(BaseEnemyGroup.RealEnemyList realEnemyList, BaseEnemyGroup.EnemyAttribute enemyAttribute)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            MouseUnit m = GameController.Instance.CreateMouseUnit(realEnemyList.Get(i), realEnemyList.enemyInfo);
            m.SetMaxHpAndCurrentHp(m.mMaxHp * enemyAttribute.HpRate); // 对老鼠最大生命值进行修正
            m.NumericBox.MoveSpeed.SetBase(m.NumericBox.MoveSpeed.baseValue*enemyAttribute.MoveSpeedRate); // 对老鼠移动速度修正
            m.NumericBox.Attack.SetBase(m.mBaseAttack * enemyAttribute.AttackRate); // 对老鼠攻击力修正
            m.NumericBox.Defense.SetBase(enemyAttribute.Defence); // 对老鼠减伤修正

            // TODO：覆盖特殊参数
        }
    }

    /// <summary>
    /// 根据实际刷怪位置表来刷BOSS
    /// </summary>
    /// <param name="realEnemyList"></param>
    private void CreateBoss(BaseEnemyGroup.RealEnemyList realEnemyList, float hp, BaseEnemyGroup.EnemyAttribute enemyAttribute)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            BossUnit b = GameController.Instance.CreateBossUnit(realEnemyList.Get(i), realEnemyList.enemyInfo, hp, -1);
            b.SetMaxHpAndCurrentHp(b.mMaxHp * enemyAttribute.HpRate); // 对老鼠最大生命值进行修正
            // 要是把上面那行注释了，然后把下面这个语句块取消注释，就能还原原版的卡暴击方法
            // 认真的吗？原版的BUG也还原给你看
            //{
            //    int timeLeft = 60;
            //    CustomizationTask t = new CustomizationTask();
            //    t.AddTaskFunc(delegate {
            //        if (timeLeft > 0)
            //            timeLeft--;
            //        else
            //        {
            //            b.SetMaxHpAndCurrentHp(b.mMaxHp * enemyAttribute.HpRate); // 对老鼠最大生命值进行修正
            //            return true;
            //        }
            //        return false;
            //    });
            //}

            b.NumericBox.MoveSpeed.SetBase(b.NumericBox.MoveSpeed.baseValue * enemyAttribute.MoveSpeedRate); // 对老鼠移动速度修正
            b.NumericBox.Attack.SetBase(b.mBaseAttack * enemyAttribute.AttackRate); // 对老鼠攻击力修正
            b.NumericBox.Defense.SetBase(enemyAttribute.Defence); // 对老鼠减伤修正

            //Debug.Log("开始覆盖特殊参数");
            // 覆盖特殊参数
            foreach (var keyValuePair in enemyAttribute.ParamDict)
            {
                //Debug.Log("Key="+ keyValuePair.Key);
                b.AddParamArray(keyValuePair.Key, keyValuePair.Value);
            }
            //Debug.Log("覆盖特殊参数结束");
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
