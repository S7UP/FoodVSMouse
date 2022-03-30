using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseStateImplementor
{
    public void OnIdleStateEnter();
    public void OnMoveStateEnter();
    public void OnAttackStateEnter();
    public void OnStaticStateEnter();
    public void OnCastStateEnter();
    public void OnTransitionStateEnter();
    public void OnIdleState();
    public void OnMoveState();
    public void OnAttackState();
    public void OnStaticState();
    public void OnCastState();
    public void OnTransitionState();
    public void OnIdleStateExit();
    public void OnMoveStateExit();
    public void OnAttackStateExit();
    public void OnStaticStateExit();
    public void OnCastStateExit();
    public void OnTransitionStateExit();
}
