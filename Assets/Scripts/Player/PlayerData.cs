using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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
    public int version = 0; // 当前存档版本号
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>> ChapterCardGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>>();
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>> ChapterKeyGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>>();
    public string name = "不知名的冒险家"; // 玩家名
    public float currentExp = 0; // 玩家当前经验值
    public int level = 1; // 玩家当前等级
    public int weapons = 0; // 玩家武器
    public int suit = 0; // 玩家套装种类
    public int[] jewelArray = new int[3] { -1, -1, -1 }; // 宝石（3个）
    public int difficult = 0; // 难度(0,1,2,3)
    
    // 以下是非序列化内容
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
            PlayerData data = null;
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
        return weapons;
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
}
