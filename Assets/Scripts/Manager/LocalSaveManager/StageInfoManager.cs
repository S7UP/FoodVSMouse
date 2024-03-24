using System;
using System.Collections.Generic;

/// <summary>
/// �ؿ���Ϣ�����ߣ���������̬�ؿ�����Ҵ浵��
/// </summary>
public class StageInfoManager
{
    /// <summary>
    /// �ؿ���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // ����Ψһid
        public int level;
        public string[] preStageArray; // ǰ�ùؿ���ID
        public string other; // ����������������������˵����

        public string GetUnLockCondition()
        {
            string s = "��������:\n*"+level+"����";
            if(preStageArray!=null && preStageArray.Length > 0)
            {
                s += "\n*ͨ���ؿ���";
                foreach (var item in preStageArray)
                {
                    s += item + ";";
                }
            }
            if(other != null)
            {
                s += "\n*" + other;
            }
            return s;
        }
    }

    /// <summary>
    /// �½���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public string bgPath;
        public string introduce;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// ���½���Ϣ����������С�½ڣ� �������ߡ�֧��֮��ģ�
    /// </summary>
    public class BigChapter_Static
    {
        public string id; // ���½�ID����Mainline
        public string name; // ���½���������������
        public List<Chapter_Static> chapterList;
    }

    public class BigChapter_Local
    {
        public string id;
        public int currentIndex;  // ��ǰ����С�½ڵ��±�
    }

    /// <summary>
    /// �ؿ���Ϣ������Ҵ浵��أ���̬���ݣ�
    /// </summary>
    [System.Serializable]
    public class StageInfo_Local
    {
        public string id; // ����Ψһid�������澲̬���ݵ�ӳ�䣩
        public bool isUnlocked; // �Ƿ����
        public int rank = -1; // ͨ��������Ѷ�(-1����δͨ����
        public float diffRate = 1.0f; // ͨ��������Ѷȱ��ʣ�����ͨ��rank=3�������
        public int[] cardLevelArray = new int[] { -1, -1 }; // ��Ӧ�Ǽ���-1����Ϊ�գ��ɱ��������
        public int[] cardCountArray = new int[] { 0, 0 }; // ��Ӧ�Ǽ���Ƭ��
        public List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // ��ǰЯ���Ŀ�Ƭ�飨ѡ��������ؿ������Խӣ�
        public List<char> currentCardKeyList = new List<char>(); // ��ǰ��λ���Ʊ�
        public List<string> SelectedTagList = new List<string>(); // ��ѡ����
        public bool isNoLimit; // �Ƿ�ѡ����ģʽ
        public int minPassTime = -1; // ���ͨ����ʱ����Ϸ֡������Ϊ-1�����û�м�¼��
        public float firstPassPlayTime = -1; // �״�ͨ��ʱ���������ʱ��֮�ͣ��룩����Ϊ-1�����û��¼��
        public float totalPlayTime = 0; // ʵ��������ʱ��֮�ͣ��룩
    }

    /// <summary>
    /// ��ҵĹؿ��浵��Ϣ
    /// </summary>
    [System.Serializable]
    public class PlayerStageInfo
    {
        public float version = 0f; // �浵�汾
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>(); // �ؿ�ID-�ؿ������Ϣӳ���
        public Dictionary<string, BigChapter_Local> localBigChapterInfoDict = new Dictionary<string, BigChapter_Local>(); // ���½�ID-���½���Ϣӳ���
    }

    private static Dictionary<string, BigChapter_Static> bigChapterDict = new Dictionary<string, BigChapter_Static>(); // ���½�IDӳ���
    private static Dictionary<string, StageInfo_Static> staticInfoDict = new Dictionary<string, StageInfo_Static>(); // �ؿ�ID-�ؿ���̬��Ϣӳ���
    private static Dictionary<string, BaseStage.StageInfo> detailInfoDict = new Dictionary<string, BaseStage.StageInfo>(); // �ؿ�ID-�ؿ���ϸ��Ϣӳ���
    private static Dictionary<string, Action> firstPassRewardActionDict = new Dictionary<string, Action>(); // �ؿ�ID-��ͨ����ӳ���
    public static PlayerStageInfo mPlayerStageInfo;
    private static bool isLoad = false;

    public static void Load()
    {
        if (!isLoad)
        {
            // ��������Ԥ��ؿ�
            bigChapterDict.Add("Mainline", new BigChapter_Static() { id = "Mainline", name="���߾���", chapterList = LoadChapterList("Stage/Mainline/") } );
            bigChapterDict.Add("Spurline", new BigChapter_Static() { id = "Spurline", name = "֧�߾���", chapterList = LoadChapterList("Stage/Spurline/") });
            bigChapterDict.Add("WarriorChallenge", new BigChapter_Static() { id = "WarriorChallenge", name = "��ʿ��ս", chapterList = LoadChapterList("Stage/WarriorChallenge/") });
            bigChapterDict.Add("MagicTower", new BigChapter_Static() { id = "MagicTower", name = "ħ������", chapterList = LoadChapterList("Stage/MagicTower/") });
            bigChapterDict.Add("LegendChallenge", new BigChapter_Static() { id = "LegendChallenge", name = "��˵��ս", chapterList = LoadChapterList("Stage/LegendChallenge/") });
            bigChapterDict.Add("SpeedRun", new BigChapter_Static() { id = "SpeedRun", name = "��ͨ��ս", chapterList = LoadChapterList("Stage/SpeedRun/") });
            bigChapterDict.Add("Unused", new BigChapter_Static() { id = "Unused", name = "��������", chapterList = LoadChapterList("Stage/Unused/") });
            // ������ͨ����
            LoadMainlineFirstPassReward();
            // ������Ҵ浵��Ϣ
            {
                if (!JsonManager.TryLoadFromLocal("PlayerStageInfo", out mPlayerStageInfo))
                {
                    mPlayerStageInfo = new PlayerStageInfo();
                    Save();
                }
                // ���汾������һЩ����
                if(mPlayerStageInfo.version < 1.1)
                {
                    // ��1.1�汾�У���ҪǨ�����ߡ�֧�ߡ���ʿ�Ĵ浵��Ϣ����mPlayerStageInfo
                    PlayerStageInfo MainlineStageInfo;
                    PlayerStageInfo SpurlineStageInfo;
                    PlayerStageInfo WarriorChallengeStageInfo;

                    if (JsonManager.TryLoadFromLocal("Mainline", out MainlineStageInfo))
                    {
                        foreach (var keyValuePair in MainlineStageInfo.localStageInfoDict)
                            mPlayerStageInfo.localStageInfoDict.Add(keyValuePair.Key, keyValuePair.Value);
                        JsonManager.DeleteFromLocal("Mainline");
                    }


                    if (JsonManager.TryLoadFromLocal("Spurline", out SpurlineStageInfo))
                    {
                        foreach (var keyValuePair in SpurlineStageInfo.localStageInfoDict)
                            mPlayerStageInfo.localStageInfoDict.Add(keyValuePair.Key, keyValuePair.Value);
                        JsonManager.DeleteFromLocal("Spurline");
                    }


                    if (JsonManager.TryLoadFromLocal("WarriorChallenge", out WarriorChallengeStageInfo))
                    {
                        foreach (var keyValuePair in WarriorChallengeStageInfo.localStageInfoDict)
                            mPlayerStageInfo.localStageInfoDict.Add(keyValuePair.Key, keyValuePair.Value);
                        JsonManager.DeleteFromLocal("WarriorChallenge");
                    }
                    mPlayerStageInfo.version = 1.1f;
                    Save();
                }
                // 1.2��
                if(mPlayerStageInfo.version < 1.2f)
                {
                    mPlayerStageInfo.localBigChapterInfoDict.Add("Mainline", new BigChapter_Local() { id = "Mainline", currentIndex = 0 });
                    mPlayerStageInfo.localBigChapterInfoDict.Add("Spurline", new BigChapter_Local() { id = "Spurline", currentIndex = 0 });
                    mPlayerStageInfo.localBigChapterInfoDict.Add("WarriorChallenge", new BigChapter_Local() { id = "WarriorChallenge", currentIndex = 0 });
                    mPlayerStageInfo.localBigChapterInfoDict.Add("MagicTower", new BigChapter_Local() { id = "MagicTower", currentIndex = 0 });
                    mPlayerStageInfo.localBigChapterInfoDict.Add("LegendChallenge", new BigChapter_Local() { id = "LegendChallenge", currentIndex = 0 });
                    mPlayerStageInfo.version = 1.2f;
                    Save();
                }
                // 1.3�棨���յ�һ�棩
                if(mPlayerStageInfo.version < 1.3f)
                {
                    Move("LC1-2", "LC1-5"); // ����Ǩ��
                    Move("LC1-1", "LC1-2"); // ����Ǩ��
                    mPlayerStageInfo.version = 1.3f;
                    Save();
                }
                // 1.4�棨���մ����ڲ�汾��
                if (mPlayerStageInfo.version < 1.4f)
                {
                    // �ѻþ�Ǩ�Ƶ�LC1-6
                    Move("LC1-7", "LC1-6"); // �þ�Ǩ��
                    mPlayerStageInfo.localBigChapterInfoDict.Add("SpeedRun", new BigChapter_Local() { id = "SpeedRun", currentIndex = 0 }); // �����ͨ
                    mPlayerStageInfo.localBigChapterInfoDict.Add("Unused", new BigChapter_Local() { id = "Unused", currentIndex = 0 }); // �����������
                    mPlayerStageInfo.version = 1.4f;
                    Save();
                }
                // 1.5�棨���մ����ڲ�汾2��������ɽ��˵��ֲ��
                if (mPlayerStageInfo.version < 1.5f)
                {
                    Move("LC1-3", "OLD-LC1-3"); // ����Ǩ��
                    Move("LC1-4", "OLD-LC1-4"); // ���Ǩ��
                    Move("LC1-5", "OLD-LC1-5"); // ����Ǩ��
                    mPlayerStageInfo.version = 1.5f;
                    Save();
                }
            }
            isLoad = true;
        }
    }



    #region �������õķ���
    /// <summary>
    /// �ӱ��ش浵��ȡ�ؿ���������
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static StageInfo_Local GetLocalStageInfo(string id)
    {
        if (!mPlayerStageInfo.localStageInfoDict.ContainsKey(id))
        {
            mPlayerStageInfo.localStageInfoDict.Add(id, new StageInfo_Local() { id=id });
            Save();
        }
        StageInfo_Local local_info = mPlayerStageInfo.localStageInfoDict[id];
        // ÿ�λ�ȡʱ�����Խ���һ�¹ؿ�
        if (!local_info.isUnlocked && CanUnlockStage(id))
        {
            local_info.isUnlocked = true;
            Save();
        }
        return local_info;
    }

    /// <summary>
    /// ��ȡ��̬�ؿ����ݣ�������������֮��ģ�
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static StageInfo_Static GetStaticStageInfo(string id)
    {
        if (staticInfoDict.ContainsKey(id))
            return staticInfoDict[id];
        else
            return new StageInfo_Static();
    }

    /// <summary>
    /// ���ݹؿ�����ȡ��ϸ�Ĺؿ���Ϣ���������ֵȲ�����
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static BaseStage.StageInfo GetDetailInfo(string id)
    {
        if (detailInfoDict.ContainsKey(id))
            return detailInfoDict[id];
        else
            return null;
    }

    /// <summary>
    /// ��ȡ���½���Ϣ
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static BigChapter_Static GetBigChapterStatic(string id)
    {
        if (bigChapterDict.ContainsKey(id))
            return bigChapterDict[id];
        else
            return null;
    }

    public static BigChapter_Local GetBigChapterLocal(string id)
    {
        if (mPlayerStageInfo.localBigChapterInfoDict.ContainsKey(id))
            return mPlayerStageInfo.localBigChapterInfoDict[id];
        else
            return null;
    }

    /// <summary>
    /// ��ȡ��ͨ�����¼�
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Action GetFirstPassRewardAction(string id)
    {
        if (firstPassRewardActionDict.ContainsKey(id))
            return firstPassRewardActionDict[id];
        else
            return null;
    }

    /// <summary>
    /// ����һ�α��صĴ浵
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<PlayerStageInfo>(mPlayerStageInfo, "PlayerStageInfo");
    }
    #endregion

    #region ˽�з�������Ҫ�Ǽ��ض�����
    /// <summary>
    /// �����½��飨���ߡ�֧�ߡ���ʿ��ħ���ȣ�
    /// </summary>
    /// <param name="resPath"></param>
    private static List<Chapter_Static> LoadChapterList(string resPath)
    {
        List<Chapter_Static> chapterList = new List<Chapter_Static>();
        ExcelManager.CSV csv = ExcelManager.ReadCSV(resPath+"ChapterList", 4);
        for (int i = 0; i < csv.GetRow(); i++)
        {
            Chapter_Static chapter = new Chapter_Static();
            string id = csv.GetValue(i, 0);// �½�ID
            string name = csv.GetValue(i, 1);
            string bgPath = csv.GetValue(i, 2);
            string introduce = csv.GetValue(i, 3);

            chapter.name = name;
            chapter.bgPath = bgPath;
            chapter.introduce = introduce;

            List<StageInfo_Static> stageInfoList = new List<StageInfo_Static>();
            ExcelManager.CSV c2 = ExcelManager.ReadCSV(resPath + id, 4);
            for (int j = 0; j < c2.GetRow(); j++)
            {
                string[] preStage;
                string s = c2.GetValue(j, 2);
                if (s.Equals("��"))
                    preStage = null;
                else
                    preStage = s.Split('/');
                int level = 0;
                int.TryParse(c2.GetValue(j, 1), out level);

                StageInfo_Static s_info = new StageInfo_Static() {
                    id = c2.GetValue(j, 0),
                    level = level,
                    preStageArray = preStage,
                    other = (c2.GetValue(j, 3).Equals("��")?null: c2.GetValue(j, 3))
                };
                BaseStage.StageInfo res;
                // �����ֵ�
                staticInfoDict.Add(s_info.id, s_info);
                if (JsonManager.TryLoadFromResource<BaseStage.StageInfo>(resPath + s_info.id, out res))
                    detailInfoDict.Add(s_info.id, res);
                stageInfoList.Add(s_info);
            }
            chapter.stageList = stageInfoList;
            chapterList.Add(chapter);
        }
        return chapterList;
    }

    /// <summary>
    /// ����������ͨ����
    /// </summary>
    private static void LoadMainlineFirstPassReward()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV("MainlineFirstPassExpMap", 2);
        for (int _i = 0; _i < csv.GetRow(); _i++)
        {
            int i = _i;
            firstPassRewardActionDict.Add(csv.GetValue(i, 0),
                delegate {
                    float exp = PlayerManager.GetNextLevelExp(int.Parse(csv.GetValue(i, 1)));
                    GameNormalPanel panel = GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel] as GameNormalPanel;
                    if (panel != null)
                        panel.SetExpTips("��ϲ���һ��ͨ�����أ�����" + exp + "�㾭��ֵ��");
                    PlayerData.GetInstance().AddExp(exp);
                });
        }
    }

    /// <summary>
    /// �ܷ����ĳ��id�Ĺؿ�
    /// </summary>
    /// <returns></returns>
    private static bool CanUnlockStage(string id)
    {
        int level = PlayerData.GetInstance().GetLevel();
        StageInfo_Static info_static = GetStaticStageInfo(id);
        if (info_static.id == null)
            return false;
        // ���жϵȼ�
        if (level < info_static.level)
            return false;
        // ���ж�ǰ�ùؿ�
        if(info_static.preStageArray != null)
        {
            foreach (var p_id in info_static.preStageArray)
            {
                if (!IsPass(p_id))
                    return false;
            }
        }
        // TODO �ж���������
        return true;
    }

    /// <summary>
    /// ����ѯĳ��ͨ�����
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private static bool IsPass(string id)
    {
        StageInfo_Local local_info = GetLocalStageInfo(id);
        if (local_info != null)
            return local_info.rank > -1;
        else
            return false;
    }

    /// <summary>
    /// Ǩ�ƹؿ��浵������ִ�����л������أ���ԭIDλ�ᱻɾ����
    /// </summary>
    /// <param name="old_id">ԭ�ؿ�ID</param>
    /// <param name="new_id">Ǩ�ƺ�Ĺؿ�ID</param>
    private static void Move(string old_id, string new_id)
    {
        if (mPlayerStageInfo.localStageInfoDict.ContainsKey(old_id))
        {
            if (mPlayerStageInfo.localStageInfoDict.ContainsKey(new_id))
            {
                mPlayerStageInfo.localStageInfoDict[new_id] = mPlayerStageInfo.localStageInfoDict[old_id];
            }
            else
            {
                mPlayerStageInfo.localStageInfoDict.Add(new_id, mPlayerStageInfo.localStageInfoDict[old_id]);
            }
            // ɾ��ԭ��¼
            mPlayerStageInfo.localStageInfoDict.Remove(old_id);
        }
    }
    #endregion
}
