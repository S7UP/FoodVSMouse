using System.Collections.Generic;
using System;
/// <summary>
/// 技能队列管理器
/// 适用于固定出招顺序的敌人，如常规BOSS
/// 它是通过直接操纵master的SkillAbilityManager技能管理器来达到其作用的
/// </summary>
public sealed class SkillQueueAbilityManager
{
    private BaseUnit master;
    private List<SkillAbility> skillList = new List<SkillAbility>();
    // 下一个技能使用的优先级：下一个临时技能 > 下一个技能下标队列出队后下标所对应的技能 > 正常顺序技能
    private int index;
    private SkillAbility currentSkillAbility; // 当前在使用的技能
    private SkillAbility nextSkillAbility; // 下一个临时技能
    private Queue<int> nextSkillIndexQueue = new Queue<int>(); // 下一个技能的下标队列
    private Func<List<int>> GetNextSkillIndexQueueFunc = null; // 获取下一个技能施放队列的方法（在每次队列归0时自动调用一次）

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
            // 如果有下一个技能则优先取下一个技能，然后将下个技能置空，否则按队列的顺序来，如果队列也为空则按顺序来
            if (nextSkillAbility != null)
            {
                currentSkillAbility = nextSkillAbility;
                nextSkillAbility = null;
            }else
            {
                if(nextSkillIndexQueue.Count == 0 && GetNextSkillIndexQueueFunc != null)
                    foreach (var i in GetNextSkillIndexQueueFunc())
                    {
                        EnqueueNextSkillIndex(i);
                    }
                if(nextSkillIndexQueue.Count > 0)
                    currentSkillAbility = skillList[nextSkillIndexQueue.Dequeue() % skillList.Count];
                else
                {
                    // 下标后移，然后取余
                    index++;
                    index = index % skillList.Count;
                    currentSkillAbility = skillList[index];
                }
            }
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
    /// 设置下一个技能
    /// </summary>
    /// <param name="skill"></param>
    public void SetNextSkill(SkillAbility skill)
    {
        nextSkillAbility = skill;
    }

    /// <summary>
    /// 设置下一个技能下标
    /// </summary>
    public void SetNextSkillIndex(int index)
    {
        this.index = index;
    }

    public int GetCurrentSkillIndex()
    {
        return index;
    }

    /// <summary>
    /// 把下一个要用的技能入队
    /// </summary>
    /// <param name="skillIndex"></param>
    public void EnqueueNextSkillIndex(int skillIndex)
    {
        nextSkillIndexQueue.Enqueue(skillIndex);
    }


    /// <summary>
    /// 设置获取下一个技能释放顺序队列的方法
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public void SetGetNextSkillIndexQueueFunc(Func<List<int>> func)
    {
        GetNextSkillIndexQueueFunc = func;
    }
}
