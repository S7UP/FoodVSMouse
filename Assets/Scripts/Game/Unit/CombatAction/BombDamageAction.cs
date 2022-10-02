public class BombDamageAction : DamageAction
{

    public BombDamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target, damageValue)
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
            Target.TriggerActionPoint(ActionPointType.PreReceiveBombBurnDamage, this);
        }
    }

    //Ӧ���˺�
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveBombBurnDamage(this);
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
            Target.TriggerActionPoint(ActionPointType.PostReceiveBombBurnDamage, this);
        }
    }
}
