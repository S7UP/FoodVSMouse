public class CastState : BaseActionState
{
    public CastState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnCastStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.OnCastState();
    }

    public override void OnExit()
    {
        mBaseUnit.OnCastStateExit();
    }

    public override void OnInterrupt()
    {
        mBaseUnit.OnCastStateInterrupt();
    }

    public override void OnContinue()
    {
        mBaseUnit.OnCastStateContinue();
    }
}
