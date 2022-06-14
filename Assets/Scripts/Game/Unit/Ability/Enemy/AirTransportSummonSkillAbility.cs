using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

using static UnityEditorInternal.VersionControl.ListControl;
/// <summary>
/// ��ĸ�ٻ����е�λ����
/// </summary>
public class AirTransportSummonSkillAbility : SkillAbility
{
    private int leftUseCount = 1; // ʣ��ʹ�ô���
    private int castState = 0; // ʩ���׶Σ�0�����ڴ򿪲��ţ�1�����Ŵ򿪲����ٻ����У�2���ٻ�����Ϻ�رղ��ţ� 3�������ѹر����׼�����������л�������̬��
    private Action SummonAction; // �ٻ��¼�
    public AirTransportSummonSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public AirTransportSummonSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // �����㹻������ʣ��ʹ�ô���
        return (leftUseCount>0);
    }

    public override void BeforeSpell()
    {
        // ͣ��������״̬��������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {

    }

    /// <summary>
    /// �ڷǼ����ڼ�
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return castState == 3;
    }

    public override void AfterSpell()
    {
        castState = 0;
        leftUseCount--;
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// ���ⲿ���ã���ɴ��Ž׶�
    /// </summary>
    /// <returns></returns>
    public void SetFinishPreCast()
    {
        castState = 1;
        // ����һ�ζ����л�����λ��гɳ���������ͣ�ٹֶ���
        master.SetActionState(new CastState(master));
        // �ٹ�
        if(SummonAction!=null)
            SummonAction();
    }

    /// <summary>
    /// ���ⲿ���ã���ɿ����ٹֽ׶�
    /// </summary>
    public void SetFinishCast()
    {
        castState = 2;
        // ����һ�ζ����л�����λ��гɹ��Ŷ���
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// ���ⲿ���ã���ɹ��Ž׶β�׼����������
    /// </summary>
    public void SetFinishPostCast()
    {
        castState = 3;
    }

    /// <summary>
    /// �����ٹ��¼�
    /// </summary>
    public void SetSummonActon(Action action)
    {
        SummonAction = action;
    }

    /// <summary>
    /// ��ȡ�����׶�
    /// </summary>
    /// <returns></returns>
    public int GetCastState()
    {
        return castState;
    }
}
