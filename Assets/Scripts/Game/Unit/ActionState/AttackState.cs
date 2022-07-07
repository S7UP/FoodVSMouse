public class AttackState : BaseActionState
{
    public AttackState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnAttackStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnAttackState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnAttackStateExit();
    }

    public override void OnInterrupt()
    {
        mBaseUnit.OnAttackStateInterrupt();
    }

    public override void OnContinue()
    {
        mBaseUnit.OnAttackStateContinue();
    }
}
