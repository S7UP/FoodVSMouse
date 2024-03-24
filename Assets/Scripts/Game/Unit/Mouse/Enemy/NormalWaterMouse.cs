using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ����ˮ��
/// </summary>
public class NormalWaterMouse : MouseUnit, IInWater
{
    private bool isEnterWater; // ��TransitionStateʱʹ�ã��ж��ǲ�����ˮ�������ǳ�ˮ����

    public override void MInit()
    {
        isEnterWater = false;
        base.MInit();
        // ����ˮʴЧ��
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
        // ����Լ���ˮ������û�б����ؾͲ������е���������
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
