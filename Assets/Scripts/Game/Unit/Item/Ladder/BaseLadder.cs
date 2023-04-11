using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 基础梯子 && 弹簧类道具
/// </summary>
public class BaseLadder : BaseItem
{
    private List<BaseUnit> unitList = new List<BaseUnit>();
    private float moveDistance; // 传送距离
    private float maxHight;
    private Action<GridAction> DefaultAfterRemoveFoodUnitActionPoint;

    public override void Awake()
    {
        base.Awake();
        DefaultAfterRemoveFoodUnitActionPoint = (GridAction gridAction) => OnMasterGridAfterRemoveFood(gridAction);
    }

    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        moveDistance = 0;
        maxHight = 0;
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            // 把老鼠送走
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (unitList.Contains(m) || m.IsBoss() || !UnitManager.CanBeSelectedAsTarget(this, m))
                return;
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight() || !m.moveRotate.Equals(Vector2.left))
                return;
            // 另一个判定条件，就是目标不滞空
            if (UnitManager.IsFlying(m))
                return;

            // 把这个单位添加到表里
            unitList.Add(m);
            // 添加一个弹起的任务
            CustomizationTask t = TaskManager.AddParabolaTask(m, TransManager.TranToVelocity(12), maxHight, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false);
            // 且禁止移动
            m.DisableMove(true);
            t.AddOnExitAction(delegate {
                m.DisableMove(false);
            });
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            // 把老鼠送走
            MouseUnit m = collision.GetComponent<MouseUnit>();
            unitList.Remove(m);
        }
    }

    public override BaseGrid GetGrid()
    {
        return mGrid;
    }

    /// <summary>
    /// 把自己加入格子
    /// </summary>
    /// <param name="grid"></param>
    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
        grid.gridActionPointManager.AddListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public override void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
        mGrid.gridActionPointManager.RemoveListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// 设置移动距离
    /// </summary>
    /// <param name="d"></param>
    public void SetMoveDistanceAndMaxHight(float d, float h)
    {
        moveDistance = d;
        maxHight = h;
    }

    /// <summary>
    /// 当加入格子后，自动为格子添加的监听事件
    /// </summary>
    /// <param name="action"></param>
    public void OnMasterGridAfterRemoveFood(GridAction action)
    {
        // 每当格子美食被移除时，检测格子其余美食中有没有防御型的，如果没有则自毁
        List<FoodUnit> list = action.grid.GetFoodUnitList();
        bool flag = true;
        foreach (var unit in list)
        {
            FoodNameTypeMap f_type = (FoodNameTypeMap)unit.mType;
            if (FoodManager.DenfenceCard.Contains(f_type))
            {
                flag = false;
                break;
            }
        }
        if (flag)
        {
            ExecuteDeath();
        }
    }
}
