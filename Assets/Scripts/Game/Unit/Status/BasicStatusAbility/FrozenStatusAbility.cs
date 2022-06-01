using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����״̬
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    private FrozenState frozenState;
    private BoolModifier boolModifier;
    private BoolModifier frozenBoolModifier;

    public FrozenStatusAbility(BaseUnit pmaster, float time) : base(pmaster, time)
    {

    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ƿ����߶���
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            ClearLeftTime();
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
        if (frozenBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
            frozenBoolModifier = null;
        }
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
        // ΪĿ����ӱ�����ı�ǩ
        if(frozenBoolModifier == null)
        {
            frozenBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
        }

        // Ŀ�궯��״̬ת��Ϊ����״̬
        if (frozenState == null)
        {
            frozenState = new FrozenState(master, master.mCurrentActionState);
            master.SetActionState(frozenState);
        }
        if (boolModifier == null)
            boolModifier = master.AddDisAbleSkillModifier();
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
        OnDisableEffect();
        // ��Ч�����������Ŀ�������Ѿ�û�б��������Ч�������Ƴ�Ŀ��ı�ɫЧ��
        if (!TagsManager.IsUnitFrozen(master))
        {
            master.SetFrozeSlowEffectEnable(false);
        }
        master.RemoveNoCountUniqueStatusAbility(StringManager.Frozen);
    }

    /// <summary>
    /// ����Ψһ��״̬����״̬����ʱ�ٱ�ʩ��ͬһ״̬ʱ������ʩ��״̬���������
    /// </summary>
    public override void OnCover()
    {
        // Ϊ����������״̬ǰ���ȼ��Ŀ���Ƿ��Ѵ������״̬�ˣ������ֱ�����ó���ʱ��
        StatusAbility sa = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.Frozen);
        if (sa != null)
        {
            FrozenStatusAbility f = (sa as FrozenStatusAbility);
            f.totalTime = (f.totalTime.baseValue > this.totalTime.baseValue ? f.totalTime : this.totalTime);
            f.leftTime = Mathf.Max(f.leftTime, this.leftTime);
            // ֮���������Ӧ�ûᱻϵͳ��������������ԭ����sa)��ΪĿ���״̬�����ﲻ��Ҫ�����Ƿ������Լ���ô������֪������¾�����
        }
    }
}
