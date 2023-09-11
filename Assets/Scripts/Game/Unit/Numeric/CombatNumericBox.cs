using System.Collections.Generic;

using S7P.Numeric;
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
    // ��������
    public MultiplyFloatModifierCollector SkillSpeed = new MultiplyFloatModifierCollector();
    // �˺�����(ȡֵΪ0-100)
    public FloatNumeric Defense { get; set; } = new FloatNumeric();
    // �˺�����
    public MultiplyFloatModifierCollector DamageRate = new MultiplyFloatModifierCollector();
    // �ƶ��ٶ�(��/��)
    public FloatNumeric MoveSpeed = new FloatNumeric();
    // ����ֵ
    public FloatNumeric Shield = new FloatNumeric();
    // �����������ܾ�����
    public BoolNumeric IsDisableSkill = new BoolNumeric();
    // �ҽ�����
    public MultiplyFloatModifierCollector BurnRate = new MultiplyFloatModifierCollector();
    // Ⱥ�˱���
    public MultiplyFloatModifierCollector AoeRate = new MultiplyFloatModifierCollector();

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
        SkillSpeed.Clear();
        Defense.Initialize();
        MoveSpeed.Initialize();
        Shield.Initialize();
        IsDisableSkill.Initialize();
        DamageRate.Clear();
        BurnRate.Clear();
        AoeRate.Clear();

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
        while (Shield.FinalAddCollector.GetModifierList().Count > 0)
        {
            float realShieldValue = Shield.FinalAddCollector.GetModifier(0).Value * (1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // �����ܹ��ֵ�ȫ���˺�
            {
                Shield.FinalAddCollector.GetModifier(0).Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue); // ����ʣ��ֵ����
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.FinalAddCollector.RemoveModifier(Shield.FinalAddCollector.GetModifier(0)); // �Ƴ����㻤��
            }
        }

        // ��������װ���໤��
        while (Shield.AddCollector.GetModifierList().Count > 0)
        {
            float realShieldValue = Shield.AddCollector.GetModifier(0).Value * (1+Shield.PctAddCollector.TotalValue) *(1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // �����ܹ��ֵ�ȫ���˺�
            {
                Shield.AddCollector.GetModifier(0).Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue)/ (1 + Shield.PctAddCollector.TotalValue); // ����ʣ��ֵ����
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.AddCollector.RemoveModifier(Shield.FinalAddCollector.GetModifier(0)); // �Ƴ����㻤��
            }
        }
        return dmg;
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
        BoolDict[key].AddModifier(boolModifier);
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
            BoolDict[key].RemoveModifier(boolModifier);
        }
    }
}