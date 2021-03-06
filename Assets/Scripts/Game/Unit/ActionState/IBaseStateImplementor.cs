public interface IBaseStateImplementor
{
    public void OnIdleStateEnter();
    public void OnMoveStateEnter();
    public void OnAttackStateEnter();
    public void OnCastStateEnter();
    public void OnTransitionStateEnter();
    public void OnIdleState();
    public void OnMoveState();
    public void OnAttackState();
    public void OnCastState();
    public void OnTransitionState();
    public void OnIdleStateExit();
    public void OnMoveStateExit();
    public void OnAttackStateExit();
    public void OnCastStateExit();
    public void OnTransitionStateExit();
    public void OnIdleStateInterrupt();
    public void OnMoveStateInterrupt();
    public void OnAttackStateInterrupt();
    public void OnCastStateInterrupt();
    public void OnTransitionStateInterrupt();
    public void OnIdleStateContinue();
    public void OnMoveStateContinue();
    public void OnAttackStateContinue();
    public void OnCastStateContinue();
    public void OnTransitionStateContinue();
}
