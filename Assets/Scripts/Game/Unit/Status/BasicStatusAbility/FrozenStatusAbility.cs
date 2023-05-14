using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 冰冻状态
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    // private FrozenState frozenState;
    // private BoolModifier boolModifier;
    private BoolModifier frozenBoolModifier;
    //private StunStatusAbility stunStatusAbility; // 晕眩效果
    private bool isForce = false; // 是否强制生效（无视免疫效果

    public FrozenStatusAbility(BaseUnit pmaster, float time, bool isForce) : base(pmaster, time)
    {
        this.isForce = isForce;
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 检查是否免疫冻结
        if (!isForce && (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen) || master.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun)))
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
        if (frozenBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
            frozenBoolModifier = null;
        }
        // 移除晕眩效果
        master.RemoveNoCountUniqueStatusAbility(StringManager.Stun);
        // 移除冰冻特效
        if (master.IsContainEffect(StringManager.Frozen))
        {
            master.RemoveEffectFromDict(StringManager.Frozen);
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
        // 为目标施加晕眩效果
        StatusAbility s = master.GetNoCountUniqueStatus(StringManager.Stun);
        if (s == null)
        {
            s = new StunStatusAbility(master, leftTime, isForce);
            master.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
        }
        else
        {
            s.leftTime = Mathf.Max(s.leftTime, this.leftTime);
            s.totalTime = (s.totalTime.baseValue > this.totalTime.baseValue ? s.totalTime : this.totalTime);
        }
        // 添加变色效果
        master.SetFrozeSlowEffectEnable(true);
        // 添加冰冻特效
        if (!master.IsContainEffect(StringManager.Frozen))
        {
            GameManager.Instance.audioSourceManager.PlayEffectMusic("Frozen");

            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/Frozen"), null, "Frozen", "Break", true);
            GameController.Instance.AddEffect(e);
            string name;
            int order;
            if (master.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 5);
            }
            master.AddEffectToDict(StringManager.Frozen, e, new Vector2(0, 0));
        }
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
        if (!StatusManager.IsUnitFrozen(master))
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
            // 晕眩时长也要重置！
            StatusAbility s = master.GetNoCountUniqueStatus(StringManager.Stun);
            if (s == null)
                s = new StunStatusAbility(master, f.totalTime.baseValue, false);
            s.totalTime = (s.totalTime.baseValue > this.totalTime.baseValue ? s.totalTime : this.totalTime);
            s.leftTime = Mathf.Max(s.leftTime, this.leftTime);
            master.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
        }
    }
}
