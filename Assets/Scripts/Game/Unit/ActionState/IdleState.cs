public class IdleState : BaseActionState
{
    public IdleState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnIdleStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnIdleState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnIdleStateExit();
    }

    public override void OnInterrupt()
    {
        mBaseUnit.OnIdleStateInterrupt();
    }

    public override void OnContinue()
    {
        mBaseUnit.OnIdleStateContinue();
    }
}
