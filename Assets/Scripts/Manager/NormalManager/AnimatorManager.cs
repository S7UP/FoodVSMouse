using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����Ani�Ĺ���
/// </summary>
public class AnimatorManager
{
    /// <summary>
    /// ��ȡ��ǰ�������Ž���(��������Ϊ��ѭ��������С������Ϊ��ǰѭ���ڲ��Űٷֱ�)
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static float GetNormalizedTime(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// ��ȡ����ÿ֡�ĳ���ʱ��
    /// </summary>
    /// <returns></returns>
    public static float GetLengthPerFrame(Animator animator)
    {
        return 1/ animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
    }

    /// <summary>
    /// ����һ��Animator��ǰ����Clip����֡��
    /// </summary>
    /// <returns></returns>
    public static int GetTotalFrame(Animator animator)
    {
        float frameRate = animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate; // ֡Ƶ��ÿ���ж���֡��
        float length = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length; // ����ʱ�����룩
        return Mathf.CeilToInt(frameRate*length);
    }

    /// <summary>
    /// ����һ��Animator��ǰ����Clip���ŵڼ�֡��Ĭ��ʱ��Ϊ0ʱ��ʼΪ��1֡��
    /// ��ˣ���ǰ֡����֡������Ϊ0ʱ���������������꣬��������Ƕ������ŵ����һ֡�����Ҫ�ж�����������Ӧ��������Ϊ1ʱ��
    /// ����һ��13֡�Ķ������������֡Ϊ13�������ڲ��ŵ�13֡����Ҫ�жϲ�����һ�̵�ʱ��Ӧ���������ǰ֡Ϊ13+1=14
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentFrame(Animator animator)
    {
        return 1 + Mathf.FloorToInt(GetNormalizedTime(animator) * animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
    }
}
