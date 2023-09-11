using System.Collections.Generic;

using UnityEngine;

public class TaskController
{
    private List<ITask> addList = new List<ITask>();
    private List<ITask> rmList = new List<ITask>();
    private List<ITask> TaskList = new List<ITask>(); // ������������
    private Dictionary<string, ITask> TaskDict = new Dictionary<string, ITask>(); // �����ֵ䣨����¼���ò�ʵ��ִ���߼���ִ���߼���������У�

    public void Initial()
    {
        addList.Clear();
        rmList.Clear();
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
        // TaskList.Add(t);
        if(!addList.Contains(t) && !TaskList.Contains(t))
        {
            addList.Add(t);
            t.OnEnter();
        }
        else
        {
            Debug.LogError("���棡�����������ͬ����������ֹ��");
        }
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
        //TaskList.Remove(t);
        if (addList.Contains(t))
        {
            addList.Remove(t);
            t.OnExit();
        }
        else if (!rmList.Contains(t) && TaskList.Contains(t))
        {
            rmList.Add(t);
            t.OnExit();
        }
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
            RemoveUniqueTask(key);

        foreach (var t in rmList)
            TaskList.Remove(t);
        rmList.Clear();
        foreach (var t in addList)
            TaskList.Add(t);
        addList.Clear();

        List<ITask> exitTask = new List<ITask>();
        List<ITask> updateTask = new List<ITask>();
        foreach (var t in TaskList)
        {
            if (t.IsMeetingExitCondition())
                exitTask.Add(t);
            else
                updateTask.Add(t);
        }

        foreach (var t in exitTask)
            RemoveTask(t);
        foreach (var t in updateTask)
            t.OnUpdate();
    }
}
