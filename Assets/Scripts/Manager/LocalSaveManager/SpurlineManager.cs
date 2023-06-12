using System.Collections.Generic;
/// <summary>
/// ֧�߹ؿ�������
/// </summary>
public class SpurlineManager
{
    private const string path = "Stage/Spurline/";
    private const string localPath = "Spurline";
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
    public class SaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    public static Chapter_Static_List ChapterList { get { if (_ChapterList == null) Load(); return _ChapterList; } }
    private static Chapter_Static_List _ChapterList;
    public static int currentIndex; // ��ǰѡ�е��½��±꣨�����������±꣩

    public static SaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static SaveInLocal _PlayerSave;


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
                    _PlayerSave = new SaveInLocal();
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
            case "NM1-1":return level >= 15;
            case "NM1-2":return level >= 15;
            case "NM1-3": return level >= 15;
            case "NM1-4": return level >= 15;
            case "NM1-5": return level >= 15;
            case "NM1-6": return level >= 15;
            case "NM1-7": return level >= 15;
            case "NM1-8": return level >= 15;
            case "NM1-9": return level >= 15;
            case "NM1-10": return level >= 15;
            case "NM1-11": return level >= 15;
            case "NM1-12": return level >= 15;
            case "NM1-13": return level >= 15;
            case "NM1-14": return level >= 15;
            case "NM1-15": return level >= 15;
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
        JsonManager.SaveOnLocal<SaveInLocal>(PlayerSave, localPath);
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
