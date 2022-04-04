using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ս����ֵϻ�ӣ�������������н�ɫս����ֵ�Ĵ洢�������ˢ�µ�
/// </summary>
public class CombatNumericBox
{
    // ����ֵ
    public FloatNumeric Hp = new FloatNumeric();
    // ������
    public FloatNumeric Attack = new FloatNumeric();
    // �����ٶ�
    public FloatNumeric AttackSpeed = new FloatNumeric();
    // �˺�����(ȡֵΪ0-100)
    public FloatNumeric Defense = new FloatNumeric();
    // �ƶ��ٶ�(��/��)
    public FloatNumeric MoveSpeed = new FloatNumeric();
    // ��̣���
    public FloatNumeric Range = new FloatNumeric();
    // ����ֵ
    public FloatNumeric Shield = new FloatNumeric();
    // �����������ܾ�����
    public BoolNumeric IsDisableSkill = new BoolNumeric();



    public void Initialize()
    {
        // �����ʼ��baseֵ
        Hp.SetBase(0);
        Attack.SetBase(0);
        AttackSpeed.SetBase(0);
        Defense.SetBase(0);
        MoveSpeed.SetBase(0);
        Range.SetBase(0);
        Shield.SetBase(0);
    }

    /// <summary>
    /// �ⲿ��ӻ��ܣ���̬��
    /// </summary>
    public void AddDynamicShield(float value)
    {
        Shield.AddFinalAddModifier(new FloatModifier(value));
    }

    /// <summary>
    /// �Ի�������˺�
    /// �����ڶ������ʱ�����յ����ȼ�Ϊ BUFF�໤�ܣ���ʱ�ࣩ>װ���໤�ܣ������ԣ���ͬ������ ��ѭ�Ƚ��ȳ�ԭ�򣬼����ϵĶ��������˺�
    /// </summary>
    /// <returns>ʣ���˺������ʣ���˺�С�ڵ���0˵���˺���ȫ����������</returns>
    public float DamageShield(float dmg)
    {
        // ��������BUFF�໤��
        while (Shield.FinalAddCollector.Modifiers.Count > 0)
        {
            float realShieldValue = Shield.FinalAddCollector.Modifiers[0].Value * (1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // �����ܹ��ֵ�ȫ���˺�
            {
                Shield.FinalAddCollector.Modifiers[0].Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue); // ����ʣ��ֵ����
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.FinalAddCollector.Modifiers.RemoveAt(0); // �Ƴ����㻤��
            }
        }

        // ��������װ���໤��
        while (Shield.AddCollector.Modifiers.Count > 0)
        {
            float realShieldValue = Shield.AddCollector.Modifiers[0].Value * (1+Shield.PctAddCollector.TotalValue) *(1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // �����ܹ��ֵ�ȫ���˺�
            {
                Shield.AddCollector.Modifiers[0].Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue)/ (1 + Shield.PctAddCollector.TotalValue); // ����ʣ��ֵ����
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.AddCollector.Modifiers.RemoveAt(0); // �Ƴ����㻤��
            }
        }
        return dmg;
    }

    /// <summary>
    /// ��ӽ��ü���Ч������Ĭ��
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddDisAbleSkillModifier()
    {
        BoolModifier boolModifier = new BoolModifier(true);
        IsDisableSkill.AddDecideModifier(boolModifier);
        return boolModifier;
    }

    /// <summary>
    /// ������߽��ü���Ч������Ĭ���ߣ�
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddImmuneDisAbleSkillModifier()
    {
        BoolModifier boolModifier = new BoolModifier(true);
        IsDisableSkill.AddImmuneModifier(boolModifier);
        return boolModifier;
    }

    /// <summary>
    /// �Ƴ����ü���Ч������Ĭ��
    /// </summary>
    /// <returns></returns>
    public void RemoveDisAbleSkillModifier(BoolModifier boolModifier)
    {
        IsDisableSkill.RemoveDecideModifier(boolModifier);
    }

    /// <summary>
    /// �Ƴ����߽��ü���Ч������Ĭ���ߣ�
    /// </summary>
    /// <returns></returns>
    public void RemoveImmuneDisAbleSkillModifier(BoolModifier boolModifier)
    {
        IsDisableSkill.RemoveImmuneModifier(boolModifier);
    }
}