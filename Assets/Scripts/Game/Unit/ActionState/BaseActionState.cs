using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IBaseActionState
{
    // 首次进入这个状态
    public void OnEnter();
    // 退出这个状态
    public void OnExit();
    // 这个状态的帧更新
    public void OnUpdate();
    // 这个状态被中断时的事件，存在恢复的可能性（暂停）
    public void OnInterrupt();
    // 这个状态从中断到恢复时的事件
    public void OnContinue();
}

public class BaseActionState: IBaseActionState
{
    // 持有的Unit引用
    protected BaseUnit mBaseUnit;
    private Action EnterAction;
    private Action UpdateAction;
    private Action ExitAction;
    private Action InterruptAction;
    private Action ContinueAction;

    public BaseActionState(BaseUnit baseUnit)
    {
        mBaseUnit = baseUnit;
    }

    // 当进入时
    public virtual void OnEnter()
    {
        if (EnterAction != null)
            EnterAction();
    }

    // 当退出时
    public virtual void OnExit()
    {
        if (ExitAction != null)
            ExitAction();
    }

    // 实现动作状态
    public virtual void OnUpdate()
    {
        if (UpdateAction != null)
            UpdateAction();
    }

    public virtual void OnInterrupt()
    {
        if (InterruptAction != null)
            InterruptAction();
    }

    public virtual void OnContinue()
    {
        if (ContinueAction != null)
            ContinueAction();
    }


    public void SetEnterAction(Action action)
    {
        EnterAction = action;
    }

    public void SetExitAction(Action action)
    {
        ExitAction = action;
    }

    public void SetUpdateAction(Action action)
    {
        UpdateAction = action;
    }

    public void SetInterruptAction(Action action)
    {
        InterruptAction = action;
    }

    public void SetContinueAction(Action action)
    {
        ContinueAction = action;
    }
}