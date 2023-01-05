using System.Collections.Generic;

using UnityEngine;

public class PenguinMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility; // ƽA����
    private ThrowIceBombSkillAbility throwIceBombSkillAbility; // Ͷ����ը������
    private bool isFindTarget;
    private Vector3 targetPosition;
    public override void MInit()
    {
        isFindTarget = false;
        base.MInit();
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // �����Ҷ���ʱ�Զ���ը��
        if (!throwIceBombSkillAbility.isThrow && GetColumnIndex() < 7)
        {
            SearchTargetPosition();
            if (isFindTarget)
            {
                ExcuteThrow();
            }
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

        // Ͷ����ը������
        if (infoList.Count > 1)
        {
            throwIceBombSkillAbility = new ThrowIceBombSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(throwIceBombSkillAbility);
            throwIceBombSkillAbility.master = this;
        }
    }

    /// <summary>
    /// ��һ�ν��빥��״̬ʱ��ʹ��Ͷ������ȡ����ͨ����
    /// </summary>
    public override void OnAttackStateEnter()
    {
        if (!throwIceBombSkillAbility.isThrow)
        {
            SearchTargetPosition();
            if (isFindTarget)
            {
                ExcuteThrow();
            }
            // ��ϵ�ǰ����
            generalAttackSkillAbility.EndActivate();
        }
        else
        {
            base.OnAttackStateEnter();
        }
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ���һ���ж��������������ڼ�
        return !throwIceBombSkillAbility.isSpelling && base.IsMeetGeneralAttackCondition();
    }

    /// <summary>
    /// ִ��Ͷ������һ��
    /// </summary>
    private void ExcuteThrow()
    {
        throwIceBombSkillAbility.SetSkillConditionEnable();
    }

    /// <summary>
    /// ��Ҫ����Ͷ������������ƶ���ͼ
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if (throwIceBombSkillAbility.isThrow)
        {
            animatorController.Play("Move1", true);
        }
        else
        {
            animatorController.Play("Move0", true);
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
        // ����������һ�κ��˳�����״̬
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            if (throwIceBombSkillAbility != null)
            {
                throwIceBombSkillAbility.CloseSkill();
            }
        }else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5 && mAttackFlag)
        {
            mAttackFlag = false;
            if (throwIceBombSkillAbility != null)
            {
                SearchTargetPosition(); // Ͷ������ǰ�ټ��һ��
                throwIceBombSkillAbility.ThrowEntity(targetPosition);
            }
        }
    }

    public override void OnCastStateExit()
    {
        mAttackFlag = true;
    }

    /// <summary>
    /// Ѱ��Ͷ�����
    /// </summary>
    private void SearchTargetPosition()
    {
        int rowIndex = GetRowIndex();
        List<BaseUnit> list = GameController.Instance.GetAllyList()[rowIndex];
        // �ҵ�ǰ�п����ĵ�λ
        bool flag = false;
        int minColumnIndex = GetColumnIndex() + 1; // ���ó�ʼ����Ϊ�Լ�������������һ���Ա���������ΧΪ������ߵ��Լ����ڸ���
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                if (!UnitManager.CanBeSelectedAsTarget(this, item))
                    continue;
                int temp = item.GetColumnIndex();
                if (temp < minColumnIndex)
                {
                    minColumnIndex = temp;
                    flag = true;
                }
            }
        }
        if (flag)
        {
            isFindTarget = true;
            targetPosition = MapManager.GetGridLocalPosition(minColumnIndex, rowIndex);
            return;
        }

        // ���û�У����������������ĵ�λ
        // ����
        if (rowIndex > 0)
        {
            list = GameController.Instance.GetAllyList()[rowIndex-1];
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (!UnitManager.CanBeSelectedAsTarget(this, item))
                        continue;
                    int temp = item.GetColumnIndex();
                    if (temp < minColumnIndex)
                    {
                        minColumnIndex = temp;
                        flag = true;
                    }
                }
            }
        }
        // ����
        if (rowIndex < 6)
        {
            list = GameController.Instance.GetAllyList()[rowIndex + 1];
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (!UnitManager.CanBeSelectedAsTarget(this, item))
                        continue;
                    int temp = item.GetColumnIndex();
                    if (temp < minColumnIndex)
                    {
                        minColumnIndex = temp;
                        flag = true;
                    }
                }
            }
        }
        // 
        if (flag)
        {
            isFindTarget = true;
            targetPosition = MapManager.GetGridLocalPosition(minColumnIndex, rowIndex);
            return;
        }
        else
        {
            // ��tm���ſ����𣿣�����һ�ſ���û�У�
            // ���������target���ḳֵ����λ���ּ���ǰ��ֱ���зſ�
        }
    }

    /// <summary>
    /// ���������˵����ˮ���ε�Σ��Ȩ�غ�½����һ����
    /// </summary>
    public override void SetGridDangerousWeightDict()
    {
        GridDangerousWeightDict[GridType.Water] = GridDangerousWeightDict[GridType.Default];
    }
}
