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
        if (u1 == null || u2 == null)
            return false;

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
        if (u == null || b == null)
            return false;

        // ֻҪ��һ�������������ͷ���false
        foreach (var func in u.CanHitFuncList)
        {
            if (!func(u, b))
                return false;
        }
        foreach (var func in b.CanHitFuncList)
        {
            if (!func(b, u))
                return false;
        }

        return u.CanHit(b) && b.CanHit(u);
    }


    /// <summary>
    /// u1�ܷ��u2��Ϊ����Ŀ��ѡ��
    /// </summary>
    /// <param name="u1">����Ϊ�գ���Ϊ����������ж��ܷ���Ϊ����Ŀ��</param>
    /// <param name="u2">����Ϊ�գ���Ϊ��һ������false</param>
    /// <returns></returns>
    public static bool CanBeSelectedAsTarget(BaseUnit u1, BaseUnit u2)
    {
        if (u2 == null)
            return false;

        foreach (var func in u2.CanBeSelectedAsTargetFuncList)
        {
            if (!func(u2, u1))
                return false;
        }
        return u2.CanBeSelectedAsTarget(u1);
    }
}
