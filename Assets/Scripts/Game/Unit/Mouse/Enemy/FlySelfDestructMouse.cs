using UnityEngine;
using S7P.Numeric;
public class FlySelfDestructMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // 免疫炸弹秒杀效果
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // 免疫冻结
    private Vector2 start_position; // 起始坠机点
    private Vector2 target_position; // 目标落点
    private Vector3 last_pos;

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // 降落列默认为3，即左四列
        base.MInit();
        mHeight = 1;
        // 初始免疫炸弹秒杀效果
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // 在受到伤害结算之后，直接判定为击坠状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExecuteDestruct(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { ExecuteDestruct(); });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExecuteDestruct();
        }
    }

    /// <summary>
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void ExecuteDestruct()
    {
        if (!isDrop)
        {
            // 设置为不可选取，以及不可击中
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // 标记已坠机，此后该实例一些行为会发生变化
            isDrop = true;
            // 移除免疫炸弹秒杀效果
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
            // 添加冻结免疫效果
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
            // 同时移除身上所有定身类控制效果
            StatusManager.RemoveAllSettleDownDebuff(this);
            // 确定起始点和最终点，其中最终点为起始点+1.5格*当前标准移动速度值，但最小不能为左一列
            start_position = transform.position;
            target_position = transform.position + (Vector3)moveRotate * 1.5f * MapManager.gridWidth * TransManager.TranToStandardVelocity(NumericBox.MoveSpeed.Value);
            if(target_position.x < MapManager.GetColumnX(0))
            {
                target_position = new Vector2(MapManager.GetColumnX(0), target_position.y);
            }
            last_pos = start_position;
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
        if (isDrop && !isDeathState)
        {
            float t = animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime();
            Vector3 pos = Vector3.Lerp(start_position, target_position, t);
            SetPosition(GetPosition()+(pos - last_pos));
            last_pos = pos;
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
            BaseUnit unit = grid.GetHighestAttackPriorityUnit(this);
            if (unit != null && unit.tag!="Character")
            {
                new BombDamageAction(CombatAction.ActionType.CauseDamage, this, unit, unit.mCurrentHp).ApplyAction();
            }
        }
    }

    /// <summary>
    /// 是否能被作为目标选中
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        // 如果在坠机状态则不可被选为目标
        if (isDrop)
            return false;
        else
            return base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// 是否能被子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        // 如果在坠机状态下则不可被子弹击中
        if (isDrop)
            return false;
        else
            return base.CanHit(bullet);
    }
}
