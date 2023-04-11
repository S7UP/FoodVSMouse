using S7P.Numeric;

using System.Collections.Generic;

public class BoolNumeric
{
    // ������ֵȡ���ڵ�ǰ�ֵ������Ȩ�ز����ռ����Ľ��
    public bool Value { get; private set; }

    private Dictionary<int, BoolModifierCollector> CollectorDict { get; } = new Dictionary<int, BoolModifierCollector>();
    private int HighestPriorityIndex; // ��ǰ�ֵ��������key����

    public void Initialize()
    {
        CollectorDict.Clear();
        CollectorDict.Add(0, new BoolModifierCollector());
        HighestPriorityIndex = 0;
        UpdateValue();
    }

    public void AddModifier(BoolModifier modifier)
    {
        AddModifier(0, modifier);
    }

    public void AddModifier(int weights, BoolModifier modifier)
    {
        if (!CollectorDict.ContainsKey(weights))
        {
            CollectorDict.Add(weights, new BoolModifierCollector());
            if (weights > HighestPriorityIndex)
                HighestPriorityIndex = weights;
        }
        CollectorDict[weights].AddModifier(modifier);
        UpdateValue();
    }

    public void RemoveModifier(BoolModifier modifier)
    {
        RemoveModifier(0, modifier);
    }

    public void RemoveModifier(int weights, BoolModifier modifier)
    {
        if (CollectorDict.ContainsKey(weights))
            CollectorDict[weights].RemoveModifier(modifier);
        else
            return;

        if (CollectorDict[weights].GetModifierList().Count == 0)
        {
            CollectorDict.Remove(weights);
            if(HighestPriorityIndex == weights)
            {
                HighestPriorityIndex = 0;
                foreach (var keyValuePair in CollectorDict)
                {
                    if (keyValuePair.Key > HighestPriorityIndex)
                        HighestPriorityIndex = keyValuePair.Key; 
                }
            }
        }
        UpdateValue();
    }

    private void UpdateValue()
    {
        if (CollectorDict.ContainsKey(HighestPriorityIndex))
            Value = CollectorDict[HighestPriorityIndex].TotalValue;
        else
            Value = false;
    }
}
