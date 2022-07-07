/// <summary>
/// ��ˮ״̬debuff
/// </summary>
public class WaterStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private BoolModifier waterStatusBoolModifier; // ��ˮ״̬��־

    public WaterStatusAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ƿ�������ˮ
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
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
        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
        if (waterStatusBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.WaterGridState, waterStatusBoolModifier);
            waterStatusBoolModifier = null;
        }
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        if (slowDownFloatModifier == null)
        {
            // ���ˮ���μ���Ч�����������Ч��ֵͨ����ȡ��ǰ�ؿ�Ԥ��ֵ
            // slowDownFloatModifier = new FloatModifier(GameController.Instance.GetNumberManager().GetValue(StringManager.WaterSlowDown));
            slowDownFloatModifier = new FloatModifier(-50);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
        if(waterStatusBoolModifier == null)
        {
            waterStatusBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.WaterGridState, waterStatusBoolModifier);
        }
    }

    /// <summary>
    /// ������Ч���ڼ�
    /// </summary>
    public override void OnEffecting()
    {
        // ����Դ�ĳ����˺�
        // float percentDamgePerSeconds = GameController.Instance.GetNumberManager().GetValue(StringManager.WaterPerCentDamge);
        new DamageAction(CombatAction.ActionType.CauseDamage, null, master, master.NumericBox.Hp.Value*0.05f/ConfigManager.fps).ApplyAction();
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
