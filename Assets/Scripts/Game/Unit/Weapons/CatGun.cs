using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 猫猫枪
/// </summary>
public class CatGun : BaseWeapons
{
    public override void MInit()
    {
        base.MInit();
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        List<BaseUnit>[] list = GameController.Instance.GetEnemyList();
        foreach (var m in list[GetRowIndex()])
        {
            if (m.CanBeSelectedAsTarget())
                return true;
        }
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        BaseBullet b = GameController.Instance.CreateBullet(this.master, transform.position, Vector2.right, BulletStyle.CatBullet);
        b.SetDamage(0);
        b.SetStandardVelocity(24.0f);
    }
}
