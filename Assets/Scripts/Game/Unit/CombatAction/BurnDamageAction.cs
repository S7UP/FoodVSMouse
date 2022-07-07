public class BurnDamageAction : DamageAction
{

    public BurnDamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target, damageValue)
    {
    }

    //ǰ�ô���
    private void PreProcess()
    {
        //���� ����˺�ǰ �ж���
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
        }
        //���� �����˺�ǰ �ж���
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveBurnDamage, this);
        }
    }

    //Ӧ���˺�
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveBurnDamage(this);
        PostProcess();
    }

    //���ô���
    private void PostProcess()
    {
        //���� ����˺��� �ж���
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
        }
        //���� �����˺��� �ж���
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveBurnDamage, this);
        }
    }
}
