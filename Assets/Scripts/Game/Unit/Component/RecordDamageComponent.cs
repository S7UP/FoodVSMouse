/// <summary>
/// �˺���¼��������ˣ�
/// </summary>
public class RecordDamageComponent
{
    private const float TransRate = 1.0f; // ԭ�˺����������� �� ת����
    private BaseUnit master;
    public float recordDamage; // �˺���¼ֵ
    
    public RecordDamageComponent(BaseUnit master)
    {
        this.master = master;
    }

    /// <summary>
    /// ��ʼ����ÿ��ʵ����ʱ��Ҫ��
    /// </summary>
    public void Initilize()
    {
        recordDamage = 0;
    }

    /// <summary>
    /// �������
    /// </summary>
    public void AddRecordDamage(float value)
    {
        recordDamage += value;
    }

    /// <summary>
    /// ����һ������
    /// </summary>
    public void TriggerRecordDamage(float dmg)
    {
        dmg = TransRate * dmg;
        if (recordDamage >= dmg)
        {
            recordDamage -= dmg; // ���ͼ�¼ֵ
            new DamageAction(CombatAction.ActionType.RealDamage, null, master, dmg).ApplyAction();
        }
        else
        {
            new DamageAction(CombatAction.ActionType.RealDamage, null, master, recordDamage).ApplyAction();
            recordDamage = 0;
        }
        
    }

}
