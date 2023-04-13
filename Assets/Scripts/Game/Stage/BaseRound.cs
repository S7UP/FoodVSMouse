using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class BaseRound
{
    public enum RoundMode
    {
        Fixed = 0, // �̶�ģʽ
        HalfRandom = 1, // �����ģʽ
    }

    [System.Serializable]
    public class RoundInfo
    {
        public string name; // ��������
        public string info; // ���ֱ�ע
        public List<RoundInfo> roundInfoList; // �����С��
        public List<BaseEnemyGroup> baseEnemyGroupList; // ��
        public int interval; // ������֮��ļ��
        public int endTime; // ��β���ȴ�ʱ��
        public RoundMode roundMode; // ��ǰ��ˢ��ģʽ
        public bool isShowBigWaveRound; // ��ǰ���Ƿ�Ҫ��ʾΪһ������
        public bool isBossRound; // �Ƿ�ΪBOSS�ִ�
        public List<BaseEnemyGroup> bossList; // BOSS�б�
        public string musicRefenceName; // BGM��������
        public Dictionary<string, List<float>> ParamArrayDict = new Dictionary<string, List<float>>(); // ���ֵĲ����仯�ֵ�


        /// <summary>
        /// ��ȡȫ����������
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllBGM()
        {
            List<string> list = new List<string>();
            if (musicRefenceName == null || musicRefenceName.Equals(""))
                return list;
            list.Add(musicRefenceName);
            foreach (var round in roundInfoList)
            {
                foreach (var bgm in round.GetAllBGM())
                {
                    list.Add(bgm);
                }
            }
            return list;
        }

        /// <summary>
        /// ��ȡ�����ܳ���ʱ��
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
        /// ��ȡ�����е�BOSS�������������֣�
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
        /// ��ȡ�����е�BOSS��Ϣ��
        /// </summary>
        /// <returns></returns>
        public List<BaseEnemyGroup> GetBossList()
        {
            List<BaseEnemyGroup> list = new List<BaseEnemyGroup>();
            if (isBossRound && bossList != null)
            {
                foreach (var g in bossList)
                {
                    list.Add(g);
                }
            }
            return BaseRound.GetBossList(list, roundInfoList);
        }

        /// <summary>
        /// ǰ�������ǰ��
        /// </summary>
        /// <returns></returns>
        public List<RoundInfo> PreorderTraversal()
        {
            List<RoundInfo> list = new List<RoundInfo>();
            // �ȼ����Լ�����Ϊ����ִ���Լ���
            list.Add(this);
            // ��ÿС�ֶ�����ǰ�������
            if (roundInfoList != null)
            {
                foreach (var roundInfo in roundInfoList)
                {
                    // ����������μ���
                    foreach (var info in roundInfo.PreorderTraversal())
                    {
                        list.Add(info);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// ͨ�������ķ�ʽ��ȡ������������ֵ����е�������������ϵ�ͳ��
        /// </summary>
        /// <returns></returns>
        public List<BaseEnemyGroup> GetAllEnemyList()
        {
            List<BaseEnemyGroup> list = new List<BaseEnemyGroup>();
            // Boss���εĵ��˲�ͳ��
            if (isBossRound)
                return list;
            if (baseEnemyGroupList != null)
                foreach (var item in baseEnemyGroupList)
                {
                    BaseEnemyGroup g = new BaseEnemyGroup();
                    g.mEnemyInfo = new BaseEnemyGroup.EnemyInfo() { type = item.mEnemyInfo.type, shape = item.mEnemyInfo.shape };
                    g.mCount = item.mCount;
                    list.Add(g);
                }
            list = BaseRound.GetAllEnemyList(list, roundInfoList);
            return list;
        }
    }

    public RoundInfo mRoundInfo;
    public List<BaseRound> mRoundList;
    public int mCurrentIndex;
    private BaseEnemyGroup mCurrentEnemyGroup; // ��ǰ����ִ�е���
    //private IEnumerator currentEnumerator;

    /// <summary>
    /// ����RoundInfo��ʼ��
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

    // ʹ��Э��ִ�б��ֵĳ����߼�
    public IEnumerator Execute()
    {
        // ����һ�β����仯�ļ���
        foreach (var keyValuePair in mRoundInfo.ParamArrayDict)
            GameController.Instance.mCurrentStage.ChangeParamValue(keyValuePair.Key, keyValuePair.Value);

        // ������ƫ����
        if (mRoundInfo.roundMode.Equals(RoundMode.Fixed))
            GameController.Instance.mCurrentStage.PushZeroToRowOffsetStack();
        else if(mRoundInfo.roundMode.Equals(RoundMode.HalfRandom))
            GameController.Instance.mCurrentStage.PushRandomToRowOffsetStack();

        // �ж��Ƿ�Ҫ��ʾһ�󲨣��Լ�����ʾ������
        if (mRoundInfo.isShowBigWaveRound)
        {
            bool isLastWave = GameController.Instance.mCurrentStage.IsNextBigWaveIsLastWave();
            GameController.Instance.mGameNormalPanel.ShowBigWave(isLastWave);
        }

        // ����BGM
        GameManager.Instance.audioSourceManager.PlayBGMusic(mRoundInfo.musicRefenceName);
        GameNormalPanel.Instance.ShowBGM(MusicManager.GetMusicInfo(mRoundInfo.musicRefenceName));

        // ��ִ���Լ���ˢ���������
        if (mRoundInfo.isBossRound)
        {
            // �����BOSS�ִ�
            for (int i = 0; i < mRoundInfo.bossList.Count; i++)
            {
                // ��ȡ��ǰ��
                mCurrentEnemyGroup = mRoundInfo.bossList[i];
                // ��ȡ����ʵ��ˢ��λ�ñ�
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // ֪ͨGameControllerˢ����
                CreateBoss(realEnemyList, mCurrentEnemyGroup.mHp, mCurrentEnemyGroup.GetEnemyAttribute());
                // û�м����BOSS��ͬʱˢ�µ�
            }

            // ִ�����ִ���������ֵ�ˢ��������
            // ��BOSS�ִ��У�������˳��ִ��һ��֮������ѭ�����ֵ����һ��С��ֱ��BOSS����
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
                // ���û�ж���С�֣����Ϊÿ����һ��BOSS�Ƿ���ڣ��������������һ�׶�
                while (GameController.Instance.IsHasBoss())
                {
                    yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(60));
                }
            }

        }
        else
        {
            // С���ִ�
            for (int i = 0; i < mRoundInfo.baseEnemyGroupList.Count; i++)
            {
                // ��ȡ��ǰ��
                mCurrentEnemyGroup = mRoundInfo.baseEnemyGroupList[i];
                // ��ȡ����ʵ��ˢ��λ�ñ�
                BaseEnemyGroup.RealEnemyList realEnemyList = mCurrentEnemyGroup.TransFormToRealEnemyGroup();
                // ֪ͨGameControllerˢ����
                CreateEnemies(realEnemyList, mCurrentEnemyGroup.GetEnemyAttribute());

                // �������֡��ˢ����һ��
                yield return GameController.Instance.mCurrentStage.StartCoroutine(GameController.Instance.mCurrentStage.WaitForIEnumerator(mRoundInfo.interval));
            }

            // ִ�����ִ���������ֵ�ˢ��������
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
    /// ����ʵ��ˢ��λ�ñ���ˢ��
    /// </summary>
    private void CreateEnemies(BaseEnemyGroup.RealEnemyList realEnemyList, BaseEnemyGroup.EnemyAttribute enemyAttribute)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            MouseUnit m = GameController.Instance.CreateMouseUnit(realEnemyList.Get(i), realEnemyList.enemyInfo);
            m.SetMaxHpAndCurrentHp(m.mMaxHp * enemyAttribute.HpRate * NumberManager.GetCurrentEnemyHpRate()); // �������������ֵ��������
            m.NumericBox.MoveSpeed.SetBase(m.NumericBox.MoveSpeed.baseValue*enemyAttribute.MoveSpeedRate); // �������ƶ��ٶ�����
            m.NumericBox.Attack.SetBase(m.mBaseAttack * enemyAttribute.AttackRate); // �����󹥻�������
            m.NumericBox.Defense.SetBase(enemyAttribute.Defence); // �������������

            // TODO�������������
        }
    }

    /// <summary>
    /// ����ʵ��ˢ��λ�ñ���ˢBOSS
    /// </summary>
    /// <param name="realEnemyList"></param>
    private void CreateBoss(BaseEnemyGroup.RealEnemyList realEnemyList, float hp, BaseEnemyGroup.EnemyAttribute enemyAttribute)
    {
        for (int i = 0; i < realEnemyList.GetSize(); i++)
        {
            BossUnit b = GameController.Instance.CreateBossUnit(realEnemyList.Get(i), realEnemyList.enemyInfo, hp, -1);
            b.SetMaxHpAndCurrentHp(b.mMaxHp * enemyAttribute.HpRate * NumberManager.GetCurrentEnemyHpRate()); // �������������ֵ��������
            b.NumericBox.MoveSpeed.SetBase(b.NumericBox.MoveSpeed.baseValue * enemyAttribute.MoveSpeedRate); // �������ƶ��ٶ�����
            b.NumericBox.Attack.SetBase(b.mBaseAttack * enemyAttribute.AttackRate); // �����󹥻�������
            b.NumericBox.Defense.SetBase(enemyAttribute.Defence); // �������������

            // �����������
            foreach (var keyValuePair in enemyAttribute.ParamDict)
            {
                b.AddParamArray(keyValuePair.Key, keyValuePair.Value);
            }
            // ����������������ˢ��һ��״̬
            b.OnHertStageChanged();
        }
    }


    /// <summary>
    /// ��ȡ�������������ڵ�ǰ��
    /// </summary>
    public BaseRound GetCurrentRound()
    {
        return mRoundList[mCurrentIndex];
    }

    /// <summary>
    /// ��ȡҶ�ӽڵ㣨��С�����ڵ㣬���ݹ鵽�����ٵݹ���Ǹ��ڵ㣩
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
    /// ��ȡ�ȴ����
    /// </summary>
    /// <returns></returns>
    public int GetInterval()
    {
        return mRoundInfo.interval;
    }

    /// <summary>
    /// ��ȡ��β�ȴ�ʱ��
    /// </summary>
    /// <returns></returns>
    public int GetEndTime()
    {
        return mRoundInfo.endTime;
    }

    /// <summary>
    /// ��ȡ�����ܳ���ʱ��
    /// </summary>
    /// <returns></returns>
    public int GetTotalTimer()
    {
        return mRoundInfo.GetTotalTimer();
    }

    /// <summary>
    /// ��ȡһ���Ѿ���ʼ����Ĭ��ֵ��RoundInfo
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

    /// <summary>
    /// ͨ�������ķ�ʽ��ȡ���������ֵ����е�������������ϵ�ͳ��
    /// </summary>
    /// <returns></returns>
    public static List<BaseEnemyGroup> GetAllEnemyList(List<BaseEnemyGroup> list, List<RoundInfo> roundInfoList)
    {
        if (roundInfoList != null)
        {
            foreach (var round in roundInfoList)
            {
                List<BaseEnemyGroup> l = round.GetAllEnemyList();
                for (int i = 0; i < l.Count; i++)
                {
                    BaseEnemyGroup g = l[i];
                    bool flag = false;
                    for (int j = 0; j < list.Count; j++)
                    {
                        BaseEnemyGroup g2 = list[j];
                        if (g.mEnemyInfo.type == g2.mEnemyInfo.type && g.mEnemyInfo.shape == g2.mEnemyInfo.shape)
                        {
                            g2.mCount += g.mCount;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        BaseEnemyGroup g3 = new BaseEnemyGroup();
                        g3.mEnemyInfo = new BaseEnemyGroup.EnemyInfo() { type = g.mEnemyInfo.type, shape = g.mEnemyInfo.shape };
                        g3.mCount = g.mCount;
                        list.Add(g3);
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// ͨ�������ķ�ʽ��ȡ���������ֵ�BOSSͳ��
    /// </summary>
    /// <returns></returns>
    public static List<BaseEnemyGroup> GetBossList(List<BaseEnemyGroup> list, List<RoundInfo> roundInfoList)
    {
        foreach (var round in roundInfoList)
        {
            foreach (var g in round.GetBossList())
            {
                list.Add(g);
            }
        }
        return list;
    }

}
