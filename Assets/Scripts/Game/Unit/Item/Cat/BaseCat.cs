using UnityEngine;

/// <summary>
/// 猫猫
/// </summary>
public class BaseCat : BaseItem
{
    private int rowIndex; // 固定的行下标
    private bool isTrigger; // 是否触发（惊醒）

    public override void MInit()
    {
        base.MInit();
        rowIndex = 0;
        isTrigger = false;
        mBoxCollider2D.size = new Vector2(MapManager.gridWidth/2, MapManager.gridHeight/2);
        moveRotate = Vector2.right; // 默认方向为向右
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(12.0f)); // 默认移速为2格/s
    }

    /// <summary>
    /// 设置行下标的同时更新y坐标同步
    /// </summary>
    public void SetRowIndex(int rowIndex)
    {
        this.rowIndex = rowIndex;
        transform.position = new Vector3(transform.position.x, MapManager.GetRowY(rowIndex), transform.position.z);
    }

    /// <summary>
    /// 你惊动了猫猫！（仅第一次调用有效）
    /// </summary>
    public void OnTriggerEvent()
    {
        if (!isTrigger)
        {
            isTrigger = true;
            SetActionState(new TransitionState(this));
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Trigger");
    }

    public override void OnTransitionState()
    {
        if (currentStateTimer == 0)
            return;
        if(AnimatorManager.GetNormalizedTime(animator)>1.0)
            SetActionState(new MoveState(this));
    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move");
    }

    public override void OnMoveState()
    {
        SetPosition((Vector2)GetPosition() + moveRotate * GetMoveSpeed());
        // 到最右侧消失
        if (GetColumnIndex() >= MapController.xColumn)
        {
            ExecuteDeath();
        }
    }


    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() == GetRowIndex())
            {
                OnTriggerEvent();

                if (m.IsBoss())
                    // 对BOSS造成900点伤害
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, m, 900).ApplyAction();
                else
                    // 把老鼠创死
                    m.ExecuteDeath();
            }
        }
    }

    /// <summary>
    /// 判定不随y坐标改变，定死在一行
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return rowIndex;
    }
}
