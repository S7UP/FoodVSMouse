using UnityEngine;
/// <summary>
/// 会主动检测友方单位的敌方弹幕
/// </summary>
public class EnemyBullet : BaseBullet
{
    public Rigidbody2D r2D;
    public bool isIgnoreFood;
    public bool isIgnoreCharacter;

    public override void Awake()
    {
        base.Awake();
        r2D = GetComponent<Rigidbody2D>();
    }

    public void OnCollsion(Collider2D collision)
    {
        if (!isIgnoreFood && collision.tag.Equals("Food"))
        {
            FoodUnit u = collision.GetComponent<FoodUnit>();
            if (UnitManager.CanBulletHit(u, this))
            {
                TakeDamage(u);
            }
        }else if(!isIgnoreCharacter && collision.tag.Equals("Character"))
        {
            CharacterUnit u = collision.GetComponent<CharacterUnit>();
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
