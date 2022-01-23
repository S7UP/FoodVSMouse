using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseActionState
{
    protected Animator animator;

    public MoveState(BaseUnit baseUnit):base(baseUnit)
    {
        animator = baseUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        animator.Play("Move");
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnMoveState();
    }
}
