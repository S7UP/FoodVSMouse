using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��è��
/// </summary>
public class PandaMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility; // ƽA����
    private ThrowLittleMouseSkillAbility throwLittleMouseSkillAbility; // Ͷ�����缼��

    private float throwPercent; // �Ӷ�����Ҫ�����Ѫ�߰ٷֱ�

    public override void MInit()
    {
        base.MInit();
        // ����
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        throwPercent = 0.5f;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetingThrowCondition())
        {
            ExcuteThrow();
        }
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

        // Ͷ��С������
        if (infoList.Count > 1)
        {
            throwLittleMouseSkillAbility = new ThrowLittleMouseSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(throwLittleMouseSkillAbility);
            throwLittleMouseSkillAbility.master = this;
        }
    }

    /// <summary>
    /// Ͷ����������
    /// </summary>
    /// <returns></returns>
    private bool IsMeetingThrowCondition()
    {
        return (GetHeathPercent() < throwPercent && !throwLittleMouseSkillAbility.isThrow && GetColumnIndex()>2);
    }

    /// <summary>
    /// ִ��Ͷ������һ��
    /// </summary>
    private void ExcuteThrow()
    {
        throwLittleMouseSkillAbility.SetSkillConditionEnable();
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // ����������һ�κ��˳�����״̬
        if (AnimatorManager.GetNormalizedTime(animator)>1.0)
        {
            if (throwLittleMouseSkillAbility != null)
            {
                throwLittleMouseSkillAbility.CloseSkill();
            }
        }
        else if (AnimatorManager.GetNormalizedTime(animator) > 0.4 && mAttackFlag)
        {
            mAttackFlag = false;
            if (throwLittleMouseSkillAbility != null)
            {
                // Ͷ�����X����Ϊ  ����X�������������ĸ�  ��  ��������ĵ�X �����еĽ�Сֵ������ԶҲֻ���ӵ������
                float targetX = Mathf.Max( transform.position.x - 4 * GameController.Instance.mMapController.gridWidth, MapManager.GetColumnX(1));
                throwLittleMouseSkillAbility.ThrowEntity(new Vector3(targetX, MapManager.GetRowY(GetRowIndex()), 0));
            }
        }
    }

    public override void OnCastStateExit()
    {
        mAttackFlag = true;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        if (IsHasTarget())
        {
            // ������Ⱥ��
            BaseGrid grid = GetCurrentTarget().GetGrid();
            if (grid != null)
            {
                foreach (var item in grid.GetAttackableFoodUnitList())
                {
                    TakeDamage(item);
                }
            }
            else
            {
                TakeDamage(GetCurrentTarget());
            }
        }
            
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
    }


    public override void OnIdleStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Idle1", true);
        }
        else
        {
            animatorController.Play("Idle0", true);
        }
        
    }

    public override void OnMoveStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Move1", true);
        }
        else
        {
            animatorController.Play("Move0", true);
        }
    }

    public override void OnAttackStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Attack1");
        }
        else
        {
            animatorController.Play("Attack0");
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }
}
