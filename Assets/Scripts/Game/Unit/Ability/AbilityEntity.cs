/// <summary>
/// ����ʵ�壬�洢��ĳ����λĳ�����������ݺ�״̬
/// </summary>
public abstract class AbilityEntity
{
    public string name; // ��
    public BaseUnit master { get; set; }

    public AbilityEntity()
    {

    }

    public AbilityEntity(BaseUnit pmaster)
    {
        master = pmaster;
    }


    public virtual void Init()
    {

    }

    public virtual BaseUnit GetMaster()
    {
        return master;
    }

    //���Լ�������
    public void TryActivateAbility()
    {
        if (CanActive())
        {
            ActivateAbility();
        }
    }

    //��������
    public virtual void ActivateAbility()
    {

    }

    /// <summary>
    /// ÿ֡�������˸���
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// �ֶ�����ֹͣ����
    /// </summary>
    public void TryEndActivate()
    {
        EndActivate();
    }

    //��������
    public virtual void EndActivate()
    {
        
    }

    /// <summary>
    /// �Ƿ�ﵽ�˼��������
    /// </summary>
    /// <returns></returns>
    public virtual bool CanActive()
    {
        return true;
    }

    //��������ִ����
    public virtual AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    //Ӧ������Ч��
    public virtual void ApplyAbilityEffect(BaseUnit targetEntity)
    {
        //Ӧ������Ч��
    }
}
