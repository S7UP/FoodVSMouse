using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ҹ�ϵ���������ռ���
/// </summary>
public class BoolModifierCollector
{
    // ������Ϊ��ʱ��TotalValueĬ��Ϊfalse
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
