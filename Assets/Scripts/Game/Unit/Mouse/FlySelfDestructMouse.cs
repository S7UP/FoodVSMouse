using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class FlySelfDestructMouse : MouseUnit
{
    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true); // 免疫炸弹秒杀效果
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // 免疫冻结
    private Vector2 start_position; // 起始坠机点
    private Vector2 target_position; // 目标落点

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // 降落列默认为3，即左四列
        base.MInit();
        // 初始免疫炸弹秒杀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // 在受到伤害结算之后，直接判定为击坠状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExcuteDrop(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { ExcuteDrop(); });
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
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (GetColumnIndex() <= dropColumn && !isDrop);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    private void ExcuteDrop()
    {
        if (!isDrop)
        {
            // 标记已坠机，此后该实例一些行为会发生变化
            isDrop = true; 
            // 移除免疫炸弹秒杀效果
            if (IgnoreBombInstantKill != null)
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
            // 添加冻结免疫效果
            if(IgnoreFrozen != null)
            {
                NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
                // 同时移除身上所有定身类控制效果
                StatusManager.RemoveAllSettleDownDebuff(this);
            }
            // 确定起始点和最终点，其中最终点为起始点+1.5格*当前标准移动速度值，但最小不能为左一列
            start_position = transform.position;
            target_position = transform.position + (Vector3)moveRotate * 1.5f * MapManager.gridWidth * TransManager.TranToStandardVelocity(NumericBox.MoveSpeed.Value);
            if(target_position.x < MapManager.GetColumnX(0))
            {
                target_position = new Vector2(MapManager.GetColumnX(0), target_position.y);
            }
            // 设为转场状态
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isDrop)
        {
            animatorController.Play("Move1");
        }
        else
        {
            animatorController.Play("Move0", true);
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
            // 这种情况下表明已坠落，执行死亡
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
    /// 正常死亡时带走一个卡片
    /// </summary>
    public override void BeforeDeath()
    {
        base.BeforeDeath();
        BaseGrid grid = GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex());
        if (grid != null)
        {
            BaseUnit unit = grid.GetHighestAttackPriorityUnit();
            if (unit != null && unit.tag!="Character")
            {
                new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, unit, float.MaxValue).ApplyAction();
            }
        }
    }
}
