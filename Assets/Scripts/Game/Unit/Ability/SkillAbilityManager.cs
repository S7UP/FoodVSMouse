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
    public SkillAbility lastSkill = null;

    public void Initialize()
    {
        skillAbilityList.Clear();
    }

    /// <summary>
    /// ����ʹ����һ�������ͷ������ļ���
    /// ����ʩ�Ż��ƣ���ǰ�޼���ʩ��ʱ��ȡ��ʩ�ż��������ȼ���ߵļ���ʩ��
    /// ��ǰ��ʩ�ż���ʱ���ȼ��ü��ܱ����Ƿ��������ԣ�����У������ȴ��������ʩ�Ž����Ż������һ�����ܵ�ʩ�ţ�
    /// ���û�У����������ʩ�ż������Ƿ��и������ȼ��ļ��ܣ�����и������ȼ��ģ���ִ�е�ǰ���ܵ��жϷ��������øø������ȼ��ļ���
    /// </summary>
    public void TryAvailableSkill()
    {
        // �����ռ���ˣ����ͷŵļ����������ԣ���ֱ���˳�
        if (lastSkill != null && lastSkill.isExclusive)
            return;
        SkillAbility skillAbility = lastSkill;
        foreach (var item in skillAbilityList)
        {
            // ��Ϊ��ѡ���ܵ�����������1�����ܱ���ﵽʩ��������2����ǰ�������ȼ���ԭ����ѡ�������ȼ���
            if (item.CanActive() && (skillAbility==null || item > skillAbility))
            {
                skillAbility = item;
            }
        }
        // ����и����ȼ��ļ��ܿ����ͷţ���ֹ��һ�����ܣ�����У�����ִ���Զ��ͷż���
        if (skillAbility != null && skillAbility != lastSkill)
        {
            if(lastSkill != null)
            {
                lastSkill.EndActivate();
            }
            skillAbility.ActivateAbility();
            lastSkill = skillAbility;
        }
            
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
        // ����ʩ�Ž������
        if(lastSkill!=null && !lastSkill.isSpelling)
        {
            lastSkill = null;
        }
    }
}
