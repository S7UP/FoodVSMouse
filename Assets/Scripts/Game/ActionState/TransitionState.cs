using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ڴ���ת�׶��޵�֡������״̬
/// </summary>
public class TransitionState : BaseActionState
{
    public TransitionState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnTransitionStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnTransitionState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnTransitionStateExit();
    }

    public override void OnInterrupt()
    {
        mBaseUnit.OnTransitionStateInterrupt();
    }

    public override void OnContinue()
    {
        mBaseUnit.OnTransitionStateContinue();
    }
}
