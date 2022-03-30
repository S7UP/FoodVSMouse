using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntNumeric
{
    // 最终数值
    public int Value { get; private set; }
    // 基础数值
    public int baseValue { get; private set; }
    // 装备数值加成
    public int add { get; private set; }
    // 装备百分比加成
    public int pctAdd { get; private set; }
    // BUFF加成
    public int finalAdd { get; private set; }
    // BUFF百分比加成
    public int finalPctAdd { get; private set; }

    private IntModifierCollector AddCollector { get; } = new IntModifierCollector();
    private IntModifierCollector PctAddCollector { get; } = new IntModifierCollector();
    private IntModifierCollector FinalAddCollector { get; } = new IntModifierCollector();
    private IntModifierCollector FinalPctAddCollector { get; } = new IntModifierCollector();


    public void Initialize()
    {
        baseValue = add = pctAdd = finalAdd = finalPctAdd = 0;
    }
    public int SetBase(int value)
    {
        baseValue = value;
        Update();
        return baseValue;
    }
    public void AddAddModifier(IntModifier modifier)
    {
        add = AddCollector.AddModifier(modifier);
        Update();
    }
    public void AddPctAddModifier(IntModifier modifier)
    {
        pctAdd = PctAddCollector.AddModifier(modifier);
        Update();
    }
    public void AddFinalAddModifier(IntModifier modifier)
    {
        finalAdd = FinalAddCollector.AddModifier(modifier);
        Update();
    }
    public void AddFinalPctAddModifier(IntModifier modifier)
    {
        finalPctAdd = FinalPctAddCollector.AddModifier(modifier);
        Update();
    }
    public void RemoveAddModifier(IntModifier modifier)
    {
        add = AddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemovePctAddModifier(IntModifier modifier)
    {
        pctAdd = PctAddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemoveFinalAddModifier(IntModifier modifier)
    {
        finalAdd = FinalAddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemoveFinalPctAddModifier(IntModifier modifier)
    {
        finalPctAdd = FinalPctAddCollector.RemoveModifier(modifier);
        Update();
    }

    public void Update()
    {
        var value1 = baseValue;
        var value2 = (value1 + add) * (100 + pctAdd) / 100f;
        var value3 = (value2 + finalAdd) * (100 + finalPctAdd) / 100f;
        Value = (int)value3;
    }
}
