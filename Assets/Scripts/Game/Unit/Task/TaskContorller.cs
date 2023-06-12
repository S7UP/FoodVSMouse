using System.Collections.Generic;

public class TaskController
{
    private List<ITask> TaskList = new List<ITask>(); // ������������
    private Dictionary<string, ITask> TaskDict = new Dictionary<string, ITask>(); // �����ֵ䣨����¼���ò�ʵ��ִ���߼���ִ���߼���������У�

    public void Initial()
    {
        TaskList.Clear();
        TaskDict.Clear();
    }

    /// <summary>
    /// ���Ψһ������
    /// </summary>
    public void AddUniqueTask(string key, ITask t)
    {
        if (!TaskDict.ContainsKey(key))
        {
            TaskDict.Add(key, t);
            AddTask(t);
        }
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        TaskList.Add(t);
        t.OnEnter();
    }

    /// <summary>
    /// �Ƴ�Ψһ������
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        if (TaskDict.ContainsKey(key))
        {
            RemoveTask(TaskDict[key]);
            TaskDict.Remove(key);
        }
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        TaskList.Remove(t);
        t.OnExit();
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        if (TaskDict.ContainsKey(key))
            return TaskDict[key];
        return null;
    }

    public void ShutDownAll()
    {
        List<ITask> delList = new List<ITask>();
        foreach (var t in TaskList)
        {
            if (t.IsClearWhenDie())
            {
                t.ShutDown();
                delList.Add(t);
            }
        }
        foreach (var t in delList)
        {
            TaskList.Remove(t);
        }
    }

    /// <summary>
    /// Task�����
    /// </summary>
    public void Update()
    {
        List<string> deleteKeyList = new List<string>();
        foreach (var keyValuePair in TaskDict)
        {
            ITask t = keyValuePair.Value;
            if (t.IsMeetingExitCondition())
                deleteKeyList.Add(keyValuePair.Key);
        }
        foreach (var key in deleteKeyList)
        {
            RemoveUniqueTask(key);
        }
        List<ITask> deleteTask = new List<ITask>();
        List<ITask> WillUpdateTask = new List<ITask>();
        foreach (var t in TaskList)
        {
            if (t.IsMeetingExitCondition())
                deleteTask.Add(t);
            else
                WillUpdateTask.Add(t);
        }
        foreach (var t in deleteTask)
        {
            RemoveTask(t);
        }
        foreach (var t in WillUpdateTask)
        {
            t.OnUpdate();
        }
    }
}
