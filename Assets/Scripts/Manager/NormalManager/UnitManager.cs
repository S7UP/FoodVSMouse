using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return u1.CanBlock(u2) && u2.CanBlock(u1);
    }

    /// <summary>
    /// ��ⵥλ���ӵ��ܷ��������ж�
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        return u.CanHit(b) && b.CanHit(u);
    }
}
