using System;
/// <summary>
/// ���������
/// </summary>
public class NinjaRetinueMouse : MouseUnit
{
    private BoolModifier IgnoreModifier = new BoolModifier(true); // ����ʱ���ӿ���Ч��
    private static Func<BaseUnit, BaseBullet, bool> noHitedFunc = delegate { return false; }; // ����ʱ���ɱ�����
    private static Func<BaseUnit, BaseUnit, bool> noSelectAsTargetFunc = delegate { return false; }; // ����ʱ����ΪĿ�걻ѡȡ
    private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; }; // ����ʱ���赲Ҳ������

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
        //CloseCollision();
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        AddCanHitFunc(noHitedFunc);
        AddCanBlockFunc(noBlockFunc);
        AddCanBeSelectedAsTargetFunc(noSelectAsTargetFunc);
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
        // OpenCollision();
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        RemoveCanHitFunc(noHitedFunc);
        RemoveCanBlockFunc(noBlockFunc);
        RemoveCanBeSelectedAsTargetFunc(noSelectAsTargetFunc);
    }
}
