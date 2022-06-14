using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ж�������������������һ��ս��ʵ�������ж������Ӽ������Ƴ���������������
/// </summary>
public sealed class ActionPointManager
{

    private Dictionary<ActionPointType, ActionPoint> ActionPoints { get; set; } = new Dictionary<ActionPointType, ActionPoint>();


    public void Initialize()
    {
        ActionPoints.Clear();

        ActionPoints.Add(ActionPointType.PostCauseCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostReceiveCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostCauseDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostReceiveDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostReceiveReboundDamage, new ActionPoint());

        ActionPoints.Add(ActionPointType.PreCauseCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PreCauseDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PreReceiveCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PreReceiveDamage, new ActionPoint());
    }

    public void AddListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        ActionPoints[actionPointType].Listeners.Add(action);
    }

    public void RemoveListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        ActionPoints[actionPointType].Listeners.Remove(action);
    }

    /// <summary>
    /// ����ĳ���ж����繥�������ˡ�������ơ��������ƣ�����ȡ������ж�ʵ��action����action�л�ȡ����ж���һЩ���� �� �˺�ֵ������ֵ��
    /// </summary>
    /// <param name="actionPointType"></param>
    /// <param name="action"></param>
    public void TriggerActionPoint(ActionPointType actionPointType, CombatAction action)
    {
        if(ActionPoints.ContainsKey(actionPointType))
            foreach (var item in ActionPoints[actionPointType].Listeners)
            {
                item.Invoke(action);
            }
    }
}

/// <summary>
/// �ж�������
/// </summary>
public enum ActionPointType
{
    PreCauseDamage,//����˺�ǰ
    PreReceiveDamage,//�����˺�ǰ

    PostCauseDamage,//����˺���
    PostReceiveDamage,//�����˺���

    PreReceiveBurnDamage,//���ܻҽ��˺�ǰ
    PostReceiveBurnDamage,//���ܻҽ��˺���

    PreReceiveReboundDamage,//���ܷ����˺�ǰ
    PostReceiveReboundDamage,//���ܷ����˺���

    PreCauseCure, //�������ǰ
    PreReceiveCure, // ��������ǰ

    PostCauseCure, //������ƺ�
    PostReceiveCure, // �������ƺ�

    PreCauseShield, //�������ǰ
    PreReceiveShield, // ���ܻ���ǰ

    PostCauseShield, //������ܺ�
    PostReceiveShield, // ���ܻ��ܺ�
}