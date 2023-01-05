using UnityEngine;
/// <summary>
/// 会主动检测敌方单位的友方弹幕
/// </summary>
public class AllyBullet : BaseBullet
{
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
            MouseUnit u;
            bool flag = collision.TryGetComponent<MouseUnit>(out u);
            if (flag && !unitList.Contains(u) && UnitManager.CanBulletHit(u, this))
            {
                unitList.Add(u);
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

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit u;
            bool flag = collision.TryGetComponent<MouseUnit>(out u);
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
        animator.runtimeAnimatorController = runtimeAnimatorController;
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public static AllyBullet GetInstance(RuntimeAnimatorController runtimeAnimatorController, BaseUnit master, float dmg)
    {
        AllyBullet e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/AllyBullet").GetComponent<AllyBullet>();
        e.MInit();
        e.SetCollisionLayer("AllyBullet");
        e.mMasterBaseUnit = master;
        e.SetDamage(dmg);
        e.SetRuntimeAnimatorController(runtimeAnimatorController);
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
