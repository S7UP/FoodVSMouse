/// <summary>
/// 反弹伤害行动
/// </summary>
public class ReboundDamageAction : DamageAction
{

    public ReboundDamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target, damageValue)
    {

    }

    //前置处理
    private void PreProcess()
    {
        //触发 造成伤害前 行动点
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
        }
        //触发 承受伤害前 行动点
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PreReceiveReboundDamage, this);
        }
    }

    //应用伤害
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveDamage(this);
        PostProcess();
    }

    //后置处理
    private void PostProcess()
    {
        //触发 造成伤害后 行动点
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
        }
        //触发 承受伤害后 行动点
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PostReceiveReboundDamage, this);
        }
    }
}