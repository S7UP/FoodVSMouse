using UnityEngine;
/// <summary>
/// 基础障碍
/// </summary>
public class BaseBarrier : BaseItem
{
    private bool canRemove;

    public override void MInit()
    {
        canRemove = false; // 默认为不可移除
        SetHide(false); // 默认是显示的
        base.MInit();
    }

    /// <summary>
    /// 设置是否可以被强制移除
    /// </summary>
    public void SetRemoveAble(bool enable)
    {
        canRemove = enable;
    }

    /// <summary>
    /// 可否被强制移除
    /// </summary>
    /// <returns></returns>
    public override bool CanRemove()
    {
        return canRemove;
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
        return GetHeight()==bullet.mHeight;
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

    /// <summary>
    /// 隐藏此道具
    /// </summary>
    public void SetHide(bool isHide)
    {
        if (isHide)
            spriteRenderer.enabled = false;
        else
            spriteRenderer.enabled = true;
    }
}
