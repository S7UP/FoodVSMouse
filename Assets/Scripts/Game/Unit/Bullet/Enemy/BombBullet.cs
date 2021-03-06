using UnityEngine;

/// <summary>
/// 纵火炸弹、抛物线移动式的子弹
/// </summary>
public class BombBullet : ParabolaBullet
{
    /// <summary>
    /// 对周围造成AOE爆破效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个爆炸效果
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 3, 3, 0, 0, true, false);
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
