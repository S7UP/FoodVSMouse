/// <summary>
/// �ı乥��״̬
/// </summary>
public class AttackSpeedStatusAbility : StatusAbility
{
    private FloatModifier floatModifier; // ��ǰ�ṩ����Ч����������
    private float changePercent; // �������ٸı�ٷֱ�

    public AttackSpeedStatusAbility(BaseUnit pmaster, float changePercent) : base(pmaster)
    {
        this.changePercent = changePercent;
        floatModifier = new FloatModifier(changePercent);
    }

    public AttackSpeedStatusAbility(BaseUnit pmaster, float time, float changePercent) : base(pmaster, time)
    {
        this.changePercent = changePercent;
        floatModifier = new FloatModifier(changePercent);
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ��� ����Ǽ�����Ч�����Լ�Ŀ���Ƿ����߼�����
        if (changePercent < 0 && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreDecAttackSpeed))
        {
            // ���ߵĻ�ֱ�ӽ���Ч������
            SetEffectEnable(false);
        }
        else
        {
            OnEnableEffect();
        }
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnDisableEffect()
    {
        master.NumericBox.AttackSpeed.RemovePctAddModifier(floatModifier);
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        master.NumericBox.AttackSpeed.AddPctAddModifier(floatModifier);
    }

    /// <summary>
    /// ������Ч���ڼ�
    /// </summary>
    public override void OnEffecting()
    {

    }

    /// <summary>
    /// �ڷ�����Ч���ڼ�
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// �����������������ʱ���ǻ��ϵ��
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
        // return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        OnDisableEffect();
    }
}
