using UnityEngine;
/// <summary>
/// �����ϰ�
/// </summary>
public class BaseBarrier : BaseItem
{

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }
         if (collision.tag.Equals("Bullet"))
        {
            // ��⵽�ӵ���λ��ײ��
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (bullet.GetRowIndex()==this.GetRowIndex() && UnitManager.CanBulletHit(this, bullet)) // ���˫���ܷ������
            {
                bullet.TakeDamage(this);
            }
        }
    }

    public override bool CanHit(BaseBullet bullet)
    {
        return true;
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }
}
