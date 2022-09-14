using UnityEngine;

/// <summary>
/// ��ѣ״̬
/// </summary>
public class StunStatusAbility : StatusAbility
{
    private FrozenState frozenState;
    private BoolModifier boolModifier;
    private BoolModifier stunBoolModifier = new BoolModifier(true);
    private bool isForce = false; // �Ƿ�ǿ����Ч����������Ч��

    public StunStatusAbility(BaseUnit pmaster, float time, bool isForce) : base(pmaster, time)
    {
        this.isForce = isForce;
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ƿ����߶���
        if (!isForce && master.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun))
        {
            ClearLeftTime();
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
        master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Stun, stunBoolModifier);
        if (frozenState != null)
        {
            frozenState.TryExitCurrentState();
            frozenState = null;
        }
        if (boolModifier != null)
        {
            master.RemoveDisAbleSkillModifier(boolModifier);
            boolModifier = null;
        }
    }

    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        // ΪĿ����ӱ����εı�ǩ
        master.NumericBox.AddDecideModifierToBoolDict(StringManager.Stun, stunBoolModifier);

        // Ŀ�궯��״̬ת��Ϊ����״̬
        if (frozenState == null)
        {
            frozenState = new FrozenState(master, master.mCurrentActionState);
            master.SetActionState(frozenState);
        }
        if (boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
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

        master.RemoveNoCountUniqueStatusAbility(StringManager.Stun);
    }

    /// <summary>
    /// ����Ψһ��״̬����״̬����ʱ�ٱ�ʩ��ͬһ״̬ʱ������ʩ��״̬���������
    /// </summary>
    public override void OnCover()
    {
        // Ϊ����������״̬ǰ���ȼ��Ŀ���Ƿ��Ѵ������״̬�ˣ������ֱ�����ó���ʱ��
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.Stun);
        if (sa != null)
        {
            StunStatusAbility f = (sa as StunStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // ֮���������Ӧ�ûᱻϵͳ��������������ԭ����sa)��ΪĿ���״̬�����ﲻ��Ҫ�����Ƿ������Լ���ô������֪������¾�����
        }
    }
}
