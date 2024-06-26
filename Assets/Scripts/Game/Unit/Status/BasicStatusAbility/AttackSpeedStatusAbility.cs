using S7P.Numeric;
/// <summary>
/// 改变攻速状态
/// </summary>
public class AttackSpeedStatusAbility : StatusAbility
{
    private FloatModifier floatModifier; // 当前提供减速效果的修饰器
    private float changePercent; // 基础攻速改变百分比

    public AttackSpeedStatusAbility(BaseUnit pmaster, float changePercent) : base(pmaster)
    {
        this.changePercent = changePercent;
        floatModifier = new FloatModifier(changePercent);
    }

    public AttackSpeedStatusAbility(BaseUnit pmaster, float time, float changePercent) : base(pmaster, time)
    {
        this.changePercent = changePercent;
        floatModifier = new FloatModifier(changePercent);
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 检测 如果是减攻速效果，以及目标是否免疫减攻速
        if (changePercent < 0 && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreDecAttackSpeed))
        {
            // 免疫的话直接禁用效果即可
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
        master.NumericBox.AttackSpeed.RemovePctAddModifier(floatModifier);
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        master.NumericBox.AttackSpeed.AddPctAddModifier(floatModifier);
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
    }
}
