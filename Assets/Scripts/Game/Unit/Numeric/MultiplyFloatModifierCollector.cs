using System;
using System.Collections.Generic;

using S7P.Numeric;
/// <summary>
/// 单浮点形修饰器的收集器（使用乘算）
/// </summary>
public class MultiplyFloatModifierCollector
{
    public float TotalValue { get; private set; } = 1;
    public List<FloatModifier> Modifiers { get; } = new List<FloatModifier>();
    private List<Action<MultiplyFloatModifierCollector>> beforeValueChangeActionList = new List<Action<MultiplyFloatModifierCollector>>();
    private List<Action<MultiplyFloatModifierCollector>> afterValueChangeActionList = new List<Action<MultiplyFloatModifierCollector>>();

    public void Clear()
    {
        Modifiers.Clear();
        afterValueChangeActionList.Clear();
        TotalValue = 1;
    }

    public float GetMax()
    {
        if (Modifiers.Count <= 0)
            return 0;
        float max = Modifiers[0].Value;
        foreach (var item in Modifiers)
        {
            if (item.Value > max)
                max = item.Value;
        }
        return max;
    }

    public float GetMin()
    {
        if (Modifiers.Count <= 0)
            return 0;
        float min = Modifiers[0].Value;
        foreach (var item in Modifiers)
        {
            if (item.Value < min)
                min = item.Value;
        }
        return min;
    }

    public float AddModifier(FloatModifier modifier)
    {
        Modifiers.Add(modifier);
        Update();
        return TotalValue;
    }

    public float RemoveModifier(FloatModifier modifier)
    {
        Modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }

    public bool Contains(FloatModifier mod)
    {
        return Modifiers.Contains(mod);
    }

    public List<FloatModifier> GetModifierList()
    {
        return Modifiers;
    }

    private void Update()
    {
        foreach (var action in beforeValueChangeActionList)
            action(this);
        TotalValue = 1;
        foreach (var item in Modifiers)
        {
            TotalValue *= item.Value;
        }
        foreach (var action in afterValueChangeActionList)
            action(this);
    }

    #region 供外界调用的方法
    public void AddBeforeValueChangeAction(Action<MultiplyFloatModifierCollector> action)
    {
        beforeValueChangeActionList.Add(action);
    }

    public void RemoveBeforeValueChangeAction(Action<MultiplyFloatModifierCollector> action)
    {
        beforeValueChangeActionList.Remove(action);
    }

    public void AddAfterValueChangeAction(Action<MultiplyFloatModifierCollector> action)
    {
        afterValueChangeActionList.Add(action);
    }

    public void RemoveAfterValueChangeAction(Action<MultiplyFloatModifierCollector> action)
    {
        afterValueChangeActionList.Remove(action);
    }
    #endregion
}
