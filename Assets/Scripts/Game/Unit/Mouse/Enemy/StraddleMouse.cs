using UnityEngine;
/// <summary>
/// 滑板鼠
/// </summary>
public class StraddleMouse : MouseUnit
{
    private bool isJumped; // 是否跳过一次
    private float maxSpeed; // 最大速度
    private float a; // 加速度（每帧）

    public override void MInit()
    {
        isJumped = false;
        maxSpeed = TransManager.TranToVelocity(4.0f); // 最大速度为4.0标准移动速度
        a = TransManager.TranToVelocity(3.0f) / 9 / 60; // 加速度为在9秒内增加3.0标准移动速度的加速度
        base.MInit();
    }

    public override bool IsDamageJudgment()
    {
        if (!isJumped)
            return true;
        else
            return base.IsDamageJudgment();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 在未进行第一次跳跃时，普通攻击被替换为跳跃
        if (!isJumped)
        {
            // 添加一个弹起任务
            isJumped = true;
            // 进入不可选取状态
            CloseCollision();
            // 跳跃格子数等于 0.75*当前移动速度标准值
            float dist = 0.75f*TransManager.TranToStandardVelocity(GetMoveSpeed());
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * dist * MapManager.gridWidth, false));
            t.AddOtherEndEvent(delegate { OpenCollision(); NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(1.0f)); });
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isJumped)
            animatorController.Play("Attack");
        else
            animatorController.Play("Jump");
    }

    public override void OnMoveStateEnter()
    {
        if (isJumped)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnMoveState()
    {
        // 在跳跃前加速，加的是基础移动速度
        if (!isJumped)
        {
            NumericBox.MoveSpeed.SetBase(NumericBox.MoveSpeed.baseValue + a);
            if (NumericBox.MoveSpeed.baseValue > maxSpeed)
                NumericBox.MoveSpeed.SetBase(maxSpeed);
        }
        base.OnMoveState();
    }

    public override void OnIdleStateEnter()
    {
        if (isJumped)
            animatorController.Play("Idle", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }
}
