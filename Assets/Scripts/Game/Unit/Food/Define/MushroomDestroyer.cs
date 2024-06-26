using UnityEngine;
/// <summary>
/// 炸炸菇
/// </summary>
public class MushroomDestroyer : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // 添加炸弹的免疫修饰
        FoodManager.AddBombModifier(this);
    }

    /// <summary>
    /// 炸弹掉落等效于正常死亡
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 即时型炸弹不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 即时型炸弹不需要
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        if (IsDamageJudgment())
        {
            // 灰烬型卡片直接销毁自身
            ExecuteDeath();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 原地产生一个爆炸效果
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
        // 添加对应的判定检测器
        {
            RetangleAreaEffectExecution e = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideAlly");
            e.SetInstantaneous(); // 设置成立即生效
            e.isAffectFood = true;
            int count = 0;
            e.SetOnFoodEnterAction((unit)=> 
            {
                if (LuminescentMold.CureUnit(unit))
                    count++;
            });
            // 爆炸结束时结算
            e.AddBeforeDestoryAction((e) => 
            {
                count = Mathf.Min(count, 9); // count最大为9
                // 返还10%*解除霉菌数的CD
                mBuilder.AddLeftCDPercent(-count*0.1f);
                // 返还75*(9-解除的霉菌数)火苗
                SmallStove.CreateAddFireEffect(e.transform.position, 75*(9-count));
            });
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }

    /// <summary>
    /// 亡语
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        // 伤害判定为消失时触发
        ExecuteDamage();
    }
}
