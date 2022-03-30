using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 整形修饰器的收集器
/// </summary>
public class IntModifierCollector
{
    public int TotalValue { get; private set; }
    private List<IntModifier> Modifiers { get; } = new List<IntModifier>();

    public int AddModifier(IntModifier modifier)
    {
        Modifiers.Add(modifier);
        Update();
        return TotalValue;
    }

    public int RemoveModifier(IntModifier modifier)
    {
        Modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }

    public void Update()
    {
        TotalValue = 0;
        foreach (var item in Modifiers)
        {
            TotalValue += item.Value;
        }
    }
}
