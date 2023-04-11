using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ��������
/// </summary>
public class NonMainstreamMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private DodgeSpikesSkillAbility dodgeSpikesSkillAbility;
    private FloatModifier floatModifier = new FloatModifier(200); // ����������
    private FloatModifier defenceModifier; // ����������
    private FloatModifier attackModifier; // �ٷֱȹ������ӳ�������
    private int currentActionCount; // ��ǰ����������
    private float defenceValue; // ���˷���
    private float addAttackPercentValue; // �������ӳɷ���

    public override void MInit()
    {
        base.MInit();
        defenceValue = 0.5f;
        addAttackPercentValue = 0;
        NumericBox.MoveSpeed.AddPctAddModifier(floatModifier);  // ֱ�ӻ���ƶ��ٶȼӳ�
        defenceModifier = new FloatModifier(defenceValue); // ��ʼ���90%����
        NumericBox.Defense.AddAddModifier(defenceModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
        currentActionCount = 0;
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

        // ����ͻ������
        if (infoList.Count > 1)
        {
            dodgeSpikesSkillAbility = new DodgeSpikesSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(dodgeSpikesSkillAbility);
            dodgeSpikesSkillAbility.SetEndEvent(SkillEndEvent); // ���ü��ܽ����¼�
        }
    }

    /// <summary>
    /// �����ӵ���λ����������ײ�ж�ʱ
    /// </summary>
    public override void OnBulletCollision(BaseBullet bullet)
    {
        if (bullet.GetRowIndex() != GetRowIndex())
            return;

        // ���ƶ�״̬�� �� û�б��赲ʱ ��������ͻ������
        if(mCurrentActionState!=null && mCurrentActionState is MoveState && !IsHasTarget())
        {
            dodgeSpikesSkillAbility.SetSkillEnable();
        }else if (UnitManager.CanBulletHit(this, bullet))
        {
            // ������������ӵ���ײ�߼�
            bullet.TakeDamage(this);
        }
    }

    /// <summary>
    /// ��д���ӵ���ײ���ж��������ܼ����ڼ䲻���ӵ��ж��������������
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet baseBullet)
    {
        return (IsBlock() || isFrozenState) && base.CanHit(baseBullet);
    }

    /// <summary>
    /// ֻ�б��赲���߱�����ѣʱ�Żᱻ��Ϊ����Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return (IsBlock() || isFrozenState) && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override void OnCastStateEnter()
    {
        currentActionCount++;
        animatorController.Play("Dodge"+(currentActionCount%3));
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        // ���������� �� ���赲����ֹͣ��ܼ���
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce() || IsHasTarget())
            dodgeSpikesSkillAbility.EndSkill();
    }

    /// <summary>
    /// ��ͨ���������󣬹������ӳɱ�ǩ����
    /// </summary>
    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        addAttackPercentValue = 0;
        // �������ӳ���ֵ��ǩ�滻
        NumericBox.Attack.RemovePctAddModifier(attackModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
    }

    /// <summary>
    /// �����ܼ��ܽ���ʱ���¼�
    /// </summary>
    private void SkillEndEvent()
    {
        defenceValue = Mathf.Max(0, defenceValue - 0.25f);
        addAttackPercentValue = Mathf.Min(100, addAttackPercentValue + 50);
        // ������ֵ��ǩ�滻
        NumericBox.Defense.RemoveAddModifier(defenceModifier);
        if (defenceValue > 0)
        {
            defenceModifier = new FloatModifier(defenceValue);
            NumericBox.Defense.AddAddModifier(defenceModifier);
        }
        // �������ӳ���ֵ��ǩ�滻
        NumericBox.Attack.RemovePctAddModifier(attackModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
    }

}
