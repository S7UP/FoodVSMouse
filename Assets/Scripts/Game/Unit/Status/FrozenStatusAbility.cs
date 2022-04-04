using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冰冻状态
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    private FrozenState frozenState;
    private BoolModifier boolModifier;

    public FrozenStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 目标动作状态转化为冻结状态
        frozenState = new FrozenState(master, master.mCurrentActionState);
        master.SetActionState(frozenState);
        if(boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public override void OnDisableEffect()
    {
        if (frozenState != null)
        {
            frozenState.TryExitCurrentState();
            frozenState = null;
        }
        if(boolModifier != null)
            master.RemoveDisAbleSkillModifier(boolModifier);
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        // 目标动作状态转化为冻结状态
        if(frozenState == null)
        {
            frozenState = new FrozenState(master, master.mCurrentActionState);
            master.SetActionState(frozenState);
        }
        if (boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
    }

    /// <summary>
    /// 在启用效果期间
    /// </summary>
    public override void OnEffecting()
    {

    }

    /// <summary>
    /// 在非启用效果期间
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// 结束的条件，与持续时间是或关系！
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
        // return false;
    }

    /// <summary>
    /// BUFF结束时要做的事
    /// </summary>
    public override void AfterEffect()
    {
        frozenState.TryExitCurrentState();
        if (boolModifier != null)
            master.RemoveDisAbleSkillModifier(boolModifier);
    }
}
