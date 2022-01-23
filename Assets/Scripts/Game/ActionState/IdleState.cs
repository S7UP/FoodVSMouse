using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseActionState
{
    protected Animator animator;

    public IdleState(BaseUnit baseUnit) : base(baseUnit)
    {
        animator = baseUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        animator.Play("Idle");
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnIdleState();
    }

}
