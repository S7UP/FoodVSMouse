using System.Collections.Generic;
/// <summary>
/// 章节管理器（静态存储章节相关信息）
/// </summary>
public class ChapterManager
{
    private static Dictionary<ChapterNameTypeMap, string> chapterNameDict = new Dictionary<ChapterNameTypeMap, string>() {
        { ChapterNameTypeMap.MeiWei, "美味岛"},
        { ChapterNameTypeMap.HouShan, "火山岛"},
        { ChapterNameTypeMap.SkyCastle, "浮空岛"},
        { ChapterNameTypeMap.ForgottenIsland, "遗忘岛"},
    };

    private static Dictionary<ChapterNameTypeMap, Dictionary<int, string>> sceneNameDict = new Dictionary<ChapterNameTypeMap, Dictionary<int, string>>() {
        // 美味岛
        { ChapterNameTypeMap.MeiWei, new Dictionary<int, string>(){
            { 0, "曲奇岛"},
            { 1, "色拉岛（陆）"},
            { 2, "色拉岛（水）"},
            { 3, "慕斯岛"},
            { 4, "香槟岛（陆）"},
            { 5, "香槟岛（水）"},
            { 6, "神殿"},
            { 7, "布丁岛（日）"},
            { 8, "布丁岛（夜）"},
            { 9, "可可岛（日）"},
            { 10, "可可岛（夜）"},
            { 11, "咖喱岛（日）"},
            { 12, "咖喱岛（夜）"},
            { 13, "深渊"},
        }},
        // 火山岛
        { ChapterNameTypeMap.HouShan, new Dictionary<int, string>(){
            { 0, "芥末小屋（日）"},
            { 1, "芥末小屋（夜）"},
            { 2, "薄荷海滩（日）"},
            { 3, "薄荷海滩（夜）"},
            { 4, "芝士城堡"},
            { 5, "炭烧雨林（日）"},
            { 6, "炭烧雨林（夜）"},
            { 7, "抹茶庄园（日）"},
            { 8, "抹茶庄园（夜）"},
            { 9, "玛奇朵港"},
            { 10, "棉花糖天空（日）"},
            { 11, "棉花糖天空（夜）"},
            { 12, "果酱部落（日）"},
            { 13, "果酱部落（夜）"},
            { 14, "雪顶火山"},
        }},
        // 浮空岛
        { ChapterNameTypeMap.SkyCastle, new Dictionary<int, string>(){
            { 0, "茴香竹筏（日）"},
            { 1, "茴香竹筏（夜）"},
            { 2, "孜然断桥（日）"},
            { 3, "孜然断桥（夜）"},
            { 4, "卤料花园"},
            { 5, "月桂天空（日）"},
            { 6, "月桂天空（夜）"},
            { 7, "香叶空港（日）"},
            { 8, "香叶空港（夜）"},
            { 9, "香料飞船"},
            { 10, "花椒浮岛（日）"},
            { 11, "花椒浮岛（夜）"},
            { 12, "丁香彩虹（日）"},
            { 13, "丁香彩虹（夜）"},
            { 14, "十三香中心岛"},
        }},
        // 遗忘岛
        { ChapterNameTypeMap.ForgottenIsland, new Dictionary<int, string>(){
            { 0, "深渊古堡"},
            { 1, "梦魇天空"},
            { 2, "灼热炼狱"},
            { 3, "水火之间"},
            { 4, "水火之间Ex"},
            { 5, "巫毒研究所"},
            { 6, "冰封遗迹"},
            { 7, "冰封遗迹Ex"},
            { 8, "冰封遗迹（天灾）"},
        }},
    };


    /// <summary>
    /// 获取所有章节的名称表
    /// </summary>
    /// <returns></returns>
    public static List<string> GetChapterNameList()
    {
        List<string> l = new List<string>();
        foreach (var keyValuePair in chapterNameDict)
        {
            l.Add(keyValuePair.Value);
        }
        return l;
    }

    /// <summary>
    /// 获取某个章节的所有场景名称表
    /// </summary>
    /// <returns></returns>
    public static List<string> GetSceneNameList(ChapterNameTypeMap chapterIndex)
    {
        List<string> l = new List<string>();
        foreach (var keyValuePair in sceneNameDict[chapterIndex])
        {
            l.Add(keyValuePair.Value);
        }
        return l;
    }
}
