using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ����������
/// </summary>
public class FrogMouse : MouseUnit, IInWater
{
    private bool isJumped; // �Ƿ�����һ��
    private bool isInWater; // �Ƿ�λ��ˮ��
    private FloatModifier FrogSpeedModifier = new FloatModifier(200); // �����ܴ������ƶ��ٶȼӳ�
    private FloatModifier FrogInWaterSpeedModifier = new FloatModifier(200); // ��������ˮ���ٴ������ƶ��ٶȼӳɣ�������������㣩

    public override void MInit()
    {
        isJumped = false;
        isInWater = false;
        base.MInit();
        // ��ʼ���Я�����ܵ��ƶ��ٶ�
        NumericBox.MoveSpeed.AddPctAddModifier(FrogSpeedModifier);
        // ����ˮʴ
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }

    public override bool IsDamageJudgment()
    {
        if (!isJumped)
            return true;
        else
            return base.IsDamageJudgment();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ��δ���е�һ����Ծʱ����ͨ�������滻Ϊ��Ծ
        if (!isJumped)
        {
            // ���һ����������
            isJumped = true;
            // ��Ծ���������� 0.5*��ǰ�ƶ��ٶȱ�׼ֵ
            float v = TransManager.TranToStandardVelocity(GetMoveSpeed());
            float dist;
            if (transform.position.x <= MapManager.GetColumnX(0.5f))
                dist = Mathf.Min(0.5f * v * MapManager.gridWidth, 1.1f * MapManager.gridWidth);
            else
                dist = Mathf.Min(0.5f * v * MapManager.gridWidth, Mathf.Abs(transform.position.x - MapManager.GetColumnX(0)));

            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false);
            DisableMove(true);
            t.AddOnExitAction(delegate
            {
                DisableMove(false);
                // ������������ʧ��ʧȥ���ܴ����������ƶ��ٶȼӳ�
                NumericBox.MoveSpeed.RemovePctAddModifier(FrogSpeedModifier);
                NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
                DisableMove(false);
            });
            AddTask(t);
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    public void OnEnterWater()
    {
        isInWater = true;
        // ��ˮ������д������������ټӳ�
        if (!isJumped)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(FrogSpeedModifier);
            NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(FrogSpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(FrogInWaterSpeedModifier);
        }
        SetActionState(new TransitionState(this));
        // ���ˮ����Ч
        EffectManager.AddWaterWaveEffectToUnit(this, Vector2.zero);
    }

    public void OnStayWater()
    {

    }

    public void OnExitWater()
    {
        SetNoCollideAllyUnit();
        isInWater = false;
        // �Ƴ�������ˮ����ǲ������ټӳ�
        NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
        SetActionState(new TransitionState(this));
        // �Ƴ�ˮ����Ч
        EffectManager.RemoveWaterWaveEffectFromUnit(this);
    }

    public override void OnTransitionStateEnter()
    {
        if (isInWater)
        {
            if (isJumped)
                animatorController.Play("Enter1");
            else
                animatorController.Play("Enter0");
        }
        else
        {
            if (isJumped)
                animatorController.Play("Exit1");
            else
                animatorController.Play("Exit0");
        }
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isJumped)
        {
            if(isInWater)
                animatorController.Play("Attack1");
            else
                animatorController.Play("Attack0");
        }
        else
        {
            if (isInWater)
                animatorController.Play("Jump1");
            else
                animatorController.Play("Jump0");
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isJumped)
        {
            if(isInWater)
                animatorController.Play("Move3", true); // ˮ����·
            else
                animatorController.Play("Move1", true); // ½����·
        }
        else
        {
            if (isInWater)
                animatorController.Play("Move2", true); // ˮ����Ӿ
            else
                animatorController.Play("Move0", true); // ½������
        }
    }

    public override void OnIdleStateEnter()
    {
        if (isInWater)
            animatorController.Play("Idle1", true);
        else
            animatorController.Play("Idle0", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }

    public override void OnDieStateEnter()
    {
        // ����Լ���ˮ������û�б����ؾͲ������е���������
        if (WaterGridType.IsInWater(this) && !Environment.WaterManager.IsBearing(this))
            animatorController.Play("Die1", true);
        else
            animatorController.Play("Die0", true);
    }
}
