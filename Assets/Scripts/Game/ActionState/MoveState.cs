using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseActionState
{

    public MoveState(BaseUnit baseUnit):base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnMoveStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnMoveState();
    }
}
