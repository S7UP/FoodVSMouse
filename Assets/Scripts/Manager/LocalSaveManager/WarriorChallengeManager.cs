using System.Collections.Generic;
/// <summary>
/// ���߹ؿ�������
/// </summary>
public class WarriorChallengeManager
{
    private const string path = "Stage/WarriorChallenge/";
    private const string localPath = "WarriorChallenge";
    private static bool isLoad; // �Ƿ���ع��ˣ���ֹ�ظ����أ�

    /// <summary>
    /// ���������½ڱ���̬���ݣ�
    /// </summary>
    [System.Serializable]
    public class Chapter_Static_List
    {
        public List<Chapter_Static> chapterList;
    }

    /// <summary>
    /// �½���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public int mapId;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// �ؿ���Ϣ����̬���ݣ�
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // ����Ψһid
        public int chapterId;
        public int sceneId;
        public int stageId;
        public string unlockCondition; // ������������������˵����
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

        public StageInfo_Local(string id)
        {
            this.id = id;
        }
    }

    /// <summary>
    /// ����ڱ��ص���ʿ�ش浵
    /// </summary>
    [System.Serializable]
    public class WarriorChallengeSaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    public static Chapter_Static_List ChapterList { get { if (_ChapterList == null) Load(); return _ChapterList; } }
    private static Chapter_Static_List _ChapterList;
    public static int currentIndex; // ��ǰѡ�е��½��±꣨�����������±꣩

    public static WarriorChallengeSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static WarriorChallengeSaveInLocal _PlayerSave;


    /// <summary>
    /// ����ȫ����̬�����߹ؿ���Ϣ
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // ���ؾ�̬��ʿ�ؿ�
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                _ChapterList = result;
            }
            // ����Ҵ浵���عؿ����
            {
                if (!JsonManager.TryLoadFromLocal(localPath, out _PlayerSave))
                {
                    _PlayerSave = new WarriorChallengeSaveInLocal();
                    Save();
                }
            }
            isLoad = true;
        }
    }

    /// <summary>
    /// �ӱ��ش浵��ȡ�ؿ���������
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static StageInfo_Local GetLocalStageInfo(string id)
    {
        if (!PlayerSave.localStageInfoDict.ContainsKey(id))
        {
            PlayerSave.localStageInfoDict.Add(id, new StageInfo_Local(id));
            Save();
        }
        StageInfo_Local local_info = PlayerSave.localStageInfoDict[id];
        // ÿ�λ�ȡʱ�����Խ���һ�¹ؿ�
        if (!local_info.isUnlocked && CanUnlockStage(id))
        {
            local_info.isUnlocked = true;
            Save();
        }
        return local_info;
    }

    /// <summary>
    /// �ܷ����ĳ��id�Ĺؿ�
    /// </summary>
    /// <returns></returns>
    public static bool CanUnlockStage(string id)
    {
        int level = PlayerData.GetInstance().GetLevel();
        // �����ֻ��ö����
        switch (id)
        {
            case "EX1-1":return level >= 15;
            case "EX1-2":return level >= 15;
            case "EX2-1": return level >= 30;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// ����һ�α��صĴ浵
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<WarriorChallengeSaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// ��ȡ��ǰ�½���Ϣ
    /// </summary>
    /// <returns></returns>
    public static Chapter_Static GetCurrentChapter()
    {
        // ��������
        if (currentIndex >= ChapterList.chapterList.Count)
            currentIndex = ChapterList.chapterList.Count - 1;
        return ChapterList.chapterList[currentIndex];
    }
}
