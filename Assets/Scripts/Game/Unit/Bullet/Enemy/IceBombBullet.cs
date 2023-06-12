using Environment;
using System;
using UnityEngine;

/// <summary>
/// 抛物线移动式的子弹
/// </summary>
public class IceBombBullet : ParabolaBullet
{
    /// <summary>
    /// 对周围造成AOE冰冻效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个爆炸效果
        Vector2 pos = Vector2.zero;
        if (baseUnit != null && baseUnit.IsValid())
        {
            // 如果单位存在，则在单位位置爆炸
            pos = baseUnit.transform.position;
        }
        else
        {
            // 否则位于格子正中心爆炸
            pos = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        Action<BaseUnit> action = (u) => {
            EnvironmentFacade.AddIceDebuff(u, 100);
        };
        r.SetOnEnemyEnterAction(action);
        r.SetOnFoodEnterAction(action);
        r.SetOnCharacterEnterAction(action);
        GameController.Instance.AddAreaEffectExecution(r);

        ExecuteHitAction(baseUnit);
        KillThis();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {

    }
}
