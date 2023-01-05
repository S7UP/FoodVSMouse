/// <summary>
/// ����Ч��
/// </summary>
public class SlowStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private BoolModifier slowDownBoolModifier = new BoolModifier(true);

    public SlowStatusAbility(float PercentValue, BaseUnit pmaster, float time) : base(pmaster, time)
    {
        slowDownFloatModifier = new FloatModifier(PercentValue);
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ч�������Ŀ���Ƿ����߼���)
        if(master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
        {
            ClearLeftTime(); // ʱ������
            SetEffectEnable(false);
        }
        else
        {
            SetEffectEnable(true);
        }
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnDisableEffect()
    {
        master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.SlowDown, slowDownBoolModifier);
        master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        // ΪĿ������ѱ������ٵı�ǩ
        master.NumericBox.AddDecideModifierToBoolDict(StringManager.SlowDown, slowDownBoolModifier);
        // ����
        master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
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
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        SetEffectEnable(false);
    }
}
