using System;
/// <summary>
/// 忍者鼠随从
/// </summary>
public class NinjaRetinueMouse : MouseUnit
{
    private BoolModifier IgnoreModifier = new BoolModifier(true); // 出现时无视控制效果
    private static Func<BaseUnit, BaseBullet, bool> noHitedFunc = delegate { return false; }; // 出现时不可被击中
    private static Func<BaseUnit, BaseUnit, bool> noSelectAsTargetFunc = delegate { return false; }; // 出现时不作为目标被选取
    private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; }; // 出现时不阻挡也不攻击

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
