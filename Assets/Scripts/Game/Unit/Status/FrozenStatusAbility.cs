using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����״̬
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    private FrozenState frozenState;

    public FrozenStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // Ŀ�궯��״̬ת��Ϊ����״̬
        frozenState = new FrozenState(master, master.mCurrentActionState);
        master.SetActionState(frozenState);
        master.isDisableSkill = true;
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnDisableEffect()
    {
        if (frozenState != null)
        {
            frozenState.TryExitCurrentState();
            frozenState = null;
        }
        master.isDisableSkill = false;
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        // Ŀ�궯��״̬ת��Ϊ����״̬
        if(frozenState == null)
        {
            frozenState = new FrozenState(master, master.mCurrentActionState);
            master.SetActionState(frozenState);
        }
        master.isDisableSkill = true;
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
        return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        frozenState.TryExitCurrentState();
        master.isDisableSkill = false;
    }
}
