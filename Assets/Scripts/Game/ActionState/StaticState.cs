using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticState : BaseActionState
{
    protected BaseActionState lastState; // ÉÏÒ»¸ö×´Ì¬

    public StaticState(BaseUnit baseUnit, BaseActionState baseActionState) : base(baseUnit)
    {
        lastState = baseActionState;
    }

    public override void OnEnter()
    {
        mBaseUnit.OnStaticStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnStaticState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnStaticStateExit();
        mBaseUnit.SetActionState(lastState);
    }
}
