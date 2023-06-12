using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡信息管理者（包括处理静态关卡，玩家存档）
/// </summary>
public class StageInfoManager
{
    /// <summary>
    /// 关卡信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct StageInfo_Static
    {
        public string id; // 自身唯一id
        public int level;
        public string[] preStageArray; // 前置关卡的ID
        public string other; // 其他解锁的条件（仅文字说明）

        public string GetUnLockCondition()
        {
            string s = "解锁条件:\n*"+level+"级；";
            if(preStageArray!=null && preStageArray.Length > 0)
            {
                s += "\n*通过关卡：";
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
    /// 章节信息（静态数据）
    /// </summary>
    [System.Serializable]
    public struct Chapter_Static
    {
        public string name;
        public string bgPath;
        public List<StageInfo_Static> stageList;
    }

    /// <summary>
    /// 大章节信息（包括大量小章节， 比如主线、支线之类的）
    /// </summary>
    public class BigChapter_Static
    {
        public string id; // 大章节ID，如Mainline
        public string name; // 大章节名，如主线任务
        public List<Chapter_Static> chapterList;

        public int currentIndex; // 当前处在小章节的下标
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
        public float diffRate = 1.0f; // 通过的最高难度比率（仅在通过rank=3后解锁）
        public List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // 当前携带的卡片组（选卡场景与关卡场景对接）
        public List<char> currentCardKeyList = new List<char>(); // 当前键位控制表
        public List<string> SelectedTagList = new List<string>(); // 已选词条
    }

    /// <summary>
    /// 玩家的关卡存档信息
    /// </summary>
    [System.Serializable]
    public class PlayerStageInfo
    {
        public float version = 0; // 存档版本
        public Dictionary<string, StageInfo_Local> localStageInfoDict = new Dictionary<string, StageInfo_Local>(); // 关卡ID-关卡完成信息映射表
    }


    private const float version = 1.1f; // 最新版本号（若存档版本号低于该版本号，则对存档进行一次更新）
    private static Dictionary<string, BigChapter_Static> bigChapterDict = new Dictionary<string, BigChapter_Static>(); // 大章节ID映射表
    private static Dictionary<string, StageInfo_Static> staticInfoDict = new Dictionary<string, StageInfo_Static>(); // 关卡ID-关卡静态信息映射表
    private static Dictionary<string, BaseStage.StageInfo> detailInfoDict = new Dictionary<string, BaseStage.StageInfo>(); // 关卡ID-关卡详细信息映射表
    private static Dictionary<string, Action> firstPassRewardActionDict = new Dictionary<string, Action>(); // 关卡ID-首通奖励映射表
    public static PlayerStageInfo mPlayerStageInfo;
    private static bool isLoad = false;

    public static void Load()
    {
        if (!isLoad)
        {
            // 加载所有预设关卡
            bigChapterDict.Add("Mainline", new BigChapter_Static() { id = "Mainline", name="主线剧情", chapterList = LoadChapterList("Stage/Mainline/") } );
            bigChapterDict.Add("Spurline", new BigChapter_Static() { id = "Spurline", name = "支线剧情", chapterList = LoadChapterList("Stage/Spurline/") });
            bigChapterDict.Add("WarriorChallenge", new BigChapter_Static() { id = "WarriorChallenge", name = "勇士挑战", chapterList = LoadChapterList("Stage/WarriorChallenge/") });
            bigChapterDict.Add("MagicTower", new BigChapter_Static() { id = "MagicTower", name = "魔塔蛋糕", chapterList = LoadChapterList("Stage/MagicTower/") });
            bigChapterDict.Add("LegendChallenge", new BigChapter_Static() { id = "LegendChallenge", name = "传说挑战", chapterList = LoadChapterList("Stage/LegendChallenge/") });
            // 加载首通奖励
            LoadMainlineFirstPassReward();
            // 加载玩家存档信息
            {
                if (!JsonManager.TryLoadFromLocal("PlayerStageInfo", out mPlayerStageInfo))
                {
                    mPlayerStageInfo = new PlayerStageInfo();
                    Save();
                }
                // 检测版本号来作一些更新
                if(mPlayerStageInfo.version < 1.1)
                {
                    // 在1.1版本中，需要迁移主线、支线、勇士的存档信息至该mPlayerStageInfo
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

            }
            isLoad = true;
        }
    }

    #region 供外界调用的方法
    /// <summary>
    /// 从本地存档获取关卡的完成情况
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
        // 每次获取时都尝试解锁一下关卡
        if (!local_info.isUnlocked && CanUnlockStage(id))
        {
            local_info.isUnlocked = true;
            Save();
        }
        return local_info;
    }

    /// <summary>
    /// 获取静态关卡数据（包括解锁条件之类的）
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
    /// 根据关卡来获取详细的关卡信息（包括出怪等参数）
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
    /// 获取大章节信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static BigChapter_Static GetBigChapter(string id)
    {
        if (bigChapterDict.ContainsKey(id))
            return bigChapterDict[id];
        else
            return null;
    }

    /// <summary>
    /// 获取首通奖励事件
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
    /// 保存一次本地的存档
    /// </summary>
    public static void Save()
    {
        JsonManager.SaveOnLocal<PlayerStageInfo>(mPlayerStageInfo, "PlayerStageInfo");
    }
    #endregion

    #region 私有方法，主要是加载东西的
    /// <summary>
    /// 加载章节组（主线、支线、勇士、魔塔等）
    /// </summary>
    /// <param name="resPath"></param>
    private static List<Chapter_Static> LoadChapterList(string resPath)
    {
        List<Chapter_Static> chapterList = new List<Chapter_Static>();
        ExcelManager.CSV csv = ExcelManager.ReadCSV(resPath+"ChapterList", 3);
        for (int i = 0; i < csv.GetRow(); i++)
        {
            Chapter_Static chapter = new Chapter_Static();
            string id = csv.GetValue(i, 0);// 章节ID
            string name = csv.GetValue(i, 1);
            string bgPath = csv.GetValue(i, 2);

            chapter.name = name;
            chapter.bgPath = bgPath;

            List<StageInfo_Static> stageInfoList = new List<StageInfo_Static>();
            ExcelManager.CSV c2 = ExcelManager.ReadCSV(resPath + id, 4);
            for (int j = 0; j < c2.GetRow(); j++)
            {
                string[] preStage;
                string s = c2.GetValue(j, 2);
                if (s.Equals("无"))
                    preStage = null;
                else
                    preStage = s.Split('/');
                int level = 0;
                int.TryParse(c2.GetValue(j, 1), out level);

                StageInfo_Static s_info = new StageInfo_Static() {
                    id = c2.GetValue(j, 0),
                    level = level,
                    preStageArray = preStage,
                    other = (c2.GetValue(j, 3).Equals("无")?null: c2.GetValue(j, 3))
                };
                BaseStage.StageInfo res;
                // 加入字典
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
    /// 加载主线首通奖励
    /// </summary>
    private static void LoadMainlineFirstPassReward()
    {
        ExcelManager.CSV csv = ExcelManager.ReadCSV("MainlineFirstPassExpMap", 2);
        for (int i = 0; i < csv.GetRow(); i++)
        {
            firstPassRewardActionDict.Add(csv.GetValue(i, 0),
                delegate {
                    float exp = PlayerManager.GetNextLevelExp(int.Parse(csv.GetValue(i, 1)));
                    GameNormalPanel panel = GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel] as GameNormalPanel;
                    if (panel != null)
                        panel.SetExpTips("恭喜你第一次通过本关！奖励" + exp + "点经验值！");
                    PlayerData.GetInstance().AddExp(exp);
                });
        }
    }

    /// <summary>
    /// 能否解锁某个id的关卡
    /// </summary>
    /// <returns></returns>
    private static bool CanUnlockStage(string id)
    {
        int level = PlayerData.GetInstance().GetLevel();
        StageInfo_Static info_static = GetStaticStageInfo(id);
        if (info_static.id == null)
            return false;
        // 先判断等级
        if (level < info_static.level)
            return false;
        // 再判断前置关卡
        if(info_static.preStageArray != null)
        {
            foreach (var p_id in info_static.preStageArray)
            {
                if (!IsPass(p_id))
                    return false;
            }
        }
        // TODO 判断其他条件
        return true;

        // 这里就只能枚举了
        //switch (id)
        //{
        //    case "1-1": return level >= 1;
        //    case "1-2": return level >= 2;
        //    case "1-3": return level >= 3;
        //    case "1-4": return level >= 4;
        //    case "1-5": return level >= 5;
        //    case "1-6": return level >= 6;
        //    case "1-7": return level >= 7;
        //    case "2-1": return level >= 8;
        //    case "2-2": return level >= 9;
        //    case "2-3": return level >= 10;
        //    case "2-4": return level >= 11;
        //    case "2-5": return level >= 12;
        //    case "2-6": return level >= 13;
        //    case "2-7": return level >= 14;
        //    case "3-1": return level >= 15;
        //    case "3-2": return level >= 16;
        //    case "3-3": return level >= 17;
        //    case "3-4": return level >= 18;
        //    case "3-5": return level >= 19;
        //    case "4-1": return level >= 20;
        //    case "4-2": return level >= 21;
        //    case "4-3": return level >= 22;
        //    case "4-4": return level >= 23;
        //    case "4-5": return level >= 24;
        //    case "5-1": return level >= 25;
        //    case "5-2": return level >= 26;
        //    case "5-3": return level >= 27;
        //    case "5-4": return level >= 28;
        //    case "5-5": return level >= 29;
        //    default:
        //        break;
        //}
        //return false;
    }

    /// <summary>
    /// 仅查询某关通过情况
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
    #endregion
}
