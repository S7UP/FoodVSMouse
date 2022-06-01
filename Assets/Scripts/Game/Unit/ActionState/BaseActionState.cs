using System.Collections;
using System.Collections.Generic;
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

    public virtual void OnInterrupt()
    {
        
    }

    public virtual void OnContinue()
    {
        
    }
}