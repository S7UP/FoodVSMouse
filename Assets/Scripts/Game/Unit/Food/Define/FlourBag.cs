using UnityEngine;
/// <summary>
/// 面粉袋
/// </summary>
public class FlourBag : FoodUnit
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
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
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
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
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
        return (mAttackFlag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>attackPercent);
    }

    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        ExecuteDeath();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        BaseCardBuilder builder = GetCardBuilder();

        GameManager.Instance.audioSourceManager.PlayEffectMusic("Thump");
        // 添加对应的判定检测器
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1, 1, "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetAffectHeight(0);
            int count = 0;
            r.SetOnEnemyEnterAction((m) => {
                count++;
                if(mShape >= 1)
                    m.AddStatusAbility(new FrozenSlowStatusAbility(-90, m, 240));

                if (m.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
                {
                    new DamageAction(CombatAction.ActionType.RealDamage, this, m, 90 * mCurrentAttack).ApplyAction();
                    UnitManager.TriggerRecordDamage(m);
                }
                else
                {
                    UnitManager.Execute(this, m);
                }
            });
            r.AddBeforeDestoryAction(delegate {
                if (builder != null)
                {
                    Debug.Log("count="+count);
                    float percent = Mathf.Min(0.75f, 0.15f*count);
                    builder.AddLeftCDPercent(-percent);
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
