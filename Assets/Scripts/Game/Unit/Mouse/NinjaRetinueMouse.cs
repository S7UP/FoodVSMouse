/// <summary>
/// ���������
/// </summary>
public class NinjaRetinueMouse : MouseUnit
{

    /// <summary>
    /// ������ȡ����
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

        if (AnimatorManager.GetNormalizedTime(animator) > 1.0f)
            SetActionState(new MoveState(this));
    }

    public override void OnTransitionStateExit()
    {
        OpenCollision();
    }
}
