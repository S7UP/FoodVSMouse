using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 改变移速状态
/// </summary>
public class ChangeVelocityStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // 当前提供减速效果的修饰器
    private float changePercent; // 基础移速度改变百分比

    public ChangeVelocityStatusAbility(BaseUnit pmaster, float changePercent) : base(pmaster)
    {
        this.changePercent = changePercent;
        slowDownFloatModifier = new FloatModifier(changePercent);
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 检测 如果是减速效果，以及目标是否免疫减速
        if (changePercent < 0 && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
        {
            // 免疫的话直接禁用效果即可
            SetEffectEnable(false);
        }
        else
        {
            OnEnableEffect();
        }
    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public override void OnDisableEffect()
    {
        master.NumericBox.MoveSpeed.RemovePctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        master.NumericBox.MoveSpeed.AddPctAddModifier(slowDownFloatModifier);
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
        OnDisableEffect();
    }
}
