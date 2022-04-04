using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 且关系的修饰器收集器
/// </summary>
public class BoolModifierCollector
{
    // 当集合为空时，TotalValue默认为false
    public bool TotalValue { get; private set; }
    public List<BoolModifier> Modifiers { get; } = new List<BoolModifier>();

    public bool AddModifier(BoolModifier modifier)
    {
        Modifiers.Add(modifier);
        Update();
        return TotalValue;
    }

    public bool RemoveModifier(BoolModifier modifier)
    {
        Modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }

    public void Update()
    {
        if(Modifiers.Count <= 0)
        {
            TotalValue = false;
        }
        else
        {
            TotalValue = true;
            foreach (var item in Modifiers)
            {
                if (!item.Value)
                {
                    TotalValue = false;
                    return;
                }
            }
        }
    }
}
