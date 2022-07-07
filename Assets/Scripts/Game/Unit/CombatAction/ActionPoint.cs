using System;
using System.Collections.Generic;

/// <summary>
/// 行动点，一次战斗行动会触发战斗实体一系列的行动点
/// </summary>
public sealed class ActionPoint
{
    public List<Action<CombatAction>> Listeners { get; set; } = new List<Action<CombatAction>>();
}
