using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastState : BaseActionState
{
    public CastState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnCastStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnCastState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnCastStateExit();
    }
}
