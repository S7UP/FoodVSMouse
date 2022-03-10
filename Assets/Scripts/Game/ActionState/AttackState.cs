using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseActionState
{
    public AttackState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnAttackStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnAttackState();
    }
}
