using System.Collections.Generic;
using System.IO;

using UnityEngine;
[System.Serializable]
/// <summary>
/// ��Ҵ浵��Ϣ
/// </summary>
public class PlayerData
{
    public static string path = "PlayerData";
    public static string filepath = Application.streamingAssetsPath + "/Json/" + path + ".json";

    /// <summary>
    /// �������Я����Ƭ����Ϣ
    /// </summary>
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>> ChapterCardGroupDict;
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>> ChapterKeyGroupDict;

    // �����Ƿ����л�����
    private BaseStage.ChapterStageValue currentChapterStageValue;
    private List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // ��ǰЯ���Ŀ�Ƭ�飨ѡ��������ؿ������Խӣ�
    private List<char> currentCardKeyList = new List<char>(); // ��ǰ��λ���Ʊ�
    private WeaponsInfo currentWeaponsInfo;
    private CharacterInfo currentCharacterInfo;

    /// <summary>
    /// �ӱ��ؼ�����Ҵ浵����
    /// </summary>
    public static PlayerData LoadPlayerData()
    {
        if (!File.Exists(filepath))
        {
            // ��������ڴ浵�����½�һ���浵
            JsonManager.Save<PlayerData>(new PlayerData(), path);
        }
        // Ȼ���ȡ�浵
        PlayerData data = JsonManager.Load<PlayerData>(path);
        // ��Ⲣ�޸��浵��Ϣ��������
        CheckAndFixPlayerData(data);
        return data;
    }

    /// <summary>
    /// ��Ⲣ�޸��浵��Ϣ��������
    /// </summary>
    private static void CheckAndFixPlayerData(PlayerData data)
    {
        if (data.ChapterCardGroupDict == null)
            data.ChapterCardGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>>();
        if (data.ChapterKeyGroupDict == null)
            data.ChapterKeyGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>>();
        // ת��
        data.Save();
    }

    /// <summary>
    /// ����ĳ�صĿ����
    /// </summary>
    /// <param name="chapter">�½ں�</param>
    /// <param name="scene">������</param>
    /// <param name="stage">�ؿ���</param>
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
    /// ����ĳ�صļ�λ���ñ�
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
    /// ����ĳ�صĿ���
    /// </summary>
    /// <param name="chapter">�½ں�</param>
    /// <param name="scene">������</param>
    /// <param name="stage">�ؿ���</param>
    /// <param name="cardGroupList">����</param>
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
    /// ����ĳ�صļ�λ���ñ�
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
    /// �����Լ�
    /// </summary>
    public void Save()
    {
        JsonManager.Save<PlayerData>(this, path);
    }

    /// <summary>
    /// ��ȡ��ǰѡ��Ŀ�Ƭ��Ϣ��
    /// </summary>
    /// <returns></returns>
    public List<AvailableCardInfo> GetCurrentSelectedCardInfoList()
    {
        return currentSelectedCardInfoList;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ���Ʊ�
    /// </summary>
    /// <returns></returns>
    public List<char> GetCurrentCardKeyList()
    {
        return currentCardKeyList;
    }

    /// <summary>
    /// ���õ�ǰѡ��Ŀ�Ƭ��Ϣ��
    /// </summary>
    /// <param name="list"></param>
    public void SetCurrentSelectedCardInfoList(List<AvailableCardInfo> list)
    {
        currentSelectedCardInfoList = list;
    }

    /// <summary>
    /// ���õ�ǰ��λ���Ʊ�
    /// </summary>
    /// <param name="list"></param>
    public void SetCurrentCardKeyList(List<char> list)
    {
        currentCardKeyList = list;
    }

    /// <summary>
    /// ���õ�ǰ���ڵĹؿ����ֵ
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <param name="stageIndex"></param>
    public void SetCurrentChapterStageValue(int chapterIndex, int sceneIndex, int stageIndex)
    {
        currentChapterStageValue = new BaseStage.ChapterStageValue() { chapterIndex = chapterIndex, sceneIndex = sceneIndex, stageIndex = stageIndex };
    }

    /// <summary>
    /// ��ȡ��ǰ�ؿ����ֵ
    /// </summary>
    /// <returns></returns>
    public BaseStage.ChapterStageValue GetCurrentChapterStageValue()
    {
        return currentChapterStageValue;
    }

    public void SetWeaponsInfo(WeaponsInfo info)
    {
        currentWeaponsInfo = info;
    }

    public WeaponsInfo GetWeaponsInfo()
    {
        return currentWeaponsInfo;
    }

    public void SetCharacterInfo(CharacterInfo info)
    {
        currentCharacterInfo = info;
    }

    public CharacterInfo GetCharacterInfo()
    {
        return currentCharacterInfo;
    }
}
