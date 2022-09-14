using UnityEngine;

public class FrozenSlowStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // 当前提供减速效果的修饰器
    private BoolModifier slowDownBoolModifier;

    public FrozenSlowStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 减速效果（检测目标是否免疫减速、冰冻减速)
        if(master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozenSlowDown) || master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
        {
            ClearLeftTime(); // 时间清零
            SetEffectEnable(false);
        }
        else
        {
            SetEffectEnable(true);
        }
    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public override void OnDisableEffect()
    {
        if (slowDownBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
            slowDownBoolModifier = null;
        }

        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        if(slowDownBoolModifier == null)
        {
            // 为目标添加已冰冻减速的标签
            slowDownBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
        }

        if(slowDownFloatModifier == null)
        {
            // 实际减速效果应用
            slowDownFloatModifier = new FloatModifier(-50);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
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
        SetEffectEnable(false);
        // 此效果结束后，如果目标身上已经没有冰冻类减益效果，则移除目标的变色效果
        if (!TagsManager.IsUnitFrozen(master))
        {
            master.SetFrozeSlowEffectEnable(false);
        }
        // 注意，这里是直接从表中移除这个项
        master.RemoveNoCountUniqueStatusAbility(StringManager.FrozenSlowDown);
    }

    /// <summary>
    /// 用于唯一性状态，当状态存在时再被施加同一状态时，调用施加状态的这个方法
    /// </summary>
    public override void OnCover()
    {
        // 为对象添加这个状态前，先检查目标是否已处于这个状态了，如果是直接重置持续时间
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.FrozenSlowDown);
        if (sa != null)
        {
            FrozenSlowStatusAbility f = (sa as FrozenSlowStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // 之后这个对象应该会被系统抛弃，继续沿用原对象（sa)作为目标的状态，这里不需要管它是否被抛弃以及怎么抛弃，知道这件事就行了
        }
    }
}
