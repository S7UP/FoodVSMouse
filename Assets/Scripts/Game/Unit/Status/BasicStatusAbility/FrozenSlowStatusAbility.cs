using UnityEngine;

public class FrozenSlowStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private BoolModifier slowDownBoolModifier;

    public FrozenSlowStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

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
        if (slowDownBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
            slowDownBoolModifier = null;
        }

        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        if(slowDownBoolModifier == null)
        {
            // ΪĿ������ѱ������ٵı�ǩ
            slowDownBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.FrozenSlowDown, slowDownBoolModifier);
        }

        if(slowDownFloatModifier == null)
        {
            // ʵ�ʼ���Ч��Ӧ��
            slowDownFloatModifier = new FloatModifier(-50);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
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
        // return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        SetEffectEnable(false);
        // ��Ч�����������Ŀ�������Ѿ�û�б��������Ч�������Ƴ�Ŀ��ı�ɫЧ��
        if (!TagsManager.IsUnitFrozen(master))
        {
            master.SetFrozeSlowEffectEnable(false);
        }
        // ע�⣬������ֱ�Ӵӱ����Ƴ������
        master.RemoveNoCountUniqueStatusAbility(StringManager.FrozenSlowDown);
    }

    /// <summary>
    /// ����Ψһ��״̬����״̬����ʱ�ٱ�ʩ��ͬһ״̬ʱ������ʩ��״̬���������
    /// </summary>
    public override void OnCover()
    {
        // Ϊ����������״̬ǰ���ȼ��Ŀ���Ƿ��Ѵ������״̬�ˣ������ֱ�����ó���ʱ��
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.FrozenSlowDown);
        if (sa != null)
        {
            FrozenSlowStatusAbility f = (sa as FrozenSlowStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // ֮���������Ӧ�ûᱻϵͳ��������������ԭ����sa)��ΪĿ���״̬�����ﲻ��Ҫ�����Ƿ������Լ���ô������֪������¾�����
        }
    }
}
