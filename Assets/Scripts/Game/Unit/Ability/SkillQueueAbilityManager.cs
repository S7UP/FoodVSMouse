using System.Collections.Generic;
using System;
/// <summary>
/// ���ܶ��й�����
/// �����ڹ̶�����˳��ĵ��ˣ��糣��BOSS
/// ����ͨ��ֱ�Ӳ���master��SkillAbilityManager���ܹ��������ﵽ�����õ�
/// </summary>
public sealed class SkillQueueAbilityManager
{
    private BaseUnit master;
    private List<SkillAbility> skillList = new List<SkillAbility>();
    // ��һ������ʹ�õ����ȼ�����һ����ʱ���� > ��һ�������±���г��Ӻ��±�����Ӧ�ļ��� > ����˳����
    private int index;
    private SkillAbility currentSkillAbility; // ��ǰ��ʹ�õļ���
    private SkillAbility nextSkillAbility; // ��һ����ʱ����
    private Queue<int> nextSkillIndexQueue = new Queue<int>(); // ��һ�����ܵ��±����
    private Func<List<int>> GetNextSkillIndexQueueFunc = null; // ��ȡ��һ������ʩ�Ŷ��еķ�������ÿ�ζ��й�0ʱ�Զ�����һ�Σ�

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
        // �������û�м�����ֱ������Update�������������Ϊ����Ч
        if (skillList.Count <= 0)
            return;

        // ��ǰû�м�����ʩ�Ż��߼����Ѿ�ʩ�����
        if(currentSkillAbility==null || !currentSkillAbility.isSpelling)
        {
            master.skillAbilityManager.RemoveSkillAbility(currentSkillAbility);
            // �������һ������������ȡ��һ�����ܣ�Ȼ���¸������ÿգ����򰴶��е�˳�������������ҲΪ����˳����
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
                    // �±���ƣ�Ȼ��ȡ��
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
    /// ��յ�ǰ���ܱ�Ȼ������µļ��ܱ�������BOSS�л��׶κ���������
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
    /// ǿ�����õ�ǰʹ�ü���
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
    /// ������һ������
    /// </summary>
    /// <param name="skill"></param>
    public void SetNextSkill(SkillAbility skill)
    {
        nextSkillAbility = skill;
    }

    /// <summary>
    /// ������һ�������±�
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
    /// ����һ��Ҫ�õļ������
    /// </summary>
    /// <param name="skillIndex"></param>
    public void EnqueueNextSkillIndex(int skillIndex)
    {
        nextSkillIndexQueue.Enqueue(skillIndex);
    }


    /// <summary>
    /// ���û�ȡ��һ�������ͷ�˳����еķ���
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public void SetGetNextSkillIndexQueueFunc(Func<List<int>> func)
    {
        GetNextSkillIndexQueueFunc = func;
    }
}
