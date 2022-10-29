using System;
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
        foreach (var func in u1.CanBlockFuncList)
        {
            if (!func(u1, u2))
                return false;
        }
        foreach (var func in u2.CanBlockFuncList)
        {
            if (!func(u2, u1))
                return false;
        }
        return u1.CanBlock(u2) && u2.CanBlock(u1);
    }

    /// <summary>
    /// 检测单位与子弹能否发生被弹判定
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        // 只要有一个不满足条件就返回false
        foreach (var func in u.CanHitFuncList)
        {
            if (!func(u, b))
                return false;
        }

        return u.CanHit(b) && b.CanHit(u);
    }


    // 禁止被弹幕攻击方法
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate
    {
        return false;
    };
    /// <summary>
    /// 为单位添加不会被常规子弹命中的效果（请和下面的方法配套使用）
    /// </summary>
    /// <param name="u"></param>
    public static void AddNoHitFunc(BaseUnit u)
    {
        u.AddCanHitFunc(noHitFunc);
    }
    /// <summary>
    /// 为单位移除不被常规子弹命中的效果（请和上面的方法配套使用）
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveNoHitFunc(BaseUnit u)
    {
        u.RemoveCanHitFunc(noHitFunc);
    }

    // 禁止阻挡方法
    private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate
    {
        return false;
    };
    /// <summary>
    /// 为单位添加不会被阻挡的效果（请和下面的方法配套使用）
    /// </summary>
    /// <param name="u"></param>
    public static void AddNoBlockFunc(BaseUnit u)
    {
        u.AddCanBlockFunc(noBlockFunc);
    }
    /// <summary>
    /// 为单位移除不会被阻挡的效果（请和上面的方法配套使用）
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveNoBlockFunc(BaseUnit u)
    {
        u.RemoveCanBlockFunc(noBlockFunc);
    }
}
