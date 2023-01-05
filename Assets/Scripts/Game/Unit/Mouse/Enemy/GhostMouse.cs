/// <summary>
/// ������
/// </summary>
public class GhostMouse : MouseUnit
{
    private static BoolModifier IgnoreModifier = new BoolModifier(true);
    private RetangleAreaEffectExecution r;

    public override void MInit()
    {
        base.MInit();
        // ���ֻ�о���һ����ܱ���ΪĿ��
        AddCanBeSelectedAsTargetFunc(BeSelectedAsTargetCondition);
        // ���ֻ���ӵ������߾����Լ�һ����ܱ�����
        AddCanHitFunc(CanHitFunc);
        // ���߼��١���ѣ������Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
        // ��ȫ����ˮ
        WaterGridType.AddNoAffectByWater(this, IgnoreModifier);
        // ��ȫ�������
        SkyGridType.AddNoAffectBySky(this, IgnoreModifier);
        // ������ʱ����һ����ΧЧ��
        CreateArea();
    }

    public override void MUpdate()
    {
        base.MUpdate();
        r.transform.position = transform.position; // ����
    }

    private bool BeSelectedAsTargetCondition(BaseUnit u1, BaseUnit u2)
    {
        if (u2 == null || !u2.IsAlive())
            return false;
        // ���ܱ�������ΪĿ��
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
    /// ����һ����ΧЧ���������ΧЧ���ڵ���ʳ��ʧȥ�赲�����Ҳ��ᱻѡȡΪ����Ŀ��
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
    /// ������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }
}
