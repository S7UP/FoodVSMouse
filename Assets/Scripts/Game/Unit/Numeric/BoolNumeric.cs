public class BoolNumeric
{
    // ������ֵ
    // ������ֵ�ļ�����������ռ����Ļ���㣬������Ч�����ռ������Գ�������Value��ֵ���Ƿǣ���������Ч�����ռ�����Ϊtrueʱ���������Ӿ�����Ч�����ռ�����Ӱ���ǿ��������ֵΪfalse
    // ��ʵ������Ĭ���Ĭ���ߣ��ٶ�������ֵ�����Ƿ���ü���
    // ��ô�������ʩ�ӳ�Ĭ����Ч��ʱ��������������Ч�����ռ��������trueֵ�����ռ����ľ���ֵ����true������������ֵ�ĳ���ֵΪtrue�������ܱ�����
    // Ȼ��Ҫʵ�����߳�Ĭ�Ļ�������Ҫ��������Ч�����ռ����д���trueֵ�����ռ����ľ���ֵ����true����������ֵΪ �����ռ����Ļ��ϵ�����������˾�����Ч�����ռ�����true��������ս��ΪĬ��ֵfalse�����������±����ã������˳�ĬЧ��
    public bool Value { get; private set; }
    // ������Ч�����ռ���
    public BoolModifierCollector DecideCollector { get; } = new BoolModifierCollector(); // δ���κ�ֵʱĬ�Ϸ���false
    // ������Ч�����ռ���
    public BoolModifierCollector ImmuneCollector { get; } = new BoolModifierCollector(); // δ���κ�ֵʱĬ�Ϸ���false


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
        // �����ǵ�Чд��
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
