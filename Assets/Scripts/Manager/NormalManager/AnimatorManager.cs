using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 处理Ani的工具
/// </summary>
public class AnimatorManager
{
    /// <summary>
    /// 获取当前动画播放进度(整数部分为已循环次数，小数部分为当前循环内播放百分比)
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static float GetNormalizedTime(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// 获取动画每帧的持续时长
    /// </summary>
    /// <returns></returns>
    public static float GetLengthPerFrame(Animator animator)
    {
        return 1/ animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
    }

    /// <summary>
    /// 计算一个Animator当前动画Clip的总帧数
    /// </summary>
    /// <returns></returns>
    public static int GetTotalFrame(Animator animator)
    {
        float frameRate = animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate; // 帧频（每秒有多少帧）
        float length = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length; // 动画时长（秒）
        return Mathf.CeilToInt(frameRate*length);
    }

    /// <summary>
    /// 计算一个Animator当前动画Clip播放第几帧（默认时间为0时初始为第1帧）
    /// 因此，当前帧与总帧数求余为0时，不代表动画播放完，而代表的是动画播放到最后一帧，如果要判定动画播放完应当是求余为1时。
    /// 比如一个13帧的动画，求出播放帧为13，代表在播放第13帧，而要判断播放完一刻的时机应当是求出当前帧为13+1=14
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentFrame(Animator animator)
    {
        return 1 + Mathf.FloorToInt(GetNormalizedTime(animator) * animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
    }
}
