using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using UnityEngine;
[System.Serializable]
/// <summary>
/// ��Ҵ浵��Ϣ
/// </summary>
public class PlayerData
{
    public static string path = "PlayerData";
    private static PlayerData _Instance; // ��Ҵ浵ʵ��

    /// <summary>
    /// �������Я����Ƭ����Ϣ
    /// </summary>
    public int version = 0; // ��ǰ�浵�汾��
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>> ChapterCardGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<AvailableCardInfo>>>>>();
    public Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>> ChapterKeyGroupDict = new Dictionary<int, Dictionary<int, Dictionary<int, List<List<char>>>>>();
    public string name = "��֪����ð�ռ�"; // �����
    public float currentExp = 0; // ��ҵ�ǰ����ֵ
    public int level = 1; // ��ҵ�ǰ�ȼ�
    public int weapons = 0; // �������
    public int suit = 0; // �����װ����
    public int[] jewelArray = new int[3] { -1, -1, -1 }; // ��ʯ��3����
    public int difficult = 0; // �Ѷ�(0,1,2,3)
    
    // �����Ƿ����л�����
    private BaseStage.StageInfo currentStageInfo;
    private List<AvailableCardInfo> currentSelectedCardInfoList = new List<AvailableCardInfo>(); // ��ǰЯ���Ŀ�Ƭ�飨ѡ��������ؿ������Խӣ�
    private List<char> currentCardKeyList = new List<char>(); // ��ǰ��λ���Ʊ�


    /// <summary>
    /// �ӱ��ؼ�����Ҵ浵����
    /// </summary>
    public static PlayerData GetInstance()
    {
        if(_Instance == null)
        {
            PlayerData data = null;
            if (!JsonManager.TryLoadFromLocal(path, out data))
            {
                // ��������ڴ浵�����½�һ���浵
                data = new PlayerData();
                data.Save();
            }
            // ��Ⲣ�޸��浵��Ϣ��������
            CheckAndFixPlayerData(data);
            _Instance = data;
        }
        return _Instance;
    }

    /// <summary>
    /// ��Ⲣ�޸��浵��Ϣ��������
    /// </summary>
    private static void CheckAndFixPlayerData(PlayerData data)
    {
        if(data.version < PlayerManager.version)
        {
            data.version = PlayerManager.version;
            // ת��
            data.Save();
        }
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
        JsonManager.SaveOnLocal(this, path);
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
    /// ���õ�ǰ�ؿ�
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
    /// ��ȡ��ǰ�ؿ���Ϣ
    /// </summary>
    /// <returns></returns>
    public BaseStage.StageInfo GetCurrentStageInfo()
    {
        if (currentStageInfo == null)
            SetCurrentStageInfo(0, 0, 0);
        return currentStageInfo;
    }

    /// <summary>
    /// ���õ�ǰʹ�������ı��
    /// </summary>
    /// <param name="type"></param>
    public void SetWeapons(int type)
    {
        weapons = type;
    }

    /// <summary>
    /// ��ȡ��ǰʹ�õ������ı��
    /// </summary>
    /// <returns></returns>
    public int GetWeapons()
    {
        return weapons;
    }

    /// <summary>
    /// ��ȡ��ǰʹ�õ���װ���
    /// </summary>
    /// <returns></returns>
    public int GetCharacter()
    {
        return suit;
    }

    /// <summary>
    /// ��ȡ��ǰ��Ϸ�Ѷ�
    /// </summary>
    /// <returns></returns>
    public int GetDifficult()
    {
        return difficult;
    }

    /// <summary>
    /// ���õ�ǰ��Ϸ�Ѷ�
    /// </summary>
    public void SetDifficult(int diff)
    {
        difficult = diff;
    }
}
