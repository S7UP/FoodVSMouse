using System.Collections.Generic;
/// <summary>
/// 主线关卡管理者
/// </summary>
public class MainlineManager
{
    private const string path = "Mainline/";
    private const string localPath = "Mainline";
    private static bool isLoad; // 是否加载过了（防止重复加载）

    /// <summary>
    /// 所有主线章节表（静态数据）
    /// </summary>
    [System.Serializable]
    public class Chapter_Static_List
    {
        public List<Chapter_Static> chapterList;
    }

    /// <summary>
    /// 章节信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public int mapId;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// 关卡信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // 自身唯一id
        public int chapterId;
        public int sceneId;
        public int stageId;
        public string unlockCondition; // 解锁的条件（仅文字说明）
    }

    /// <summary>
    /// 关卡信息（与玩家存档相关，动态数据）
    /// </summary>
    [System.Serializable]
    public class StageInfo_Local
    {
        public string id; // 自身唯一id（是上面静态数据的映射）
        public bool isUnlocked; // 是否解锁
        public int rank = -1; // 通过的最高难度(-1代表未通过）

        public StageInfo_Local(string id)
        {
            this.id = id;
        }
    }

    /// <summary>
    /// 存放在本地的主线存档
    /// </summary>
    [System.Serializable]
    public class MainlineSaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    public static Chapter_Static_List ChapterList { get { if (_ChapterList == null) Load(); return _ChapterList; } }
    private static Chapter_Static_List _ChapterList;
    public static int currentIndex; // 当前选中的章节下标（就是上面标的下标）

    public static MainlineSaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static MainlineSaveInLocal _PlayerSave;

    public static Dictionary<string, float> MainlineFirstPassExpDict { get { if (_MainlineFirstPassExpDict == null) Load(); return _MainlineFirstPassExpDict; } }
    private static Dictionary<string, float> _MainlineFirstPassExpDict;


    /// <summary>
    /// 加载全部静态的主线关卡信息
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // 加载静态主线关卡
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                _ChapterList = result;
            }
            // 加载首通时的等级给予经验值映射表
            _MainlineFirstPassExpDict = new Dictionary<string, float>();
            ExcelManager.CSV csv = ExcelManager.ReadCSV("MainlineFirstPassExpMap", 2);
            for (int i = 0; i < csv.GetRow(); i++)
            {
                _MainlineFirstPassExpDict.Add(csv.GetValue(i, 0), PlayerManager.GetNextLevelExp(int.Parse(csv.GetValue(i, 1))));
            }
            // 从玩家存档加载关卡情况
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
    /// 从本地存档获取关卡的完成情况
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
        // 每次获取时都尝试解锁一下关卡
        if (!local_info.isUnlocked && CanUnlockStage(id))
        {
            local_info.isUnlocked = true;
            Save();
        }
        return local_info;
    }

    /// <summary>
    /// 能否解锁某个id的关卡
    /// </summary>
    /// <returns></returns>
    public static bool CanUnlockStage(string id)
    {
        int level = PlayerData.GetInstance().GetLevel();
        // 这里就只能枚举了
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
    /// 保存一次本地的存档
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<MainlineSaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// 获取当前章节信息
    /// </summary>
    /// <returns></returns>
    public static Chapter_Static GetCurrentChapter()
    {
        return ChapterList.chapterList[currentIndex];
    }

    /// <summary>
    /// 获取首通时的经验值加成,0代表查无此项
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
