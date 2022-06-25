using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : BaseUnit
{
    // 若依附于格子，则当前对象应当持有格子的引用，否则无
    public BaseGrid mGrid;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public override void Awake()
    {
        base.Awake();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
        animator = transform.Find("SpriteGo").GetComponent<Animator>();
    }

    public override void MInit()
    {
        base.MInit();
        mGrid = null;

    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Item;
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
        if(mGrid!=null)
            RemoveFromGrid();
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
    /// 设置图像显示与判定中心偏移量
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteRenderer.transform.localPosition = vector2;
    }
}
