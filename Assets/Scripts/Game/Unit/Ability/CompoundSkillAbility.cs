using System.Collections.Generic;
using System;
/// <summary>
/// 复合任务技能存储实体
/// </summary>
public class CompoundSkillAbility : CustomizationSkillAbility
{
    private List<Func<CustomizationTask>> CreateTaskFuncList = new List<Func<CustomizationTask>>();
    private Queue<CustomizationTask> TaskQueue = new Queue<CustomizationTask>();
    private CustomizationTask currentTask;
    private int index = 0;

    public CompoundSkillAbility(BaseUnit master):base(master)
    {

    }
    public CompoundSkillAbility(BaseUnit master, SkillAbilityInfo info):base(master, info)
    {
    }

    public override void BeforeSpell()
    {
        index = -1;
        currentTask = null;
        TaskQueue.Clear();
        base.BeforeSpell();
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (currentTask == null)
        {
            index++;
        }else if (currentTask.IsMeetingExitCondition())
        {
            currentTask.OnExit();
            currentTask = null;
            index++;
        }


        if (index >= CreateTaskFuncList.Count)
            return;
        if(currentTask == null)
        {
            if (TaskQueue.Count > 0)
                currentTask = TaskQueue.Dequeue();
            else
            {
                currentTask = CreateTaskFuncList[index]();
            }
            currentTask.OnEnter();
        }

        currentTask.OnUpdate();
    }

    public override bool IsMeetCloseSpellingCondition()
    {
        return currentTask == null && TaskQueue.Count <= 0 && index >= CreateTaskFuncList.Count;
    }

    public void AddAction(Action action)
    {
        AddSpellingFunc(delegate {
            action();
            return true;
        });
    }

    /// <summary>
    /// 添加一个在技能期间的任务
    /// </summary>
    public void AddSpellingFunc(Func<bool> func)
    {
        Func<CustomizationTask> f = delegate {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(func);
            return t;
        };
        CreateTaskFuncList.Add(f);
    }

    public void AddCreateTaskFunc(Func<CustomizationTask> func)
    {
        CreateTaskFuncList.Add(func);
    }
}