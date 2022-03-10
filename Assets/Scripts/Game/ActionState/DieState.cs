using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目标死亡时，触发时已是duringDeath
/// </summary>
public class DieState : BaseActionState
{
    public DieState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnDieStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.DuringDeath();
    }

    public override void OnExit()
    {
        // 你觉得离开死亡意味着什么？复活？
        // 不，事实上，永远也无法达到离开死亡的真实！因此在正常流程下不应该调用到这里
        Debug.LogWarning("警告：有对象从死亡状态转换到其他状态了！！！");
    }
}
