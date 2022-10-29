using UnityEngine;
using System;
using static UnityEngine.UI.CanvasScaler;
/// <summary>
/// 会主动检测友方单位的敌方弹幕
/// </summary>
public class EnemyBullet : BaseBullet
{
    public bool isAffectCharacter;
    public bool isAffectFood;
    public Func<BaseUnit, BaseUnit> GetTargetFunc;

    public override void MInit()
    {
        GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetHighestAttackPriorityUnit();
            }
            return unit;
        };
        isAffectFood = true;
        isAffectCharacter = true;
        spriteRenderer.sprite = null;
        animator.runtimeAnimatorController = null;
        base.MInit();
        
    }

    /// <summary>
    /// 根据传入目标格子获取最终击中的目标
    /// </summary>
    /// <returns></returns>
    private BaseUnit GetTarget(BaseUnit unit)
    {
        return GetTargetFunc(unit);
    }

    public void OnCollsion(Collider2D collision)
    {
        if (isAffectFood && collision.tag.Equals("Food"))
        {
            FoodUnit u = collision.GetComponent<FoodUnit>();
            BaseUnit targetUnit = GetTarget(u);
            if (targetUnit!=null && UnitManager.CanBulletHit(targetUnit, this))
            {
                TakeDamage(targetUnit);
            }
        }else if(isAffectCharacter && collision.tag.Equals("Character"))
        {
            CharacterUnit u = collision.GetComponent<CharacterUnit>();
            BaseUnit targetUnit = GetTarget(u);
            if (targetUnit != null && UnitManager.CanBulletHit(targetUnit, this))
            {
                TakeDamage(targetUnit);
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

    public static EnemyBullet GetInstance(RuntimeAnimatorController runtimeAnimatorController, BaseUnit master, float dmg)
    {
        EnemyBullet e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/EnemyBullet").GetComponent<EnemyBullet>();
        e.MInit();
        e.SetCollisionLayer("ItemCollideAlly");
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
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/EnemyBullet", gameObject);
    }
}
