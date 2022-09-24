using UnityEngine;

/// <summary>
/// 抛物线移动式的子弹
/// </summary>
public class IceBombBullet : ParabolaBullet
{
    /// <summary>
    /// 对周围造成AOE冰冻效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个爆炸效果
        IceAreaEffectExecution iceEffect = IceAreaEffectExecution.GetInstance();
        iceEffect.Init(this.mMasterBaseUnit, 600, GetRowIndex(), 3, 3, -0.5f, 0, true, false); // 第二个参数为持续时间（帧）
        iceEffect.isAffectCharacter = true; // 对人有效
        if (baseUnit != null && baseUnit.IsValid())
        {
            // 如果单位存在，则在单位位置爆炸
            iceEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // 否则位于格子正中心爆炸
            iceEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(iceEffect);
        ExecuteHitAction(baseUnit);
        KillThis();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {

    }
}
