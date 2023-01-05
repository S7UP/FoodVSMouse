using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 雷电长棍面包
/// </summary>
public class RaidenBaguette : FoodUnit
{
    private const string TaskName = "RaidenBaguetteDebuff";

    // 这种类型单位的表都放这里
    private static List<RaidenBaguette> thisUnitList = new List<RaidenBaguette>();
    private static List<float> AttackSpeedList = new List<float>() { 
        0.117f, 0.125f, 0.133f, 0.143f, 0.154f, 0.167f, 0.182f, 0.2f, 
        0.22f, 0.25f, 0.286f, 0.308f, 0.33f, 0.364f, 0.4f, 0.44f, 0.5f
    };
    private static Sprite Lightning_Sprite;

    private GeneralAttackSkillAbility generalAttackSkillAbility;

    public override void Awake()
    {
        if(Lightning_Sprite == null)
        {
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        }

        base.Awake();
    }

    public override void MInit()
    {
        if (!thisUnitList.Contains(this))
            thisUnitList.Add(this);
        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
        NumericBox.AttackSpeed.SetBase(AttackSpeedList[mLevel]);
        if (generalAttackSkillAbility != null)
            generalAttackSkillAbility.UpdateNeedEnergyByAttackSpeed();
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击
    /// </summary>
    public override void LoadSkillAbility()
    {
        foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        {
            if (item.skillType == SkillAbility.Type.GeneralAttack)
            {
                generalAttackSkillAbility = new GeneralAttackSkillAbility(this, item);
                skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
            }
        }
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 不需要
        return true;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 场上最少存在两个雷电
        return GetThisUnitList().Count >= 2;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 取消所有能量并切换为攻击贴图
        foreach (var u in GetThisUnitList())
        {
            u.SetActionState(new AttackState(u));
            u.generalAttackSkillAbility.ClearCurrentEnergy();
        }
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
        // 切换为静止贴图
        foreach (var u in GetThisUnitList())
        {
            u.SetActionState(new IdleState(u));
        }
        mAttackFlag = true;
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
        ExecuteAttack();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        GetThisUnitList().Remove(this);
    }

    public override void ExecuteRecycle()
    {
        base.ExecuteRecycle();
        GetThisUnitList().Remove(this);
    }

    //////////////////////////////////////////////////// 以下为静态方法
    /// <summary>
    /// 获取在场上全部的雷电长棍面包对象
    /// </summary>
    /// <returns></returns>
    public static List<RaidenBaguette> GetThisUnitList()
    {
        // 先检查一遍
        List<RaidenBaguette> delList = new List<RaidenBaguette>();
        foreach (var u in thisUnitList)
        {
            if (!u.IsValid())
            {
                delList.Add(u);
            }
        }
        foreach (var u in delList)
        {
            thisUnitList.Remove(u);
        }
        return thisUnitList;
    }

    /// <summary>
    /// 执行一次电击
    /// </summary>
    public static void ExecuteAttack()
    {
        List<BaseUnit> hitedUnitList = new List<BaseUnit>(); // 已被电击的单位，目的是防止二次电击

        List<RaidenBaguette> list = GetThisUnitList();
        // 使用对角矩阵遍历所有组合，保证两两关联
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                RaidenBaguette u1 = list[i];
                RaidenBaguette u2 = list[j];
                // 计算距离与方向向量
                float dist = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).magnitude;
                Vector2 rot = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).normalized;
                // 产生伤害判定域
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(Vector2.Lerp(u1.transform.position, u2.transform.position, 0.5f), dist / MapManager.gridWidth, 0.5f, "ItemCollideEnemy");
                    r.transform.right = rot;
                    r.isAffectMouse = true;
                    r.SetInstantaneous();
                    r.SetOnEnemyEnterAction((u)=> {
                        if (hitedUnitList.Contains(u))
                            return;
                        // 造成一次伤害为900*(两者当前攻击力之和/20)的爆破灰烬效果
                        CombatActionManager.BombBurnDamageUnit(null, u, 900*(u1.mCurrentAttack+u2.mCurrentAttack)/20);
                        // 如果是二转，还可以附加唯一的1秒80%减速效果和3秒的20%增伤效果
                        if (u1.mShape >= 2)
                        {
                            LightningTask t;
                            if (u.GetTask(TaskName) == null)
                            {
                                t = new LightningTask(u);
                                u.AddUniqueTask(TaskName, t);
                            }
                            else
                            {
                                t = u.GetTask(TaskName) as LightningTask;
                                t.Refresh(); // 刷新
                            }
                        }
                        // 记录已被电击过一次
                        hitedUnitList.Add(u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                // 产生电流特效
                {
                    BaseLaser l = BaseLaser.GetAllyInstance(null, 0, u1.transform.position, rot, null, Lightning_Sprite, null, null, null, null, null, null);
                    l.mMaxLength = dist;
                    l.mCurrentLength = dist;
                    l.mVelocity = TransManager.TranToVelocity(36);
                    l.isCollide = false;
                    l.laserRenderer.SetSortingLayerName("Effect");

                    int timeLeft = 20;
                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate
                    {
                        if (timeLeft > 0)
                            timeLeft--;
                        else
                        {
                            // 折叠消失
                            l.laserRenderer.SetVerticalOpenTime(-20);
                            timeLeft = 20;
                            return true;
                        }
                        return false;
                    });
                    t.AddTaskFunc(delegate
                    {
                        if (timeLeft > 0)
                            timeLeft--;
                        else
                            return true;
                        return false;
                    });
                    t.OnExitFunc = delegate
                    {
                        l.ExecuteRecycle();
                    };
                    l.AddTask(t);

                    GameController.Instance.AddLaser(l);
                }
            }
            // 产生以自身为中心1*1的电场
            {
                RaidenBaguette currentUnit = list[i];
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(currentUnit.transform.position, 1, 1, "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetInstantaneous();
                r.SetOnEnemyEnterAction((u) => {
                    if (hitedUnitList.Contains(u))
                        return;
                    // 造成一次伤害为900*(当前攻击力/20)的爆破灰烬效果
                    CombatActionManager.BombBurnDamageUnit(null, u, 900 * currentUnit.mCurrentAttack / 10);
                    // 如果是二转，还可以附加唯一的1秒80%减速效果和3秒的20%增伤效果
                    if (currentUnit.mShape >= 2)
                    {
                        LightningTask t;
                        if (u.GetTask(TaskName) == null)
                        {
                            t = new LightningTask(u);
                            u.AddUniqueTask(TaskName, t);
                        }
                        else
                        {
                            t = u.GetTask(TaskName) as LightningTask;
                            t.Refresh(); // 刷新
                        }
                    }
                    // 记录已被电击过一次
                    hitedUnitList.Add(u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }


    /// <summary>
    /// 二转的电击任务
    /// </summary>
    private class LightningTask : ITask
    {
        private FloatModifier AddDamageModifier = new FloatModifier(1.2f); // 增伤
        private FloatModifier moveSpeedModifier = new FloatModifier(-80); // 减速

        private const int SlowDownTime = 60;
        private const int AddDamageTime = 180;

        private int slowDownTimeLeft;
        private int addDamageTimeLeft;

        private BaseUnit unit;

        public LightningTask(BaseUnit unit)
        {
            this.unit = unit;
            
        }

        public void OnEnter()
        {
            Refresh();
        }

        public void OnUpdate()
        {
            if (slowDownTimeLeft > 0)
                slowDownTimeLeft--;
            else
                unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier);

            if (addDamageTimeLeft > 0)
                addDamageTimeLeft--;
            else
                unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);
        }

        public bool IsMeetingExitCondition()
        {
            return slowDownTimeLeft <= 0 && addDamageTimeLeft <= 0;
        }

        public void OnExit()
        {
            // 移除减速
            unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier);
            // 移除增伤
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);
        }

        /// <summary>
        /// 刷新时间
        /// </summary>
        public void Refresh()
        {
            // 移除增伤
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);

            slowDownTimeLeft = SlowDownTime;
            addDamageTimeLeft = AddDamageTime;

            // 减速
            unit.AddStatusAbility(new SlowStatusAbility(-80, unit, SlowDownTime));
            unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedModifier);
            // 增伤
            unit.NumericBox.DamageRate.AddModifier(AddDamageModifier);
        }
    }
} 
