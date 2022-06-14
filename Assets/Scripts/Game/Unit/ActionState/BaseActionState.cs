using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IBaseActionState
{
    // �״ν������״̬
    public void OnEnter();
    // �˳����״̬
    public void OnExit();
    // ���״̬��֡����
    public void OnUpdate();
    // ���״̬���ж�ʱ���¼������ڻָ��Ŀ����ԣ���ͣ��
    public void OnInterrupt();
    // ���״̬���жϵ��ָ�ʱ���¼�
    public void OnContinue();
}

public class BaseActionState: IBaseActionState
{
    // ���е�Unit����
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

    // ������ʱ
    public virtual void OnEnter()
    {
        if (EnterAction != null)
            EnterAction();
    }

    // ���˳�ʱ
    public virtual void OnExit()
    {
        if (ExitAction != null)
            ExitAction();
    }

    // ʵ�ֶ���״̬
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