using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 基础水军
/// </summary>
public class NormalWaterMouse : MouseUnit, IInWater
{
    private bool isEnterWater; // 于TransitionState时使用，判断是播放下水动画还是出水动画

    public override void MInit()
    {
        isEnterWater = false;
        base.MInit();
        // 免疫水蚀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }

    public void OnEnterWater()
    {
        isEnterWater = true;
        SetActionState(new TransitionState(this));
        EffectManager.AddWaterWaveEffectToUnit(this, Vector2.zero);
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        isEnterWater = false;
        SetActionState(new TransitionState(this));
        EffectManager.RemoveWaterWaveEffectFromUnit(this);
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
        // 如果自己在水域中且没有被承载就播放特有的淹死动画
        if (WaterGridType.IsInWater(this) && !Environment.WaterManager.IsBearing(this))
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
        // EffectManager.RemoveWaterWaveEffectFromUnit(this);
        base.AfterDeath();
    }
}
