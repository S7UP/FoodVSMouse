using System.Collections.Generic;

/// <summary>
/// 单浮点形修饰器的收集器（使用乘算）
/// </summary>
public class MultiplyFloatModifierCollector
{
    public float TotalValue { get; private set; } = 1;
    public List<FloatModifier> Modifiers { get; } = new List<FloatModifier>();

    public void Clear()
    {
        Modifiers.Clear();
        Update();
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

    public void Update()
    {
        TotalValue = 1;
        foreach (var item in Modifiers)
        {
            TotalValue *= item.Value;
        }
    }
}
