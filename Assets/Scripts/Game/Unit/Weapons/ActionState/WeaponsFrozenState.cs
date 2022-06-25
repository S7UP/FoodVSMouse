using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsFrozenState : WeaponsActionState
{
    protected WeaponsActionState lastState; // ��һ��״̬
    private Func<bool> isMeetingExitCondition;

    public WeaponsFrozenState(BaseWeapons baseWeapons, WeaponsActionState baseActionState) : base(baseWeapons)
    {
        lastState = baseActionState;
    }

    public override void OnEnter()
    {
        master.OnFrozenStateEnter();
        master.isFrozenState = true;
        master.AnimatorStop(); // ֹͣ����
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
        master.AnimatorContinue(); // �ſ�����
        master.SetActionState(lastState);
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }

    /// <summary>
    /// �ⶳ��������Ϊ�Զ���
    /// </summary>
    public bool IsMeetingExitCondition()
    {
        return (isMeetingExitCondition != null ? isMeetingExitCondition() : false);
    }

    /// <summary>
    /// ���Խ�����ǰ״̬�������ֶ�����
    /// </summary>
    public void TryExitCurrentState()
    {
        OnExit();
    }
}
