using System.Collections.Generic;

/// <summary>
/// �����������������ռ���
/// </summary>
public class FloatModifierCollector
{
    public float TotalValue { get; private set; }
    public List<FloatModifier> Modifiers { get; } = new List<FloatModifier>();

    public void Clear()
    {
        Modifiers.Clear();
        Update();
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
        TotalValue = 0;
        foreach (var item in Modifiers)
        {
            TotalValue += item.Value;
        }
    }
}
