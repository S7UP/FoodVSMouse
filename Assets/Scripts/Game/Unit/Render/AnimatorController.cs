using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEditor.Progress;

/// <summary>
/// 对某个Animator的控制器，同一时间可能存在很多控制动画变化的任务，但需要控制器根据优先级只执行其中之一
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
    // 当字典的所有集合完全无任务执行时，默认执行该defaultTask
    public AnimatorControllerTask defaultTask = new AnimatorControllerTask("Idle");

    public Animator animator;
    public Dictionary<AnimatorControllerTask.Type, List<AnimatorControllerTask>> taskListDict = new Dictionary<AnimatorControllerTask.Type, List<AnimatorControllerTask>>(); // 当前待执行的动画集
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
    /// 根据优先级确定目前需要播放的动画，触发该方法的时机为添加或者移除任务
    /// 从分类上，优先级为 死亡 > 冻结 > 攻击 > 移动 > 待机
    /// 同类型动画中，以最后的优先级最大
    /// </summary>
    public void UpdateTheMostImportantTask()
    {
        foreach (var item in taskTypeList)
        {
            int count = taskListDict[item].Count;
            if (count > 0)
            {
                // 如果发现要播放的动画切换了，则要执行新动画的播放，同时掐掉其他动画的播放
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
        // 安全性校验
        if (currentTask == null)
        {
            defaultTask.PlayClip();
        }
    }

    /// <summary>
    /// 遍历所有动画播放的任务
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
