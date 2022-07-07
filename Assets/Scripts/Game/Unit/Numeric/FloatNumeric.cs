public class FloatNumeric
{
    // ������ֵ
    public float Value { get; private set; }
    // ������ֵ
    public float baseValue { get; private set; }
    // װ����ֵ�ӳ�
    public float add { get; private set; }
    // װ���ٷֱȼӳ�
    public float pctAdd { get; private set; }
    // BUFF�ӳ�
    public float finalAdd { get; private set; }
    // BUFF�ٷֱȼӳ�
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
        add = AddCollector.AddModifier(modifier);
        Update();
    }
    public void AddPctAddModifier(FloatModifier modifier)
    {
        pctAdd = PctAddCollector.AddModifier(modifier);
        Update();
    }
    public void AddFinalAddModifier(FloatModifier modifier)
    {
        finalAdd = FinalAddCollector.AddModifier(modifier);
        Update();
    }
    public void AddFinalPctAddModifier(FloatModifier modifier)
    {
        finalPctAdd = FinalPctAddCollector.AddModifier(modifier);
        Update();
    }
    public void RemoveAddModifier(FloatModifier modifier)
    {
        add = AddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemovePctAddModifier(FloatModifier modifier)
    {
        pctAdd = PctAddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemoveFinalAddModifier(FloatModifier modifier)
    {
        finalAdd = FinalAddCollector.RemoveModifier(modifier);
        Update();
    }
    public void RemoveFinalPctAddModifier(FloatModifier modifier)
    {
        finalPctAdd = FinalPctAddCollector.RemoveModifier(modifier);
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
