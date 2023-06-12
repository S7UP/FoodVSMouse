using System.Collections.Generic;
/// <summary>
/// 支线关卡管理者
/// </summary>
public class SpurlineManager
{
    private const string path = "Stage/Spurline/";
    private const string localPath = "Spurline";
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
    /// 存放在本地的勇士关存档
    /// </summary>
    [System.Serializable]
    public class SaveInLocal
    {
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>();
    }

    public static Chapter_Static_List ChapterList { get { if (_ChapterList == null) Load(); return _ChapterList; } }
    private static Chapter_Static_List _ChapterList;
    public static int currentIndex; // 当前选中的章节下标（就是上面标的下标）

    public static SaveInLocal PlayerSave { get { if (_PlayerSave == null) Load(); return _PlayerSave; } }
    private static SaveInLocal _PlayerSave;


    /// <summary>
    /// 加载全部静态的主线关卡信息
    /// </summary>
    private static void Load()
    {
        if (!isLoad)
        {
            // 加载静态勇士关卡
            Chapter_Static_List result;
            if (JsonManager.TryLoadFromResource(path + "ChapterList", out result))
            {
                _ChapterList = result;
            }
            // 从玩家存档加载关卡情况
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
    /// 保存一次本地的存档
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<SaveInLocal>(PlayerSave, localPath);
    }

    /// <summary>
    /// 获取当前章节信息
    /// </summary>
    /// <returns></returns>
    public static Chapter_Static GetCurrentChapter()
    {
        // 保护机制
        if (currentIndex >= ChapterList.chapterList.Count)
            currentIndex = ChapterList.chapterList.Count - 1;
        return ChapterList.chapterList[currentIndex];
    }
}
