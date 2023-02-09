/// <summary>
/// �˺��ж�
/// </summary>
public class DamageAction : CombatAction
{
    //�˺���ֵ
    public float DamageValue { get; set; }

    public DamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target)
    {
        DamageValue = damageValue;
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
        }
    }

    //Ӧ���˺�
    public override void ApplyAction()
    {
        // ����˺�С�ڵ���0��ô��������Ч
        if (DamageValue <= 0)
            return;
        PreProcess();
        if(Target!=null)
            Target.ReceiveDamage(this);
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
        }
    }
}