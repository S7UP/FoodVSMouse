
using UnityEngine;
/// <summary>
/// 鼠国列车部件（作为车头和车身的父类）
/// </summary>
public abstract class RatTrainComponent : MouseModel
{
    private BaseRatTrain master; // 绑定的BOSS对象
    private float dmgRate; // 伤害传导倍率
    private float burnDmgRate; // 灰烬伤害传导倍率
    protected int bodyLayerIndex; // 位于主人的第几节

    public override void MInit()
    {
        bodyLayerIndex = -1;
        master = null;
        dmgRate = 0;
        burnDmgRate = 0;
        base.MInit();
        // 添加等同于BOSS的免疫机制
        BossUnit.AddBossIgnoreDebuffEffect(this);
        canTriggerCat = false;
        canTriggerLoseWhenEnterLoseLine = false;
        SetIsMouseUnit(false);

        isBoss = true; // 车厢也算BOSS判定
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void LoadSkillAbility()
    {

    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnMoveState()
    {

    }

    /// <summary>
    /// 要在范围内才能被选为攻击目标
    /// </summary>
    /// <param name="otherUnit"></param>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    /// <returns></returns>
    public override float GetMoveSpeed()
    {
        if(master!=null)
            return master.GetMoveSpeed();
        return 0;
    }

    public override float OnDamage(float dmg)
    {
        return 0;
    }

    public override float OnAoeDamage(float dmg)
    {
        return 0;
    }

    public override float OnDamgeIgnoreShield(float dmg)
    {
        return 0;
    }

    public override float OnRealDamage(float dmg)
    {
        return 0;
    }

    public override float OnBurnDamage(float dmg)
    {
        return 0;
    }

    public override float OnBombBurnDamage(float dmg)
    {
        return 0;
    }

    public override void AddRecordDamage(float value)
    {

    }

    public override void OnCure(float cure)
    {
        // 然而什么也不会发生，这意味着教皇的回复效果并不能通过车厢传导给BOSS
    }

    public BaseRatTrain GetMaster()
    {
        return master;
    }

    public void SetMaster(BaseRatTrain master)
    {
        this.master = master;
        SetMaxHpAndCurrentHp(master.mMaxHp);
        SetGetCurrentHpFunc(delegate { return master.mCurrentHp; });
        SetGetBurnRateFunc(delegate { return master.mBurnRate; });
        // 伤害传递机制
        AddActionPointListener(ActionPointType.PreReceiveDamage, (combat) =>
        {
            if (combat is DamageAction)
            {
                
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, master);
                    d.DamageValue = dmgAction.DamageValue * burnDmgRate;
                    d.ApplyAction();
                }
                else
                {
                    DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, master);
                    d.DamageValue = dmgAction.DamageValue * dmgRate;
                    d.ApplyAction();
                }
            }
        });
    }

    public float GetDmgRate()
    {
        return dmgRate;
    }

    public void SetDmgRate(float dmgrate, float burnrate)
    {
        dmgRate = dmgrate;
        burnDmgRate = burnrate;
    }

    public override bool IsOutOfBound()
    {
        return false;
    }

    /// <summary>
    /// 在与猫判定时是否能触发猫
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerCat()
    {
        return false;
    }

    /// <summary>
    /// 在越过失败判定线后是否会触发游戏失败判定
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }


    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, -1, 0, bodyLayerIndex);
    }
}
