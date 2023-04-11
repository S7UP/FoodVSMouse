using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ��еͶ����
/// </summary>
public class AerialBombardmentMouse : MouseUnit, IFlyUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private FlyThrowBombSkillAbility flyThrowBombSkillAbility;

    private bool isDrop; // �Ƿ񱻻���

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        // ����
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // Ͷը������
        if (infoList.Count > 1)
        {
            flyThrowBombSkillAbility = new FlyThrowBombSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(flyThrowBombSkillAbility);
        }
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void CheckUnit(Collider2D collision)
    {
        // ����CD�����ܳ����䡢�������������ʱ�������κ���ײ�¼�
        if (isDrop || isDeathState || flyThrowBombSkillAbility.isSpelling || !flyThrowBombSkillAbility.IsEnergyEnough())
        {
            return;
        }

        if(collision.tag.Equals("Food"))
        {
            // ��⵽��λ��ײ�ˣ�
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            // ��Ȿ����ʳ����ܻ����ȼ���λ
            BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnit(this);
            if (target != null)
            {
                // ��Ǽ��ܷ���Ŀ���ˣ�����ִ��Ͷ������
                flyThrowBombSkillAbility.SetSkillConditionEnable(target.transform.position.x);
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        CheckUnit(collision);
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);
        CheckUnit(collision);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            // ����ǰ����ֵ���ڷ���״̬�ٽ�㣬����Ҫǿ��ͬ������ֵ���ٽ��
            if(mCurrentHp> mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0]*mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ǿ�Ƹ���һ����ͼ
            // �����ʱ��Ͷ������ôӦ������ȡ��Ͷ��״̬
            if (flyThrowBombSkillAbility.isSpelling)
            {
                flyThrowBombSkillAbility.EndActivate();
            }
            // ��Ϊת��״̬����״̬�µľ���ʵ�����¼�������
            SetActionState(new TransitionState(this));
        }
    }


    /// <summary>
    /// ����������״̬ʱ��Ӧ����ȫ�����ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return !(mCurrentActionState is TransitionState) && base.CanHit(bullet);
    }

    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return !(mCurrentActionState is TransitionState) && base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// ����ͼ����ʱҪ������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // ����һ���л����ǵ�0�׶ε���ͼʱ�����˳�����״̬��������0�׶ε�Ѫ���ٷֱ���Ϊ����1.0������֮����Զ�ﲻ������Ȼ�󲥷�׹�䶯��
        // ��ǰȡֵ��ΧΪ1~3ʱ����
        // 0 ����
        // 1 �������->�����ƶ�
        // 2 �����ƶ�
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
        {
            ExecuteDrop();
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // ����������һ�κ�֪ͨ����Ч������
        //if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            flyThrowBombSkillAbility.CloseSkill();
        }
    }

    /// <summary>
    /// ����ת��״̬ʱҪ�����£�������ָ����ձ�����ʱҪ����
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // �߶Ƚ���Ϊ����߶�
    }
}
