using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// �������ܹ������������ڵ�λ�ϵģ�����ÿ����λ������ӵ�м��ܵĶ���
/// </summary>
public sealed class SkillAbilityManager
{
    public List<SkillAbility> skillAbilityList = new List<SkillAbility>();
    public bool exclusiveLock = false;

    public void Initialize()
    {
        skillAbilityList.Clear();
        // AddSkillAbility(new GeneralAttackSkillAbility()); // ����Ĭ��ÿ����λ������ͨ��������
    }

    /// <summary>
    /// ����ʹ����һ�������ͷ������ļ���
    /// </summary>
    public void TryAvailableSkill()
    {
        
        // �����ռ���ˣ����ͷŵļ����������ԣ���ֱ���˳�
        if (exclusiveLock)
            return;
        SkillAbility skillAbility = null;
        foreach (var item in skillAbilityList)
        {
            // ��Ϊ��ѡ���ܵ�����������1�����ܱ���ﵽʩ��������2����ǰ�������ȼ���ԭ����ѡ�������ȼ���
            if (item.CanActive() && (skillAbility==null || skillAbility > item))
            {
                skillAbility = item;
            }
        }
        // ����ȷ����ѡ���ܺ�ִ���Զ��ͷż���
        if (skillAbility != null)
            skillAbility.ActivateAbility();
    }

    public void AddSkillAbility(SkillAbility skillAbility)
    {
        skillAbilityList.Add(skillAbility);
    }

    public void RemoveSkillAbility(SkillAbility skillAbility)
    {
        skillAbilityList.Remove(skillAbility);
    }

    public void TryActivateSkillAbility(int index)
    {
        skillAbilityList[index].TryActivateAbility();
    }

    public void TryActivateSkillAbility(string name)
    {
        foreach (var item in skillAbilityList)
        {
            if (item.name.Equals(name))
            {
                item.TryActivateAbility();
                return;
            }
        }
    }

    public void Update()
    {
        foreach (var item in skillAbilityList)
        {
            item.Update();
        }
        // �����ͷ�һ�������ͷŵļ���
        TryAvailableSkill();
    }

    public void SetExclusiveLock(bool isLock)
    {
        exclusiveLock = isLock;
    }
}
