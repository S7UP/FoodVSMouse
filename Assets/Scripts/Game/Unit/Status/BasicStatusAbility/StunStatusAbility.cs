using UnityEngine;
using S7P.Numeric;

/// <summary>
/// ��ѣ״̬
/// </summary>
public class StunStatusAbility : StatusAbility
{
    private BoolModifier disableSkillMod = new BoolModifier(true);
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
        // ����Ƿ�������ѣ
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
        if (master.mCurrentActionState is FrozenState)
        {
            (master.mCurrentActionState as FrozenState).TryExitCurrentState();
        }
        master.NumericBox.IsDisableSkill.RemoveModifier(disableSkillMod);

        // �Ƴ���ѣ��Ч
        if (master.IsContainEffect(StringManager.Stun))
        {
            master.RemoveEffectFromDict(StringManager.Stun);
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
        if (!(master.mCurrentActionState is FrozenState))
        {
            master.SetActionState(new FrozenState(master, master.mCurrentActionState));
        }

        master.NumericBox.IsDisableSkill.AddModifier(disableSkillMod);

        // �����ѣ��Ч
        if (!master.IsContainEffect(StringManager.Stun))
        {
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/Stun"), null, "Stun", null, true);
            string name;
            int order;
            if(master.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 5);
            }
            GameController.Instance.AddEffect(e);
            master.AddEffectToDict(StringManager.Stun, e, new Vector2(0, 0));
        }
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
        StatusAbility s = master.statusAbilityManager.GetNoCountUniqueStatus(StringManager.Stun);
        if (s != null)
        {
            s.totalTime = (s.totalTime.baseValue > this.totalTime.baseValue ? s.totalTime : this.totalTime);
            s.leftTime = Mathf.Max(s.leftTime, this.leftTime);
            // ֮���������Ӧ�ûᱻϵͳ��������������ԭ����sa)��ΪĿ���״̬�����ﲻ��Ҫ�����Ƿ������Լ���ô������֪������¾�����
        }
    }
}
