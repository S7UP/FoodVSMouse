using UnityEngine;
/// <summary>
/// 会主动检测友方单位的敌方弹幕
/// </summary>
public class EnemyBullet : BaseBullet
{
    public Rigidbody2D r2D;

    public override void Awake()
    {
        base.Awake();
        r2D = GetComponent<Rigidbody2D>();
    }

    public void OnCollsion(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (UnitManager.CanBulletHit(u, this))
            {
                TakeDamage(u);
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollsion(collision);
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        OnCollsion(collision);
    }
}
