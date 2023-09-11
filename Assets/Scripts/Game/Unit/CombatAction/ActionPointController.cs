using System;
using System.Collections.Generic;

/// <summary>
/// 行动点管理器，在这里管理一个战斗实体所有行动点的添加监听、移除监听、触发流程
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
    /// 触发某个行动，如攻击、受伤、输出治疗、接受治疗，并获取具体的行动实例action，从action中获取这次行动的一些属性 如 伤害值、治疗值等
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
/// 行动点类型
/// </summary>
public enum ActionPointType
{
    PreCauseDamage,//造成伤害前
    WhenCauseDamage,
    PostCauseDamage,//造成伤害后

    PreReceiveDamage,//承受伤害前
    WhenReceiveDamage,
    PostReceiveDamage,//承受伤害后

    PreCauseCure, //输出治疗前
    PostCauseCure, //输出治疗后

    PreReceiveCure, // 接受治疗前
    PostReceiveCure, // 接受治疗后

    PreCauseShield, //输出护盾前
    PostCauseShield, //输出护盾后

    PreReceiveShield, // 接受护盾前
    PostReceiveShield, // 接受护盾后
}