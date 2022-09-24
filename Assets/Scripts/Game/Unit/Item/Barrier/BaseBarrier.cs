using UnityEngine;
/// <summary>
/// �����ϰ�
/// </summary>
public class BaseBarrier : BaseItem
{
    private bool canRemove;

    public override void MInit()
    {
        canRemove = false; // Ĭ��Ϊ�����Ƴ�
        SetHide(false); // Ĭ������ʾ��
        base.MInit();
    }

    /// <summary>
    /// �����Ƿ���Ա�ǿ���Ƴ�
    /// </summary>
    public void SetRemoveAble(bool enable)
    {
        canRemove = enable;
    }

    /// <summary>
    /// �ɷ�ǿ���Ƴ�
    /// </summary>
    /// <returns></returns>
    public override bool CanRemove()
    {
        return canRemove;
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
        return GetHeight()==bullet.mHeight;
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

    /// <summary>
    /// ���ش˵���
    /// </summary>
    public void SetHide(bool isHide)
    {
        if (isHide)
            spriteRenderer.enabled = false;
        else
            spriteRenderer.enabled = true;
    }
}
