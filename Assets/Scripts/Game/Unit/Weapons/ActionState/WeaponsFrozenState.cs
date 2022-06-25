using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsFrozenState : WeaponsActionState
{
    protected WeaponsActionState lastState; // 上一个状态
    private Func<bool> isMeetingExitCondition;

    public WeaponsFrozenState(BaseWeapons baseWeapons, WeaponsActionState baseActionState) : base(baseWeapons)
    {
        lastState = baseActionState;
    }

    public override void OnEnter()
    {
        master.OnFrozenStateEnter();
        master.isFrozenState = true;
        master.AnimatorStop(); // 停止动画
    }

    public override void OnUpdate()
    {
        master.OnFrozenState();
        if (IsMeetingExitCondition())
            TryExitCurrentState();
    }

    public override void OnExit()
    {
        master.OnFrozenStateExit();
        master.isFrozenState = false;
        master.AnimatorContinue(); // 放开动画
        master.SetActionState(lastState);
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }

    /// <summary>
    /// 解冻的条件，为自动档
    /// </summary>
    public bool IsMeetingExitCondition()
    {
        return (isMeetingExitCondition != null ? isMeetingExitCondition() : false);
    }

    /// <summary>
    /// 尝试结束当前状态，允许手动调用
    /// </summary>
    public void TryExitCurrentState()
    {
        OnExit();
    }
}
