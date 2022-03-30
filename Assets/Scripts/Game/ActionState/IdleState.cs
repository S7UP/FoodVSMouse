using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseActionState
{
    public IdleState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnIdleStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnIdleState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnIdleStateExit();
    }
}
