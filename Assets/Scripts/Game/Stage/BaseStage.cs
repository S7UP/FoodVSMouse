using System;
using System.Collections;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

using static BaseRound;

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
        public string fileName; // �ļ���
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
        public bool isEnableJewelCount; // �Ƿ����ñ�ʯ��������
        public int jewelCount; // ��ʯ����
        public int pauseCount = -1; // ��ͣ����
        public int difficulty = 0; // �Ѷ�ͼ��

        public List<BaseRound.RoundInfo> roundInfoList; // �ؿ�������
        public StageMode defaultMode; // Ĭ�Ϲؿ�ģʽ
        /// <summary>
        /// ��·����һ��List������֧���ڶ���List����ÿ����֧����·�ı��
        /// ���� {{1,2,6,7},{3,4,5}} ������������֧����һ��֧����1267·���ڶ���֧����345·
        /// �ñ���Ҫ���ˢ���飨BaseEnemyGroup���е�����һͬʵ��ˢ��
        /// </summary>
        public List<List<int>> apartList;
        public int perpareTime; // ����ǰ��׼��ʱ��

        public bool isEnableStartCard; // �Ƿ��ṩ��ʼ��Ƭ
        public List<AvailableCardInfo>[][] startCardInfoList; // �ṩ��ʼ��Ƭ�����Ϣ��
        public string startBGMRefence; // ��ʼBGM����

        public Dictionary<string, List<float>> ParamArrayDict = new Dictionary<string, List<float>>(); // ���ֵĲ����仯�ֵ�

        /// <summary>
        /// �ڽ���ս������ǰ������Դ
        /// </summary>
        public IEnumerator LoadResWhenEnterCombatScene()
        {
            // ���뱾�ػ���ֵ���Դ
            GameManager.Instance.ResourceLoadingController.LoadFood(PlayerData.GetInstance().GetCurrentDynamicStageInfo().selectedCardInfoList); // ���뱾�ؿ�Ƭ����������Դ
            GameManager.Instance.ResourceLoadingController.LoadMouse(GetAllEnemyList(new List<BaseEnemyGroup>(), roundInfoList)); // ���뱾�ص�������������Դ
            GameManager.Instance.ResourceLoadingController.LoadBoss(GetBossList(new List<BaseEnemyGroup>(), roundInfoList)); // ���뱾��BOSS����������Դ
            // Ԥ����BGM
            foreach (var bgm in GetBGMNameList())
                yield return GameManager.Instance.StartCoroutine(AudioSourceController.AsyncLoadBGMusic(bgm));
        }

        public void UnLoadResWhenExitCombatScene()
        {
            // ж�ر��ػ���ֵ���Դ
            GameManager.Instance.ResourceLoadingController.UnLoadFood(PlayerData.GetInstance().GetCurrentDynamicStageInfo().selectedCardInfoList); // ж�ر��ؿ�Ƭ����������Դ
            GameManager.Instance.ResourceLoadingController.UnLoadMouse(GetAllEnemyList(new List<BaseEnemyGroup>(), roundInfoList)); // ж�ر��ص�������������Դ
            GameManager.Instance.ResourceLoadingController.UnLoadBoss(GetBossList(new List<BaseEnemyGroup>(), roundInfoList)); // ж�ر���BOSS����������Դ
            // ж��BGM
            //foreach (var bgm in GetBGMNameList())
            //    AudioSourceController.UnLoadBGMusic(bgm);
        }

        private List<string> GetBGMNameList()
        {
            List<string> bgmList = new List<string>();
            if (startBGMRefence != null && !startBGMRefence.Equals(""))
                bgmList.Add(startBGMRefence);
            foreach (var round in roundInfoList)
            {
                foreach (var bgm in round.GetAllBGM())
                {
                    bgmList.Add(bgm);
                }
            }
            return bgmList;
        }

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

        /// <summary>
        /// ��ȡ����Ϣ˳������������ĳ����ֵ�ִ��˳��
        /// </summary>
        /// <returns></returns>
        public List<BaseRound.RoundInfo> GetRoundInfoSequenceList()
        {
            List<RoundInfo> list = new List<RoundInfo>();
            // ��ÿ���ֶ�����ǰ�������
            foreach (var roundInfo in roundInfoList)
            {
                // ����������μ���
                foreach (var info in roundInfo.PreorderTraversal())
                {
                    list.Add(info);
                }
            }
            return list;
        }
    
        public List<float> GetParamList(string key)
        {
            if (ParamArrayDict.ContainsKey(key))
                return ParamArrayDict[key];
            else
                return null;
        }

        public float GetParamValue(string key, int index)
        {
            if (ParamArrayDict.ContainsKey(key))
            {
                index = Mathf.Max(0, Mathf.Min(index, ParamArrayDict[key].Count - 1));
                return ParamArrayDict[key][index];
            }
            else
                return 0;
        }

        public float GetParamValue(string key)
        {
            return GetParamValue(key, 0);
        }
    }


    // ����
    public StageInfo mStageInfo;
    public List<BaseRound> mRoundList;
    public BaseRound mCurrentRound;
    public int mTotalRoundTime;
    public int mEarlyTime; // ǰ������ǰ���ֶ������ʱ��
    public int mCurrentRoundIndex;
    private List<Stack<int>> apartRowOffsetStackList; // ��ǰ��·���Ӧ��ƫ����
    public bool isWinWhenClearAllBoss; // �ڻ�������BOSS���Ƿ��ж�Ϊʤ�� ����ǣ���ֻҪ�������һ��BOSS����ʤ����������Ҫ�����������������ʤ��
    public int bossLeft; // ȫ��ʣ��BOSS��������Ϸ��ʼʱ����ͳ���ж��ٸ�BOSS
    public bool isEndRound; // �Ƿ�������

    private Queue<BoolModifier> waveQueue; // ÿ�γ��ӵ�Ԫ�ز���ֵ������ǰ��Ϸ������ʾ��ʹ�á�һ�󲨡���ʾ���ǡ����һ������ʾ
    public Dictionary<string, List<float>> ParamArrayDict = new Dictionary<string, List<float>>(); // �ùؿ�Ŀǰ�Ĳ����仯�ֵ�
    public Dictionary<string, Action<string, List<float>, List<float>>> ParamChangeActionDict = new Dictionary<string, Action<string, List<float>, List<float>>>();

    /// <summary>
    /// ֻҪ��Round�ж�ȡ�������ͻᴥ��
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
    /// ��Ӳ������޸ĵļ���
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
    /// �Ƴ��������޸ĵļ���
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
    public IEnumerator WaitForIEnumerator(int time, bool isSkipWhenBossDie)
    {
        for (int i = 0; i < time; i++)
        {
            if (GameController.Instance.isPause)
                i--;
            else if (!GameController.Instance.IsHasEnemyInScene() || (isSkipWhenBossDie && !GameController.Instance.IsHasBossAlive()))
            {
                if(!isSkipWhenBossDie)
                    mEarlyTime += time - i;
                break; // �������û�е������������ȴ���������һ��
            }
            yield return null;
        }
    }

    /// <summary>
    /// �ؿ���ʼ
    /// </summary>
    public IEnumerator Execute()
    {
        InitByParam();
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
        // �ȵ�һ֡����һ֡Ҫ����
        yield return StartCoroutine(WaitForIEnumerator(1, false));
        // Ȼ���ǳ�ʼ���ɿ�Ƭ
        ConstructeStartCard();
        // ����BGM
        GameManager.Instance.audioSourceController.PlayBGMusic(mStageInfo.startBGMRefence);
        GameNormalPanel.Instance.ShowBGM(MusicManager.GetMusicInfo(mStageInfo.startBGMRefence));
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
            //yield return StartCoroutine(mCurrentRound.Execute());
            if(mCurrentRound.mRoundInfo.isBossRound)
                yield return StartCoroutine(mCurrentRound.ExecuteSpawnBoss(mCurrentRound.mRoundInfo.isSkipWhenBossDie));
            else
                yield return StartCoroutine(mCurrentRound.ExecuteSpawnEnemy(false));

            PopRowOffsetStack();
        }
        isEndRound = true;
        //Debug.Log("������ϣ�");
    }

    /// <summary>
    /// ���ݳ�ʼ��������һЩ����
    /// </summary>
    private void InitByParam()
    {
        // ��è
        if (mStageInfo.GetParamValue("isNoCat") == 1)
            GameController.Instance.mItemController.RemoveAllCats();

        // ����һ�γ�ʼ����
        foreach (var keyValuePair in mStageInfo.ParamArrayDict)
            ChangeParamValue(keyValuePair.Key, keyValuePair.Value);
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
        //Debug.Log("��ʼ�浵�ؿ���Ϣ��");
        JsonManager.Save(stageInfo, "Stage/Chapter/"+stageInfo.chapterIndex+"/"+stageInfo.sceneIndex+"/"+stageInfo.stageIndex);
        //Debug.Log("�ؿ���Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ɾ��ĳ���ؿ���Ϣ
    /// </summary>
    /// <param name="stageInfo"></param>
    public static void Delete(StageInfo stageInfo)
    {
        //Debug.Log("��ʼɾ���ؿ���Ϣ��");
        JsonManager.Delete("Stage/Chapter/" + stageInfo.chapterIndex + "/" + stageInfo.sceneIndex + "/" + stageInfo.stageIndex);
        //Debug.Log("�ؿ���Ϣɾ���ɹ���");
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
    /// ���ݴӱ��ض�ȡ��StageInfo����ʼ�����ر���
    /// </summary>
    public virtual void Init()
    {
        mCurrentRound = null;
        mCurrentRoundIndex = -1;
        isEndRound = false;
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
            //Debug.LogWarning("�޷��ҵ���ǰ�ؿ���RoundInfo��Ϣ��");
        }

        // ��ƫ������ʼ��
        apartRowOffsetStackList = new List<Stack<int>>();
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList.Add(new Stack<int>()); // ע�⣬��������ʱ���������ֵ
        }

        // ���㱾��BOSS��
        foreach (var item in mStageInfo.roundInfoList)
        {
            bossLeft += item.GetBossCount();
        }
        // ��������д���BOSS����ʤ��������Ϊ��������BOSS,�����������е���
        if (bossLeft > 0)
            isWinWhenClearAllBoss = true;
        else
            isWinWhenClearAllBoss = false;

        // ���㱾��һ�󲨵���ʾ�������
        waveQueue = new Queue<BoolModifier>(); // ��ʵ����Ϸ��ȡ����Ҫ��ʾһ�󲨹�ʱ���ö��г���ֵ��������ʾ��һ�󲨡������ǡ����һ����
        BoolModifier lastModifier = null;
        foreach (var roundInfo in mStageInfo.GetRoundInfoSequenceList())
        {
            // ÿ�ֱ�����ȥ��������������ӵ��������һ�����ӽ�ȥ������ʱ��ס��������Ϊ��ǰ��һ������
            // ���е�ǰ��������������˼�ǣ�true��ʾʹ�����һ����ʾ��false��ʾʹ��һ����ʾ��Ĭ��Ϊfalse
            if (roundInfo.isShowBigWaveRound)
            {
                BoolModifier b = new BoolModifier(false);
                waveQueue.Enqueue(b);
                lastModifier = b;
            }

            // �����ǰ����BOSS�֣���ǿ��������һ��������ʾΪ�����һ����������Ϊtrue
            if (roundInfo.isBossRound && lastModifier != null)
                lastModifier.Value = true;
        }
        // ���������ǿ��������һ��������ʾΪ�����һ����������Ϊtrue
        if (lastModifier != null)
            lastModifier.Value = true;

        ParamArrayDict.Clear();
        ParamChangeActionDict.Clear();
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
        // float per = 1.0f / (GetTotalRoundCount() + 1);
        if (GetCurrentRoundIndex() == -1)
        {
            // ���Ϊ׼���ִ�
            //return per * Mathf.Min(1.0f, (float)mCurrentRoundTimerLeft / mStageInfo.perpareTime);
            return 0;
        }
        else
        {
            //return (GetCurrentRoundIndex() + 1 + Mathf.Min(1.0f, (float)mCurrentRoundTimerLeft / mStageInfo.roundInfoList[GetCurrentRoundIndex()].GetTotalTimer())) * per;
            if (mTotalRoundTime == 0)
                return 0;
            return Mathf.Min(1, (float)(GameController.Instance.GetCurrentStageFrame() + mEarlyTime - mStageInfo.perpareTime)/mTotalRoundTime);
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
        if (GetCurrentRound() != null && GetCurrentRound().mRoundInfo.isBossRound)
            mEarlyTime--;
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
        int offset = 0;
        foreach (var item in apartRowOffsetStackList[apartIndex])
        {
            offset += item;
        }
        return offset;
    }

    /// <summary>
    /// ����ƫ��ջ���������������������ģʽ��
    /// </summary>
    public void PushRandomToRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Push(UnityEngine.Random.Range(0, mStageInfo.apartList[j].Count)); // ע�⣬��������ʱ���������ֵ
        }
    }

    /// <summary>
    /// ����ƫ��ջ�����0�������̶ֹ�ģʽ��
    /// </summary>
    public void PushZeroToRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Push(0);
        }
    }

    /// <summary>
    /// ����ƫ��ջ���Ƴ�
    /// </summary>
    public void PopRowOffsetStack()
    {
        for (int j = 0; j < mStageInfo.apartList.Count; j++)
        {
            apartRowOffsetStackList[j].Pop();
        }
    }

    /// <summary>
    /// BOSS��-1������BOSS���󱻻���ʱ����
    /// </summary>
    public void DecBossCount()
    {
        bossLeft--;
    }

    /// <summary>
    /// ��ȡ��һ���Ƿ���ʾΪ���һ��
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
            //Debug.LogWarning("��ǰһ�󲨶�����Ϊ�գ�Ĭ�ϵ�ǰ����ʾΪ��һ�󲨡�");
            return false;
        }
    }

    /// <summary>
    /// ��ʼ���ɿ�Ƭ
    /// </summary>
    private void ConstructeStartCard()
    {
        // дһ�����ջ��ƣ���Ҫ�����ڼ����ϰ汾��δ�ڱ༭�����ٴδ򿪹��Ĺؿ�
        // ��Ϊ startCardInfoList ����v0.26�汾�¼ӵ��ֶΣ�����ϰ汾�ؿ���ȡ������Ϊ��
        // �ڱ༭���У�ֻҪ�򿪹�����ؿ��ı༭��壬ϵͳ���Զ�Ϊ������һ�� startCardInfoList
        if (mStageInfo.startCardInfoList == null)
            return;

        BaseCardController c = GameController.Instance.mCardController;
        // ��ʼ���ɿ�Ƭ״̬��Ϣ���ݵ�ǰ�ؿ��Ŀ�Ƭ������
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
