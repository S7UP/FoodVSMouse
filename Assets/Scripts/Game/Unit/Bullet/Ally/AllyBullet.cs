using UnityEngine;
/// <summary>
/// 会主动检测敌方单位的友方弹幕
/// </summary>
public class AllyBullet : BaseBullet
{
    private static RuntimeAnimatorController DefaultAllyBullet_RuntimeAnimatorController;

    public override void Awake()
    {
        if (DefaultAllyBullet_RuntimeAnimatorController == null)
            DefaultAllyBullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/DefaultAllyBullet");
        base.Awake();
    }

    public override void MInit()
    {
        spriteRenderer.sprite = null;
        animator.runtimeAnimatorController = null;
        base.MInit();
        
    }

    public void OnCollsion(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            BaseUnit u;
            bool flag = collision.TryGetComponent(out u);
            if (flag && !unitList.Contains(u) && UnitManager.CanBulletHit(u, this))
            {
                TakeDamage(u);
                unitList.Add(u);
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

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            BaseUnit u;
            bool flag = collision.TryGetComponent(out u);
            if (flag)
            {
                unitList.Remove(u);
            }
        }
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeAnimatorController)
    {
        if (runtimeAnimatorController == null)
            animator.runtimeAnimatorController = DefaultAllyBullet_RuntimeAnimatorController;
        else
            animator.runtimeAnimatorController = runtimeAnimatorController;
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public static AllyBullet GetInstance(BulletStyle style, RuntimeAnimatorController runtimeAnimatorController, BaseUnit master, float dmg)
    {
        AllyBullet e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/AllyBullet").GetComponent<AllyBullet>();
        e.MInit();
        e.style = style;
        e.SetCollisionLayer("AllyBullet");
        e.mMasterBaseUnit = master;
        e.SetDamage(dmg);
        e.SetRuntimeAnimatorController(runtimeAnimatorController);
        if(master!=null)
            e.transform.position = master.transform.position;
        return e;
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/AllyBullet", gameObject);
    }
}
