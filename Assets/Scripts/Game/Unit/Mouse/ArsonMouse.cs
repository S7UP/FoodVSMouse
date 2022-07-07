using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �ݻ���
/// </summary>
public class ArsonMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility; // ƽA����
    private ThrowBombSkillAbility throwBombSkillAbility; // Ͷ��ը������
    private bool isFindTarget;
    private Vector3 targetPosition;
    public int preTime; // Ԥ��ʱ��

    public override void MInit()
    {
        base.MInit();
        isFindTarget = false;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // �����Ҷ���ʱ�Զ���ը��
        if (!throwBombSkillAbility.isThrow && GetColumnIndex() < 7)
        {
            SearchTargetPosition();
            if (isFindTarget)
            {
                ExcuteThrow();
            }
        }
    }

    /// <summary>
    /// ���ؼ��ܣ�������ͨ�����뼼��
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
        // Ͷ��ը������
        if (infoList.Count > 1)
        {
            throwBombSkillAbility = new ThrowBombSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(throwBombSkillAbility);
            throwBombSkillAbility.master = this;
        }

    }

    /// <summary>
    /// ��һ�ν��빥��״̬ʱ��ʹ��Ͷ������ȡ����ͨ����
    /// </summary>
    public override void OnAttackStateEnter()
    {
        if (!throwBombSkillAbility.isThrow)
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
            Debug.Log("base.OnAttackStateEnter()");
        }
    }

    /// <summary>
    /// ִ��Ͷ������һ��
    /// </summary>
    private void ExcuteThrow()
    {
        throwBombSkillAbility.SetSkillConditionEnable();
    }

    /// <summary>
    /// ��Ҫ����Ͷ������������ƶ���ͼ
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if (throwBombSkillAbility.isThrow)
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
        if (throwBombSkillAbility.IsFinishPreCast())
        {
            animatorController.Play("Cast");
        }
        else
        {
            animatorController.Play("PreCast");
        }
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // �ȵȴ������Ƿ������Ԥ����������ɺ����ִ�к����Ͷ��ʱ���ж��߼�
        if (!throwBombSkillAbility.IsFinishPreCast())
            return;

        // ����������һ�κ��˳�����״̬
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            if (throwBombSkillAbility != null)
            {
                throwBombSkillAbility.CloseSkill();
            }
        }else if (AnimatorManager.GetNormalizedTime(animator) > 0.5 && mAttackFlag)
        {
            mAttackFlag = false;
            if (throwBombSkillAbility != null)
            {
                SearchTargetPosition();  // Ͷ������ǰ�ټ��һ��
                throwBombSkillAbility.ThrowEntity(targetPosition);
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
                if (!item.CanBeSelectedAsTarget())
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
                    if (!item.CanBeSelectedAsTarget())
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
                    if (!item.CanBeSelectedAsTarget())
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
}
