/// <summary>
/// –‹√® ÛÀÊ¥”
/// </summary>
public class PandaRetinueMouse : MouseUnit
{
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Fly", true);
    }

    public override void OnTransitionState()
    {
        //if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        //{
        //    SetActionState(new MoveState(this));
        //}
    }
}
