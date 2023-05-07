using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;

public class FloatNumeric
{
    // 最终数值
    private float _Value;
    public float Value
    {
        get
        {
            if (maxValueFuncIndex > -1)
                return GetValueFuncDict[maxValueFuncIndex]();
            return _Value;
        }
    }
    // 基础数值
    private float _baseValue;
    public float baseValue
    {
        get
        {
            if (maxBaseValueFuncIndex > -1)
                return GetBaseValueFuncDict[maxBaseValueFuncIndex]();
            return _baseValue;
        }
    }
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
    // 由外部添加的强制修改获取最终数值的方法，优先级大于默认的最终数值计算，key值越大优先级越高
    private Dictionary<int, Func<float>> GetValueFuncDict = new Dictionary<int, Func<float>>();
    private int maxValueFuncIndex = -1;

    private Dictionary<int, Func<float>> GetBaseValueFuncDict = new Dictionary<int, Func<float>>();
    private int maxBaseValueFuncIndex = -1;

    public void Initialize()
    {
        _baseValue = add = pctAdd = finalAdd = finalPctAdd = 0;
        AddCollector.Clear();
        PctAddCollector.Clear();
        FinalAddCollector.Clear();
        FinalPctAddCollector.Clear();
        GetValueFuncDict.Clear();
        maxValueFuncIndex = -1;
        GetBaseValueFuncDict.Clear();
        maxBaseValueFuncIndex = -1;
        Update();
    }
    public float SetBase(float value)
    {
        _baseValue = value;
        Update();
        return _baseValue;
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
        var value1 = _baseValue;
        var value2 = (value1 + add) * (100 + pctAdd) / 100f;
        var value3 = (value2 + finalAdd) * (100 + finalPctAdd) / 100f;
        _Value = (float)value3;
    }

    public void AddGetValueFunc(int key, Func<float> func)
    {
        if (GetValueFuncDict.ContainsKey(key))
            GetValueFuncDict[key] = func;
        else
            GetValueFuncDict.Add(key, func);

        if (key > maxValueFuncIndex)
            maxValueFuncIndex = key;
    }

    public void RemoveGetValueFunc(int key)
    {
        if (GetValueFuncDict.ContainsKey(key))
        {
            GetValueFuncDict.Remove(key);
            if (maxValueFuncIndex == key)
            {
                maxValueFuncIndex = -1;
                foreach (var keyValuePair in GetValueFuncDict)
                {
                    if (keyValuePair.Key > maxValueFuncIndex)
                        maxValueFuncIndex = keyValuePair.Key;
                }
            }
        }
    }

    public void AddGetBaseValueFunc(int key, Func<float> func)
    {
        if (GetBaseValueFuncDict.ContainsKey(key))
            GetBaseValueFuncDict[key] = func;
        else
            GetBaseValueFuncDict.Add(key, func);

        if (key > maxBaseValueFuncIndex)
            maxBaseValueFuncIndex = key;
    }

    public void RemoveGetBaseValueFunc(int key)
    {
        if (GetBaseValueFuncDict.ContainsKey(key))
        {
            GetBaseValueFuncDict.Remove(key);
            if (maxBaseValueFuncIndex == key)
            {
                maxBaseValueFuncIndex = -1;
                foreach (var keyValuePair in GetBaseValueFuncDict)
                {
                    if (keyValuePair.Key > maxBaseValueFuncIndex)
                        maxBaseValueFuncIndex = keyValuePair.Key;
                }
            }
        }
    }
}
