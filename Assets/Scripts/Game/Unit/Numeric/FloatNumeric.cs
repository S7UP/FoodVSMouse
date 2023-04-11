using S7P.Numeric;

using UnityEngine;

public class FloatNumeric
{
    // 最终数值
    public float Value { get; private set; }
    // 基础数值
    public float baseValue { get; private set; }
    // 装备数值加成
    public float add { get; private set; }
    // 装备百分比加成
    public float pctAdd { get; private set; }
    // BUFF加成
    public float finalAdd { get; private set; }
    // BUFF百分比加成
    public float finalPctAdd { get; private set; }

    public FloatModifierCollector AddCollector { get; } = new FloatModifierCollector();
    public FloatModifierCollector PctAddCollector { get; } = new FloatModifierCollector();
    public FloatModifierCollector FinalAddCollector { get; } = new FloatModifierCollector();
    public FloatModifierCollector FinalPctAddCollector { get; } = new FloatModifierCollector();


    public void Initialize()
    {
        baseValue = add = pctAdd = finalAdd = finalPctAdd = 0;
        AddCollector.Clear();
        PctAddCollector.Clear();
        FinalAddCollector.Clear();
        FinalPctAddCollector.Clear();
        Update();
    }
    public float SetBase(float value)
    {
        baseValue = value;
        Update();
        return baseValue;
    }
    public void AddAddModifier(FloatModifier modifier)
    {
        AddCollector.AddModifier(modifier);
        add = AddCollector.TotalValue;
        Update();
    }
    public void AddPctAddModifier(FloatModifier modifier)
    {
        PctAddCollector.AddModifier(modifier);
        pctAdd = PctAddCollector.TotalValue;
        Update();
    }
    public void AddFinalAddModifier(FloatModifier modifier)
    {
        FinalAddCollector.AddModifier(modifier);
        finalAdd = FinalAddCollector.TotalValue;
        Update();
    }
    public void AddFinalPctAddModifier(FloatModifier modifier)
    {
        FinalPctAddCollector.AddModifier(modifier);
        finalPctAdd = Mathf.Min(0, Mathf.Max(-95, FinalPctAddCollector.MinValue));
        Update();
    }
    public void RemoveAddModifier(FloatModifier modifier)
    {
        AddCollector.RemoveModifier(modifier);
        add = AddCollector.TotalValue;
        Update();
    }
    public void RemovePctAddModifier(FloatModifier modifier)
    {
        PctAddCollector.RemoveModifier(modifier);
        pctAdd = PctAddCollector.TotalValue;
        Update();
    }
    public void RemoveFinalAddModifier(FloatModifier modifier)
    {
        FinalAddCollector.RemoveModifier(modifier);
        finalAdd = FinalAddCollector.TotalValue;
        Update();
    }
    public void RemoveFinalPctAddModifier(FloatModifier modifier)
    {
        FinalPctAddCollector.RemoveModifier(modifier);
        finalPctAdd = Mathf.Min(0, Mathf.Max(-95, FinalPctAddCollector.MinValue));
        Update();
    }

    public void Update()
    {
        var value1 = baseValue;
        var value2 = (value1 + add) * (100 + pctAdd) / 100f;
        var value3 = (value2 + finalAdd) * (100 + finalPctAdd) / 100f;
        Value = (float)value3;
    }
}
