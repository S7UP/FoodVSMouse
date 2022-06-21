using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class KangarooMouse : MouseUnit
{
    private BoolModifier IgnoreFrozenModifier = new BoolModifier(true); // ���߱���������
    private BoolModifier IgnoreStunModifier = new BoolModifier(true); // ������ѣ������
    private bool isFirstRoot; // �Ƿ��гԹ�����Ч��  Ҳ������Ϊ�Ƿ�Ϊ��Ծ״̬�ı�׼  false = ��Ծ״̬  true = ����Ծ״̬
    private bool isJumping; // ������Ծ������
    private float jumpDistance; // ��Ծ����

    public override void MInit()
    {
        base.MInit();
        isFirstRoot = false;
        // ���߼���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
        // ��һ���ܵ�����Ч��ǰ���߶���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
        // ��Ӽ���
        // �ڵ�һ�ζ���Ч���������������ߵ�ԭ����ʩ�Ӷ���Ч�����������������Ƴ����߶���Ч��
        statusAbilityManager.AddAfterRemoveStatusAbilityEvent(StringManager.Frozen, 
            delegate {
                if (isFirstRoot)
                    return;
                isFirstRoot = true;
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
                animator.Play("Drop");
            });
        //statusAbilityManager.AddAfterRemoveStatusAbilityEvent(StringManager.Stun, 
        //    delegate {
        //        if (isFirstRoot)
        //            return;
        //        isFirstRoot = true;
        //        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
        //        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
        //        animator.Play("Drop");
        //    });
        jumpDistance = 1 + 0.5f * mShape;
    }

    /// <summary>
    /// ��ͨ��������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����Ծ״̬�£���ͨ�������˺�ִ�б��滻Ϊ������ǰ��һ��,����ж�Ϊ��������Ծ
        // ����Ծ״̬���������߼�
        if (isFirstRoot)
        {
            return base.IsMeetGeneralAttackCondition();
        }
        else
        {
            return base.IsMeetGeneralAttackCondition() && !isJumping;
        }
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ����Ծ״̬�£���ͨ�������˺�ִ�б��滻Ϊ������ǰ��һ��
        if (isFirstRoot)
        {
            //base.ExecuteDamage();
            Debug.Log("base.ExecuteDamage()");
            if (IsHasTarget())
                TakeDamage(GetCurrentTarget());
        }
        else
        {
            // ���һ������������һ��
            isJumping = true;
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * jumpDistance * MapManager.gridWidth, false));
            t.AddOtherEndEvent(delegate { isJumping = false; });
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isFirstRoot)
            animator.Play("Attack");
        else
            animator.Play("Jump");
    }

    public override void OnMoveStateEnter()
    {
        if (isFirstRoot)
            animator.Play("Move1");
        else
            animator.Play("Move0");
    }

    public override void OnDieStateEnter()
    {
        if (isFirstRoot)
            animator.Play("Die1");
        else
            animator.Play("Die0");
    }

    public override void OnIdleStateEnter()
    {
        if (isFirstRoot)
            animator.Play("Idle");
        else
            animator.Play("Move0");
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }
}
