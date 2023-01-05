/// <summary>
/// 幽灵鼠
/// </summary>
public class GhostMouse : MouseUnit
{
    private static BoolModifier IgnoreModifier = new BoolModifier(true);
    private RetangleAreaEffectExecution r;

    public override void MInit()
    {
        base.MInit();
        // 添加只有距离一格才能被视为目标
        AddCanBeSelectedAsTargetFunc(BeSelectedAsTargetCondition);
        // 添加只有子弹发射者距离自己一格才能被击中
        AddCanHitFunc(CanHitFunc);
        // 免疫减速、晕眩、冰冻效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
        // 完全免疫水
        WaterGridType.AddNoAffectByWater(this, IgnoreModifier);
        // 完全免疫天空
        SkyGridType.AddNoAffectBySky(this, IgnoreModifier);
        // 在生成时附带一个范围效果
        CreateArea();
    }

    public override void MUpdate()
    {
        base.MUpdate();
        r.transform.position = transform.position; // 跟随
    }

    private bool BeSelectedAsTargetCondition(BaseUnit u1, BaseUnit u2)
    {
        if (u2 == null || !u2.IsAlive())
            return false;
        // 不能被道具视为目标
        if (u2 is BaseItem)
            return false;
        return (u2.transform.position - transform.position).magnitude < 0.5f*MapManager.gridWidth;
    }

    private bool CanHitFunc(BaseUnit self, BaseBullet b)
    {
        BaseUnit master = b.mMasterBaseUnit;
        return master != null && master.IsAlive() && (master.transform.position - transform.position).magnitude < 0.5f*MapManager.gridWidth;
    }

    private bool ToFoodNoBlockFunc(BaseUnit u1, BaseUnit u2)
    {
        return false;
    }

    /// <summary>
    /// 产生一个范围效果，这个范围效果内的美食会失去阻挡能力且不会被选取为攻击目标
    /// </summary>
    private void CreateArea()
    {
        r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "ItemCollideAlly");
        r.isAffectFood = true;
        r.SetOnFoodEnterAction((unit)=> {
            unit.AddCanBlockFunc(ToFoodNoBlockFunc);
            // unit.AddCanBeSelectedAsTargetFunc(ToFoodNoSelectedAsTarget);
        });
        r.SetOnFoodExitAction((unit) => {
            unit.RemoveCanBlockFunc(ToFoodNoBlockFunc);
            // unit.RemoveCanBeSelectedAsTargetFunc(ToFoodNoSelectedAsTarget);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        r.MDestory();
    }

    /// <summary>
    /// 不攻击
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }
}
