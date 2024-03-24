using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static BaseStage;

/// <summary>
/// ���Թ����������һЩ��̬���ԣ�
/// </summary>
public class AttributeManager
{
    public Dictionary<int, Dictionary<int, FoodUnit.Attribute>> foodAttributeDict = new Dictionary<int, Dictionary<int, FoodUnit.Attribute>>();
    public Dictionary<int, Dictionary<int, MouseUnit.Attribute>> mouseAttributeDict = new Dictionary<int, Dictionary<int, MouseUnit.Attribute>>();
    public Dictionary<int, Dictionary<int, MouseUnit.Attribute>> bossAttributeDict = new Dictionary<int, Dictionary<int, MouseUnit.Attribute>>();
    public Dictionary<int, Dictionary<int, BaseUnit.Attribute>> itemAttributeDict = new Dictionary<int, Dictionary<int, BaseUnit.Attribute>>();
    public Dictionary<int, Dictionary<int, BaseUnit.Attribute>> characterAttributeDict = new Dictionary<int, Dictionary<int, BaseUnit.Attribute>>();
    public Dictionary<int, Dictionary<int, BaseCardBuilder.Attribute>> cardBuilderAttributeDict = new Dictionary<int, Dictionary<int, BaseCardBuilder.Attribute>>();
    public Dictionary<int, Dictionary<int, Dictionary<int, BaseStage.StageInfo>>> stageInfoDict = new Dictionary<int, Dictionary<int, Dictionary<int, BaseStage.StageInfo>>>();

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Initial()
    {
        foodAttributeDict.Clear();
        mouseAttributeDict.Clear();
        bossAttributeDict.Clear();
        itemAttributeDict.Clear();
        characterAttributeDict.Clear();
        cardBuilderAttributeDict.Clear();
        stageInfoDict.Clear();
    }

    /// <summary>
    /// ��ȡ�ض���ʳ��λ������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public FoodUnit.Attribute GetFoodUnitAttribute(int type, int shape)
    {
        FoodUnit.Attribute attr;
        if (!foodAttributeDict.ContainsKey(type))
            foodAttributeDict.Add(type, new Dictionary<int, FoodUnit.Attribute>());
        if (!foodAttributeDict[type].ContainsKey(shape) && JsonManager.TryLoadFromResource("Food/" + type + "/" + shape + "", out attr))
            //foodAttributeDict[type].Add(shape, JsonManager.Load<FoodUnit.Attribute>("Food/" + type + "/" + shape + ""));
            foodAttributeDict[type].Add(shape, attr);
        return foodAttributeDict[type][shape];
    }

    /// <summary>
    /// ��ȡ�ض�BOSS��λ������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public MouseUnit.Attribute GetBossUnitAttribute(int type, int shape)
    {
        MouseUnit.Attribute attr;
        if (!bossAttributeDict.ContainsKey(type))
            bossAttributeDict.Add(type, new Dictionary<int, MouseUnit.Attribute>());
        if (!bossAttributeDict[type].ContainsKey(shape) && JsonManager.TryLoadFromResource("Boss/" + type + "/" + shape + "", out attr))
            //bossAttributeDict[type].Add(shape, JsonManager.Load<MouseUnit.Attribute>("Boss/" + type + "/" + shape + ""));
            bossAttributeDict[type].Add(shape, attr);
        return bossAttributeDict[type][shape];
    }

    /// <summary>
    /// ��ȡ�ض����ߵ�λ������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public BaseUnit.Attribute GetItemUnitAttribute(int type, int shape)
    {
        BaseUnit.Attribute attr;
        if (!itemAttributeDict.ContainsKey(type))
            itemAttributeDict.Add(type, new Dictionary<int, BaseUnit.Attribute>());
        if (!itemAttributeDict[type].ContainsKey(shape) && JsonManager.TryLoadFromResource("Item/" + type + "/" + shape + "", out attr))
            //itemAttributeDict[type].Add(shape, JsonManager.Load<BaseUnit.Attribute>("Item/" + type + "/" + shape + ""));
            itemAttributeDict[type].Add(shape, attr);
        return itemAttributeDict[type][shape];
    }

    /// <summary>
    /// ��ȡ�ض���Ƭ�Ľ�����Ϣ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public BaseCardBuilder.Attribute GetCardBuilderAttribute(int type, int shape)
    {
        BaseCardBuilder.Attribute attr;
        if (!cardBuilderAttributeDict.ContainsKey(type))
            cardBuilderAttributeDict.Add(type, new Dictionary<int, BaseCardBuilder.Attribute>());
        if (!cardBuilderAttributeDict[type].ContainsKey(shape) && JsonManager.TryLoadFromResource("CardBuilder/" + type + "/" + shape + "", out attr))
            cardBuilderAttributeDict[type].Add(shape, attr);
        return cardBuilderAttributeDict[type][shape];
    }

    /// <summary>
    /// ��ȡĳ�����������йؿ����������У�
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <returns></returns>
    public List<BaseStage.StageInfo> GetStageInfoListFromScene(int chapterIndex, int sceneIndex)
    {
        if (!stageInfoDict.ContainsKey(chapterIndex))
            stageInfoDict.Add(chapterIndex, new Dictionary<int, Dictionary<int, StageInfo>>());
        if (!stageInfoDict[chapterIndex].ContainsKey(sceneIndex))
        {
            stageInfoDict[chapterIndex].Add(sceneIndex, new Dictionary<int, StageInfo>());
            LoadStageListFromScene(chapterIndex, sceneIndex);
        }
        
        List<BaseStage.StageInfo> list = new List<BaseStage.StageInfo>();

        for (int i = 0; i < stageInfoDict[chapterIndex][sceneIndex].Count; i++)
        {
            list.Add(stageInfoDict[chapterIndex][sceneIndex][i]);
        }
        return list;
    }

    /// <summary>
    /// �����¼��صķ�ʽ�ٻ�ȡĳ�����������йؿ���������ɾ���ؿ���
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <returns></returns>
    public List<BaseStage.StageInfo> ReloadStageInfoListFromScene(int chapterIndex, int sceneIndex)
    {
        if (!stageInfoDict.ContainsKey(chapterIndex))
            stageInfoDict.Add(chapterIndex, new Dictionary<int, Dictionary<int, StageInfo>>());
        if (!stageInfoDict[chapterIndex].ContainsKey(sceneIndex))
        {
            stageInfoDict[chapterIndex].Add(sceneIndex, new Dictionary<int, StageInfo>());
        }
        stageInfoDict[chapterIndex].Remove(sceneIndex);
        return GetStageInfoListFromScene(chapterIndex, sceneIndex);
    }

    /// <summary>
    /// ����ĳ�����������йؿ�
    /// </summary>
    public void LoadStageListFromScene(int chapterIndex, int sceneIndex)
    {
        DirectoryInfo direction = new DirectoryInfo(Application.streamingAssetsPath + "/Json/Stage/Chapter/" + chapterIndex + "/" + sceneIndex); 
        FileInfo[] files = direction.GetFiles("*"); // ��ȡ����ļ���Ŀ¼�µ������ļ�
        foreach (var f in files)
        {
            if (f.Extension.Equals(".json")){
                int arrayIndex = int.Parse(f.Name.Replace(f.Extension, "")); // ȥ��׺�����indexҲ�ǹؿ�����
                GetStageInfo(chapterIndex, sceneIndex, arrayIndex);
            }
        }
    }

    /// <summary>
    /// ��ȡĳ�ص���Ϣ
    /// </summary>
    /// <param name="chapterIndex">�½ں�</param>
    /// <param name="sceneIndex">������</param>
    /// <param name="arrayIndex">�ؿ����</param>
    /// <returns></returns>
    public BaseStage.StageInfo GetStageInfo(int chapterIndex, int sceneIndex, int arrayIndex)
    {
        //StageInfo info;
        if (!stageInfoDict.ContainsKey(chapterIndex))
            stageInfoDict.Add(chapterIndex, new Dictionary<int, Dictionary<int, StageInfo>>());
        if (!stageInfoDict[chapterIndex].ContainsKey(sceneIndex))
            stageInfoDict[chapterIndex].Add(sceneIndex, new Dictionary<int, StageInfo>());
        if (!stageInfoDict[chapterIndex][sceneIndex].ContainsKey(arrayIndex))
        {
            BaseStage.StageInfo info = JsonManager.Load<StageInfo>("Stage/Chapter/" + chapterIndex + "/" + sceneIndex + "/" + arrayIndex);
            if (info.fileName == null || info.fileName.Equals(""))
                info.fileName = info.name;
            stageInfoDict[chapterIndex][sceneIndex].Add(arrayIndex, info);
        }
        return stageInfoDict[chapterIndex][sceneIndex][arrayIndex];
    }
}
