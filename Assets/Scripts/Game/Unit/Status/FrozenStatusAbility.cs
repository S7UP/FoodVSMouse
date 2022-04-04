using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����״̬
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    private FrozenState frozenState;
    private BoolModifier boolModifier;

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
        if(boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
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
        if(boolModifier != null)
            master.RemoveDisAbleSkillModifier(boolModifier);
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
        if (boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
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
        frozenState.TryExitCurrentState();
        if (boolModifier != null)
            master.RemoveDisAbleSkillModifier(boolModifier);
    }
}
