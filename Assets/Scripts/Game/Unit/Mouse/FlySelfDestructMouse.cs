using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlySelfDestructMouse : MouseUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true); // ����ը����ɱЧ��
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // ���߶���
    private Vector2 start_position; // ��ʼ׹����
    private Vector2 target_position; // Ŀ�����

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 3; // ������Ĭ��Ϊ3����������
        // ��ʼ����ը����ɱЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // ���ܵ��˺�����֮��ֱ���ж�Ϊ��׹״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExcuteDrop(); });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExcuteDrop();
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (GetColumnIndex() <= dropColumn && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    private void ExcuteDrop()
    {
        if (!isDrop)
        {
            // �����׹�����˺��ʵ��һЩ��Ϊ�ᷢ���仯
            isDrop = true; 
            // �Ƴ�����ը����ɱЧ��
            if (IgnoreBombInstantKill != null)
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
            // ��Ӷ�������Ч��
            if(IgnoreFrozen != null)
            {
                NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
                // ͬʱ�Ƴ��������ж���Ч��
                statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
            }
            // ȷ����ʼ������յ㣬�������յ�Ϊ��ʼ��+3��*��ǰ��׼�ƶ��ٶ�ֵ������С����Ϊ��һ��
            start_position = transform.position;
            target_position = transform.position + Vector3.left * 3 * MapManager.gridWidth * TransManager.TranToStandardVelocity(NumericBox.MoveSpeed.Value);
            if(target_position.x < MapManager.GetColumnX(0))
            {
                target_position = new Vector2(MapManager.GetColumnX(0), target_position.y);
            }
            // ��Ϊת��״̬
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isDrop)
        {
            animator.Play("Move1");
        }
        else
        {
            animator.Play("Move0");
        }
    }

    public override void OnMoveState()
    {
        if (currentStateTimer == 0)
            return;

        if (isDrop && !isDeathState)
        {
            float t = AnimatorManager.GetNormalizedTime(animator);
            rigibody2D.MovePosition(Vector2.Lerp(start_position, target_position, t));
            // ��������±�����׹�䣬ִ������
            if (t >= 1)
            {
                ExecuteDeath();
            }
        }
        else
        {
            base.OnMoveState();
        }
    }

    /// <summary>
    /// ��������ʱ����һ����Ƭ
    /// </summary>
    public override void BeforeDeath()
    {
        base.BeforeDeath();
        BaseGrid grid = GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex());
        if (grid != null)
        {
            BaseUnit unit = grid.GetHighestAttackPriorityUnit();
            if (unit != null)
            {
                new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, unit, float.MaxValue).ApplyAction();
            }
        }
    }
}
