/// <summary>
/// 基础水军
/// </summary>
public class NormalWaterMouse : MouseUnit, IInWater
{
    private bool isEnterWater; // 于TransitionState时使用，判断是播放下水动画还是出水动画
    private bool isDieInWater; // 是否在水中死亡

    public override void MInit()
    {
        isEnterWater = false;
        isDieInWater = false;
        base.MInit();
        // 免疫水蚀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }

    public void OnEnterWater()
    {
        isEnterWater = true;
        SetActionState(new TransitionState(this));
        EffectManager.AddWaterWaveEffectToUnit(this);
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        // 出水也要分情况，是有可能死亡后清除DEBUFF效果然后强制出水的
        if (isDeathState)
        {
            isEnterWater = false;
            isDieInWater = true;
        }
        else
        {
            isEnterWater = false;
            SetActionState(new TransitionState(this));
            EffectManager.RemoveWaterWaveEffectFromUnit(this);
        }
    }

    public override void OnTransitionStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Enter");
        else
            animatorController.Play("Exit");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnDieStateEnter()
    {
        if (isDieInWater)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    public override void OnAttackStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Attack1");
        else
            animatorController.Play("Attack0");
    }

    public override void OnIdleStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Idle1");
        else
            animatorController.Play("Idle0");
    }

    public override void AfterDeath()
    {
        EffectManager.RemoveWaterWaveEffectFromUnit(this);
        base.AfterDeath();
    }

    /// <summary>
    /// 对于水上的普通老鼠来说，在水地形的危险权重和陆地是一样的
    /// </summary>
    public override void SetGridDangerousWeightDict()
    {
        GridDangerousWeightDict[GridType.Water] = GridDangerousWeightDict[GridType.Default];
    }
}
