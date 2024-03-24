
using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;

public class IronMan : BossUnit
{
    private RuntimeAnimatorController HeavyArmor_Run;

    private float dmgRecord; // 受到的伤害总和
    private Queue<float> lost_hp_percent_queue = new Queue<float>();
    private float current_lost_hp_percent;

    private RingUI FinalSkillRingUI;

    private static RuntimeAnimatorController LaserHitEffect_Run;
    private static RuntimeAnimatorController Missile_Run;
    private static Sprite Laser_Spr;
    

    private FloatModifier p_dmgRateMod = new FloatModifier(1.0f);

    public override void Awake()
    {
        if(LaserHitEffect_Run == null)
        {
            LaserHitEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/LaserHitEffect");
            Missile_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/Missile");
            Laser_Spr = GameManager.Instance.GetSprite("Boss/17/Laser");
        }
        // 默认皮肤
        {
            HeavyArmor_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/0/HeavyArmor");
        }
        base.Awake();
    }

    public override void MInit()
    {
        dmgRecord = 0;
        p_dmgRateMod.Value = 1.0f;
        lost_hp_percent_queue.Clear();
        current_lost_hp_percent = 9999;
        base.MInit();
        // 大招UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this, 0.25f * MapManager.gridHeight * Vector3.down));
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(0);
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // 受击事件
        {
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) => {
                if (combatAction is DamageAction)
                {
                    float triggerFinalSkillDamage = mMaxHp * current_lost_hp_percent;
                    DamageAction dmgAction = combatAction as DamageAction;
                    dmgRecord += dmgAction.RealCauseValue;
                    FinalSkillRingUI.SetPercent(dmgRecord / triggerFinalSkillDamage);
                    if (dmgRecord >= triggerFinalSkillDamage)
                    {
                        CustomizationTask task = TaskManager.GetRingUIChangeTask(FinalSkillRingUI, 60, 1, 0);
                        task.AddOnExitAction(delegate {
                            FinalSkillRingUI.Hide();
                        });
                        FinalSkillRingUI.mTaskController.AddTask(task);
                        dmgRecord -= triggerFinalSkillDamage;
                        CustomizationSkillAbility s = SKill2Init(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[2]);
                        mSkillQueueAbilityManager.SetNextSkill(s);

                        if (lost_hp_percent_queue.Count > 0)
                            current_lost_hp_percent = lost_hp_percent_queue.Dequeue();
                        else
                        {
                            current_lost_hp_percent = 9999;
                        }
                    }
                }
            });
        }

        // 添加出现的技能
        {
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            Func<BaseUnit, BaseUnit, bool> noSelcetedFunc = delegate { return false; };

            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 60;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Appear");
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if(current_lost_hp_percent!=9999)
                        FinalSkillRingUI.Show();
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
                    // 添加被动减伤
                    NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);
                    NumericBox.DamageRate.AddModifier(p_dmgRateMod);

                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                    return true;
                }
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
    }

    public override void MDestory()
    {
        base.MDestory();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.IronMan, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // 特殊参数初始化
        // 获取大招损失生命值条件
        {
            Action<float[]> action = delegate
            {
                float[] arr = GetParamArray("p_lost_hp_percent");
                lost_hp_percent_queue.Clear();
                foreach (var val in arr)
                    lost_hp_percent_queue.Enqueue(val / 100);
                if (lost_hp_percent_queue.Count > 0)
                    current_lost_hp_percent = lost_hp_percent_queue.Dequeue();
                else
                {
                    current_lost_hp_percent = 9999;
                    FinalSkillRingUI.Hide();
                }
            };
            AddParamChangeAction("p_lost_hp_percent", action);
            action(null);
        }

        // 被动减伤
        {
            Action<float[]> action = delegate
            {
                // 移除加防
                NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);
                p_dmgRateMod.Value = 1 - GetParamValue("p_defence") / 100;
                NumericBox.DamageRate.AddModifier(p_dmgRateMod);
            };
            AddParamChangeAction("p_defence", action);
            action(null);
        }

        // 根据传入的skin换皮（0为原皮，1为变异）
        {
            Action<float[]> action = delegate
            {
                int skin = Mathf.FloorToInt(GetParamValue("skin"));
                HeavyArmor_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/"+skin+"/HeavyArmor");
                animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/" + skin + "/0");
            };
            action(null);
            AddParamChangeAction("skin", action);
        }
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(SKill0Init(infoList[0]));
        list.Add(SKill1Init(infoList[1]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////

    #region 一技能
    /// <summary>
    /// 产生落地的伤害判定
    /// </summary>
    private void CreateS0LandDamageArea(Vector2 pos)
    {
        float dmg_trans = GetParamValue("dmg_trans0") / 100;
        // 对格子
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "CollideGrid");
            r.transform.position = pos;
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.SetAffectHeight(0);
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction dmgAction = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, dmgAction.RealCauseValue * dmg_trans).ApplyAction();
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 对老鼠
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
            r.transform.position = pos;
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetOnEnemyEnterAction((u) => {
                if (!u.IsBoss())
                    UnitManager.Execute(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private void CreateS0AttackDamageArea(Vector2 pos)
    {
        float dmg = GetParamValue("dmg0");
        // 产生击中特效
        {
            BaseEffect e = BaseEffect.CreateInstance(LaserHitEffect_Run, null, "Disappear", null, false);
            e.SetSpriteRendererSorting("Effect", 10);
            e.transform.position = pos;
            GameController.Instance.AddEffect(e);
        }
        // 击中毁卡判定
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth / 2, MapManager.gridHeight / 2), "CollideGrid");
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.AddBeforeDestoryAction(delegate {
                BaseGrid g = null;
                float min = float.MaxValue;
                foreach (var _g in r.gridList.ToArray())
                {
                    float dist = (_g.transform.position - r.transform.position).magnitude;
                    if (dist < min)
                    {
                        g = _g;
                        min = dist;
                    }
                }
                if (g != null)
                    g.TakeDamage(this, dmg, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // 击中处决老鼠判定
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
            r.SetInstantaneous();
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && MouseManager.IsGeneralMouse(u);
            });
            r.SetOnEnemyEnterAction((u) => {
                UnitManager.Execute(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private Vector2 FindS0Pos()
    {
        List<int> list = new List<int>() { 0, 1, 2, 4, 5, 6 };

        float min = float.MaxValue;
        List<int> selectedList = new List<int>() { 0, 1, 2, 4, 5, 6 };
        foreach (var rowIndex in list)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, float.MaxValue, false);
            float xPos = MapManager.GetColumnX(0);
            if (unit != null)
            {
                xPos = unit.transform.position.x;
            }

            if (xPos < min)
            {
                min = xPos;
                selectedList.Clear();
                selectedList.Add(rowIndex);
            }
            else if (xPos == min)
            {
                selectedList.Add(rowIndex);
            }
        }
        // 在这些行里随机挑一行吧
        {
            int rowIndex = selectedList[GetRandomNext(0, selectedList.Count)];
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, float.MaxValue, false);
            if(unit != null)
                return MapManager.GetGridLocalPosition(unit.GetColumnIndex(), rowIndex);
            else
                return MapManager.GetGridLocalPosition(0, rowIndex);
        }
        
    }

    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int wait = Mathf.FloorToInt(GetParamValue("wait0") * 60);
        int move_time = Mathf.FloorToInt(GetParamValue("move_time0")*60);
        int interval = 15;

        int fly_time = 30;
        float fly_height = 10*MapManager.gridHeight;

        // 变量
        int timeLeft = 0;
        int attack_timeLeft = 0;
        FloatModifier yOffsetMod = new FloatModifier(0);
        Vector2 start = Vector2.zero;
        Vector2 end = Vector2.zero;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTaskFunc(delegate {
                    animatorController.Play("PreFly");
                    return true;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Fly1", true);
                        return true;
                    }
                    return false;
                });
                task.AddTimeTaskFunc(fly_time, null,
                    (leftTime, totalTime) =>
                    {
                        RemoveSpriteOffsetY(yOffsetMod);
                        float rate = 1 - (float)leftTime / totalTime;
                        yOffsetMod.Value = fly_height * rate;
                        AddSpriteOffsetY(yOffsetMod);
                    }, null);
                task.AddTimeTaskFunc(fly_time,
                    delegate {
                        transform.position = FindS0Pos();
                        animatorController.Play("Down", true);
                    },
                    (leftTime, totalTime) =>
                    {
                        RemoveSpriteOffsetY(yOffsetMod);
                        float rate = (float)leftTime / totalTime;
                        yOffsetMod.Value = fly_height * rate;
                        AddSpriteOffsetY(yOffsetMod);
                    },
                    delegate {
                        RemoveSpriteOffsetY(yOffsetMod);
                        animatorController.Play("Land");
                            // 落地后造成范围伤害
                            CreateS0LandDamageArea(transform.position);
                    });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("PreFly1");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Fly");
                        start = transform.position;
                        if (transform.position.y <= MapManager.GetRowY(3))
                            end = new Vector2(transform.position.x, MapManager.GetRowY(0));
                        else
                            end = new Vector2(transform.position.x, MapManager.GetRowY(6));
                        timeLeft = move_time;
                        attack_timeLeft = interval;
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    attack_timeLeft--;
                    float r = 1 - (float)timeLeft / move_time;
                    transform.position = Vector2.Lerp(start, end, r);
                    if(attack_timeLeft <= 0)
                    {
                        attack_timeLeft += interval;
                        CreateS0AttackDamageArea(transform.position);
                    }

                    if (timeLeft <= 0)
                    {
                        animatorController.Play("Land");
                        return true;
                    }
                    return false;
                });
                if(wait > 0)
                {
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Breakup");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            // 移除加防
                            NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);
                            animatorController.Play("Idle1", true);
                            timeLeft = wait;
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            // 重新加防
                            NumericBox.DamageRate.AddModifier(p_dmgRateMod);
                            animatorController.Play("Appear");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                    });
                }
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion

    #region 二技能
    private MouseModel CreateS1HeavyArmor(Vector2 pos)
    {
        FloatModifier dmgRateMod = new FloatModifier(1 - GetParamValue("defence1")/100);

        MouseModel m = MouseModel.GetInstance(HeavyArmor_Run);
        m.transform.position = pos;
        m.SetBaseAttribute(mMaxHp, 10, 1.0f, 1.0f, 0, 0.5f, 0);
        m.NumericBox.DamageRate.AddModifier(dmgRateMod);
        // 折后伤害传递给本体
        m.actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combat) => 
        { 
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, this);
                d.DamageValue = dmgAction.RealCauseValue;
                if (d.IsDamageType(DamageAction.DamageType.BombBurn))
                    d.DamageValue /= m.mBurnRate;
                d.ApplyAction();
            }
        });
        m.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        m.SetGetBurnRateFunc(delegate { return mBurnRate; });
        m.isBoss = true;
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBlockFunc(delegate { return false; });
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        m.AttackClipName = "AttackLeftUp";

        m.SetActionState(new IdleState(this));
        GameController.Instance.AddMouseUnit(m);

        {
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return !IsAlive();
            });
            task.AddOnExitAction(delegate {
                m.ExecuteDeath();
            });
            m.taskController.AddTask(task);
        }

        return m;
    }

    private BaseGrid GetS1AttackGrid(List<BaseGrid> execludeGridList)
    {
        float max_hp = float.MinValue;
        List<BaseUnit> l1 = new List<BaseUnit>();
        foreach (var u in GameController.Instance.GetEachAlly().ToArray())
        {
            if((u is FoodUnit) && FoodManager.IsAttackableFoodType((u as FoodUnit)) && u.GetGrid()!=null && !execludeGridList.Contains(u.GetGrid()))
            {
                if(u.mMaxHp > max_hp)
                {
                    l1.Clear();
                    max_hp = u.mMaxHp;
                    l1.Add(u);
                }else if(u.mMaxHp == max_hp)
                {
                    l1.Add(u);
                }
            }
        }

        float max_dist = float.MinValue;
        BaseGrid g = null;
        foreach (var u in l1)
        {
            float dist = (transform.position - u.transform.position).magnitude;
            if(dist > max_dist)
            {
                max_dist = dist;
                g = u.GetGrid();
            }
        }
        return g;
    }

    private void CreateS1LaserAndHit(Vector2 start, Vector2 end)
    {
        float dist = (end - start).magnitude;
        Vector2 rot = (end - start).normalized;
        // 产生激光特效
        {
            BaseLaser l = BaseLaser.GetAllyInstance(null, 0, start, rot, null, Laser_Spr, null, null, null, null, null, null);
            l.mMaxLength = dist;
            l.mCurrentLength = dist;
            l.mVelocity = TransManager.TranToVelocity(108);
            l.isCollide = false;
            l.laserRenderer.SetSortingLayerName("Effect");

            int timeLeft = 20;
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate 
            {
                l.laserRenderer.SetVerticalOpenTime(-20);
                timeLeft = 20;
            });
            t.AddTaskFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
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

        // 产生击中特效
        {
            BaseEffect e = BaseEffect.CreateInstance(LaserHitEffect_Run, null, "Disappear", null, false);
            e.SetSpriteRendererSorting("Effect", 10);
            e.transform.position = end;
            GameController.Instance.AddEffect(e);
        }

        // 伤害判定
        {
            Action<BaseUnit> action = (u) => 
            {
                BurnManager.BurnDamage(this, u);
            };

            // 对美食单位
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(end, new Vector2(MapManager.gridWidth / 2, MapManager.gridHeight / 2), "CollideGrid");
                r.SetInstantaneous();
                r.isAffectGrid = true;
                r.AddBeforeDestoryAction(delegate {
                    BaseGrid g = null;
                    float min = float.MaxValue;
                    foreach (var _g in r.gridList.ToArray())
                    {
                        float dist = (_g.transform.position - r.transform.position).magnitude;
                        if (dist < min)
                        {
                            g = _g;
                            min = dist;
                        }
                    }
                    if (g != null)
                        g.TakeAction(this, action, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // 对老鼠单位
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(end, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction(action);
                r.SetInstantaneous();
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        float colIndex = 9 - GetParamValue("right_col1");
        int move_time = Mathf.FloorToInt(GetParamValue("move_time1")*60);
        int num = Mathf.FloorToInt(GetParamValue("num1"));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait1_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait1_1") * 60);
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time1")*60);

        // 变量
        FloatModifier dmgRateMod = new FloatModifier(1 - GetParamValue("defence1") / 100);
        int timeLeft = 0;
        List<BaseGrid> execludeGridList = new List<BaseGrid>();
        MouseModel model = null;
        Vector3 laserStartPos = Vector2.zero;
        Vector3 laserEndPos = Vector2.zero;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            execludeGridList.Clear();
        };
        {
            // 准备起飞
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("PreFly1");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Fly2");
                        return true;
                    }
                    return false;
                });
                return task;
            });
            // 移动到特定点
            c.AddCreateTaskFunc(delegate
            {
                Vector2 startPos = Vector2.zero;
                Vector2 endPos = Vector2.zero;

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    startPos = transform.position;
                    List<int> list = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                    int rowIndex = list[GetRandomNext(0, list.Count)];
                    endPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    NumericBox.DamageRate.AddModifier(dmgRateMod);
                });
                task.AddTimeTaskFunc(move_time, 
                    delegate { mHeight = 1; },
                    (leftTime, totalTime) => 
                    {
                        float rate = 1 - (float)leftTime / totalTime;
                        transform.position = Vector2.Lerp(startPos, endPos, rate);
                    },
                    delegate 
                    {
                        mHeight = 0;
                        transform.position = endPos;
                        animatorController.Play("Land");
                    }
                    );
                task.AddTaskFunc(delegate {
                    if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Breakup");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        NumericBox.DamageRate.RemoveModifier(dmgRateMod);
                        // 自身隐藏且取消判定，然后生成一个傀儡装甲代替自身
                        SetAlpha(0);
                        CloseCollision();
                        model = CreateS1HeavyArmor(transform.position);
                        model.animatorController.Play("Appear");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if(!model.IsAlive() || model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                return task;
            });
            // 激光攻击
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < num; _i++)
                {
                    int i = _i;
                    task.AddTaskFunc(delegate {
                        // 寻找攻击目标并攻击
                        BaseGrid g = GetS1AttackGrid(execludeGridList);
                        Vector3 attackPos = Vector3.zero; // 攻击点
                        if (g != null)
                        {
                            execludeGridList.Add(g);
                            attackPos = g.transform.position;
                        }
                        else
                        {
                            attackPos = MapManager.GetGridLocalPosition(0, GetRowIndex());
                        }
                        Vector3 delta = attackPos - transform.position;
                        if (i == 0)
                        {
                            laserStartPos = transform.position + new Vector3(-1.038f, 0.917f);
                            model.animatorController.Play("AttackLeftUp");
                        }
                        else if (i == 1)
                        {
                            laserStartPos = transform.position + new Vector3(-0.645f, 0.076f);
                            model.animatorController.Play("AttackLeftDown");
                        }
                        else if (i == 2)
                        {
                            laserStartPos = transform.position + new Vector3(1.148f, 0.141f);
                            model.animatorController.Play("AttackRightDown");
                        }
                        else
                        {
                            laserStartPos = transform.position + new Vector3(1.804f, 0.984f);
                            model.animatorController.Play("AttackRightUp");
                        }
                        laserEndPos = attackPos;
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        if (model.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.5f)
                        {
                            CreateS1LaserAndHit(laserStartPos, laserEndPos);
                            CreateS1LaserAndHit(laserStartPos, new Vector2(transform.position.x - MapManager.gridWidth * GetParamValue("left1", i), transform.position.y));
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            model.animatorController.Play("Idle");
                            timeLeft = wait0;
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                            return true;
                        return false;
                    });
                }
                // 等一下
                task.AddTaskFunc(delegate {
                    timeLeft = wait1;
                    return true;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                        return true;
                    return false;
                });
                return task;
            });


            // 解体自爆
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    model.animatorController.Play("Disappear");
                });
                task.AddTaskFunc(delegate {
                    if (model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        model.MDestory();
                        // 自身显示且开启判定
                        SetAlpha(1);
                        OpenCollision();
                        animatorController.Play("Appear");
                        // 全场晕眩
                        {
                            Action<BaseUnit> action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false)); };

                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13*MapManager.gridWidth, 9*MapManager.gridHeight), "BothCollide");
                            r.SetInstantaneous();
                            r.isAffectFood = true;
                            r.isAffectMouse = true;
                            r.SetOnFoodEnterAction(action);
                            r.SetOnEnemyEnterAction(action);
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion

    #region 三技能
    private Vector2 GetS2AttackPos(int colIndex)
    {
        int[] valArray = new int[5];
        Vector2[] pArray = new Vector2[5];
        for (int i = 1; i <= 5; i++)
            pArray[i-1] = MapManager.GetGridLocalPosition(colIndex, i);

        foreach (var u in GameController.Instance.GetEachAlly())
        {
            if(u is FoodUnit)
            {
                FoodUnit f = u as FoodUnit;
                if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f))
                {
                    for (int i = 0; i < pArray.Length; i++)
                    {
                        Vector2 p = pArray[i];
                        if(Mathf.Abs(f.transform.position.x - p.x)<1.15f*MapManager.gridWidth && Mathf.Abs(f.transform.position.y - p.y) < 1.15f * MapManager.gridHeight)
                            valArray[i]++;
                    }
                }
            }
        }

        int max = int.MinValue;
        List<Vector2> posList = new List<Vector2>();
        for (int i = 0; i < valArray.Length; i++)
        {
            if(valArray[i] > max)
            {
                max = valArray[i];
                posList.Clear();
                posList.Add(pArray[i]);
            }
            else if(valArray[i] == max)
            {
                posList.Add(pArray[i]);
            }
        }

        return posList[GetRandomNext(0, posList.Count)];
    }

    /// <summary>
    /// 发射一发导弹
    /// </summary>
    private void CreateS2Missile(Vector2 endPos)
    {
        int t = 120;
        float deltaX = 0.238f, deltaY = 2.061f;

        EnemyBullet b = EnemyBullet.GetInstance(Missile_Run, this, 0);
        b.isnDelOutOfBound = true;
        b.AddHitAction((b, u) => {
            // 炸格子
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 2.25f, 2.25f, "CollideGrid");
                r.isAffectGrid = true;
                r.SetInstantaneous();
                r.SetOnGridEnterAction((g) => {
                    g.TakeAction(this, (u) => {
                        BurnManager.BurnDamage(this, u);
                    }, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // 炸老鼠
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 3, 3, "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetInstantaneous();
                r.SetOnEnemyEnterAction((u) => {
                    BurnManager.BurnDamage(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        });
        
        Vector2 startPos = transform.position + new Vector3(deltaX, deltaY);
        float dist = (startPos - endPos).magnitude;
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, t, 5*MapManager.gridHeight, startPos, endPos, true));
        GameController.Instance.AddBullet(b);
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int num = Mathf.FloorToInt(GetParamValue("num2"));
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2")*60);
        float stand_colIndex = 9 - GetParamValue("right_col2");
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        int stun_time = Mathf.FloorToInt(GetParamValue("stun2") * 60);
        int max_add_time = Mathf.FloorToInt(GetParamValue("max_add_time2") * 60);
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);
        float dmg_rate = 1 - GetParamValue("defence2")/100;
        int count = Mathf.FloorToInt(GetParamValue("count2"));

        // 变量
        int timeLeft = 0;
        int av_add_time_left = 0; // 还可以被用来延长的时间
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        Vector2 attackPos = Vector2.zero;
        FloatModifier dmgMod = new FloatModifier(dmg_rate);
        // 被炸后加2秒的设定
        Action<CombatAction> bombAction = (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    int add = 0; // 初步确定要延长的时间
                    if (av_add_time_left <= add_time)
                    {
                        add = av_add_time_left;
                        av_add_time_left = 0;
                    }
                    else
                    {
                        add = add_time;
                        av_add_time_left -= add_time;
                    }

                    if (wait0 - timeLeft > add)
                        timeLeft += add;
                    else
                    {
                        add -= (wait0 - timeLeft);
                        av_add_time_left += add;
                        timeLeft = wait0;
                    }
                }
            }
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            // 获得减伤
            NumericBox.DamageRate.AddModifier(dmgMod);
        };
        {
            // 导弹攻击循环
            c.AddCreateTaskFunc(delegate {
                RingUI ru = null;
                CustomizationTask ruTask = null;

                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < num; _i++)
                {
                    int i = _i;
                    int colIndex = Mathf.FloorToInt(GetParamValue("target_left_col2", i))-1;
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("PreFly1");
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Fly2");
                            attackPos = GetS2AttackPos(colIndex);
                            startPos = transform.position;
                            endPos = new Vector2(MapManager.GetColumnX(stand_colIndex), attackPos.y);
                            return true;
                        }
                        return false;
                    });
                    task.AddTimeTaskFunc(move_time, 
                    delegate { mHeight = 1; },
                    (leftTime, totalTime) => 
                    {
                        float rate = 1 - (float)leftTime / totalTime;
                        transform.position = Vector2.Lerp(startPos, endPos, rate);
                    },
                    delegate 
                    {
                        mHeight = 0;
                        transform.position = endPos;
                        animatorController.Play("Land");
                    }
                    );
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("PreAttack");
                            timeLeft = wait0;
                            av_add_time_left = max_add_time;
                            // 加入读条UI
                            {
                                ru = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru);
                                ruTask = TaskManager.GetWaitRingUITask(ru, this, 0.25f * MapManager.gridHeight * Vector3.down);
                                ru.mTaskController.AddTask(ruTask);
                            }
                            // 添加被炸后加时设定
                            {
                                AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            }

                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        // 控制读条UI
                        {
                            ru.SetPercent(1 - (float)timeLeft / wait0);
                        }

                        if (timeLeft <= 0)
                        {   
                            // 移除读条UI
                            {
                                ru.mTaskController.RemoveTask(ruTask);
                                ru = null;
                            }
                            // 移除被炸加时
                            RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);

                            return true;
                        }
                        return false;
                    });

                    for (int j = 0; j < count; j++)
                    {
                        task.AddTaskFunc(delegate {
                            animatorController.Play("Attack");
                            // 产生弹体
                            CreateS2Missile(attackPos);
                            return true;
                        });
                        task.AddTaskFunc(delegate {
                            return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                        });
                    }

                    task.AddTaskFunc(delegate {
                        animatorController.Play("PostAttack");
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Idle");
                            timeLeft = wait1;
                            // 加入读条UI
                            {
                                ru = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru);
                                ruTask = TaskManager.GetWaitRingUITask(ru, this, 0.25f * MapManager.gridHeight * Vector3.down);
                                ru.mTaskController.AddTask(ruTask);
                            }
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        // 控制读条UI
                        {
                            ru.SetPercent(1 - (float)timeLeft / wait1);
                        }

                        if (timeLeft <= 0)
                        {
                            // 移除读条UI
                            {
                                ru.mTaskController.RemoveTask(ruTask);
                                ru = null;
                            }
                            return true;
                        }
                        return false;
                    });
                }
                return task;
            });

            // 瘫痪，晕眩
            c.AddCreateTaskFunc(delegate {
                float lost_hp_val = GetParamValue("lost_hp_percent2")/100 * mMaxHp;
                float hpLeft = lost_hp_val;

                RingUI ru_time = null;
                CustomizationTask ru_timeTask = null;

                RingUI ru_hp = null;
                CustomizationTask ru_hpTask = null;

                Action<CombatAction> hitAction = (combatAction) =>
                {
                    if (combatAction is DamageAction)
                    {
                        DamageAction dmgAction = combatAction as DamageAction;
                        hpLeft -= dmgAction.RealCauseValue;
                    }
                };

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    // 移除加防
                    NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);
                    // 移除减伤
                    NumericBox.DamageRate.RemoveModifier(dmgMod);
                    animatorController.Play("Die");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 自我晕眩动画
                        animatorController.Play("Stun", true);
                        timeLeft = stun_time;
                        // 加入读条UI
                        {
                            // 时间条
                            {
                                ru_time = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru_time);
                                ru_timeTask = TaskManager.GetWaitRingUITask(ru_time, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.left);
                                ru_time.mTaskController.AddTask(ru_timeTask);
                            }
                            // 剩余生命条
                            {
                                ru_hp = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru_hp);
                                ru_hpTask = TaskManager.GetStunRingUITask(ru_hp, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.right);
                                ru_hp.mTaskController.AddTask(ru_hpTask);
                            }
                        }
                        // 开始计算此期间受击伤害
                        actionPointController.AddListener(ActionPointType.PostReceiveDamage, hitAction);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    // 控制读条UI
                    {
                        ru_time.SetPercent((float)timeLeft / stun_time);
                        ru_hp.SetPercent((float)hpLeft / lost_hp_val);
                    }
                    if (timeLeft <= 0 || hpLeft <= 0)
                    {
                        // 移除读条UI
                        {
                            ru_time.mTaskController.RemoveTask(ru_timeTask);
                            ru_time = null;
                            ru_hp.mTaskController.RemoveTask(ru_hpTask);
                            ru_hp = null;
                        }
                        animatorController.Play("Appear");
                        // 移除计算此期间的受击伤害
                        actionPointController.RemoveListener(ActionPointType.PostReceiveDamage, hitAction);
                        // 显示大招UI
                        if (current_lost_hp_percent < 9999)
                            FinalSkillRingUI.Show(); // 重新显示UI
                        NumericBox.DamageRate.AddModifier(p_dmgRateMod); // 重新加防
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle", true);
                        return true;
                    }
                    return false;
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion
}
