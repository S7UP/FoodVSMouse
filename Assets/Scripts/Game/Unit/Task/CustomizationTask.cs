using System.Collections.Generic;
using System;
/// <summary>
/// 自定义任务
/// </summary>
public class CustomizationTask : BaseTask
{
    private Queue<Func<bool>> TaskQueue = new Queue<Func<bool>>();
    private Func<bool> currentTask;


    protected override void O_OnEnter()
    {

    }

    protected override void O_OnUpdate()
    {
        if (currentTask == null)
        {
            if (TaskQueue.Count > 0)
                currentTask = TaskQueue.Dequeue();
            else
                return;
        }

        if (currentTask())
            currentTask = null;
    }

    protected override bool O_IsMeetingCondition()
    {
        return (currentTask == null && TaskQueue.Count <= 0);
    }

    protected override void O_OnExit()
    {

    }


    public void AddTaskFunc(Func<bool> func)
    {
        TaskQueue.Enqueue(func);
    }

    /// <summary>
    /// 在当前队头添加一个任务
    /// </summary>
    /// <param name="func"></param>
    public void AddTaskFuncInFront(Func<bool> func)
    {
        Queue<Func<bool>> q = new Queue<Func<bool>>();
        for (int i = 0; i < TaskQueue.Count; i++)
        {
            q.Enqueue(TaskQueue.Dequeue());
        }
        TaskQueue.Enqueue(func);
        for (int i = 0; i < q.Count; i++)
        {
            TaskQueue.Enqueue(q.Dequeue());
        }
    }

    public Func<bool> GetTimeTaskFunc(int totalTime, Action<int> EnterAction, Action<int, int> UpdateAction, Action<int> ExitAction)
    {
        int timeLeft = totalTime;
        return delegate
        {
            if (EnterAction != null && timeLeft == totalTime)
                EnterAction(totalTime);
            timeLeft--;
            if (UpdateAction != null)
                UpdateAction(timeLeft, totalTime);
            if (timeLeft <= 0)
            {
                if (ExitAction != null)
                    ExitAction(totalTime);
                return true;
            }
            return false;
        };
    }

    /// <summary>
    /// 添加与时间有关的Task
    /// </summary>
    /// <param name="EnterAction">参数为总时间</param>
    /// <param name="UpdateAction">第一个参数为剩余时间，第二个参数为总时间</param>
    /// <param name="ExitAction">参数为总时间</param>
    public void AddTimeTaskFunc(int totalTime, Action<int> EnterAction, Action<int, int> UpdateAction, Action<int> ExitAction)
    {
        TaskQueue.Enqueue(GetTimeTaskFunc(totalTime, EnterAction, UpdateAction, ExitAction));
    }

    public void AddTimeTaskFunc(int totalTime)
    {
        AddTimeTaskFunc(totalTime, null, null, null);
    }

    public void AddTimeTaskFuncInFront(int totalTime, Action<int> EnterAction, Action<int, int> UpdateAction, Action<int> ExitAction)
    {
        AddTaskFuncInFront(GetTimeTaskFunc(totalTime, EnterAction, UpdateAction, ExitAction));
    }

    public void AddTimeTaskFuncInFront(int totalTime)
    {
        AddTimeTaskFuncInFront(totalTime, null, null, null);
    }
}
