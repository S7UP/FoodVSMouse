/// <summary>
/// 动画控制器某个状态的记录者
/// </summary>
public class AnimatorStateRecorder
{
    private AnimatorController animatorController;
    public bool isPlaying; // 该动画是否在播放
    public string aniName; // 要播放的动画片段名
    public float timer; // 动画播放时的起始帧
    public float aniTime = -1; // 动画播放总时长 -1代表Animator不存在名为aniName的状态
    public float speed = 1; // 动画播放速度
    public bool isCycle; // 是否循环播放
    public bool isPause; // 是否暂停播放

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
    /// 暂停动画
    /// </summary>
    public void Pause()
    {
        isPause = true;
    }

    /// <summary>
    /// 继续播放动画
    /// </summary>
    public void Resume()
    {
        isPause = false;
    }

    /// <summary>
    /// 获取播放百分比
    /// </summary>
    /// <returns></returns>
    public float GetNormalizedTime()
    {
        return timer / aniTime;
    }

    /// <summary>
    /// 是否完成一次播放
    /// </summary>
    /// <returns></returns>
    public bool IsFinishOnce()
    {
        return GetNormalizedTime() >= 1.0f;
    }

    /// <summary>
    /// 该动画是否在播放状态
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return isPlaying;
    }
}
