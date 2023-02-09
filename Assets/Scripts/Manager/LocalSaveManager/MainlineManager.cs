using System.Collections.Generic;
/// <summary>
/// ���߹ؿ�������
/// </summary>
public class MainlineManager
{
    private const string path = "Mainline/";
    private const string localPath = "Mainline";
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
    /// ����ڱ��ص����ߴ浵
    /// </summary>
    [System.Serializable]
    public class MainlineSaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    public static Chapter_Static_List ChapterList { get { if (_ChapterList == null) Load(); return _ChapterList; } }
    private static Chapter_Static_List _ChapterList;
    public static int currentIndex; // ��ǰѡ�е��½��±꣨�����������±꣩

    public static MainlineSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static MainlineSaveInLocal _PlayerSave;

    public static Dictionary<string, float> MainlineFirstPassExpDict { get { if (_MainlineFirstPassExpDict == null) Load(); return _MainlineFirstPassExpDict; } }
    private static Dictionary<string, float> _MainlineFirstPassExpDict;


    /// <summary>
    /// ����ȫ����̬�����߹ؿ���Ϣ
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // ���ؾ�̬���߹ؿ�
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                _ChapterList = result;
            }
            // ������ͨʱ�ĵȼ����辭��ֵӳ���
            _MainlineFirstPassExpDict = new Dictionary<string, float>();
            ExcelManager.CSV csv = ExcelManager.ReadCSV("MainlineFirstPassExpMap", 2);
            for (int i = 0; i < csv.GetRow(); i++)
            {
                _MainlineFirstPassExpDict.Add(csv.GetValue(i, 0), PlayerManager.GetNextLevelExp(int.Parse(csv.GetValue(i, 1))));
            }
            // ����Ҵ浵���عؿ����
            {
                if (!JsonManager.TryLoadFromLocal(localPath, out _PlayerSave))
                {
                    _PlayerSave = new MainlineSaveInLocal();
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
            case "1-1":return level >= 1;
            case "1-2": return level >= 2;
            case "1-3": return level >= 3;
            case "1-4": return level >= 4;
            case "1-5": return level >= 5;
            case "1-6": return level >= 6;
            case "1-7": return level >= 7;
            case "2-1": return level >= 8;
            case "2-2": return level >= 9;
            case "2-3": return level >= 10;
            case "2-4": return level >= 11;
            case "2-5": return level >= 12;
            case "2-6": return level >= 13;
            case "2-7": return level >= 14;
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
        JsonManager.SaveOnLocal<MainlineSaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// ��ȡ��ǰ�½���Ϣ
    /// </summary>
    /// <returns></returns>
    public static Chapter_Static GetCurrentChapter()
    {
        return ChapterList.chapterList[currentIndex];
    }

    /// <summary>
    /// ��ȡ��ͨʱ�ľ���ֵ�ӳ�,0������޴���
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static float GetFirstPassExp(string id)
    {
        if (MainlineFirstPassExpDict.ContainsKey(id))
            return MainlineFirstPassExpDict[id];
        else
            return 0;
    }
}
