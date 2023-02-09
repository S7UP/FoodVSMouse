/// <summary>
/// 伤害行动
/// </summary>
public class DamageAction : CombatAction
{
    //伤害数值
    public float DamageValue { get; set; }

    public DamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target)
    {
        DamageValue = damageValue;
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
            Target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
        }
    }

    //应用伤害
    public override void ApplyAction()
    {
        // 如果伤害小于等于0那么它不会生效
        if (DamageValue <= 0)
            return;
        PreProcess();
        if(Target!=null)
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
            Target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
    }
}