using UnityEngine;
/// <summary>
/// 基础障碍
/// </summary>
public class BaseBarrier : BaseItem
{

    public override void Awake()
    {
        base.Awake();
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
}
