using System.Collections.Generic;

/// <summary>
/// 战斗数值匣子，在这里管理所有角色战斗数值的存储、变更、刷新等
/// </summary>
public class CombatNumericBox
{
    // 生命值
    public FloatNumeric Hp = new FloatNumeric();
    // 攻击力
    public FloatNumeric Attack = new FloatNumeric();
    // 攻击速度
    public FloatNumeric AttackSpeed = new FloatNumeric();
    // 伤害减免(取值为0-100)
    public FloatNumeric Defense = new FloatNumeric();
    // 伤害比率
    public MultiplyFloatModifierCollector DamageRate = new MultiplyFloatModifierCollector();
    // 移动速度(格/秒)
    public FloatNumeric MoveSpeed = new FloatNumeric();
    // 射程（格）
    public FloatNumeric Range = new FloatNumeric();
    // 护盾值
    public FloatNumeric Shield = new FloatNumeric();
    // 禁用主动技能决定器
    public BoolNumeric IsDisableSkill = new BoolNumeric();

    // 其他自定义属性
    public Dictionary<string, IntNumeric> IntDict = new Dictionary<string, IntNumeric>();
    public Dictionary<string, FloatNumeric> FloatDict = new Dictionary<string, FloatNumeric>();
    public Dictionary<string, BoolNumeric> BoolDict = new Dictionary<string, BoolNumeric>();

    public void Initialize()
    {
        // 这里初始化base值
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
    /// 外部添加护盾（动态）
    /// </summary>
    public void AddDynamicShield(float value)
    {
        Shield.AddFinalAddModifier(new FloatModifier(value));
    }

    /// <summary>
    /// 对护盾造成伤害
    /// 当存在多个护盾时，吸收的优先级为 BUFF类护盾（限时类）>装备类护盾（永久性），同类型下 遵循先进先出原则，即先上的盾先吸收伤害
    /// </summary>
    /// <returns>剩余伤害，如果剩余伤害小于等于0说明伤害完全被护盾吸收</returns>
    public float DamageShield(float dmg)
    {
        // 遍历所有BUFF类护盾
        while (Shield.FinalAddCollector.Modifiers.Count > 0)
        {
            float realShieldValue = Shield.FinalAddCollector.Modifiers[0].Value * (1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // 护盾能够抵挡全部伤害
            {
                Shield.FinalAddCollector.Modifiers[0].Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue); // 护盾剩余值计算
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.FinalAddCollector.Modifiers.RemoveAt(0); // 移除本层护盾
            }
        }

        // 遍历所有装备类护盾
        while (Shield.AddCollector.Modifiers.Count > 0)
        {
            float realShieldValue = Shield.AddCollector.Modifiers[0].Value * (1+Shield.PctAddCollector.TotalValue) *(1 + Shield.FinalPctAddCollector.TotalValue);
            if (realShieldValue > dmg) // 护盾能够抵挡全部伤害
            {
                Shield.AddCollector.Modifiers[0].Value = (realShieldValue - dmg) / (1 + Shield.FinalPctAddCollector.TotalValue)/ (1 + Shield.PctAddCollector.TotalValue); // 护盾剩余值计算
                return dmg - realShieldValue;
            }
            else
            {
                dmg -= realShieldValue;
                Shield.AddCollector.Modifiers.RemoveAt(0); // 移除本层护盾
            }
        }
        return dmg;
    }

    /// <summary>
    /// 添加禁用技能效果（沉默）
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddDisAbleSkillModifier()
    {
        BoolModifier boolModifier = new BoolModifier(true);
        IsDisableSkill.AddDecideModifier(boolModifier);
        return boolModifier;
    }

    /// <summary>
    /// 添加免疫禁用技能效果（沉默免疫）
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddImmuneDisAbleSkillModifier()
    {
        BoolModifier boolModifier = new BoolModifier(true);
        IsDisableSkill.AddImmuneModifier(boolModifier);
        return boolModifier;
    }

    /// <summary>
    /// 移除禁用技能效果（沉默）
    /// </summary>
    /// <returns></returns>
    public void RemoveDisAbleSkillModifier(BoolModifier boolModifier)
    {
        IsDisableSkill.RemoveDecideModifier(boolModifier);
    }

    /// <summary>
    /// 移除免疫禁用技能效果（沉默免疫）
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
    /// 添加决定生效布尔修饰器
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
    /// 添加免疫生效布尔修饰器
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
    /// 移除决定生效布尔修饰器
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
    /// 移除免疫生效布尔修饰器
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