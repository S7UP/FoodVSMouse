using System.Collections.Generic;

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
    // �˺�����
    public MultiplyFloatModifierCollector DamageRate = new MultiplyFloatModifierCollector();
    // �ƶ��ٶ�(��/��)
    public FloatNumeric MoveSpeed = new FloatNumeric();
    // ��̣���
    public FloatNumeric Range = new FloatNumeric();
    // ����ֵ
    public FloatNumeric Shield = new FloatNumeric();
    // �����������ܾ�����
    public BoolNumeric IsDisableSkill = new BoolNumeric();

    // �����Զ�������
    public Dictionary<string, IntNumeric> IntDict = new Dictionary<string, IntNumeric>();
    public Dictionary<string, FloatNumeric> FloatDict = new Dictionary<string, FloatNumeric>();
    public Dictionary<string, BoolNumeric> BoolDict = new Dictionary<string, BoolNumeric>();

    public void Initialize()
    {
        // �����ʼ��baseֵ
        Hp.Initialize();
        Attack.Initialize();
        AttackSpeed.Initialize();
        Defense.Initialize();
        MoveSpeed.Initialize();
        Range.Initialize();
        Shield.Initialize();
        IsDisableSkill.Initialize();
        DamageRate.Clear();

        IntDict.Clear();
        FloatDict.Clear();
        BoolDict.Clear();
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

    public bool GetBoolNumericValue(string key)
    {
        if (BoolDict.ContainsKey(key))
        {
            return BoolDict[key].Value;
        }
        return false;
    }

    /// <summary>
    /// ��Ӿ�����Ч����������
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddDecideModifierToBoolDict(string key, BoolModifier boolModifier)
    {
        if (!BoolDict.ContainsKey(key))
        {
            BoolDict.Add(key, new BoolNumeric());
        }
        BoolDict[key].AddDecideModifier(boolModifier);
    }

    /// <summary>
    /// ���������Ч����������
    /// </summary>
    public void AddImmuneModifierToBoolDict(string key, BoolModifier boolModifier)
    {
        if (!BoolDict.ContainsKey(key))
        {
            BoolDict.Add(key, new BoolNumeric());
        }
        BoolDict[key].AddImmuneModifier(boolModifier);
    }

    /// <summary>
    /// �Ƴ�������Ч����������
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void RemoveDecideModifierToBoolDict(string key, BoolModifier boolModifier)
    {
        if (BoolDict.ContainsKey(key))
        {
            BoolDict[key].RemoveDecideModifier(boolModifier);
            if (BoolDict[key].DecideCollector.Modifiers.Count <= 0 && BoolDict[key].ImmuneCollector.Modifiers.Count <= 0)
            {
                BoolDict.Remove(key);
            }
        }
    }

    /// <summary>
    /// �Ƴ�������Ч����������
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void RemoveImmuneModifierToBoolDict(string key, BoolModifier boolModifier)
    {
        if (!BoolDict.ContainsKey(key))
        {
            BoolDict[key].RemoveImmuneModifier(boolModifier);
            if (BoolDict[key].DecideCollector.Modifiers.Count <= 0 && BoolDict[key].ImmuneCollector.Modifiers.Count <= 0)
            {
                BoolDict.Remove(key);
            }
        }
    }
}