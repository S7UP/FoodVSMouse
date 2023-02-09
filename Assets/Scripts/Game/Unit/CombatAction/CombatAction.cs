/// <summary>
/// ս���ж��������˺������Ƶ�λ������Ч��������ս���ж�����Ҫ�̳���CombatAction
/// ս���ж���ս��ʵ���������𣬰��������ж�����Ҫ�õ����������ݣ����һᴥ��һϵ���ж����¼�
/// </summary>
public class CombatAction
{
    /// <summary>
    /// ��������
    /// </summary>
    public enum ActionType
    {
        SpellSkill,  // ����Ч��
        CauseDamage, // ����˺�
        ReboundDamage, // �����˺�
        RecordDamage, // ����˺�
        GiveCure,    // �ظ�
        AssignEffect,// �ṩЧ��
        GiveShield,  // �ṩ����
        BurnDamage,   // �ҽ��˺�
        RealDamage, // ��ʵ�˺�
    }

    public CombatAction(ActionType actionType, BaseUnit creator, BaseUnit target)
    {
        mActionType = actionType;
        Creator = creator;
        Target = target;
    }

    public ActionType mActionType { get; set; }
    public BaseUnit Creator { get; set; }
    public BaseUnit Target { get; set; }

    public virtual void ApplyAction()
    {

    }
}