using S7P.Numeric;

public class SecondMasterMouse : MouseUnit
{
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // ����ը����ɱЧ��
    private bool isAngry = false;

    public override void MInit()
    {
        isAngry = false;
        base.MInit();
        // ��ʼ����ը����ɱЧ��
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // �ܵ��ҽ�Ч���������
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    if (!isAngry)
                    {
                        SetAngry();
                    }
                }
            }
        });
    }

    private void SetAngry()
    {
        if (!isAngry)
        {
            isAngry = true;
            // �Ƴ��ҽ�����
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
            burnRateMod.Value = 0.5f;
            NumericBox.BurnRate.AddModifier(burnRateMod);
            // ͬʱ�Ƴ��������ж��������Ч��
            StatusManager.RemoveAllSettleDownDebuff(this);
            // ��Ӽ��١�������������Ч��
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
            // ���200%�ƶ��ٶȼӳ�
            NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier(200));
            // ���100%�����ٶ���100%�������ӳ�
            NumericBox.Attack.AddPctAddModifier(new FloatModifier(100));
            NumericBox.AttackSpeed.AddPctAddModifier(new FloatModifier(100));
            // ���75%����
            NumericBox.DamageRate.AddModifier(new FloatModifier(0.25f));
            SetActionState(new TransitionState(this));
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnDieStateEnter()
    {
        if (isAngry)
            animatorController.Play("Die1");
        else
            base.OnDieStateEnter();
    }

    public override void OnMoveStateEnter()
    {
        if (isAngry)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move", true);
    }

    public override void OnAttackStateEnter()
    {
        base.OnAttackStateEnter();
        if (isAngry)
            animatorController.Play("Attack1");
    }
}
