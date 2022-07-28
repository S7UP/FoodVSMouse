using UnityEngine;
/// <summary>
/// ���½��ڵĵ�ͼ
/// </summary>
public class ChapterMap : BaseMap
{
    [HideInInspector]
    public int chapterIndex;
    [HideInInspector]
    public int sceneIndex;

    public static string GetSpritePath(int chapterIndex, int sceneIndex)
    {
        return "Chapter/"+ chapterIndex+"/"+sceneIndex+"/";
    }

    public string GetSpritePath()
    {
        return GetSpritePath(chapterIndex, sceneIndex);
    }

    public static ChapterMap GetInstance(int chapterIndex, int sceneIndex)
    {
        ChapterMap c = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Map/Chapter/"+chapterIndex+"/"+sceneIndex).GetComponent<ChapterMap>();
        c.chapterIndex = chapterIndex;
        c.sceneIndex = sceneIndex;
        return c;
    }
}
