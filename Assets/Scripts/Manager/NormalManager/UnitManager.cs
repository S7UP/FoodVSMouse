using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 处理游戏单位的管理者
/// </summary>
public class UnitManager
{
    /// <summary>
    /// 检测双方单位能否被互相阻挡
    /// </summary>
    /// <param name="u1"></param>
    /// <param name="u2"></param>
    /// <returns></returns>
    public static bool CanBlock(BaseUnit u1, BaseUnit u2)
    {
        return u1.CanBlock(u2) && u2.CanBlock(u1);
    }

    /// <summary>
    /// 检测单位与子弹能否发生被弹判定
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        return u.CanHit(b) && b.CanHit(u);
    }
}
