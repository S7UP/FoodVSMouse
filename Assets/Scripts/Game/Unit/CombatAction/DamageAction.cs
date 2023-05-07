using System.Collections.Generic;
/// <summary>
/// �˺��ж�
/// </summary>
public class DamageAction : CombatAction
{
    public enum DamageType
    {
        Default, // Ĭ���˺�����
        BombBurn, // ���ƻҽ���Դ
        AOE, // ��Χ�˺���Դ
        Rebound, // �����˺���Դ
    }

    private List<DamageType> DamageTypeList = new List<DamageType>(); // �����˺����͵ı�ǩ����Ϊ����Ĭ��ΪDefault
    public float DamageValue { get; set; }
    public float RealCauseValue; // ʵ������˺����ڶ�Ŀ������˺����ȡ������Ϊ0��


    public DamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target)
    {
        DamageValue = damageValue;
        RealCauseValue = 0;
    }

    /// <summary>
    /// Ϊ�������һ���˺����͵ı�ǩ
    /// </summary>
    /// <param name="type"></param>
    public void AddDamageType(DamageType type)
    {
        if (!DamageTypeList.Contains(type))
            DamageTypeList.Add(type);
    }

    /// <summary>
    /// �����Ƿ���ĳ�����͵��˺�
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsDamageType(DamageType type)
    {
        if (type is DamageType.Default && (DamageTypeList.Count == 0 || DamageTypeList.Contains(DamageType.Default)))
            return true;
        else
            return DamageTypeList.Contains(type);
    }

    public List<DamageType> GetDamageTypeList()
    {
        return DamageTypeList;
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
        if (Creator != null)
            Creator.TriggerActionPoint(ActionPointType.WhenCauseDamage, this);
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.WhenReceiveDamage, this);
            // Ĭ�ϵĽ����˺��ķ���
            if(Target.IsUseDefaultRecieveDamageActionMethod())
                UnitManager.ReceiveDamageAction(Target, this);
        }
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