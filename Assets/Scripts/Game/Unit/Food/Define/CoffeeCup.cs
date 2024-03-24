using UnityEngine;

public class CoffeeCup : FoodUnit
{
    private static RuntimeAnimatorController Bullet_Run;

    public override void Awake()
    {
        if (Bullet_Run == null)
            Bullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Food/25/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // 28秒消失
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(28 * 60);
            task.AddOnExitAction(delegate {
                ExecuteDeath();
            });
            taskController.AddTask(task);
        }
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        BaseCardBuilder builder = GetCardBuilder();
        if (builder != null)
            SmallStove.CreateAddFireEffect(transform.position, (float)builder.attr.GetCost(mLevel));
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(this, GetRowIndex());
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
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
            ExecuteDamage();
            mAttackFlag = false;
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
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return mAttackFlag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        float dmg = mCurrentAttack;
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.NoStrengthenNormal, Bullet_Run, this, dmg);
        b.transform.position = transform.position;
        b.SetStandardVelocity(24);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Puff");
        GameController.Instance.AddBullet(b);

        float s = 0;
        float maxDist = MapManager.gridWidth * 3.5f;
        if(mShape >= 1)
            maxDist = MapManager.gridWidth * 6.5f;

        if(mShape < 2)
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                s += b.GetVelocity();
                if (s >= maxDist)
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                b.KillThis();
            });
            b.AddTask(t);
        }
        else
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                s += b.GetVelocity();
                if (s <= MapManager.gridWidth)
                    b.SetDamage(2 * dmg);
                else if (s <= 3 * MapManager.gridWidth)
                    b.SetDamage((-0.5f*s / MapManager.gridWidth + 2.5f) * dmg);
                else
                    b.SetDamage(dmg);

                if (s >= maxDist)
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                b.KillThis();
            });
            b.AddTask(t);
        }
    }
}
