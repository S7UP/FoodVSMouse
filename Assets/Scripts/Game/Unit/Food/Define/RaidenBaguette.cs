using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 雷电长棍面包
/// </summary>
public class RaidenBaguette : FoodUnit
{
    // 这种类型单位的表都放这里
    private static List<RaidenBaguette> thisUnitList = new List<RaidenBaguette>();
    private static List<BaseUnit> hitedUnitList = new List<BaseUnit>();
    private static Sprite Lightning_Sprite;
    private static int currentShape; // 当前为几转
    private static int nextAttackTimeLeft; // 下次攻击剩余时间

    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private FloatModifier decMaxHpPercentMod = new FloatModifier(0); // 降低最大生命值上限的标签

    public override void Awake()
    {
        if(Lightning_Sprite == null)
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        base.Awake();
    }

    public override void MInit()
    {
        // 当第一个雷电被放下时，激活全局计时器
        if(GetThisUnitList().Count == 0)
            GameController.Instance.mTaskController.AddUniqueTask("RaidenBaguette_Attack_Task", GetGlobalAttackTimerTask());

        if (!thisUnitList.Contains(this))
            thisUnitList.Add(this);
        decMaxHpPercentMod.Value = 0;
        base.MInit();
        NumericBox.Hp.AddFinalPctAddModifier(decMaxHpPercentMod);
        currentShape = mShape;
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void MDestory()
    {
        base.MDestory();
        GetThisUnitList().Remove(this);
        if(GetThisUnitList().Count == 0)
            GameController.Instance.mTaskController.RemoveUniqueTask("RaidenBaguette_Attack_Task");
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void OnCastStateEnter()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Lighting");
        animatorController.Play("Attack");
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 降低一次最大生命值上限
    /// </summary>
    private void DecMaxHp()
    {
        NumericBox.Hp.RemoveFinalPctAddModifier(decMaxHpPercentMod);
        if (mShape < 1)
            decMaxHpPercentMod.Value -= 20;
        else
            decMaxHpPercentMod.Value -= 15;
        NumericBox.Hp.AddFinalPctAddModifier(decMaxHpPercentMod);

        if (decMaxHpPercentMod.Value <= -100)
            ExecuteDeath();
        else if (mCurrentHp > mMaxHp)
            mCurrentHp = mMaxHp;
    }

    #region 禁用普通攻击
    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {

    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    public override bool IsMeetEndGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {

    }

    public override bool IsDamageJudgment()
    {
        return false;
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击
    /// </summary>
    public override void LoadSkillAbility()
    {
        //foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        //{
        //    if (item.skillType == SkillAbility.Type.GeneralAttack)
        //    {
        //        generalAttackSkillAbility = new GeneralAttackSkillAbility(this, item);
        //        skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        //    }
        //}
    }

    protected override bool IsHasTarget()
    {
        return false;
    }

    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }
    #endregion

    #region 静态私有方法
    /// <summary>
    /// 获取在场上全部的雷电长棍面包对象
    /// </summary>
    /// <returns></returns>
    private static List<RaidenBaguette> GetThisUnitList()
    {
        // 先检查一遍
        List<RaidenBaguette> delList = new List<RaidenBaguette>();
        foreach (var u in thisUnitList)
            if (!u.IsValid())
                delList.Add(u);
        foreach (var u in delList)
            thisUnitList.Remove(u);
        return thisUnitList;
    }

    /// <summary>
    /// 执行一次电击
    /// </summary>
    private static void ExecuteAttack(List<RaidenBaguette> list)
    {
        hitedUnitList.Clear();
        // 使用对角矩阵遍历所有组合，保证两两关联
        for (int i = 0; i < list.Count; i++)
        {
            RaidenBaguette u1 = list[i];
            u1.SetActionState(new CastState(u1));
            for (int j = i + 1; j < list.Count; j++)
            {
                RaidenBaguette u2 = list[j];
                // 计算距离与方向向量
                float dist = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).magnitude;
                Vector2 rot = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).normalized;
                // 产生伤害判定域
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(Vector2.Lerp(u1.transform.position, u2.transform.position, 0.5f), dist / MapManager.gridWidth, 1, "ItemCollideEnemy");
                    r.transform.right = rot;
                    r.isAffectMouse = true;
                    r.SetInstantaneous();
                    r.SetOnEnemyEnterAction(EnemyEnterAction);
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
                    t.AddOnExitAction(delegate
                    {
                        l.ExecuteRecycle();
                    });
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
                r.SetOnEnemyEnterAction(EnemyEnterAction);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
        // 遍历完后再结算扣血
        for (int i = 0; i < list.Count; i++)
        {
            list[i].DecMaxHp();
        }
    }

    /// <summary>
    /// 当敌方单位进入电击判定区域时
    /// </summary>
    /// <param name="u"></param>
    private static void EnemyEnterAction(BaseUnit u)
    {
        if (hitedUnitList.Contains(u))
            return;
        // 计算折前伤害
        float dmg = u.mMaxHp * u.mBurnRate * u.mAoeRate; 
        // 先作用于护盾
        dmg = u.NumericBox.DamageShield(dmg);
        // 剩余伤害转为灰烬伤害
        if (dmg > 0)
            new DamageAction(CombatAction.ActionType.BurnDamage, null, u, dmg).ApplyAction();
        //  如果是二转还会施加2秒晕眩效果
        if(currentShape >= 2)
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, 120, false));
        // 记录已被电击过一次
        hitedUnitList.Add(u);
    }
    
    /// <summary>
    /// 获取全局计数器任务
    /// </summary>
    /// <returns></returns>
    private static CustomizationTask GetGlobalAttackTimerTask()
    {
        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            nextAttackTimeLeft = 600;
        });
        task.AddTaskFunc(delegate {
            List<RaidenBaguette> l1 = GetThisUnitList();
            if (l1.Count <= 0)
                return true;
            nextAttackTimeLeft--;
            if(nextAttackTimeLeft <= 0)
            {
                // 从中选出所有未被定身的雷电长棍面包
                List<RaidenBaguette> l2 = new List<RaidenBaguette>();
                foreach (var u in l1)
                {
                    if (u.GetNoCountUniqueStatus(StringManager.Stun) == null)
                        l2.Add(u);
                }
                // 如果未被定身的雷电长棍面包数量超过2，则执行一次电击
                if (l2.Count >= 2)
                {
                    ExecuteAttack(l2);
                    nextAttackTimeLeft = 600;
                }
            }
            return false;
        });
        return task;
    }
    #endregion
}
