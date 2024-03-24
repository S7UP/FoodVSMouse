
using UnityEngine;
using S7P.Numeric;
using Environment;
/// <summary>
/// 袋鼠类
/// </summary>
public class KangarooMouse : MouseUnit
{
    private BoolModifier IgnoreStunModifier = new BoolModifier(true); // 免疫晕眩修饰器
    private bool isFirstRoot; // 是否有吃过定身效果  也可以作为是否定为跳跃状态的标准  false = 跳跃状态  true = 非跳跃状态
    private bool isJumping; // 正在跳跃过程中
    private float jumpDistance; // 跳跃距离

    public override void MInit()
    {
        isFirstRoot = false;
        isJumping = false;
        base.MInit();
        // 第一次受到定身效果前免疫定身效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
        // 添加监听
        // 在第一次定身效果结束后（由于免疫的原理是施加定身效果后立即结束），移除免疫定身效果
        statusAbilityManager.AddAfterRemoveStatusAbilityEvent(StringManager.Stun,
            delegate {
                if (isFirstRoot)
                    return;
                isFirstRoot = true;
                // NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
                // 清空所有冰冻损伤效果
                IceTask t = Environment.EnvironmentFacade.GetIceDebuff(this) as IceTask;
                if(t != null)
                {
                    t.AddValue(-t.GetValue());
                }
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
                animatorController.Play("Drop");
            });
        if (mShape == 0 || mShape == 3)
            jumpDistance = 1;
        else if (mShape == 1 || mShape == 4)
            jumpDistance = 1.5f;
        else
            jumpDistance = 2;
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
            if (IsHasTarget())
                TakeDamage(GetCurrentTarget());
        }
        else
        {
            // 添加一个弹起任务，跳一格
            isJumping = true;
            //Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * jumpDistance * MapManager.gridWidth, false));
            //DisableMove(true);
            //t.AddOtherEndEvent(delegate { isJumping = false; DisableMove(false); });

            float dist = jumpDistance * MapManager.gridWidth;
            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false, true);
            t.AddOnEnterAction(delegate {
                DisableMove(true);
            });
            t.AddOnExitAction(delegate
            {
                isJumping = false; 
                DisableMove(false);
            });
            AddTask(t);
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Attack");
        else
            animatorController.Play("Jump");
    }

    public override void OnMoveStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnDieStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    public override void OnIdleStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Idle", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }
}
