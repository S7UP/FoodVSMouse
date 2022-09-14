using UnityEngine;

/// <summary>
/// 晕眩状态
/// </summary>
public class StunStatusAbility : StatusAbility
{
    private FrozenState frozenState;
    private BoolModifier boolModifier;
    private BoolModifier stunBoolModifier = new BoolModifier(true);
    private bool isForce = false; // 是否强制生效（无视免疫效果

    public StunStatusAbility(BaseUnit pmaster, float time, bool isForce) : base(pmaster, time)
    {
        this.isForce = isForce;
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 检查是否免疫冻结
        if (!isForce && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun))
        {
            ClearLeftTime();
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
        master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Stun, stunBoolModifier);
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
        // 为目标添加被击晕的标签
        master.NumericBox.AddDecideModifierToBoolDict(StringManager.Stun, stunBoolModifier);

        // 目标动作状态转化为击晕状态
        if (frozenState == null)
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
    }

    /// <summary>
    /// BUFF结束时要做的事
    /// </summary>
    public override void AfterEffect()
    {
        SetEffectEnable(false);

        master.RemoveNoCountUniqueStatusAbility(StringManager.Stun);
    }

    /// <summary>
    /// 用于唯一性状态，当状态存在时再被施加同一状态时，调用施加状态的这个方法
    /// </summary>
    public override void OnCover()
    {
        // 为对象添加这个状态前，先检查目标是否已处于这个状态了，如果是直接重置持续时间
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.Stun);
        if (sa != null)
        {
            StunStatusAbility f = (sa as StunStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // 之后这个对象应该会被系统抛弃，继续沿用原对象（sa)作为目标的状态，这里不需要管它是否被抛弃以及怎么抛弃，知道这件事就行了
        }
    }
}
