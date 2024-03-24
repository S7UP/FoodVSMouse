using S7P.Numeric;

public class SecondMasterMouse : MouseUnit
{
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // 免疫炸弹秒杀效果
    private bool isAngry = false;

    public override void MInit()
    {
        isAngry = false;
        base.MInit();
        // 初始免疫炸弹秒杀效果
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // 受到灰烬效果后会生气
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
            // 移除灰烬抗性
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
            burnRateMod.Value = 0.5f;
            NumericBox.BurnRate.AddModifier(burnRateMod);
            // 同时移除身上所有定身类控制效果
            StatusManager.RemoveAllSettleDownDebuff(this);
            // 添加减速、定身、冻结免疫效果
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
            // 获得200%移动速度加成
            NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier(200));
            // 获得100%攻击速度与100%攻击力加成
            NumericBox.Attack.AddPctAddModifier(new FloatModifier(100));
            NumericBox.AttackSpeed.AddPctAddModifier(new FloatModifier(100));
            // 获得75%减伤
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
