/// <summary>
/// 忍者鼠随从
/// </summary>
public class NinjaRetinueMouse : MouseUnit
{

    /// <summary>
    /// 播放拉取动画
    /// </summary>
    public void PlayRectifyClip()
    {
        animatorController.Play("Rectify");
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Appear");
        CloseCollision();
    }

    public override void OnTransitionState()
    {
        if (currentStateTimer == 0)
            return;

        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnTransitionStateExit()
    {
        OpenCollision();
    }
}
