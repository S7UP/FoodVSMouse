using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
/// <summary>
/// 玩家存档信息
/// </summary>
public class PlayerData
{
    public static string path = "PlayerData";
    private static PlayerData _Instance; // 玩家存档实例

    /// <summary>
    /// 主线玩家携带卡片组信息
    /// </summary>
    public int version = 1; // 当前存档版本号
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>> ChapterCardGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>>();
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>> ChapterKeyGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>>();
    public Dictionary<int, Dictionary<int, Dictionary<int, List<string>>>> SelectedTagListDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<string>>>>(); // 已选词条

    public string name = "不知名的冒险家"; // 玩家名
    public float currentExp = 0; // 玩家当前经验值
    public int level = 1; // 玩家当前等级
    public int weapons = 5; // 玩家武器
    public int suit = 0; // 玩家套装种类
    public int[] jewelArray = new int[3] { -1, -1, -1 }; // 宝石（3个）
    public int difficult = 0; // 难度(0,1,2,3)

    // 以下是非序列化内容
    private string currentStageId; // 当前关卡ID（如果是从正常的界面进入则会有这个ID记录，否则为null）
    private BaseStage.StageInfo currentStageInfo;
    private List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // 当前携带的卡片组（选卡场景与关卡场景对接）
    private List<char> currentCardKeyList = new List<char>(); // 当前键位控制表



    /// <summary>
    /// 从本地加载玩家存档数据
    /// </summary>
    public static PlayerData GetInstance()
    {
        if(_Instance == null)
        {
            PlayerData data;
            if (!JsonManager.TryLoadFromLocal(path, out data))
            {
                // 如果不存在存档，则新建一个存档
                data = new PlayerData();
                data.Save();
            }
            // 检测并修复存档信息的完整性
            CheckAndFixPlayerData(data);
            _Instance = data;
        }
        return _Instance;
    }

    /// <summary>
    /// 检测并修复存档信息的完整性
    /// </summary>
    private static void CheckAndFixPlayerData(PlayerData data)
    {
        if(data.version < PlayerManager.version)
        {
            data.TryLevelUp();
            data.version = PlayerManager.version;
            // 转存
            data.Save();
        }
    }

    /// <summary>
    /// 载入某关的卡组表
    /// </summary>
    /// <param name="chapter">章节号</param>
    /// <param name="scene">场景号</param>
    /// <param name="stage">关卡号</param>
    public List<List<AvailableCardInfo>> LoadCardGroupList(int chapter, int scene, int stage)
    {
        if (!ChapterCardGroupDict.ContainsKey(chapter))
            ChapterCardGroupDict.Add(chapter, new Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>());
        if (!ChapterCardGroupDict[chapter].ContainsKey(scene))
            ChapterCardGroupDict[chapter].Add(scene, new Dictionary<int, List<List<AvailableCardInfo>>>());
        if (!ChapterCardGroupDict[chapter][scene].ContainsKey(stage))
            ChapterCardGroupDict[chapter][scene].Add(stage, new List<List<AvailableCardInfo>>());
        return ChapterCardGroupDict[chapter][scene][stage];
    }

    /// <summary>
    /// 载入某关的键位设置表
    /// </summary>
    /// <param name="chapter"></param>
    /// <param name="scene"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public List<List<char>> LoadKeyGroupList(int chapter, int scene, int stage)
    {
        if (!ChapterKeyGroupDict.ContainsKey(chapter))
            ChapterKeyGroupDict.Add(chapter, new Dictionary<int, Dictionary<int, List<List<char>>>>());
        if (!ChapterKeyGroupDict[chapter].ContainsKey(scene))
            ChapterKeyGroupDict[chapter].Add(scene, new Dictionary<int, List<List<char>>>());
        if (!ChapterKeyGroupDict[chapter][scene].ContainsKey(stage))
            ChapterKeyGroupDict[chapter][scene].Add(stage, new List<List<char>>());
        return ChapterKeyGroupDict[chapter][scene][stage];
    }

    /// <summary>
    /// 保存某关的卡组
    /// </summary>
    /// <param name="chapter">章节号</param>
    /// <param name="scene">场景号</param>
    /// <param name="stage">关卡号</param>
    /// <param name="cardGroupList">卡组</param>
    public void SaveCardGroupList(int chapter, int scene, int stage, List<List<AvailableCardInfo>> cardGroupList)
    {
        if (!ChapterCardGroupDict.ContainsKey(chapter))
            ChapterCardGroupDict.Add(chapter, new Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>());
        if (!ChapterCardGroupDict[chapter].ContainsKey(scene))
            ChapterCardGroupDict[chapter].Add(scene, new Dictionary<int, List<List<AvailableCardInfo>>>());
        if (!ChapterCardGroupDict[chapter][scene].ContainsKey(stage))
            ChapterCardGroupDict[chapter][scene].Add(stage, cardGroupList);
        else
            ChapterCardGroupDict[chapter][scene][stage] = cardGroupList;
        Save();
    }

    /// <summary>
    /// 保存某关的键位设置表
    /// </summary>
    /// <param name="chapter"></param>
    /// <param name="scene"></param>
    /// <param name="stage"></param>
    /// <param name="cardGroupList"></param>
    public void SaveKeyGroupList(int chapter, int scene, int stage, List<List<char>> keyGroupList)
    {
        if (!ChapterKeyGroupDict.ContainsKey(chapter))
            ChapterKeyGroupDict.Add(chapter, new Dictionary<int, Dictionary<int, List<List<char>>>>());
        if (!ChapterKeyGroupDict[chapter].ContainsKey(scene))
            ChapterKeyGroupDict[chapter].Add(scene, new Dictionary<int, List<List<char>>>());
        if (!ChapterKeyGroupDict[chapter][scene].ContainsKey(stage))
            ChapterKeyGroupDict[chapter][scene].Add(stage, keyGroupList);
        else
            ChapterKeyGroupDict[chapter][scene][stage] = keyGroupList;
        Save();
    }

    /// <summary>
    /// 保存自己
    /// </summary>
    public void Save()
    {
        JsonManager.SaveOnLocal(this, path);
    }

    /// <summary>
    /// 获取当前选择的卡片信息表
    /// </summary>
    /// <returns></returns>
    public List<AvailableCardInfo> GetCurrentSelectedCardInfoList()
    {
        return currentSelectedCardInfoList;
    }

    /// <summary>
    /// 获取当前键位控制表
    /// </summary>
    /// <returns></returns>
    public List<char> GetCurrentCardKeyList()
    {
        return currentCardKeyList;
    }

    /// <summary>
    /// 设置当前选择的卡片信息表
    /// </summary>
    /// <param name="list"></param>
    public void SetCurrentSelectedCardInfoList(List<AvailableCardInfo> list)
    {
        currentSelectedCardInfoList = list;
    }

    /// <summary>
    /// 设置当前键位控制表
    /// </summary>
    /// <param name="list"></param>
    public void SetCurrentCardKeyList(List<char> list)
    {
        currentCardKeyList = list;
    }

    /// <summary>
    /// 设置当前关卡
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <param name="stageIndex"></param>
    public void SetCurrentStageInfo(int chapterIndex, int sceneIndex, int stageIndex)
    {
        currentStageInfo = BaseStage.Load(chapterIndex, sceneIndex, stageIndex);
    }

    public void SetCurrentStageInfo(BaseStage.StageInfo stageInfo)
    {
        currentStageInfo = stageInfo;
    }

    /// <summary>
    /// 获取当前关卡信息
    /// </summary>
    /// <returns></returns>
    public BaseStage.StageInfo GetCurrentStageInfo()
    {
        if (currentStageInfo == null)
            SetCurrentStageInfo(0, 0, 0);
        return currentStageInfo;
    }

    /// <summary>
    /// 设置当前使用武器的编号
    /// </summary>
    /// <param name="type"></param>
    public void SetWeapons(int type)
    {
        weapons = type;
    }

    /// <summary>
    /// 获取当前使用的武器的编号
    /// </summary>
    /// <returns></returns>
    public int GetWeapons()
    {
        if (WeaponsManager.IsWeaponsInValid(weapons))
            return weapons;
        // 非法武器编号只会返回笼包枪
        return (int)WeaponsNameTypeMap.BunGun;
    }

    /// <summary>
    /// 获取当前使用的套装编号
    /// </summary>
    /// <returns></returns>
    public int GetCharacter()
    {
        return suit;
    }

    /// <summary>
    /// 获取当前游戏难度
    /// </summary>
    /// <returns></returns>
    public int GetDifficult()
    {
        return difficult;
    }

    /// <summary>
    /// 设置当前游戏难度
    /// </summary>
    public void SetDifficult(int diff)
    {
        difficult = diff;
    }

    /// <summary>
    /// 获取当前级从0开始升到下一级所需经验值
    /// </summary>
    public float GetNextLevelExp()
    {
        return PlayerManager.GetNextLevelExp(level);
    }

    /// <summary>
    /// 增加经验值
    /// </summary>
    public void AddExp(float value)
    {
        if (value <= 0)
            return;
        currentExp += value;
        TryLevelUp();
        Save();
        // 如果当前场景中有玩家面板，则需要更新一下信息
        if (GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict.ContainsKey(StringManager.PlayerInfoPanel))
        {
            PlayerInfoPanel panel = GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.PlayerInfoPanel] as PlayerInfoPanel;
            panel.UpdatePlayerInfo();
        }
    }

    /// <summary>
    /// 在每次经验值增加后调用，尝试升级
    /// </summary>
    private bool TryLevelUp()
    {
        bool isLevelUp = false;
        float maxExp = GetNextLevelExp();
        while (maxExp > 0 && currentExp >= maxExp && level < PlayerManager.maxLevel)
        {
            level++;
            currentExp -= maxExp;
            maxExp = GetNextLevelExp();
            isLevelUp = true;
        }
        return isLevelUp;
    }

    /// <summary>
    /// 获取当前等级
    /// </summary>
    /// <returns></returns>
    public int GetLevel()
    {
        return Mathf.Min(level, PlayerManager.maxLevel);
    }

    /// <summary>
    /// 是否定满级
    /// </summary>
    /// <returns></returns>
    public bool IsMaxLevel()
    {
        return level >= PlayerManager.maxLevel;
    }

    public int GetJewel(int index)
    {
        // 检查这个槽有没有解锁，没有解锁都是-1
        bool isDeveloperMode = ConfigManager.IsDeveloperMode();
        string id = "Jewel" + (index + 1);
        if (!(isDeveloperMode || OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            return -1;
        int i = jewelArray[index];
        if (!JewelManager.IsJewelValid(i))
            return -1;
        return jewelArray[index];
    }

    public void SetJewel(int index, int value)
    {
        jewelArray[index] = value;
        Save();
    }

    /// <summary>
    /// 获取某关的已选Tag表
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <param name="stageIndex"></param>
    /// <returns></returns>
    public List<string> GetTagList(int chapterIndex, int sceneIndex, int stageIndex)
    {
        if (!SelectedTagListDict.ContainsKey(chapterIndex))
            SelectedTagListDict.Add(chapterIndex, new Dictionary<int, Dictionary<int, List<string>>>());
        if (!SelectedTagListDict[chapterIndex].ContainsKey(sceneIndex))
            SelectedTagListDict[chapterIndex].Add(sceneIndex, new Dictionary<int, List<string>>());
        if (!SelectedTagListDict[chapterIndex][sceneIndex].ContainsKey(stageIndex))
            SelectedTagListDict[chapterIndex][sceneIndex].Add(stageIndex, new List<string>());
        return SelectedTagListDict[chapterIndex][sceneIndex][stageIndex];
    }

    /// <summary>
    /// 获取当前难度比率
    /// </summary>
    /// <returns></returns>
    public float GetRankRate()
    {
        float rankRate = 1f;
        BaseStage.StageInfo info = GetCurrentStageInfo();
        foreach (var tagId in GetTagList(info.chapterIndex, info.sceneIndex, info.stageIndex))
        {
            TagInfo tagInfo = TagsManager.GetGeneralTagInfo(tagId);
            switch (tagInfo.rank)
            {
                default: rankRate *= 1;
                    break;
                case 1: rankRate *= 1.25f;break;
                case 2: rankRate *= 1.5f; break;
                case 3: rankRate *= 2f; break;
            }
        }
        return rankRate;
    }

    /// <summary>
    /// 获取当前关卡的ID
    /// </summary>
    /// <returns></returns>
    public string GetCurrentStageID()
    {
        return currentStageId;
    }

    /// <summary>
    /// 设置当前关卡ID
    /// </summary>
    /// <param name="id"></param>
    public void SetCurrentStageID(string id)
    {
        currentStageId = id;
    }
}
