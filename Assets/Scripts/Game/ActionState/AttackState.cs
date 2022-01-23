using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseActionState
{
    protected Animator animator;

    public AttackState(BaseUnit baseUnit) : base(baseUnit)
    {
        animator = baseUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        animator.Play("Attack");
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnAttackState();
    }
}
