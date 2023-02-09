/// <summary>
/// �ظ��ж�
/// </summary>
public class CureAction : CombatAction
{
    //������ֵ��ֵ
    public float CureValue { get; set; }

    public CureAction(ActionType actionType, BaseUnit creator, BaseUnit target, float cureValue) : base(actionType, creator, target)
    {
        CureValue = cureValue;
    }


    //ǰ�ô���
    private void PreProcess()
    {

    }

    //Ӧ������
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveCure(this);
        PostProcess();
    }

    //���ô���
    private void PostProcess()
    {
        //���� ������ƺ� �ж���
        if(Creator!=null)
            Creator.TriggerActionPoint(ActionPointType.PostCauseCure, this);
        //���� �������ƺ� �ж���
        if(Target!=null)
            Target.TriggerActionPoint(ActionPointType.PostReceiveCure, this);
    }
}