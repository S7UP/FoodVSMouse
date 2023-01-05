using System.Collections.Generic;
using System;
/// <summary>
/// 自定义任务
/// </summary>
public class CustomizationTask : ITask
{
    public Action OnEnterFunc;
    private Queue<Func<bool>> TaskQueue = new Queue<Func<bool>>();
    private Func<bool> currentTask;
    public Action OnExitFunc;
    private bool isEnd;

    public void OnEnter()
    {
        if (OnEnterFunc != null)
            OnEnterFunc();
    }

    public void OnUpdate()
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

    public bool IsMeetingExitCondition()
    {
        return (currentTask == null && TaskQueue.Count <= 0);
    }

    public void OnExit()
    {
        if (OnExitFunc != null)
            OnExitFunc();
        isEnd = true;
    }

    public void AddTaskFunc(Func<bool> func)
    {
        TaskQueue.Enqueue(func);
    }

    public bool IsEnd()
    {
        return isEnd;
    }
}
