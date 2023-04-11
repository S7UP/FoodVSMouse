
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
        return GameController.Instance.CheckRowCanAttack(master, GetRowIndex());
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        BaseBullet b = GameController.Instance.CreateBullet(this.master, transform.position, Vector2.right, BulletStyle.CatBullet);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.SetDamage(0);
        b.SetStandardVelocity(24.0f);
    }
}
