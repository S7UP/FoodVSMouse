using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 冰淇淋
/// </summary>
public class IceCream : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // 获取100%减伤，接近无限的生命值，以及免疫灰烬秒杀效果
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // 不可选取
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return false;
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
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// 是否为伤害判定时刻
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        BaseGrid g = GetGrid();
        if (g == null)
            return false;
        bool flag = false;
        foreach (var f in g.GetAttackableFoodUnitList())
        {
            if(f.GetCardBuilder()!=null && !f.GetCardBuilder().IsColdDown())
            {
                flag = true;
                break;
            }
        }
        return (mAttackFlag && flag);
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 如果不小心空放了冰淇淋还会重置CD
        if (mAttackFlag)
            GetCardBuilder().ResetCD();
        // 灰烬型卡片直接销毁自身
        ExecuteDeath();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        int totalCD=0; // 被冷却卡片剩余CD之和统计
        // 使当前格所有卡片CD重置
        foreach (var unit in GetGrid().GetFoodUnitList())
        {
            if(unit.mType != mType)
            {
                BaseCardBuilder builder = unit.GetCardBuilder();
                totalCD += builder.mCDLeft;
                builder.ResetCD();
            }
        }

        // 如果是二转，可以根据被冷却的卡片在被冷却前的剩余CD来返还自身CD
        if(mShape >= 2)
        {
            BaseCardBuilder builder = GetCardBuilder();
            int returnCD = Mathf.Max(builder.mCDLeft - totalCD, 0);
            builder.mCDLeft -= returnCD;
        }
    }
}
