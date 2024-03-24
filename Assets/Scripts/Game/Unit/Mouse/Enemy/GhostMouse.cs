using S7P.Numeric;

using System;
/// <summary>
/// ������
/// </summary>
public class GhostMouse : MouseUnit
{
    private static BoolModifier IgnoreModifier = new BoolModifier(true);

    public override void MInit()
    {
        base.MInit();
        typeAndShapeValue = 10;
        // ���ֻ�о���һ����ܱ���ΪĿ��
        AddCanBeSelectedAsTargetFunc(BeSelectedAsTargetCondition);
        // ���ֻ���ӵ������߾����Լ�һ����ܱ�����
        AddCanHitFunc(CanHitFunc);
        // ���߼��١���ѣ������Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
        // ��ȫ����ˮ
        WaterGridType.AddNoAffectByWater(this, IgnoreModifier);
        // ��ȫ���߿�
        Environment.SkyManager.AddNoAffectBySky(this, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.NoBearInSky, IgnoreModifier);
        // ������ʱ����һ����ΧЧ��
        CreateArea();
    }

    public override void MUpdate()
    {
        base.MUpdate();
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
    /// ����һ����ΧЧ���������ΧЧ���ڵ���ʳ��ʧȥ�赲����
    /// </summary>
    private void CreateArea()
    {
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "ItemCollideAlly");
        r.transform.position = transform.position;
        r.isAffectFood = true;
        r.SetOnFoodEnterAction((unit)=> {
            unit.AddCanBlockFunc(noBlockFunc);
        });
        r.SetOnFoodExitAction((unit) => {
            unit.RemoveCanBlockFunc(noBlockFunc);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        // ����
        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
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
