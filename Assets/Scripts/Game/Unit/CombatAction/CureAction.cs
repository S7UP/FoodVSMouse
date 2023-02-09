/// <summary>
/// 回复行动
/// </summary>
public class CureAction : CombatAction
{
    //治疗数值数值
    public float CureValue { get; set; }

    public CureAction(ActionType actionType, BaseUnit creator, BaseUnit target, float cureValue) : base(actionType, creator, target)
    {
        CureValue = cureValue;
    }


    //前置处理
    private void PreProcess()
    {

    }

    //应用治疗
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveCure(this);
        PostProcess();
    }

    //后置处理
    private void PostProcess()
    {
        //触发 输出治疗后 行动点
        if(Creator!=null)
            Creator.TriggerActionPoint(ActionPointType.PostCauseCure, this);
        //触发 接受治疗后 行动点
        if(Target!=null)
            Target.TriggerActionPoint(ActionPointType.PostReceiveCure, this);
    }
}