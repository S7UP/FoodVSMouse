public class BoolNumeric
{
    // 最终数值
    // 最终数值的计算采用以下收集器的或计算，决定生效布尔收集器可以初步决定Value的值的是非；而免疫生效布尔收集器在为true时，可以无视决定生效布尔收集器的影响而强制设最终值为false
    // 举实例：沉默与沉默免疫，假定最终数值表是是否禁用技能
    // 那么，当外界施加沉默控制效果时，则是往决定生效布尔收集器里存入true值，该收集器的决定值返回true，所以最终数值的初步值为true，即技能被禁用
    // 然后，要实现免疫沉默的话，则需要往免疫生效布尔收集器中存入true值，该收集器的决定值返回true，而最终数值为 两个收集器的或关系，所以屏蔽了决定生效布尔收集器的true结果，最终结果为默认值false，即技能重新被启用，免疫了沉默效果
    public bool Value { get; private set; }
    // 决定生效布尔收集器
    public BoolModifierCollector DecideCollector { get; } = new BoolModifierCollector(); // 未存任何值时默认返回false
    // 免疫生效布尔收集器
    public BoolModifierCollector ImmuneCollector { get; } = new BoolModifierCollector(); // 未存任何值时默认返回false


    public void Initialize()
    {
        DecideCollector.Modifiers.Clear();
        ImmuneCollector.Modifiers.Clear();
        Update();
    }

    public void AddDecideModifier(BoolModifier modifier)
    {
        DecideCollector.AddModifier(modifier);
        Update();
    }
    public void AddImmuneModifier(BoolModifier modifier)
    {
        ImmuneCollector.AddModifier(modifier);
        Update();
    }
    public void RemoveDecideModifier(BoolModifier modifier)
    {
        DecideCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemoveImmuneModifier(BoolModifier modifier)
    {
        ImmuneCollector.RemoveModifier(modifier);
        Update();
    }

    public void Update()
    {
        Value = (!ImmuneCollector.TotalValue && DecideCollector.TotalValue);
        // 以下是等效写法
        //if (ImmuneCollector.TotalValue)
        //{
        //    Value = false;
        //}
        //else
        //{
        //    Value = DecideCollector.TotalValue;
        //}
    }
}
