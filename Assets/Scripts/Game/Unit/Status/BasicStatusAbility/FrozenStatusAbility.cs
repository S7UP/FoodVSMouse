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
    private BoolModifier frozenBoolModifier;

    public FrozenStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 检查是否免疫冻结
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            ClearLeftTime();
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
        if (frozenBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
            frozenBoolModifier = null;
        }
        if (frozenState != null)
        {
            frozenState.TryExitCurrentState();
            frozenState = null;
        }
        if (boolModifier != null)
        {
            master.RemoveDisAbleSkillModifier(boolModifier);
            boolModifier = null;
        }
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        // 为目标添加被冻结的标签
        if(frozenBoolModifier == null)
        {
            frozenBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
        }

        // 目标动作状态转化为冻结状态
        if (frozenState == null)
        {
            frozenState = new FrozenState(master, master.mCurrentActionState);
            master.SetActionState(frozenState);
        }
        if (boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
        // 添加变色效果
        master.SetFrozeSlowEffectEnable(true);
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
        // 此效果结束后，如果目标身上已经没有冰冻类减益效果，则移除目标的变色效果
        if (!TagsManager.IsUnitFrozen(master))
        {
            master.SetFrozeSlowEffectEnable(false);
        }
        master.RemoveNoCountUniqueStatusAbility(StringManager.Frozen);
    }

    /// <summary>
    /// 用于唯一性状态，当状态存在时再被施加同一状态时，调用施加状态的这个方法
    /// </summary>
    public override void OnCover()
    {
        // 为对象添加这个状态前，先检查目标是否已处于这个状态了，如果是直接重置持续时间
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.Frozen);
        if (sa != null)
        {
            FrozenStatusAbility f = (sa as FrozenStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // 之后这个对象应该会被系统抛弃，继续沿用原对象（sa)作为目标的状态，这里不需要管它是否被抛弃以及怎么抛弃，知道这件事就行了
        }
    }
}
