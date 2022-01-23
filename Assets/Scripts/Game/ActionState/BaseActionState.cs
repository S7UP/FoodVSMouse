using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseActionState
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
}

public class BaseActionState: IBaseActionState
{
    // ���е�Unit����
    protected BaseUnit mBaseUnit;

    public BaseActionState(BaseUnit baseUnit)
    {
        mBaseUnit = baseUnit;
    }

    // ������ʱ
    public virtual void OnEnter()
    {

    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {

    }
}