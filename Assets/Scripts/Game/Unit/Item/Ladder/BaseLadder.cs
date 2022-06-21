using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static System.Collections.Specialized.BitVector32;
/// <summary>
/// 基础梯子&&弹簧类道具
/// </summary>
public class BaseLadder : BaseUnit
{
    private Transform spriteTrans; 
    public SpriteRenderer spriteRenderer;

    private BaseGrid mGrid;
    private float moveDistance; // 传送距离
    private Action<GridAction> DefaultAfterRemoveFoodUnitActionPoint;

    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("Ani_Ladder");
        spriteRenderer = spriteTrans.GetComponent<SpriteRenderer>();
        DefaultAfterRemoveFoodUnitActionPoint = (GridAction gridAction) => OnMasterGridAfterRemoveFood(gridAction);
    }

    public override void MInit()
    {
        base.MInit();
        moveDistance = 0;
        mGrid = null;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Item;
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
            if (m.GetRowIndex() != GetRowIndex())
                return;
            // 添加一个弹起的动任务
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 24.0f, 1.2f, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false));
            m.CloseCollision();
            t.AddOtherEndEvent(delegate { m.OpenCollision(); });
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
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    // 死亡后，将自身信息从对应格子移除，同时移除事件
    public override void AfterDeath()
    {
        RemoveFromGrid();
        if (mGrid != null)
            mGrid.gridActionPointManager.RemoveListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// 设置在同种类敌人的渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), -1, arrayIndex);
    }

    /// <summary>
    /// 设置移动距离
    /// </summary>
    /// <param name="d"></param>
    public void SetMoveDistance(float d)
    {
        moveDistance = d;
    }

    /// <summary>
    /// 设置图像显示与判定中心偏移量
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
    }

    /// <summary>
    /// 当加入格子后，自动为格子添加的监听事件
    /// </summary>
    /// <param name="action"></param>
    public void OnMasterGridAfterRemoveFood(GridAction action)
    {
        // 每当格子美食被移除时，检测格子其余美食中有没有防御型的，如果没有则自毁
        List<FoodUnit> fL = action.grid.GetFoodUnitList();
        bool flag = true;
        foreach (var item in fL)
        {
            FoodInGridType t = BaseCardBuilder.GetFoodInGridType(item.mType);
            if(t.Equals(FoodInGridType.Defence) || t.Equals(FoodInGridType.Shield))
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
