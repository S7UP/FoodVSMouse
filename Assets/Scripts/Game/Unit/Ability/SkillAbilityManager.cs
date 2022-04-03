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
    public bool exclusiveLock = false;

    public void Initialize()
    {
        skillAbilityList.Clear();
        // AddSkillAbility(new GeneralAttackSkillAbility()); // 可以默认每个单位都有普通攻击能力
    }

    /// <summary>
    /// 尝试使用下一个满足释放条件的技能
    /// </summary>
    public void TryAvailableSkill()
    {
        
        // 如果被占用了，即释放的技能有排它性，则直接退出
        if (exclusiveLock)
            return;
        SkillAbility skillAbility = null;
        foreach (var item in skillAbilityList)
        {
            // 作为候选技能的两个条件：1、技能本身达到施放条件；2、当前技能优先级比原本候选技能优先级大
            if (item.CanActive() && (skillAbility==null || skillAbility > item))
            {
                skillAbility = item;
            }
        }
        // 最终确定候选技能后，执行自动释放技能
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
        // 尝试释放一个可以释放的技能
        TryAvailableSkill();
    }

    public void SetExclusiveLock(bool isLock)
    {
        exclusiveLock = isLock;
    }
}
