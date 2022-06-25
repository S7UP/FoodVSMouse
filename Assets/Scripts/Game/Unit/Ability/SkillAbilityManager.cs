using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// 主动技能管理器（附加在单位上的），让每个单位都可以拥有技能的东西
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
    /// 尝试使用下一个满足释放条件的技能
    /// 技能施放机制：当前无技能施放时，取可施放技能中优先级最高的技能施放
    /// 当前已施放技能时，先检查该技能本身是否有排它性，如果有，则必须等待这个技能施放结束才会进行下一个技能的施放；
    /// 如果没有，则继续检查可施放技能中是否有更高优先级的技能，如果有更高优先级的，则执行当前技能的中断方法并启用该更高优先级的技能
    /// </summary>
    public void TryAvailableSkill()
    {
        // 如果被占用了，即释放的技能有排它性，则直接退出
        if (lastSkill != null && lastSkill.isExclusive)
            return;
        SkillAbility skillAbility = lastSkill;
        foreach (var item in skillAbilityList)
        {
            // 作为候选技能的两个条件：1、技能本身达到施放条件；2、当前技能优先级比原本候选技能优先级大
            if (item.CanActive() && (skillAbility==null || item > skillAbility))
            {
                skillAbility = item;
            }
        }
        // 如果有更优先级的技能可以释放，中止上一个技能（如果有），并执行自动释放技能
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
        // 尝试释放一个可以释放的技能
        TryAvailableSkill();
        // 技能施放结束检查
        if(lastSkill!=null && !lastSkill.isSpelling)
        {
            lastSkill = null;
        }
    }
}
