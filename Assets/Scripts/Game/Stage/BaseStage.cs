using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static BaseRound;

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
        public StageMode defaultMode; // 默认关卡模式
        /// <summary>
        /// 分路表，第一层List表明分支，第二层List表明每个分支包含路的编号
        /// 例如 {{1,2,6,7},{3,4,5}} 表明有两个分支，第一分支包含1267路，第二分支包含345路
        /// 该表需要配合刷怪组（BaseEnemyGroup）中的内容一同实现刷怪
        /// </summary>
        public List<List<int>> apartList;
        public int perpareTime; // 出怪前的准备时间

        public bool isEnableStartCard; // 是否提供初始卡片
        public List<AvailableCardInfo>[][] startCardInfoList; // 提供初始卡片相关信息表
        public string startBGMRefence; // 初始BGM引用

        /// <summary>
        /// 在进入战斗场景前加载资源
        /// </summary>
        public IEnumerator LoadResWhenEnterCombatScene()
        {
            // 获取所有的BGM
            List<string> bgmList = new List<string>();
            foreach (var round in roundInfoList)
            {
                foreach (var bgm in round.GetAllBGM())
                {
                    bgmList.Add(bgm);
                }
            }
            // 预加载BGM
            foreach (var bgm in bgmList)
            {
                //GameManager.Instance.AsyncGetAudioClip(bgm);
                yield return GameManager.Instance.StartCoroutine(AudioSourceManager.AsyncLoadBGMusic(bgm));
            }
        }

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

        /// <summary>
        /// 获取轮信息顺序表（就是真正的出怪轮的执行顺序）
        /// </summary>
        /// <returns></returns>
        public List<BaseRound.RoundInfo> GetRoundInfoSequenceList()
        {
            List<RoundInfo> list = new List<RoundInfo>();
            // 对每大轮都进行前序遍历，
            foreach (var roundInfo in roundInfoList)
            {
                // 将结果集依次加入
                foreach (var info in roundInfo.PreorderTraversal())
                {
                    list.Add(info);
                }
            }
            return list;
        }
    }


    // 变量
    public StageInfo mStageInfo;
    public List<BaseRound> mRoundList;
    public BaseRound mCurrentRound;
    public int mTotalRoundTime;
    public int mEarlyTime; // 前触发提前出怪而提早的时间
    public int mCurrentRoundIndex;
    private List<Stack<int>> apartRowOffsetStackList; // 当前分路组对应行偏移量
    public bool isWinWhenClearAllBoss; // 在击败所有BOSS后是否判定为胜利 如果是，则只要击败最后一个BOSS就算胜利，否则需要消灭场上所有老鼠才算胜利
    public int bossLeft; // 全关剩余BOSS数，在游戏开始时会先统计有多少个BOSS
    public bool isEndRound; // 是否出完怪了

    private Queue<BoolModifier> waveQueue; // 每次出队的元素布尔值决定当前游戏画面显示是使用“一大波”提示还是“最后一波”提示
    public Dictionary<string, List<float>> ParamArrayDict = new Dictionary<string, List<float>>(); // 该关卡目前的参数变化字典
    public Dictionary<string, Action<string, List<float>, List<float>>> ParamChangeActionDict = new Dictionary<string, Action<string, List<float>, List<float>>>();

    /// <summary>
    /// 只要在Round中读取到变量就会触发
    /// </summary>
    /// <param name="key"></param>
    public void ChangeParamValue(string key, List<float> list)
    {
        if (ParamArrayDict.ContainsKey(key))
        {
            if (ParamChangeActionDict.ContainsKey(key))
                ParamChangeActionDict[key](key, ParamArrayDict[key], list); // key, old, new
            ParamArrayDict[key] = list;
        }
        else
        {
            if (ParamChangeActionDict.ContainsKey(key))
                ParamChangeActionDict[key](key, null, list); // key, old, new
            ParamArrayDict.Add(key, list);
        }
    }

    /// <summary>
    /// 添加参数被修改的监听
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    public void AddParamChangeAction(string key, Action<string, List<float>, List<float>> action)
    {
        if (ParamChangeActionDict.ContainsKey(key))
        {
            ParamChangeActionDict[key] = action;
        }
        else
            ParamChangeActionDict.Add(key, action);
    }

    /// <summary>
    /// 移除参数被修改的监听
    /// </summary>
    /// <param name="key"></param>
    public void RemoveParamChangeAction(string key)
    {
        if (ParamChangeActionDict.ContainsKey(key))
        {
            ParamChangeActionDict.Remove(key);
        }
    }

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
            else if (!GameController.Instance.IsHasEnemyInScene())
            {
                mEarlyTime += time - i;
                break; // 如果场上没有敌人了则跳过等待，提早下一轮
            }
            yield return null;
        }
    }

    /// <summary>
    /// 关卡开始
    /// </summary>
    public IEnumerator Execute()
    {
        GameController.Instance.mProgressController.mRoundProgressBar.SetTotalRoundCount(mRoundList);
        GameController.Instance.mProgressController.mRoundProgressBar.UpdateRoundCountUI(0);
        mCurrentRoundIndex = -1;
        mTotalRoundTime = 0;
        mEarlyTime = 0;
        isEndRound = false;
        foreach (var round in mRoundList)
        {
            if(!round.mRoundInfo.isBossRound)
                mTotalRoundTime += round.GetTotalTimer();
        }
        // 先等一帧，第一帧要放人
        yield return StartCoroutine(WaitForIEnumerator(1));
        //Debug.Log("游戏开始了！现在是第"+GameController.Instance.GetCurrentStageFrame()+"帧");
        // 然后是初始生成卡片
        ConstructeStartCard();
        // 播放BGM
        GameManager.Instance.audioSourceManager.PlayBGMusic(mStageInfo.startBGMRefence);
        GameNormalPanel.Instance.ShowBGM(MusicManager.GetMusicInfo(mStageInfo.startBGMRefence));
        // yield return StartCoroutine(WaitForIEnumerator(mStageInfo.perpareTime));
        for (int i = 0; i < mStageInfo.perpareTime; i++)
        {
            if (GameController.Instance.isPause)
                i--;
            yield return null;
        }
        for (int i = 0; i < mRoundList.Count; i++)
        {
            mCurrentRound = mRoundList[i];
            GameController.Instance.mProgressController.mRoundProgressBar.UpdateRoundCountUI(i + 1);
            mCurrentRoundIndex = i;
            if (mStageInfo.defaultMode.Equals(StageMode.HalfRandom))
                PushRandomToRowOffsetStack();
            else
                PushZeroToRowOffsetStack();
            yield return StartCoroutine(mCurrentRound.Execute());
            PopRowOffsetStack();
        }
        isEndRound = true;
        //Debug.Log("出怪完毕！");
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
        //Debug.Log("开始存档关卡信息！");
        JsonManager.Save(stageInfo, "Stage/Chapter/"+stageInfo.chapterIndex+"/"+stageInfo.sceneIndex+"/"+stageInfo.stageIndex);
        //Debug.Log("关卡信息存档完成！");
    }

    /// <summary>
    /// 删除某个关卡信息
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Delete(StageInfo stageInfo)
    {
        //Debug.Log("开始删除关卡信息！");
        JsonManager.Delete("Stage/Chapter/" + stageInfo.chapterIndex + "/" + stageInfo.sceneIndex + "/" + stageInfo.stageIndex);
        //Debug.Log("关卡信息删除成功！");
    }

    public void Load()
    {
        mStageInfo = PlayerData.GetInstance().GetCurrentStageInfo();
    }

    public static StageInfo Load(int chapterIndex, int sceneIndex, int stageIndex)
    {
        return Load("Chapter/" + chapterIndex + "/" + sceneIndex + "/" + stageIndex);
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
        mCurrentRound = null;
        mCurrentRoundIndex = -1;
        isEndRound = false;
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
            //Debug.LogWarning("无法找到当前关卡的RoundInfo信息！");
        }

        // 行偏移量初始化
        apartRowOffsetStackList = new List<Stack<int>>();
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList.Add(new Stack<int>()); // 注意，生成整形时不包括最大值
        }

        // 计算本关BOSS数
        foreach (var item in mStageInfo.roundInfoList)
        {
            bossLeft += item.GetBossCount();
        }
        // 如果本关有存在BOSS，则胜利条件改为击败所有BOSS,而非消灭所有敌人
        if (bossLeft > 0)
            isWinWhenClearAllBoss = true;
        else
            isWinWhenClearAllBoss = false;

        // 计算本关一大波的显示情况队列
        waveQueue = new Queue<BoolModifier>(); // 当实际游戏读取到需要显示一大波怪时，该队列出队值决定是显示“一大波”，还是“最后一波”
        BoolModifier lastModifier = null;
        foreach (var roundInfo in mStageInfo.GetRoundInfoSequenceList())
        {
            // 每轮遍历过去，如果遇到有旗子的轮则添加一面旗子进去并且暂时记住这面旗子为当前上一个旗子
            // 其中当前布尔修饰器的意思是：true表示使用最后一波显示，false表示使用一大波显示，默认为false
            if (roundInfo.isShowBigWaveRound)
            {
                BoolModifier b = new BoolModifier(false);
                waveQueue.Enqueue(b);
                lastModifier = b;
            }

            // 如果当前轮是BOSS轮，则强制设置上一面旗子显示为“最后一波”，即置为true
            if (roundInfo.isBossRound && lastModifier != null)
                lastModifier.Value = true;
        }
        // 当遍历完后，强制设置上一面旗子显示为“最后一波”，即置为true
        if (lastModifier != null)
            lastModifier.Value = true;

        ParamArrayDict.Clear();
        ParamChangeActionDict.Clear();
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
        // float per = 1.0f / (GetTotalRoundCount() + 1);
        if (GetCurrentRoundIndex() == -1)
        {
            // 如果为准备轮次
            //return per * Mathf.Min(1.0f, (float)mCurrentRoundTimerLeft / mStageInfo.perpareTime);
            return 0;
        }
        else
        {
            //return (GetCurrentRoundIndex() + 1 + Mathf.Min(1.0f, (float)mCurrentRoundTimerLeft / mStageInfo.roundInfoList[GetCurrentRoundIndex()].GetTotalTimer())) * per;
            if (mTotalRoundTime == 0)
                return 0;
            return Mathf.Min(1, (float)(GameController.Instance.GetCurrentStageFrame() + mEarlyTime)/mTotalRoundTime);
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
        if (GetCurrentRound() != null && GetCurrentRound().mRoundInfo.isBossRound)
            mEarlyTime--;
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
        int offset = 0;
        foreach (var item in apartRowOffsetStackList[apartIndex])
        {
            offset += item;
        }
        return offset;
    }

    /// <summary>
    /// 往行偏移栈中添加随机数（用于轮随机模式）
    /// </summary>
    public void PushRandomToRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Push(UnityEngine.Random.Range(0, mStageInfo.apartList[j].Count)); // 注意，生成整形时不包括最大值
        }
    }

    /// <summary>
    /// 往行偏移栈中添加0（用于轮固定模式）
    /// </summary>
    public void PushZeroToRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Push(0);
        }
    }

    /// <summary>
    /// 从行偏移栈中移出
    /// </summary>
    public void PopRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Pop();
        }
    }

    /// <summary>
    /// BOSS数-1，仅在BOSS对象被回收时调用
    /// </summary>
    public void DecBossCount()
    {
        bossLeft--;
    }

    /// <summary>
    /// 获取下一大波是否显示为最后一波
    /// </summary>
    /// <returns></returns>
    public bool IsNextBigWaveIsLastWave()
    {
        if(waveQueue.Count > 0)
        {
            return waveQueue.Dequeue().Value;
        }
        else
        {
            //Debug.LogWarning("当前一大波队列已为空！默认当前大波显示为“一大波”");
            return false;
        }
    }

    /// <summary>
    /// 初始生成卡片
    /// </summary>
    private void ConstructeStartCard()
    {
        // 写一个保险机制，主要是用于兼容老版本但未在编辑器里再次打开过的关卡
        // 因为 startCardInfoList 是在v0.26版本新加的字段，因此老版本关卡读取不到，为空
        // 在编辑器中，只要打开过这个关卡的编辑面板，系统会自动为其生成一个 startCardInfoList
        if (mStageInfo.startCardInfoList == null)
            return;

        BaseCardController c = GameController.Instance.mCardController;
        // 初始生成卡片状态信息依据当前关卡的卡片制造器
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                BaseGrid g = GameController.Instance.mMapController.GetGrid(i, j);
                foreach (var info in mStageInfo.startCardInfoList[i][j])
                {
                    c.ConstructeByCardType(info.type, g);
                };
            }
        }
    }


}
