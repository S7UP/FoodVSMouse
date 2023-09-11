using System;
using System.Collections.Generic;

/// <summary>
/// �ж�������������������һ��ս��ʵ�������ж������Ӽ������Ƴ���������������
/// </summary>
public sealed class ActionPointController
{
    private BaseUnit master;
    private Dictionary<ActionPointType, ActionPoint> ActionPoints { get; set; } = new Dictionary<ActionPointType, ActionPoint>();
    private Dictionary<string, List<Action<BaseUnit>>> ActionListDict = new Dictionary<string, List<Action<BaseUnit>>>();

    public ActionPointController(BaseUnit master)
    {
        this.master = master;
    }

    public void Initialize()
    {
        ActionPoints.Clear();
        ActionListDict.Clear();

        ActionPoints.Add(ActionPointType.PreCauseDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.WhenCauseDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostCauseDamage, new ActionPoint());

        ActionPoints.Add(ActionPointType.PreReceiveDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.WhenReceiveDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostReceiveDamage, new ActionPoint());

        ActionPoints.Add(ActionPointType.PreCauseCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostCauseCure, new ActionPoint());

        ActionPoints.Add(ActionPointType.PreReceiveCure, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostReceiveCure, new ActionPoint());

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
                item(action);
    }

    public void AddAction(string key, Action<BaseUnit> action)
    {
        if (!ActionListDict.ContainsKey(key))
            ActionListDict.Add(key, new List<Action<BaseUnit>>());
        ActionListDict[key].Add(action);
    }

    public void RemoveAction(string key, Action<BaseUnit> action)
    {
        if (ActionListDict.ContainsKey(key))
        {
            ActionListDict[key].Remove(action);
            if (ActionListDict[key].Count <= 0)
                ActionListDict.Remove(key);
        }
    }

    public void TriggerAction(string key)
    {
        if (ActionListDict.ContainsKey(key))
        {
            foreach (var action in ActionListDict[key])
            {
                action(master);
            }
        }
    }

}

/// <summary>
/// �ж�������
/// </summary>
public enum ActionPointType
{
    PreCauseDamage,//����˺�ǰ
    WhenCauseDamage,
    PostCauseDamage,//����˺���

    PreReceiveDamage,//�����˺�ǰ
    WhenReceiveDamage,
    PostReceiveDamage,//�����˺���

    PreCauseCure, //�������ǰ
    PostCauseCure, //������ƺ�

    PreReceiveCure, // ��������ǰ
    PostReceiveCure, // �������ƺ�

    PreCauseShield, //�������ǰ
    PostCauseShield, //������ܺ�

    PreReceiveShield, // ���ܻ���ǰ
    PostReceiveShield, // ���ܻ��ܺ�
}