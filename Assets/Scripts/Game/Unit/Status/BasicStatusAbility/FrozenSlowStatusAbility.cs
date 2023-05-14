using S7P.Numeric;
public class FrozenSlowStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private BoolModifier slowDownBoolModifier = new BoolModifier(true);

    public FrozenSlowStatusAbility(float PercentValue, BaseUnit pmaster, float time) : base(pmaster, time)
    {
        slowDownFloatModifier = new FloatModifier(PercentValue);
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ч�������Ŀ���Ƿ����߼��١���������)
        if(master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozenSlowDown) || master.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
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
        master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
        master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        if (!StatusManager.IsUnitFrozen(master))
        {
            GameManager.Instance.audioSourceManager.PlayEffectMusic("Frozen");
        }
        // ΪĿ������ѱ������ٵı�ǩ
        master.NumericBox.AddDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
        // ����
        master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        // ��ӱ�ɫЧ��
        master.SetFrozeSlowEffectEnable(true);
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
        // ��Ч�����������Ŀ�������Ѿ�û�б��������Ч�������Ƴ�Ŀ��ı�ɫЧ��
        if (!StatusManager.IsUnitFrozen(master))
        {
            master.SetFrozeSlowEffectEnable(false);
        }
    }
}
