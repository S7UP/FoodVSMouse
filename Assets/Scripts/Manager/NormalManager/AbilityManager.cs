using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static LayerManager;

/// <summary>
/// �����ܵ�
/// </summary>
public class AbilityManager
{
    private static AbilityManager _instance;

    // ����
    public static AbilityManager Instance { get => GetSingleton(); } //+ ������
    /// <summary>
    /// ���⵽�
    /// 1�������õ�λ�������֣�����ʳ������
    /// 2��Ȼ��С�����֣�����ͨ�����󡢾���������
    /// 3��Ȼ����С�����ñ������֣���ƽ����������
    /// 4�����һ��List���������������ļ����飬һ����������������ɸ�����������Ϣ
    /// </summary>
    public Dictionary<UnitType, List<List<List<SkillAbility.SkillAbilityInfo>>>> AbilityDict;

    // ������紴����
    private AbilityManager()
    {
        _instance = this;
        Init();
    }

    private void Init()
    {
        AbilityDict = new Dictionary<UnitType, List<List<List<SkillAbility.SkillAbilityInfo>>>>();
        AbilityDict.Add(UnitType.Food, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        AbilityDict.Add(UnitType.Mouse, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        AbilityDict.Add(UnitType.Weapons, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        LoadAll();
    }

    public void Insert(SkillAbility.SkillAbilityInfo skillAbilityInfo, UnitType unitType, int typeIndex, int shapeIndex)
    {
        GetSkillAbilityInfoList(unitType, typeIndex, shapeIndex).Add(skillAbilityInfo);
    }

    public bool Update(SkillAbility.SkillAbilityInfo skillAbilityInfo, UnitType unitType, int typeIndex, int shapeIndex)
    {
        List<SkillAbility.SkillAbilityInfo> list = GetSkillAbilityInfoList(unitType, typeIndex, shapeIndex);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].name.Equals(skillAbilityInfo.name))
            {
                list[i] = skillAbilityInfo;
                return true;
            }
        }
        Debug.Log("����ʧ�ܣ�δ�ҵ�ƥ����Ŀ��SkillAbilityInfo.name='"+skillAbilityInfo.name+"'��ƥ����");
        return false;
    }

    public List<SkillAbility.SkillAbilityInfo> GetSkillAbilityInfoList(UnitType unitType, int typeIndex, int shapeIndex)
    {
        // ��ȫУ�飬���Index��Lenght�������ǿ����չList�Ĵ�С
        List<List<SkillAbility.SkillAbilityInfo>> shapeList = GetSkillAbilityInfoShapeList(unitType, typeIndex);
        int c = shapeIndex - shapeList.Count + 1;
        for (int i = 0; i < c; i++)
        {
            shapeList.Add(new List<SkillAbility.SkillAbilityInfo>());
        }
        return shapeList[shapeIndex];
    }

    public List<List<SkillAbility.SkillAbilityInfo>> GetSkillAbilityInfoShapeList(UnitType unitType, int typeIndex)
    {
        // ��ȫУ�飬���Index��Lenght�������ǿ����չList�Ĵ�С
        int c = typeIndex - AbilityDict[unitType].Count + 1;
        for (int i = 0; i < c; i++)
        {
            AbilityDict[unitType].Add(new List<List<SkillAbility.SkillAbilityInfo>>());
        }
        return AbilityDict[unitType][typeIndex];
    }

    /// <summary>
    /// ����ȫ��
    /// </summary>
    public void SaveAll()
    {
        Save(UnitType.Food);
        Save(UnitType.Mouse);
    }

    private void Save(UnitType unitType)
    {
        string unitTypeStr = "";
        switch (unitType)
        {
            case UnitType.Default:
                break;
            case UnitType.Food:
                unitTypeStr += "Food";
                break;
            case UnitType.Mouse:
                unitTypeStr += "Mouse";
                break;
            default:
                break;
        }
        List<List<List<SkillAbility.SkillAbilityInfo>>> typelist = AbilityDict[unitType];
        int typeIndex = 0;
        foreach (var shapeList in typelist)
        {
            int shapeIndex = 0;
            foreach (var infoList in shapeList)
            {
                JsonManager.Save<List<SkillAbility.SkillAbilityInfo>>(infoList, "Skill/" + unitTypeStr + "/" + typeIndex + "/" + shapeIndex);
                shapeIndex++;
            }
            typeIndex++;
        }
    }

    public void LoadAll()
    {
        Load(UnitType.Food);
        Load(UnitType.Mouse);
        Load(UnitType.Weapons);
    }

    private void Load(UnitType unitType)
    {
        //List<List<List<SkillAbility.SkillAbilityInfo>>> list = new List<List<List<SkillAbility.SkillAbilityInfo>>>();
        string path = Application.dataPath + "/Resources/Json/Skill";
        string unitTypeStr = "";
        switch (unitType)
        {
            case UnitType.Default:
                break;
            case UnitType.Food:
                unitTypeStr += "Food";
                break;
            case UnitType.Mouse:
                unitTypeStr += "Mouse";
                break;
            case UnitType.Weapons:
                unitTypeStr += "Weapons";
                break;
            default:
                break;
        }
        path += "/" + unitTypeStr;
        DirectoryInfo direction = new DirectoryInfo(path); // ��ȡJSON�ļ����µ������ļ�
        FileInfo[] files = direction.GetFiles("*");
        foreach (var typeFile in files)
        {
            int typeIndex = int.Parse(typeFile.Name.Replace(typeFile.Extension, "")); // ȥ��׺
            // ����������ļ��ж�ȡ
            FileInfo[] shapeFiles = new DirectoryInfo(path + "/" + typeIndex).GetFiles("*");
            foreach (var f in shapeFiles)
            {
                string name = f.Name.Replace(f.Extension, "");
                // ֻ��ȡJSON
                if (name.EndsWith(".json"))
                {
                    int shapeIndex = int.Parse(name.Replace(".json", ""));
                    List<SkillAbility.SkillAbilityInfo> infoList = JsonManager.Load<List<SkillAbility.SkillAbilityInfo>>("Skill/" + unitTypeStr + "/" + typeIndex + "/" + shapeIndex);
                    List<SkillAbility.SkillAbilityInfo> list = GetSkillAbilityInfoList(unitType, typeIndex, shapeIndex);
                    foreach (var item in infoList)
                    {
                        list.Add(item);
                    }
                }
            }
        }
    }

    public static AbilityManager GetSingleton()
    {
        if (_instance == null)
        {
            _instance = new AbilityManager();
        }
        return _instance;
    }
}
