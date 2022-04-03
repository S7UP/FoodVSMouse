using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ״̬Ч�����������ڵ�λ��ִ���ض��ĳ�������Ч����BUFF���ͣ�
/// </summary>
public class StatusAbility : AbilityEntity
{
    public StatusAbilityManager statusAbilityManager { get; set; }
    private bool isDisableEffect { get; set; } // �Ƿ���ô�BUFF������Ч������Ӱ�����ʱ������ţ�
    public bool canActiveInDeathState; // ������״̬���ܷ��������
    public FloatNumeric totalTime = new FloatNumeric(); // �ܳ���ʱ��
    public float leftTime; // ��ǰʣ��ʱ��

    public StatusAbility()
    {

    }

    public StatusAbility(BaseUnit pmaster, float time) : base(pmaster)
    {
        totalTime.SetBase(time);
        leftTime = totalTime.Value;
    }

    public void SetEffectEnable(bool isEnable)
    {
        isDisableEffect = !isEnable;
        if (isEnable)
            OnEnableEffect();
        else
            OnDisableEffect();
    }

    /// <summary>
    /// ����ܷ�������ʱʹ��
    /// </summary>
    /// <returns></returns>
    private bool IsActiveInDeath()
    {
        return master.IsAlive() || canActiveInDeathState;
    }

    // ����Ϊ��Ҫ����ʵ��ϸ��ʱOverride�ķ���
    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public virtual void BeforeEffect()
    {

    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public virtual void OnDisableEffect()
    {
        
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public virtual void OnEnableEffect()
    {

    }

    /// <summary>
    /// ������Ч���ڼ�
    /// </summary>
    public virtual void OnEffecting()
    {

    }

    /// <summary>
    /// �ڷ�����Ч���ڼ�
    /// </summary>
    public virtual void OnNotEffecting()
    {

    }

    /// <summary>
    /// �����������������ʱ���ǻ��ϵ��
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetingEndCondition()
    {
        return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public virtual void AfterEffect()
    {

    }

    // ����Ϊ�̳в�ʵ�ָ���ķ�������ʹ��Ҫ�߼���
    /// <summary>
    /// ���ø�Ч��
    /// </summary>
    public override void ActivateAbility()
    {
        // ������⣬���Ŀ��������״̬��������ܲ����������ڼ��ͷ���ֱ���˳�
        if (!IsActiveInDeath())
            return;
        BeforeEffect();
        SetEffectEnable(true);
    }

    /// <summary>
    /// ÿ֡�������˸���
    /// </summary>
    public override void Update()
    {
        // ������⣬���Ŀ��������״̬��������ܲ����������ڼ��ͷ���ֱ���˳�
        if (!IsActiveInDeath())
            return;

        if(leftTime <= 0 || IsMeetingEndCondition())
        {
            EndActivate();
            return;
        }

        if (!isDisableEffect)
            OnEffecting();
        else
            OnNotEffecting();
        leftTime -= 1;
    }

    /// <summary>
    /// ����Ч����ζ��BUFF����ʧ������Ӧ��֪ͨ�������Ĺ������Ƴ�BUFF
    /// </summary>
    public override void EndActivate()
    {
        AfterEffect();
        statusAbilityManager.RemoveStatusAbility(this);
    }

    /// <summary>
    /// �Ƿ�ﵽ�˼��������
    /// </summary>
    /// <returns></returns>
    public override bool CanActive()
    {
        return !isDisableEffect;
    }

    //��������ִ����
    public override AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    //Ӧ������Ч��
    public override void ApplyAbilityEffect(BaseUnit targetEntity)
    {
        //Ӧ������Ч��
    }
}
