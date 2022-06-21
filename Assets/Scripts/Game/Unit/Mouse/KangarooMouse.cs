using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 袋鼠类
/// </summary>
public class KangarooMouse : MouseUnit
{
    private BoolModifier IgnoreFrozenModifier = new BoolModifier(true); // 免疫冰冻修饰器
    private BoolModifier IgnoreStunModifier = new BoolModifier(true); // 免疫晕眩修饰器
    private bool isFirstRoot; // 是否有吃过定身效果  也可以作为是否定为跳跃状态的标准  false = 跳跃状态  true = 非跳跃状态
    private bool isJumping; // 正在跳跃过程中
    private float jumpDistance; // 跳跃距离

    public override void MInit()
    {
        base.MInit();
        isFirstRoot = false;
        // 免疫减速效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
        // 第一次受到定身效果前免疫定身效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
        // 添加监听
        // 在第一次定身效果结束后（由于免疫的原理是施加定身效果后立即结束），移除免疫定身效果
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
    /// 普通攻击条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 在跳跃状态下，普通攻击的伤害执行被替换为自身向前跳一格,因此判定为完成这次跳跃
        // 非跳跃状态采用正常逻辑
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
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 在跳跃状态下，普通攻击的伤害执行被替换为自身向前跳一格
        if (isFirstRoot)
        {
            //base.ExecuteDamage();
            Debug.Log("base.ExecuteDamage()");
            if (IsHasTarget())
                TakeDamage(GetCurrentTarget());
        }
        else
        {
            // 添加一个弹起任务，跳一格
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
