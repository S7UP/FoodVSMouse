using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

/// <summary>
/// 管理单位动画播放相关信息的东西
/// </summary>
public class AnimatorController
{
    public Animator animator;
    public Dictionary<string, AnimatorStateRecorder> taskListDict = new Dictionary<string, AnimatorStateRecorder>(); // 当前待执行的动画集
    public AnimatorStateRecorder currentTask; // 当前任务（当前在播放的动画）
    private BoolNumeric isPauseBoolNumeric = new BoolNumeric(); // 整体动画是否暂停
    
    private BoolModifier gamePauseModifier = new BoolModifier(true); // 游戏暂停的修饰器
    private bool isUseGamePauseModifier; // 是否使用了游戏暂停的修饰器

    private bool isNoPlayOtherClip; // 是否处于不能播放其他动画的状态

    public AnimatorController()
    {

    }

    public void Initialize()
    {
        taskListDict.Clear();
        currentTask = null;
        isUseGamePauseModifier = false;
        isNoPlayOtherClip = false;
        isPauseBoolNumeric.Initialize();
        UpdateSpeed();
    }

    /// <summary>
    /// 暂停整个控制器
    /// </summary>
    public void Pause()
    {
        if (!isUseGamePauseModifier)
        {
            isUseGamePauseModifier = true;
            isPauseBoolNumeric.AddModifier(gamePauseModifier);
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 取消暂停整个控制器
    /// </summary>
    public void Resume()
    {
        if (isUseGamePauseModifier)
        {
            isUseGamePauseModifier = false;
            isPauseBoolNumeric.RemoveModifier(gamePauseModifier);
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 添加整体暂停的修饰器
    /// </summary>
    public void AddPauseModifier(BoolModifier boolModifier)
    {
        isPauseBoolNumeric.AddModifier(boolModifier);
        UpdateSpeed();
    }

    /// <summary>
    /// 移除暂停的修饰器
    /// </summary>
    public void RemovePauseModifier(BoolModifier boolModifier)
    {
        isPauseBoolNumeric.RemoveModifier(boolModifier);
        UpdateSpeed();
    }
    
    /// <summary>
    /// 移除全部的暂停修饰器
    /// </summary>
    public void RemoveAllPauseModifier()
    {
        isPauseBoolNumeric.Initialize();
        UpdateSpeed();
    }

    /// <summary>
    /// 暂停某个动画
    /// </summary>
    public void Pause(string aniName)
    {
        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null && !a.isPause)
        {
            a.isPause = true;
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 取消暂停某个动画
    /// </summary>
    public void Resume(string aniName)
    {
        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null && a.isPause)
        {
            a.isPause = false;
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 播放某个动画，如果有则播放，否则什么事也不发生
    /// </summary>
    /// <param name="aniName"></param>
    public void Play(string aniName, bool isCycle)
    {
        if (isNoPlayOtherClip)
            return;

        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null)
        {
            FinishCurrentTask();
            a.isCycle = isCycle;
            currentTask = a;
            currentTask.isPlaying = true;
            Reset(a); // 从头开始播放
            animator.Play(a.aniName, -1, a.GetNormalizedTime());
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 播放某个动画，如果有则播放，否则什么事也不发生
    /// </summary>
    /// <param name="aniName"></param>
    public void Play(string aniName, bool isCycle, float normalizedTime)
    {
        if (isNoPlayOtherClip)
            return;

        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null)
        {
            FinishCurrentTask();
            a.isCycle = isCycle;
            currentTask = a;
            currentTask.isPlaying = true;
            a.SetNormalizedTime(normalizedTime);
            animator.Play(a.aniName, -1, a.GetNormalizedTime());
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 播放某个动画，如果有则播放，否则什么事也不发生
    /// </summary>
    /// <param name="aniName"></param>
    public void Play(string aniName)
    {
        Play(aniName, false);
    }

    /// <summary>
    /// 设置某个动画的当前播放帧
    /// </summary>
    /// <param name="aniName"></param>
    /// <param name="timer"></param>
    public void SetTimer(string aniName, int timer)
    {
        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null)
        {
            a.timer = timer;
            // 如果动画本身就在播放，则需要重新设置一下
            if (a.IsPlaying())
                animator.Play(a.aniName, -1, a.GetNormalizedTime());
        }
    }

    /// <summary>
    /// 设置某个动画的速度
    /// </summary>
    /// <param name="aniName"></param>
    /// <param name="timer"></param>
    public void SetSpeed(string aniName, float speed)
    {
        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null)
        {
            a.speed = speed;
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 结束当前动画播放
    /// </summary>
    public void FinishCurrentTask()
    {
        if(currentTask!=null)
            currentTask.isPlaying = false;
        currentTask = null;
        UpdateSpeed(); 
    }

    /// <summary>
    /// 当宿主Animator切换时
    /// </summary>
    public void ChangeAnimator(Animator animator)
    {
        this.animator = animator;
        taskListDict.Clear();
    }

    /// <summary>
    /// 遍历所有动画播放的任务
    /// </summary>
    /// <returns></returns>
    public List<AnimatorStateRecorder> GetEachTask()
    {
        List<AnimatorStateRecorder> list = new List<AnimatorStateRecorder>();
        foreach (var item in taskListDict)
        {
            list.Add(item.Value);
        }
        return list;
    }

    public void Update()
    {
        if (currentTask != null)
            currentTask.Update();
    }

    /// <summary>
    /// 获取某个动画状态（如果有）
    /// </summary>
    /// <param name="aniName"></param>
    /// <returns></returns>
    public AnimatorStateRecorder GetAnimatorStateRecorder(string aniName)
    {
        if (aniName == null)
            return null;
        if (taskListDict.ContainsKey(aniName))
        {
            return taskListDict[aniName];
        }
        // 自动读取机制
        int nameHash1 = Animator.StringToHash(aniName);
        if (animator.HasState(0, nameHash1))
        {
            AddTask(animator, aniName, false, 0, 1);
            return taskListDict[aniName];
        }
        // Debug.Log("当前Animator不存在名为{" + aniName + "}的状态");
        return null;
    }

    /// <summary>
    /// 获取当前正在播放的动画
    /// </summary>
    /// <param name="aniName"></param>
    /// <returns></returns>
    public AnimatorStateRecorder GetCurrentAnimatorStateRecorder()
    {
        return currentTask;
    }

    /// <summary>
    /// 是否整体暂停了
    /// </summary>
    /// <returns></returns>
    public bool IsPause()
    {
        return isPauseBoolNumeric.Value;
    }

    /// <summary>
    /// 设置可否切换成别的动画
    /// </summary>
    /// <param name="enable"></param>
    public void SetNoPlayOtherClip(bool enable)
    {
        isNoPlayOtherClip = enable;
    }

    /////////////////////////////////////////////////////////////////////////////////以下为私有方法///////////////////////////////////////////

    /// <summary>
    /// 更新播放速度
    /// </summary>
    private void UpdateSpeed()
    {
        if (animator == null)
            return;
        if (IsPause())
        {
            animator.speed = 0;
            return;
        }
        if (currentTask != null)
        {
            if (currentTask.isPause)
                animator.speed = 0;
            else
                animator.speed = currentTask.speed;
            return;
        }
        animator.speed = 1;
    }

    /// <summary>
    /// 重置播放进度
    /// </summary>
    /// <param name="animatorStateRecorder"></param>
    private void Reset(AnimatorStateRecorder animatorStateRecorder)
    {
        animatorStateRecorder.timer = 0;
    }

    private AnimatorStateRecorder AddTask(Animator animator, string aniName, bool isCycle)
    {
        AnimatorStateRecorder t = new AnimatorStateRecorder(this, aniName, isCycle);
        AddTask(aniName, t);
        return t;
    }

    private AnimatorStateRecorder AddTask(Animator animator, string aniName, bool isCycle, float startTimer)
    {
        AnimatorStateRecorder t = new AnimatorStateRecorder(this, aniName, isCycle, startTimer);
        AddTask(aniName, t);
        return t;
    }

    private AnimatorStateRecorder AddTask(Animator animator, string aniName, bool isCycle, float startTimer, float speed)
    {
        AnimatorStateRecorder t = new AnimatorStateRecorder(this, aniName, isCycle, startTimer, speed);
        AddTask(aniName, t);
        return t;
    }

    private void AddTask(string aniName, AnimatorStateRecorder task)
    {
        taskListDict.Add(aniName, task);
        //UpdateTheMostImportantTask();
    }

    private void RemoveTask(string aniName)
    {
        if (taskListDict.ContainsKey(aniName))
        {
            if (taskListDict[aniName] == currentTask)
                currentTask = null;
        }
        taskListDict.Remove(aniName);
        //UpdateTheMostImportantTask();
    }
}
