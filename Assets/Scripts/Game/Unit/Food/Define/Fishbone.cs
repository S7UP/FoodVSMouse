using UnityEngine;

public class Fishbone : FoodUnit
{
    private RetangleAreaEffectExecution checkArea;

    public override void MInit()
    {
        base.MInit();
        // 不会被选为攻击目标和不可被阻挡的效果
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // 添加隐匿特效
            // EnvironmentFacade.AddFogBuff(this);
        }
        // 生成检测区域
        CreateCheckArea();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 生成检测区域
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        //r.AddEnemyEnterConditionFunc((m) => {
        //    return true;
        //});
        GameController.Instance.AddAreaEffectExecution(r);

        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);

        checkArea = r;
    }

    #region 以下为普通攻击相关

    protected override bool IsHasTarget()
    {
        return checkArea != null && checkArea.mouseUnitList.Count > 0;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
        // GameManager.Instance.audioSourceController.PlayEffectMusic("Fume");
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
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        foreach (var m in checkArea.mouseUnitList.ToArray())
        {
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, m, mCurrentAttack);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
            if(mShape >= 1)
                m.AddStatusAbility(new SlowStatusAbility(-50, m, 120));
        }
    }
    #endregion
}
