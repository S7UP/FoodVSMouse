using System;
using System.Collections.Generic;

/// <summary>
/// �ж��㣬һ��ս���ж��ᴥ��ս��ʵ��һϵ�е��ж���
/// </summary>
public sealed class ActionPoint
{
    public List<Action<CombatAction>> Listeners { get; set; } = new List<Action<CombatAction>>();
}
