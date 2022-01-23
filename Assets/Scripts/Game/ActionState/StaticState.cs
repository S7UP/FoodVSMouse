using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticState : BaseActionState
{
    protected Animator animator;
    protected BaseActionState lastState; // 上一个状态

    public StaticState(BaseUnit baseUnit, BaseActionState baseActionState) : base(baseUnit)
    {
        animator = baseUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        lastState = baseActionState;
    }

    public override void OnEnter()
    {
        animator.speed = 0; // 定住动画
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnStaticState();
    }

    public override void OnExit()
    {
        animator.speed = 1; // 恢复动画
        mBaseUnit.SetActionState(lastState);
    }
}
