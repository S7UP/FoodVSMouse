
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
        foreach (var m in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if (UnitManager.CanBeSelectedAsTarget(master, m))
                return true;
        }
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        BaseBullet b = GameController.Instance.CreateBullet(this.master, transform.position, Vector2.right, BulletStyle.CatBullet);
        b.SetDamage(0);
        b.SetStandardVelocity(24.0f);
    }
}
