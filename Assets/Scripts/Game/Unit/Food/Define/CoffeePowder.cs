/// <summary>
/// 咖啡粉
/// </summary>
public class CoffeePowder : FoodUnit
{
    private static FloatModifier attackSpeedModifier = new FloatModifier(50);
    private static FloatModifier attackModifier = new FloatModifier(100);
    private static BoolModifier boolModifier = new BoolModifier(true);

    public override void MInit()
    {
        base.MInit();
        // 获取100%减伤，以及免疫灰烬秒杀效果
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // 不可选取
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder();
        if (a != null)
        {
            return a.IsFinishOnce();
        }
        return false;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 对当前格卡片施加效果
        ExecuteDamage();
        // 直接销毁自身
        ExecuteDeath();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 使当前格拥有最高受击优先级的卡片CD重置
        BaseGrid g = GetGrid();
        if (g != null)
        {
            foreach (var unit in g.GetAttackableFoodUnitList())
            {
                // 移除这些卡的负面控制效果
                StatusManager.RemoveAllSettleDownDebuff(unit);
                // 增加攻击攻速效果
                int timeLeft = 10 * 60;
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate {
                    unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
                    unit.NumericBox.Attack.AddPctAddModifier(attackModifier);

                    unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
                    unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
                };
                t.AddTaskFunc(delegate {
                    if (timeLeft > 0)
                        timeLeft--;
                    else
                        return true;
                    return false;
                });
                t.OnExitFunc = delegate {
                    unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
                    unit.NumericBox.Attack.RemovePctAddModifier(attackModifier);

                    unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
                    unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
                };
                unit.AddTask(t);
            }
        }
    }
}
