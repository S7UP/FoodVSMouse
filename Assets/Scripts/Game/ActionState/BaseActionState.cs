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
    // 持有的Unit引用
    protected BaseUnit mBaseUnit;

    public BaseActionState(BaseUnit baseUnit)
    {
        mBaseUnit = baseUnit;
    }

    // 当进入时
    public virtual void OnEnter()
    {

    }

    // 当退出时
    public virtual void OnExit()
    {

    }

    // 实现动作状态
    public virtual void OnUpdate()
    {

    }
}