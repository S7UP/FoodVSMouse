using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

using UnityEngine;
/// <summary>
/// ����������������
/// </summary>
public class AnimatorControllerTask
{
    public enum Type
    {
        AttackClip, // ��������
        IdleClip, // ֹͣ����
        MoveClip, // �ƶ�����
        FrozenClip, // ���ᶯ��
        DieClip, // ��������
    }

    public Animator animator;
    public bool isIncreaseTimerWithoutPlaying; // ��ʱ���������Ƿ���Բ���״̬
    public bool isPlaying; // �ö����Ƿ��ڲ���
    public string clipName; // Ҫ���ŵĶ���Ƭ����
    private int timer; // ��������ʱ����ʼ֡
    private float speed = 1; // ���������ٶ�

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
    /// ���Ŷ���
    /// </summary>
    public void PlayClip()
    {
        animator.Play(clipName, -1, timer/ConfigManager.fps/ AnimatorManager.GetClipTime(animator, clipName));
        animator.speed = speed;
    }

    public void SetTimer(int timer)
    {
        this.timer = timer;
        // �������������ڲ��ţ�����Ҫ��������һ��
        if (isPlaying)
            PlayClip();
    }

    public void SetSpeed(int speed)
    {
        this.speed = speed;
        // �������������ڲ��ţ�����Ҫ��������һ��
        if (isPlaying)
            PlayClip();
    }
}
