using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ı�����״̬
/// </summary>
public class ChangeVelocityStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private float changePercent; // �������ٶȸı�ٷֱ�

    public ChangeVelocityStatusAbility(BaseUnit pmaster, float changePercent) : base(pmaster)
    {
        this.changePercent = changePercent;
        slowDownFloatModifier = new FloatModifier(changePercent);
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ��� ����Ǽ���Ч�����Լ�Ŀ���Ƿ����߼���
        if (changePercent < 0 && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
        {
            // ���ߵĻ�ֱ�ӽ���Ч������
            SetEffectEnable(false);
        }
        else
        {
            OnEnableEffect();
        }
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnDisableEffect()
    {
        master.NumericBox.MoveSpeed.RemovePctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        master.NumericBox.MoveSpeed.AddPctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// ������Ч���ڼ�
    /// </summary>
    public override void OnEffecting()
    {

    }

    /// <summary>
    /// �ڷ�����Ч���ڼ�
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// �����������������ʱ���ǻ��ϵ��
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
        // return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        OnDisableEffect();
    }
}
