using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 基础障碍
/// </summary>
public class BaseBarrier : BaseUnit
{
    private BaseGrid mGrid;
    public SpriteRenderer spriteRenderer;
    public Transform spriteTrans;

    public override void Awake()
    {
        base.Awake();
        spriteRenderer = transform.Find("Ani_Barrier").GetComponent<SpriteRenderer>();
        spriteTrans = transform.Find("Ani_Barrier");
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
         if (collision.tag.Equals("Bullet"))
        {
            // 检测到子弹单位碰撞了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (bullet.GetRowIndex()==this.GetRowIndex() && UnitManager.CanBulletHit(this, bullet)) // 检测双方能否互相击中
            {
                bullet.TakeDamage(this);
            }
        }
    }

    public override bool CanHit(BaseBullet bullet)
    {
        return true;
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
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    // 死亡后，将自身信息从对应格子移除
    public override void AfterDeath()
    {
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
    /// 设置贴图对象坐标
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
    }
}
