using System;

public class BoiledWaterBoom : FoodUnit
{
    private bool isTrigger;
    public override void MInit()
    {
        isTrigger = false;
        base.MInit();
        Action<CombatAction> hitedAction = (combatAction) => {
            isTrigger = true;
        };
        AddActionPointListener(ActionPointType.PostReceiveDamage, hitedAction);
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, hitedAction);
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
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 生命值低于50或者当前所在格有普通类型卡片的存在 或者人物
        return isTrigger || GetGrid().IsContainTag(FoodInGridType.Default) || GetGrid().IsContainCharacter();
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
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 900 * mCurrentAttack / 10, GetRowIndex(), 5, 5, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
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
