using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ����״̬
/// </summary>
public class FrozenStatusAbility : StatusAbility
{
    // private FrozenState frozenState;
    // private BoolModifier boolModifier;
    private BoolModifier frozenBoolModifier;
    //private StunStatusAbility stunStatusAbility; // ��ѣЧ��
    private bool isForce = false; // �Ƿ�ǿ����Ч����������Ч��

    public FrozenStatusAbility(BaseUnit pmaster, float time, bool isForce) : base(pmaster, time)
    {
        this.isForce = isForce;
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����Ƿ����߶���
        if (!isForce && (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen) || master.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun)))
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
        if (frozenBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Frozen, frozenBoolModifier);
            frozenBoolModifier = null;
        }
        // �Ƴ���ѣЧ��
        master.RemoveNoCountUniqueStatusAbility(StringManager.Stun);
        // �Ƴ�������Ч
        if (master.IsContainEffect(StringManager.Frozen))
        {
            master.RemoveEffectFromDict(StringManager.Frozen);
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
        // ΪĿ��ʩ����ѣЧ��
        StatusAbility s = master.GetNoCountUniqueStatus(StringManager.Stun);
        if (s == null)
        {
            s = new StunStatusAbility(master, leftTime, isForce);
            master.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
        }
        else
        {
            s.leftTime = Mathf.Max(s.leftTime, this.leftTime);
            s.totalTime = (s.totalTime.baseValue > this.totalTime.baseValue ? s.totalTime : this.totalTime);
        }
        // ��ӱ�ɫЧ��
        master.SetFrozeSlowEffectEnable(true);
        // ��ӱ�����Ч
        if (!master.IsContainEffect(StringManager.Frozen))
        {
            GameManager.Instance.audioSourceManager.PlayEffectMusic("Frozen");

            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/Frozen"), null, "Frozen", "Break", true);
            GameController.Instance.AddEffect(e);
            string name;
            int order;
            if (master.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 5);
            }
            master.AddEffectToDict(StringManager.Frozen, e, new Vector2(0, 0));
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
        // return false;
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
            // ��ѣʱ��ҲҪ���ã�
            StatusAbility s = master.GetNoCountUniqueStatus(StringManager.Stun);
            if (s == null)
                s = new StunStatusAbility(master, f.totalTime.baseValue, false);
            s.totalTime = (s.totalTime.baseValue > this.totalTime.baseValue ? s.totalTime : this.totalTime);
            s.leftTime = Mathf.Max(s.leftTime, this.leftTime);
            master.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
        }
    }
}
