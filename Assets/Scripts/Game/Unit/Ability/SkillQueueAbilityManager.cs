using System.Collections.Generic;
/// <summary>
/// 技能队列管理器
/// 适用于固定出招顺序的敌人，如常规BOSS
/// 它是通过直接操纵master的SkillAbilityManager技能管理器来达到其作用的
/// </summary>
public sealed class SkillQueueAbilityManager
{
    private BaseUnit master;
    private List<SkillAbility> skillList = new List<SkillAbility>();
    private int index;
    private SkillAbility currentSkillAbility; // 当前在使用的技能

    public SkillQueueAbilityManager(BaseUnit unit)
    {
        master = unit;
    }

    public void Initial()
    {
        skillList.Clear();
        index = -1;
        currentSkillAbility = null;
    }

    public void Update()
    {
        // 如果表中没有技能则直接跳过Update，该组件可以认为不生效
        if (skillList.Count <= 0)
            return;

        // 当前没有技能在施放或者技能已经施放完毕
        if(currentSkillAbility==null || !currentSkillAbility.isSpelling)
        {
            master.skillAbilityManager.RemoveSkillAbility(currentSkillAbility);
            // 下标后移，然后取余
            index++;
            index = index % skillList.Count;
            currentSkillAbility = skillList[index];
            master.skillAbilityManager.AddSkillAbility(currentSkillAbility);
            currentSkillAbility.FullCurrentEnergy();
        }
    }

    /// <summary>
    /// 清空当前技能表然后添加新的技能表（常用于BOSS切换阶段后技能组变更）
    /// </summary>
    public void ClearAndAddSkillList(List<SkillAbility> list)
    {
        skillList.Clear();
        foreach (var skill in list)
        {
            skillList.Add(skill);
        }
    }

    /// <summary>
    /// 强制设置当前使用技能
    /// </summary>
    public void SetCurrentSkill(SkillAbility skill)
    {
        master.skillAbilityManager.TryEndAllSpellingSkillAbility();
        master.skillAbilityManager.RemoveSkillAbility(currentSkillAbility);
        currentSkillAbility = skill;
        master.skillAbilityManager.AddSkillAbility(currentSkillAbility);
        currentSkillAbility.FullCurrentEnergy();
    }

    /// <summary>
    /// 设置下一个技能下标
    /// </summary>
    public void SetNextSkillIndex(int index)
    {
        this.index = index;
    }

}
