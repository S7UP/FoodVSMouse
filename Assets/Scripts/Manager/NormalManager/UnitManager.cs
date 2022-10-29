using System;
/// <summary>
/// ������Ϸ��λ�Ĺ�����
/// </summary>
public class UnitManager
{
    /// <summary>
    /// ���˫����λ�ܷ񱻻����赲
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
    /// ��ⵥλ���ӵ��ܷ��������ж�
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        // ֻҪ��һ�������������ͷ���false
        foreach (var func in u.CanHitFuncList)
        {
            if (!func(u, b))
                return false;
        }

        return u.CanHit(b) && b.CanHit(u);
    }


    // ��ֹ����Ļ��������
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate
    {
        return false;
    };
    /// <summary>
    /// Ϊ��λ��Ӳ��ᱻ�����ӵ����е�Ч�����������ķ�������ʹ�ã�
    /// </summary>
    /// <param name="u"></param>
    public static void AddNoHitFunc(BaseUnit u)
    {
        u.AddCanHitFunc(noHitFunc);
    }
    /// <summary>
    /// Ϊ��λ�Ƴ����������ӵ����е�Ч�����������ķ�������ʹ�ã�
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveNoHitFunc(BaseUnit u)
    {
        u.RemoveCanHitFunc(noHitFunc);
    }

    // ��ֹ�赲����
    private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate
    {
        return false;
    };
    /// <summary>
    /// Ϊ��λ��Ӳ��ᱻ�赲��Ч�����������ķ�������ʹ�ã�
    /// </summary>
    /// <param name="u"></param>
    public static void AddNoBlockFunc(BaseUnit u)
    {
        u.AddCanBlockFunc(noBlockFunc);
    }
    /// <summary>
    /// Ϊ��λ�Ƴ����ᱻ�赲��Ч�����������ķ�������ʹ�ã�
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveNoBlockFunc(BaseUnit u)
    {
        u.RemoveCanBlockFunc(noBlockFunc);
    }
}
