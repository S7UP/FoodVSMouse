using System;

/// <summary>
/// 冻结状态
/// 这是一个由状态影响单位行为的状态
/// </summary>
public class FrozenState : BaseActionState
{
    protected IBaseActionState lastState; // 上一个状态
    private Func<bool> isMeetingExitCondition;

    public FrozenState(BaseUnit baseUnit, IBaseActionState baseActionState) : base(baseUnit)
    {
        lastState = baseActionState;
    }

    public FrozenState(BaseUnit baseUnit, IBaseActionState baseActionState, Func<bool> isMeetingExitCondition) : base(baseUnit)
    {
        lastState = baseActionState;
        this.isMeetingExitCondition = isMeetingExitCondition;
    }

    public override void OnEnter()
    {
        mBaseUnit.OnFrozenStateEnter();
        mBaseUnit.isFrozenState = true;
        // mBaseUnit.PauseCurrentAnimatorState(boolModifier); // 停止动画
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnFrozenState();
        if (IsMeetingExitCondition())
            TryExitCurrentState();
    }

    /// <summary>
    /// 解冻的条件，为自动档
    /// </summary>
    public bool IsMeetingExitCondition()
    {
        return (isMeetingExitCondition != null ? isMeetingExitCondition():false);
    }

    /// <summary>
    /// 尝试结束当前状态，允许手动调用
    /// </summary>
    public void TryExitCurrentState()
    {
        OnExit();
    }

    public override void OnExit()
    {
        mBaseUnit.OnFrozenStateExit();
        mBaseUnit.isFrozenState = false;
        // mBaseUnit.ResumeCurrentAnimatorState(boolModifier); // 放开动画
        mBaseUnit.SetActionState(lastState);
    }
}
