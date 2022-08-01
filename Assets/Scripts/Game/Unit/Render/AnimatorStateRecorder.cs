/// <summary>
/// ����������ĳ��״̬�ļ�¼��
/// </summary>
public class AnimatorStateRecorder
{
    private AnimatorController animatorController;
    public bool isPlaying; // �ö����Ƿ��ڲ���
    public string aniName; // Ҫ���ŵĶ���Ƭ����
    public float timer; // ��������ʱ����ʼ֡
    public float aniTime = -1; // ����������ʱ�� -1����Animator��������ΪaniName��״̬
    public float speed = 1; // ���������ٶ�
    public bool isCycle; // �Ƿ�ѭ������
    public bool isPause; // �Ƿ���ͣ����

    private AnimatorStateRecorder()
    {
        
    }

    public AnimatorStateRecorder(AnimatorController animatorController, string aniName, bool isCycle)
    {
        this.animatorController = animatorController;
        this.aniName = aniName;
        this.isCycle = isCycle;
        timer = 0;
        aniTime = AnimatorManager.GetClipTime(animatorController.animator, aniName) * 60;
    }

    public AnimatorStateRecorder(AnimatorController animatorController, string aniName, bool isCycle, float startTimer)
    {
        this.animatorController = animatorController;
        this.aniName = aniName;
        this.isCycle = isCycle;
        timer = startTimer;
        aniTime = AnimatorManager.GetClipTime(animatorController.animator, aniName) * 60;
    }

    public AnimatorStateRecorder(AnimatorController animatorController, string aniName, bool isCycle, float startTimer, float speed)
    {
        this.animatorController = animatorController;
        this.aniName = aniName;
        this.isCycle = isCycle;
        timer = startTimer;
        this.speed = speed;
        aniTime = AnimatorManager.GetClipTime(animatorController.animator, aniName) * 60;
    }

    public void Update()
    {
        if (!isPause && (!IsFinishOnce() || isCycle))
            timer+=speed;
    }

    /// <summary>
    /// ��ͣ����
    /// </summary>
    public void Pause()
    {
        isPause = true;
    }

    /// <summary>
    /// �������Ŷ���
    /// </summary>
    public void Resume()
    {
        isPause = false;
    }

    /// <summary>
    /// ��ȡ���Űٷֱ�
    /// </summary>
    /// <returns></returns>
    public float GetNormalizedTime()
    {
        return timer / aniTime;
    }

    /// <summary>
    /// �Ƿ����һ�β���
    /// </summary>
    /// <returns></returns>
    public bool IsFinishOnce()
    {
        return GetNormalizedTime() >= 1.0f;
    }

    /// <summary>
    /// �ö����Ƿ��ڲ���״̬
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return isPlaying;
    }
}
