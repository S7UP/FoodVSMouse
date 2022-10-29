using UnityEngine;
/// <summary>
/// 冰鸡蛋
/// </summary>
public class IceEggBullet : ParabolaBullet
{
    public override void MInit()
    {
        base.MInit();
        SetCanAttackFood(false);
        SetCanAttackMouse(true);
    }

    /// <summary>
    /// 对周围造成AOE冰冻效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个减速效果
        IceSlowDownAreaEffectExecution iceSlowEffect = IceSlowDownAreaEffectExecution.GetInstance();
        iceSlowEffect.Init(this.mMasterBaseUnit, 120, GetRowIndex(), 3, 3, -0.5f, 0, false, true); // 第二个参数为持续时间（帧）
        iceSlowEffect.SetAffectHeight(0); // 只有地面单位被影响
        if (baseUnit != null && baseUnit.IsValid())
        {
            // 如果单位存在，则在单位位置爆炸，并且单位受到1.5%最大生命值与攻击力中较大者伤害，对BOSS恒为攻击力
            float dmg = Mathf.Max(0.015f*baseUnit.mMaxHp, GetDamage());
            if (baseUnit is MouseUnit)
            {
                MouseUnit m = baseUnit as MouseUnit;
                if (m.IsBoss())
                {
                    dmg = GetDamage();
                }
            }
            new DamageAction(CombatAction.ActionType.CauseDamage, this.mMasterBaseUnit, baseUnit, dmg).ApplyAction();
            iceSlowEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // 否则位于格子正中心爆炸
            iceSlowEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(iceSlowEffect);
        //GameController.Instance.AddAreaEffectExecution(dmgEffect);
        ExecuteHitAction(baseUnit);
        KillThis();
    }

    //public override void OnTriggerEnter2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerStay2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerExit2D(Collider2D collision)
    //{

    //}
}
