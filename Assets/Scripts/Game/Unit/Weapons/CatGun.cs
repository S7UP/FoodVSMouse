
using UnityEngine;
/// <summary>
/// èèǹ
/// </summary>
public class CatGun : BaseWeapons
{
    public override void MInit()
    {
        base.MInit();
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(master, GetRowIndex());
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
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
