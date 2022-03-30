using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// �ظ��ж�
/// </summary>
public class ShieldAction : CombatAction
{
    //������ֵ��ֵ
    public float Value { get; set; }

    public ShieldAction(ActionType actionType, BaseUnit creator, BaseUnit target, float shieldValue) : base(actionType, creator, target)
    {
        Value = shieldValue;
    }


    //ǰ�ô���
    private void PreProcess()
    {
        //���� �������ǰ �ж���
        Creator.TriggerActionPoint(ActionPointType.PreCauseShield, this);
        //���� ���ܻ���ǰ �ж���
        Target.TriggerActionPoint(ActionPointType.PreReceiveShield, this);
    }

    //Ӧ������
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveShield(this);
        PostProcess();
    }

    //���ô���
    private void PostProcess()
    {
        //���� ������ܺ� �ж���
        Creator.TriggerActionPoint(ActionPointType.PostCauseShield, this);
        //���� ���ܻ��ܺ� �ж���
        Target.TriggerActionPoint(ActionPointType.PostReceiveShield, this);
    }
}