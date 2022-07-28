using System.Collections.Generic;
using System.IO;

using UnityEngine;
[System.Serializable]
/// <summary>
/// 玩家存档信息
/// </summary>
public class PlayerData
{
    public static string path = "PlayerData";
    public static string filepath = Application.streamingAssetsPath + "/Json/" + path + ".json";

    /// <summary>
    /// 主线玩家携带卡片组信息
    /// </summary>
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>> ChapterCardGroupDict;


    // 以下是非序列化内容
    private BaseStage.ChapterStageValue currentChapterStageValue;
    private List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // 当前携带的卡片组（选卡场景与关卡场景对接）

    /// <summary>
    /// 从本地加载玩家存档数据
    /// </summary>
    public static PlayerData LoadPlayerData()
    {
        if (!File.Exists(filepath))
        {
            // 如果不存在存档，则新建一个存档
            JsonManager.Save<PlayerData>(new PlayerData(), path);
        }
        // 然后读取存档
        PlayerData data = JsonManager.Load<PlayerData>(path);
        // 检测并修复存档信息的完整性
        CheckAndFixPlayerData(data);
        return data;
    }

    /// <summary>
    /// 检测并修复存档信息的完整性
    /// </summary>
    private static void CheckAndFixPlayerData(PlayerData data)
    {
        if (data.ChapterCardGroupDict == null)
            data.ChapterCardGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>>();
        // 转存
        data.Save();
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
    /// 保存自己
    /// </summary>
    public void Save()
    {
        JsonManager.Save<PlayerData>(this, path);
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
    /// 设置当前选择的卡片信息表
    /// </summary>
    /// <param name="list"></param>
    public void SetCurrentSelectedCardInfoList(List<AvailableCardInfo> list)
    {
        currentSelectedCardInfoList = list;
    }

    /// <summary>
    /// 设置当前所在的关卡相关值
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <param name="stageIndex"></param>
    public void SetCurrentChapterStageValue(int chapterIndex, int sceneIndex, int stageIndex)
    {
        currentChapterStageValue = new BaseStage.ChapterStageValue() { chapterIndex = chapterIndex, sceneIndex = sceneIndex, stageIndex = stageIndex };
    }

    /// <summary>
    /// 获取当前关卡相关值
    /// </summary>
    /// <returns></returns>
    public BaseStage.ChapterStageValue GetCurrentChapterStageValue()
    {
        return currentChapterStageValue;
    }
}
