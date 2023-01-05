/// <summary>
/// 减速效果
/// </summary>
public class SlowStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // 当前提供减速效果的修饰器
    private BoolModifier slowDownBoolModifier = new BoolModifier(true);

    public SlowStatusAbility(float PercentValue, BaseUnit pmaster, float time) : base(pmaster, time)
    {
        slowDownFloatModifier = new FloatModifier(PercentValue);
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 减速效果（检测目标是否免疫减速)
        if(master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
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
        master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.SlowDown, slowDownBoolModifier);
        master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        // 为目标添加已冰冻减速的标签
        master.NumericBox.AddDecideModifierToBoolDict(StringManager.SlowDown, slowDownBoolModifier);
        // 减速
        master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
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
    }
}
