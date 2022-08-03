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
        IceSlowDownAreaEffectExecution iceSlowEffect = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/IceSlowDownAreaEffect").GetComponent<IceSlowDownAreaEffectExecution>();
        iceSlowEffect.Init(this.mMasterBaseUnit, 120, GetRowIndex(), 3, 3, -0.5f, 0, false, true); // 第二个参数为持续时间（帧）
        iceSlowEffect.SetAffectHeight(0); // 只有地面单位被影响
        // 再产生一个范围伤害效果
        DamageAreaEffectExecution dmgEffect = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect").GetComponent<DamageAreaEffectExecution>();
        dmgEffect.Init(this.mMasterBaseUnit, CombatAction.ActionType.CauseDamage, 0.25f*GetDamage(), GetRowIndex(), 3, 3, -0.5f, 0, false, true);
        dmgEffect.SetAffectHeight(0);
        if (baseUnit != null && baseUnit.IsValid())
        {
            // 如果单位存在，则在单位位置爆炸，并且单位受到全额伤害
            new DamageAction(CombatAction.ActionType.CauseDamage, this.mMasterBaseUnit, baseUnit, 0.75f * GetDamage()).ApplyAction();
            iceSlowEffect.transform.position = baseUnit.transform.position;
            dmgEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // 否则位于格子正中心爆炸
            iceSlowEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
            dmgEffect.transform.position = iceSlowEffect.transform.position;
        }
        GameController.Instance.AddAreaEffectExecution(iceSlowEffect);
        GameController.Instance.AddAreaEffectExecution(dmgEffect);
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
