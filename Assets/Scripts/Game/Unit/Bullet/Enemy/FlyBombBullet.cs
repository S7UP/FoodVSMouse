using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 直线下落型炸弹
/// </summary>
public class FlyBombBullet : BaseBullet
{
    private float acc;
    private float targetY;
    private int currentRowIndex;
    public override void MInit()
    {
        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (isDeathState)
            return;
        mVelocity += acc;
        // 低于特定高度也会自爆
        if (transform.position.y < targetY)
        {
            TakeDamage(null);
        }
    }

    /// <summary>
    /// 初始化速度
    /// </summary>
    /// <param name="v"></param>
    /// <param name="acc"></param>
    public void InitVelocity(float v, float acc, float targetY, int currentRowIndex)
    {
        SetVelocity(v);
        this.acc = acc;
        this.targetY = targetY;
        this.currentRowIndex = currentRowIndex;
    }

    /// <summary>
    /// 对周围造成AOE爆破效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个爆炸效果
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 1, 1, 0, 0, true, false);
        if (baseUnit != null && baseUnit.IsValid())
        {
            // 如果单位存在，则在单位位置爆炸
            bombEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // 否则位于格子正中心爆炸
            bombEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(bombEffect);

        KillThis();
    }

    /// <summary>
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return currentRowIndex;
    }
}
