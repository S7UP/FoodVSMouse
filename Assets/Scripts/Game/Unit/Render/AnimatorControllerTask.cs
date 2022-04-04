using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

using UnityEngine;
/// <summary>
/// 动画控制器的任务
/// </summary>
public class AnimatorControllerTask
{
    public enum Type
    {
        AttackClip, // 攻击动画
        IdleClip, // 停止动画
        MoveClip, // 移动动画
        FrozenClip, // 冻结动画
        DieClip, // 死亡动画
    }

    public Animator animator;
    public bool isIncreaseTimerWithoutPlaying; // 计时器的增长是否忽略播放状态
    public bool isPlaying; // 该动画是否在播放
    public string clipName; // 要播放的动画片段名
    private int timer; // 动画播放时的起始帧
    private float speed = 1; // 动画播放速度

    private AnimatorControllerTask()
    {

    }

    public AnimatorControllerTask(string clipName)
    {
        this.clipName = clipName;
        timer = 0;
    }

    public AnimatorControllerTask(string clipName, int startTimer)
    {
        this.clipName = clipName;
        timer = startTimer;
    }

    public AnimatorControllerTask(string clipName, int startTimer, float speed)
    {
        this.clipName = clipName;
        timer = startTimer;
        this.speed = speed;
    }

    public void Update()
    {
        if(isIncreaseTimerWithoutPlaying || isPlaying)
            timer++;
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    public void PlayClip()
    {
        animator.Play(clipName, -1, timer/ConfigManager.fps/ AnimatorManager.GetClipTime(animator, clipName));
        animator.speed = speed;
    }

    public void SetTimer(int timer)
    {
        this.timer = timer;
        // 如果动画本身就在播放，则需要重新设置一下
        if (isPlaying)
            PlayClip();
    }

    public void SetSpeed(int speed)
    {
        this.speed = speed;
        // 如果动画本身就在播放，则需要重新设置一下
        if (isPlaying)
            PlayClip();
    }
}
