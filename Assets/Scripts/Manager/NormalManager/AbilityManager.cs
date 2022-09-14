using System.Collections.Generic;

using UnityEngine;

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
    public Dictionary<UnitType, List<List<List<SkillAbility.SkillAbilityInfo>>>> AbilityDict = new Dictionary<UnitType, List<List<List<SkillAbility.SkillAbilityInfo>>>>();

    // ������紴����
    private AbilityManager()
    {
        _instance = this;
        Init();
    }

    private void Init()
    {
        AbilityDict.Clear();
        AbilityDict.Add(UnitType.Food, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        AbilityDict.Add(UnitType.Mouse, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        AbilityDict.Add(UnitType.Weapons, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        AbilityDict.Add(UnitType.Boss, new List<List<List<SkillAbility.SkillAbilityInfo>>>());
        //LoadAll();
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
            shapeList.Add(null);
        }
        if (shapeList[shapeIndex] == null)
            shapeList[shapeIndex] = Load(unitType, typeIndex, shapeIndex);
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

    private List<SkillAbility.SkillAbilityInfo> Load(UnitType unitType, int type, int shape)
    {
        string path = "Skill";
        string unitTypeStr = "";
        switch (unitType)
        {
            case UnitType.Default:
                break;
            case UnitType.Food:
                unitTypeStr += "Food/";
                break;
            case UnitType.Mouse:
                unitTypeStr += "Mouse/";
                break;
            case UnitType.Weapons:
                unitTypeStr += "Weapons/";
                break;
            case UnitType.Boss:
                unitTypeStr += "Boss/";
                break;
            default:
                break;
        }
        path += "/" + unitTypeStr + type + "/" + shape;
        List<SkillAbility.SkillAbilityInfo> infoList = JsonManager.Load<List<SkillAbility.SkillAbilityInfo>>(path);
        return infoList;
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
