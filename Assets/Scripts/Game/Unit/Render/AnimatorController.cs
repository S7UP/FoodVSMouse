using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEditor.Progress;

/// <summary>
/// ��ĳ��Animator�Ŀ�������ͬһʱ����ܴ��ںܶ���ƶ����仯�����񣬵���Ҫ�������������ȼ�ִֻ������֮һ
/// </summary>
public class AnimatorController
{
    private static AnimatorControllerTask.Type[] taskTypeList = 
    { 
        AnimatorControllerTask.Type.DieClip, 
        AnimatorControllerTask.Type.FrozenClip, 
        AnimatorControllerTask.Type.AttackClip, 
        AnimatorControllerTask.Type.MoveClip, 
        AnimatorControllerTask.Type.IdleClip
    };
    // ���ֵ�����м�����ȫ������ִ��ʱ��Ĭ��ִ�и�defaultTask
    public AnimatorControllerTask defaultTask = new AnimatorControllerTask("Idle");

    public Animator animator;
    public Dictionary<AnimatorControllerTask.Type, List<AnimatorControllerTask>> taskListDict = new Dictionary<AnimatorControllerTask.Type, List<AnimatorControllerTask>>(); // ��ǰ��ִ�еĶ�����
    public AnimatorControllerTask currentTask;

    private AnimatorController()
    {

    }

    public AnimatorController(Animator animator)
    {
        this.animator = animator;
        Initialize();
    }

    private void Initialize()
    {
        foreach (var item in taskTypeList)
        {
            taskListDict.Add(item, new List<AnimatorControllerTask>());
        }
    }

    /// <summary>
    /// �������ȼ�ȷ��Ŀǰ��Ҫ���ŵĶ����������÷�����ʱ��Ϊ��ӻ����Ƴ�����
    /// �ӷ����ϣ����ȼ�Ϊ ���� > ���� > ���� > �ƶ� > ����
    /// ͬ���Ͷ����У����������ȼ����
    /// </summary>
    public void UpdateTheMostImportantTask()
    {
        foreach (var item in taskTypeList)
        {
            int count = taskListDict[item].Count;
            if (count > 0)
            {
                // �������Ҫ���ŵĶ����л��ˣ���Ҫִ���¶����Ĳ��ţ�ͬʱ�������������Ĳ���
                AnimatorControllerTask newTask = taskListDict[item][count - 1];
                if(currentTask==null || newTask != currentTask)
                {
                    currentTask = newTask;
                    foreach (var task in GetEachTask())
                    {
                        task.isPlaying = false;
                    }
                    currentTask.isPlaying = true;
                    currentTask.PlayClip();
                    return;
                }
            }
        }
        // ��ȫ��У��
        if (currentTask == null)
        {
            defaultTask.PlayClip();
        }
    }

    /// <summary>
    /// �������ж������ŵ�����
    /// </summary>
    /// <returns></returns>
    public List<AnimatorControllerTask> GetEachTask()
    {
        List<AnimatorControllerTask> list = new List<AnimatorControllerTask>();
        foreach (var type in taskTypeList)
        {
            foreach (var item in taskListDict[type])
            {
                list.Add(item);
            }
        }
        return list;
    }

    public void Update()
    {
        foreach (var type in taskTypeList)
        {
            foreach (var item in taskListDict[type])
            {
                item.Update();
            }
        }
    }

    public void AddTask(AnimatorControllerTask.Type taskType, AnimatorControllerTask task)
    {
        task.animator = animator;
        taskListDict[taskType].Add(task);
        UpdateTheMostImportantTask();
    }

    public void RemoveTask(AnimatorControllerTask.Type taskType, AnimatorControllerTask task)
    {
        taskListDict[taskType].Remove(task);
        if (task == currentTask)
            currentTask = null;
        UpdateTheMostImportantTask();
    }
}
