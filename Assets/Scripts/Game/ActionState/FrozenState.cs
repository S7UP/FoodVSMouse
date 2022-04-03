using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����״̬
/// ����һ����״̬Ӱ�쵥λ��Ϊ��״̬
/// </summary>
public class FrozenState : BaseActionState
{
    protected IBaseActionState lastState; // ��һ��״̬
    private Func<bool> isMeetingExitCondition;

    public FrozenState(BaseUnit baseUnit, IBaseActionState baseActionState) : base(baseUnit)
    {
        lastState = baseActionState;
    }

    public FrozenState(BaseUnit baseUnit, IBaseActionState baseActionState, Func<bool> isMeetingExitCondition) : base(baseUnit)
    {
        lastState = baseActionState;
        this.isMeetingExitCondition = isMeetingExitCondition;
    }

    public override void OnEnter()
    {
        mBaseUnit.isFrozenState = true;
        mBaseUnit.AnimatorStop(); // ֹͣ����
    }

    public override void OnUpdate()
    {
        if (IsMeetingExitCondition())
            TryExitCurrentState();
    }

    /// <summary>
    /// �ⶳ��������Ϊ�Զ���
    /// </summary>
    public bool IsMeetingExitCondition()
    {
        return (isMeetingExitCondition != null ? isMeetingExitCondition():false);
    }

    /// <summary>
    /// ���Խ�����ǰ״̬�������ֶ�����
    /// </summary>
    public void TryExitCurrentState()
    {
        OnExit();
    }

    public override void OnExit()
    {
        mBaseUnit.isFrozenState = false;
        mBaseUnit.AnimatorContinue(); // �ſ�����
        mBaseUnit.SetActionState(lastState);
    }
}
