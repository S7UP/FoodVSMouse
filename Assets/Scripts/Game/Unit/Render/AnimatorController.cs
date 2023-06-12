using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

/// <summary>
/// ����λ�������������Ϣ�Ķ���
/// </summary>
public class AnimatorController
{
    public Animator animator;
    public Dictionary<string, AnimatorStateRecorder> taskListDict = new Dictionary<string, AnimatorStateRecorder>(); // ��ǰ��ִ�еĶ�����
    public AnimatorStateRecorder currentTask; // ��ǰ���񣨵�ǰ�ڲ��ŵĶ�����
    private BoolNumeric isPauseBoolNumeric = new BoolNumeric(); // ���嶯���Ƿ���ͣ
    
    private BoolModifier gamePauseModifier = new BoolModifier(true); // ��Ϸ��ͣ��������
    private bool isUseGamePauseModifier; // �Ƿ�ʹ������Ϸ��ͣ��������

    private bool isNoPlayOtherClip; // �Ƿ��ڲ��ܲ�������������״̬

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
    /// ��ͣ����������
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
    /// ȡ����ͣ����������
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
    /// ���������ͣ��������
    /// </summary>
    public void AddPauseModifier(BoolModifier boolModifier)
    {
        isPauseBoolNumeric.AddModifier(boolModifier);
        UpdateSpeed();
    }

    /// <summary>
    /// �Ƴ���ͣ��������
    /// </summary>
    public void RemovePauseModifier(BoolModifier boolModifier)
    {
        isPauseBoolNumeric.RemoveModifier(boolModifier);
        UpdateSpeed();
    }
    
    /// <summary>
    /// �Ƴ�ȫ������ͣ������
    /// </summary>
    public void RemoveAllPauseModifier()
    {
        isPauseBoolNumeric.Initialize();
        UpdateSpeed();
    }

    /// <summary>
    /// ��ͣĳ������
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
    /// ȡ����ͣĳ������
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
    /// ����ĳ��������������򲥷ţ�����ʲô��Ҳ������
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
            Reset(a); // ��ͷ��ʼ����
            animator.Play(a.aniName, -1, a.GetNormalizedTime());
            UpdateSpeed();
        }
    }

    /// <summary>
    /// ����ĳ��������������򲥷ţ�����ʲô��Ҳ������
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
    /// ����ĳ��������������򲥷ţ�����ʲô��Ҳ������
    /// </summary>
    /// <param name="aniName"></param>
    public void Play(string aniName)
    {
        Play(aniName, false);
    }

    /// <summary>
    /// ����ĳ�������ĵ�ǰ����֡
    /// </summary>
    /// <param name="aniName"></param>
    /// <param name="timer"></param>
    public void SetTimer(string aniName, int timer)
    {
        AnimatorStateRecorder a = GetAnimatorStateRecorder(aniName);
        if (a != null)
        {
            a.timer = timer;
            // �������������ڲ��ţ�����Ҫ��������һ��
            if (a.IsPlaying())
                animator.Play(a.aniName, -1, a.GetNormalizedTime());
        }
    }

    /// <summary>
    /// ����ĳ���������ٶ�
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
    /// ������ǰ��������
    /// </summary>
    public void FinishCurrentTask()
    {
        if(currentTask!=null)
            currentTask.isPlaying = false;
        currentTask = null;
        UpdateSpeed(); 
    }

    /// <summary>
    /// ������Animator�л�ʱ
    /// </summary>
    public void ChangeAnimator(Animator animator)
    {
        this.animator = animator;
        taskListDict.Clear();
    }

    /// <summary>
    /// �������ж������ŵ�����
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
    /// ��ȡĳ������״̬������У�
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
        // �Զ���ȡ����
        int nameHash1 = Animator.StringToHash(aniName);
        if (animator.HasState(0, nameHash1))
        {
            AddTask(animator, aniName, false, 0, 1);
            return taskListDict[aniName];
        }
        // Debug.Log("��ǰAnimator��������Ϊ{" + aniName + "}��״̬");
        return null;
    }

    /// <summary>
    /// ��ȡ��ǰ���ڲ��ŵĶ���
    /// </summary>
    /// <param name="aniName"></param>
    /// <returns></returns>
    public AnimatorStateRecorder GetCurrentAnimatorStateRecorder()
    {
        return currentTask;
    }

    /// <summary>
    /// �Ƿ�������ͣ��
    /// </summary>
    /// <returns></returns>
    public bool IsPause()
    {
        return isPauseBoolNumeric.Value;
    }

    /// <summary>
    /// ���ÿɷ��л��ɱ�Ķ���
    /// </summary>
    /// <param name="enable"></param>
    public void SetNoPlayOtherClip(bool enable)
    {
        isNoPlayOtherClip = enable;
    }

    /////////////////////////////////////////////////////////////////////////////////����Ϊ˽�з���///////////////////////////////////////////

    /// <summary>
    /// ���²����ٶ�
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
    /// ���ò��Ž���
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
