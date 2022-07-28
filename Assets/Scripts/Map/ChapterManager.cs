using System.Collections.Generic;
/// <summary>
/// �½ڹ���������̬�洢�½������Ϣ��
/// </summary>
public class ChapterManager
{
    private static Dictionary<ChapterNameTypeMap, string> chapterNameDict = new Dictionary<ChapterNameTypeMap, string>() {
        { ChapterNameTypeMap.MeiWei, "��ζ��"},
        { ChapterNameTypeMap.HouShan, "��ɽ��"},
        { ChapterNameTypeMap.SkyCastle, "���յ�"},
        { ChapterNameTypeMap.ForgottenIsland, "������"},
    };

    private static Dictionary<ChapterNameTypeMap, Dictionary<int, string>> sceneNameDict = new Dictionary<ChapterNameTypeMap, Dictionary<int, string>>() {
        // ��ζ��
        { ChapterNameTypeMap.MeiWei, new Dictionary<int, string>(){
            { 0, "���浺"},
            { 1, "ɫ������½��"},
            { 2, "ɫ������ˮ��"},
            { 3, "Ľ˹��"},
            { 4, "���ĵ���½��"},
            { 5, "���ĵ���ˮ��"},
            { 6, "���"},
            { 7, "���������գ�"},
            { 8, "��������ҹ��"},
            { 9, "�ɿɵ����գ�"},
            { 10, "�ɿɵ���ҹ��"},
            { 11, "��ଵ����գ�"},
            { 12, "��ଵ���ҹ��"},
            { 13, "��Ԩ"},
        }},
        // ��ɽ��
        { ChapterNameTypeMap.HouShan, new Dictionary<int, string>(){
            { 0, "��ĩС�ݣ��գ�"},
            { 1, "��ĩС�ݣ�ҹ��"},
            { 2, "���ɺ�̲���գ�"},
            { 3, "���ɺ�̲��ҹ��"},
            { 4, "֥ʿ�Ǳ�"},
            { 5, "̿�����֣��գ�"},
            { 6, "̿�����֣�ҹ��"},
            { 7, "Ĩ��ׯ԰���գ�"},
            { 8, "Ĩ��ׯ԰��ҹ��"},
            { 9, "������"},
            { 10, "�޻�����գ��գ�"},
            { 11, "�޻�����գ�ҹ��"},
            { 12, "�������䣨�գ�"},
            { 13, "�������䣨ҹ��"},
            { 14, "ѩ����ɽ"},
        }},
        // ���յ�
        { ChapterNameTypeMap.SkyCastle, new Dictionary<int, string>(){
            { 0, "�����񷤣��գ�"},
            { 1, "�����񷤣�ҹ��"},
            { 2, "��Ȼ���ţ��գ�"},
            { 3, "��Ȼ���ţ�ҹ��"},
            { 4, "±�ϻ�԰"},
            { 5, "�¹���գ��գ�"},
            { 6, "�¹���գ�ҹ��"},
            { 7, "��Ҷ�ոۣ��գ�"},
            { 8, "��Ҷ�ոۣ�ҹ��"},
            { 9, "���Ϸɴ�"},
            { 10, "�����������գ�"},
            { 11, "����������ҹ��"},
            { 12, "����ʺ磨�գ�"},
            { 13, "����ʺ磨ҹ��"},
            { 14, "ʮ�������ĵ�"},
        }},
        // ������
        { ChapterNameTypeMap.ForgottenIsland, new Dictionary<int, string>(){
            { 0, "��Ԩ�ű�"},
            { 1, "�������"},
            { 2, "��������"},
            { 3, "ˮ��֮��"},
            { 4, "ˮ��֮��Ex"},
            { 5, "�׶��о���"},
            { 6, "�����ż�"},
            { 7, "�����ż�Ex"},
            { 8, "�����ż������֣�"},
        }},
    };


    /// <summary>
    /// ��ȡ�����½ڵ����Ʊ�
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
    /// ��ȡĳ���½ڵ����г������Ʊ�
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
