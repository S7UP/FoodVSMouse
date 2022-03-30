using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.EventSystems.EventTrigger;

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
        if (CanSkill())
        {
            ActivateAbility();
        }
    }

    //��������
    public virtual void ActivateAbility()
    {

    }

    //��������
    public virtual void EndActivate()
    {
        
    }

    public virtual bool CanSkill()
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

    /// <summary>
    /// ÿ֡�������˸���
    /// </summary>
    public virtual void Update()
    {

    }
}
