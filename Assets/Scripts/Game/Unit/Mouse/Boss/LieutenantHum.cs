using System;
using System.Collections.Generic;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 嗡嗡中尉
/// </summary>
public class LieutenantHum : BossUnit
{
    private static RuntimeAnimatorController ShotEffect_Run;
    private static RuntimeAnimatorController Wind_Run;
    private static RuntimeAnimatorController TailAir_Run;

    public override void Awake()
    {
        if (ShotEffect_Run == null)
        {
            ShotEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/11/ShotEffect");
            Wind_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/11/Wind");
            TailAir_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/11/TailAir");
        }
            
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // 添加出现的技能
        {
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            Func<BaseUnit, BaseUnit, bool> noSelcetedFunc = delegate { return false; };

            CompoundSkillAbility c = new CompoundSkillAbility(this);
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddCreateTaskFunc(delegate {
                CustomizationTask t = new CustomizationTask();
                t.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
                });
                t.AddTimeTaskFunc(120);
                t.AddOnExitAction(delegate
                {
                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                });
                return t;
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }


    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.LieutenantHum, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Skill0Init(infoList[0]));
        list.Add(Skill1Init(infoList[1]));
        list.Add(Skill2Init(infoList[2]));
        list.Add(Skill3Init(infoList[3]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    #region 索敌方法
    private void FindR0(out int rowIndex)
    {
        List<int> list = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
        if(list.Count > 0)
        {
            rowIndex = list[GetRandomNext(0, list.Count)];
        }
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 3;
        }
    }

    private void FindR1(out int rowIndex)
    {
        rowIndex = 3; // 默认值
        List<int> rowList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        List<int> list = new List<int>();
        int max = int.MinValue;
        foreach (var index in rowList)
        {
            int count = 0;
            foreach (var unit in GameController.Instance.GetSpecificRowEnemyList(index))
            {
                // 如果目标在传入的条件判断中为真，则计数+1
                if (unit is MouseUnit && !(unit as MouseUnit).IsBoss() && MouseManager.IsGeneralMouse(unit))
                    count++;
            }
            if (count == max)
            {
                list.Add(index);
            }
            else if (count > max)
            {
                list.Clear();
                list.Add(index);
                max = count;
            }
        }

        if (list.Count > 0)
        {
            rowIndex = list[GetRandomNext(0, list.Count)];
        }
        else
            GetRandomNext(0, 1);
    }

    private void FindP2_0(out int rowIndex, out int colIndex)
    {
        List<BaseUnit> list = new List<BaseUnit>();
        float max = float.MinValue;
        for (int i = 0; i < 7; i++)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(i, float.MinValue, MapManager.GetColumnX(8), false);
            if (unit != null)
            {
                if (unit.transform.position.x > max)
                {
                    max = unit.transform.position.x;
                    list.Clear();
                    list.Add(unit);
                }
                else if (unit.transform.position.x == max)
                {
                    list.Add(unit);
                }
            }
        }
        // 在这些行里随机挑一行吧
        if (list.Count > 0)
        {
            BaseUnit unit = list[GetRandomNext(0, list.Count)];
            rowIndex = unit.GetRowIndex();
            colIndex = unit.GetColumnIndex();
        }
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 3;
            colIndex = 8;
        }
    }

    private void FindP2_1(out int rowIndex, out int colIndex)
    {
        List<BaseUnit> list = new List<BaseUnit>();
        float min = float.MaxValue;
        for (int i = 0; i < 7; i++)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(i, float.MinValue, MapManager.GetColumnX(8), false);
            if (unit != null)
            {
                if (unit.transform.position.x < min)
                {
                    min = unit.transform.position.x;
                    list.Clear();
                    list.Add(unit);
                }
                else if (unit.transform.position.x == min)
                {
                    list.Add(unit);
                }
            }
        }
        // 在这些行里随机挑一行吧
        if (list.Count > 0)
        {
            BaseUnit unit = list[GetRandomNext(0, list.Count)];
            rowIndex = unit.GetRowIndex();
            colIndex = unit.GetColumnIndex();
        }
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 3;
            colIndex = 8;
        }
    }

    private void FindR3(out int rowIndex)
    {
        List<int> list = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
        if (list.Count > 0)
        {
            rowIndex = list[GetRandomNext(0, list.Count)];
        }
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 3;
        }
    }
    #endregion

    #region 一技能
    private CompoundSkillAbility Skill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int moveTime0 = Mathf.FloorToInt(GetParamValue("move_time0") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0") * 60);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
        };
        {
            // 移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    int rowIndex;
                    FindR0(out rowIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(8f, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait0, out task);
                    return task;
                });
            }

            // 施放旋风
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Wind", out task);
                    task.AddOnExitAction(delegate {
                        CreateS0Wind();
                        CreateS0WindArea();
                    });
                    return task;
                }); 
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private BaseUnit CreateS0Wind()
    {
        int attackCountLeft = Mathf.FloorToInt(GetParamValue("num0"));
        float v = TransManager.TranToVelocity(GetParamValue("v0"));

        MouseModel m = MouseModel.GetInstance(Wind_Run);
        m.transform.position = transform.position;
        m.SetBaseAttribute(100, 10, 1.0f, 0, 100, 0.5f, 0);
        m.SetMoveRoate(Vector2.left);
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.canDrivenAway = false;
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.MoveClipName = "Idle";
        m.AttackClipName = "Idle";
        m.DieClipName = "Disappear";
        m.SetActionState(new MoveState(m));
        SkyGridType.AddNoAffectBySky(m, new BoolModifier(true));
        GameController.Instance.AddMouseUnit(m);

        // 达到指定攻击次数后消失
        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            m.transform.position += Vector3.left * v;
            return attackCountLeft <= 0;
        });
        task.AddOnExitAction(delegate {
            m.ExecuteDeath();
        });
        m.AddTask(task);

        // 检测区域
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 0.5f, 0.5f, "CollideGrid");
            r.isAffectGrid = true;
            r.AddGridEnterConditionFunc((g) => {
                return attackCountLeft > 0;
            });
            r.SetOnGridEnterAction((g)=> {
                if (g.TakeAction(this, (u) => {
                    DamageAction action = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans") / 100).ApplyAction();
                }, false))
                {
                    attackCountLeft--;
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (m.IsAlive())
                {
                    r.transform.position = m.transform.position;
                    List<BaseGrid> delList = new List<BaseGrid>();
                    foreach (var g in r.gridList)
                    {
                        if (g.GetAttackableFoodUnitList().Count <= 0)
                            delList.Add(g);
                    }
                    foreach (var g in delList)
                    {
                        r.gridList.Remove(g);
                    }
                    return false;
                }
                return true;
            });
            t.AddOnExitAction(delegate
            {
                r.MDestory();
            });
            r.AddTask(t);
        };

        return m;
    }

    private void CreateS0WindArea()
    {
        float sv = TransManager.TranToVelocity(GetParamValue("v0"));
        // 添加风域
        WindAreaEffectExecution r = WindAreaEffectExecution.GetInstance(8.6f, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(GetRowIndex())));
        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time0") * 60), null, (lefttime, totaltime) => {
            float rate = 1 - (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("av_time0") * 60));
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time0") * 60), null, (lefttime, totaltime) => {
            float rate = (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddOnExitAction(delegate { r.MDestory(); });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
    }
    #endregion

    #region 二技能
    private CompoundSkillAbility Skill1Init(SkillAbility.SkillAbilityInfo info)
    {
        int moveTime0 = Mathf.FloorToInt(GetParamValue("move_time1_0") * 60);
        int moveTime1 = Mathf.FloorToInt(GetParamValue("move_time1_1") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait1_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait1_1") * 60);
        float f_colIndex = 9 - GetParamValue("right_col1");

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
        };
        {
            // 移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    int rowIndex;
                    FindR1(out rowIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(8.5f, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                    return task;
                });
            }

            // 使用钩子
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreHook", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("HookIdle", true);
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait0, out task);
                    return task;
                });
            }

            // 推进移动
            {
                c.AddCreateTaskFunc(delegate {
                    RetangleAreaEffectExecution r = null;
                    CustomizationTask task;
                    Vector2 pos = new Vector2(MapManager.GetColumnX(f_colIndex), transform.position.y);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime1, out task);
                    task.AddOnEnterAction(delegate { r = CreateS1PushArea(); });
                    task.AddOnExitAction(delegate {
                        if (r != null)
                            r.MDestory();
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostHook", out task);
                    return task;
                });
            }

            // 放风，停滞
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Wind", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Idle", true);
                        CreateS1WindArea();
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                    return task;
                });
            }

        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private RetangleAreaEffectExecution CreateS1PushArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance((Vector2)transform.position + 0.5f * new Vector2(MapManager.gridWidth, 0), 0.1f, 0.5f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.AddEnemyEnterConditionFunc((m) => {
            return MouseManager.IsGeneralMouse(m) && !m.IsBoss();
        });
        r.SetOnEnemyStayAction((m) => { 
            if(m.transform.position.x > r.transform.position.x)
            {
                m.transform.position = new Vector2(r.transform.position.x, m.transform.position.y);
            }
        });

        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate
        {
            r.transform.position = (Vector2)transform.position + 0.5f * new Vector2(MapManager.gridWidth, 0);
            return !IsAlive();
        });
        t.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
        return r;
    }

    private void CreateS1WindArea()
    {
        float sv = TransManager.TranToVelocity(GetParamValue("v1"));
        // 添加风域
        WindAreaEffectExecution r = WindAreaEffectExecution.GetInstance(8.6f, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(GetRowIndex())));
        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time1") * 60), null, (lefttime, totaltime) => {
            float rate = 1 - (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("av_time1") * 60));
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time1") * 60), null, (lefttime, totaltime) => {
            float rate = (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddOnExitAction(delegate { r.MDestory(); });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
    }
    #endregion

    #region 三技能
    private CompoundSkillAbility Skill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int moveTime0 = Mathf.FloorToInt(GetParamValue("move_time2_0") * 60);
        int moveTime1 = Mathf.FloorToInt(GetParamValue("move_time2_1") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2") * 60);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
        };
        {
            // 移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    int rowIndex;
                    int colIndex;
                    FindP2_0(out rowIndex, out colIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(colIndex + 1, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait0, out task);
                    return task;
                });
            }

            // 开枪移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    RetangleAreaEffectExecution r = null;
                    int rowIndex;
                    int colIndex;
                    FindP2_1(out rowIndex, out colIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(colIndex + 1, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime1, out task);
                    task.AddOnEnterAction(delegate { 
                        animatorController.Play("Attack", true);
                        r = CreateS2ShotArea();
                    });
                    task.AddOnExitAction(delegate { 
                        if(r != null)
                            r.MDestory();
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private RetangleAreaEffectExecution CreateS2ShotArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance((Vector2)transform.position - new Vector2(MapManager.gridWidth, 0), 0.75f, 0.75f, "EnemyAllyGrid");
        r.isAffectGrid = true;
        r.isAffectMouse = true;

        int interval = 15;
        int timeLeft = interval;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            r.transform.position = (Vector2)transform.position - new Vector2(MapManager.gridWidth, 0);
            timeLeft--;
            if(timeLeft <= 0)
            {
                timeLeft += interval;
                foreach (var g in r.gridList)
                {
                    bool flag = g.TakeAction(this, (u)=> {
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, u, GetParamValue("dmg2")).ApplyAction();
                    }, false);
                    // 如果没有可生效的目标，则击破云层与载具！
                    if (!flag)
                    {
                        // TODO 击破云层

                        // 击破载具
                        FoodUnit f = g.GetFoodByTag(FoodInGridType.LavaVehicle);
                        if (f != null)
                            f.ExecuteDeath();
                    }
                }
                // 攻击范围内的老鼠
                foreach (var m in r.mouseUnitList)
                {
                    if (m.IsAlive() && !m.IsBoss())
                        UnitManager.Execute(this, m);
                }

                // 生成两道特效
                {
                    for (int i = -1; i <= 1; i+=2)
                    {
                        BaseEffect e = BaseEffect.CreateInstance(ShotEffect_Run, null, "Idle", null, false);
                        e.transform.position = (Vector2)r.transform.position + new Vector2(0.375f*MapManager.gridWidth*i, 0);
                        GameController.Instance.AddEffect(e);
                    }
                }
            }
            return !IsAlive();
        });
        t.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
        return r;
    }
    #endregion

    #region 四技能
    private CompoundSkillAbility Skill3Init(SkillAbility.SkillAbilityInfo info)
    {
        int moveTime0 = Mathf.FloorToInt(GetParamValue("move_time3_0") * 60);
        int moveTime1 = Mathf.FloorToInt(GetParamValue("move_time3_1") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait3_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait3_1") * 60);
        float move_dist = GetParamValue("dash3") * MapManager.gridWidth;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
        };
        {
            // 移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    int rowIndex;
                    FindR3(out rowIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(8, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait0, out task);
                    return task;
                });
            }

            // 俯冲
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    BaseEffect e = null;
                    Vector2 pos = new Vector2(transform.position.x - move_dist, transform.position.y);
                    float v = ((Vector2)transform.position - pos).magnitude / moveTime1;
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime1, out task);
                    task.AddOnEnterAction(delegate {
                        task.taskController.AddTask(CreateS3Task());
                        CreateS3WindArea();

                        e = BaseEffect.CreateInstance(TailAir_Run, "Appear", "Idle", "Disappear", true);
                        GameController.Instance.AddEffect(e);
                        mEffectController.AddEffectToDict("LieutenantHumTailAir", e, Vector2.zero);
                    });
                    task.AddTimeTaskFunc(30, delegate {
                        if (e != null)
                            e.ExecuteDeath();
                    }, (leftTime, totalTime) => {
                        float rate = (float)leftTime / totalTime;
                        transform.position += Vector3.left * v * rate;
                    }, delegate { mEffectController.RemoveEffectFromDict("LieutenantHumTailAir"); });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void CreateS3WindArea()
    {
        float sv = TransManager.TranToVelocity(GetParamValue("v3"));
        // 添加风域
        WindAreaEffectExecution r = WindAreaEffectExecution.GetInstance(8.6f, 3, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(GetRowIndex())));
        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time3") * 60), null, (lefttime, totaltime) => {
            float rate = 1 - (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("av_time3") * 60));
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("acc_time3") * 60), null, (lefttime, totaltime) => {
            float rate = (float)lefttime / totaltime;
            r.SetVelocity(-sv * rate);
        }, null);
        t.AddOnExitAction(delegate { r.MDestory(); });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
    }

    private ITask CreateS3Task()
    {
        float fog_distInterval = MapManager.gridWidth;
        float summon_distInterval = GetParamValue("dist3") * MapManager.gridWidth;
        int type = Mathf.FloorToInt(GetParamValue("type3"));
        int shape = Mathf.FloorToInt(GetParamValue("shape3"));

        float fog_distLeft = fog_distInterval;
        float summon_distLeft = summon_distInterval;
        Vector2 LastPosition = Vector2.zero;
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            LastPosition = transform.position;
        });
        t.AddTaskFunc(delegate {
            float dist = ((Vector2)transform.position - LastPosition).magnitude;
            LastPosition = transform.position;
            fog_distLeft -= dist;
            if(fog_distLeft <= 0)
            {
                CreateS3Fog((Vector2)transform.position + fog_distLeft*Vector2.left);
                fog_distLeft += fog_distInterval;
            }

            summon_distLeft -= dist;
            if(summon_distLeft <= 0)
            {
                Vector2 pos = transform.position;
                int start_i = -1;
                int end_i = 1;
                int currentRowIndex = GetRowIndex();
                if(currentRowIndex == 0)
                {
                    end_i = -1;
                }else if(currentRowIndex == 6)
                {
                    start_i = 1;
                }
                for (int i = start_i; i <= end_i; i+=2)
                {
                    SpawnS3Enemy(pos, pos + new Vector2(0, MapManager.gridHeight * i), Vector2.left, type, shape);
                }
                summon_distLeft += summon_distInterval;
            }
            return false;
        });
        return t;
    }

    private void CreateS3Fog(Vector2 pos)
    {
        FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos);
        e.SetOpen();
        CustomizationTask fog_task = new CustomizationTask();
        fog_task.AddOnEnterAction(delegate { e.transform.position = pos; });
        fog_task.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("fog_time3")*60));
        fog_task.AddOnExitAction(delegate { e.SetDisappear(); });
        e.AddTask(fog_task);
        GameController.Instance.AddAreaEffectExecution(e);
    }

    private void SpawnS3Enemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.moveRotate = rot;
        if (m.moveRotate.x > 0)
            m.transform.localScale = new Vector2(-1, 1);
        // 一些初始出现动画不能被击中的效果
        Func<BaseUnit, BaseUnit, bool> noSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        CustomizationTask task = new CustomizationTask();
        int totalTime = 90;
        int currentTime = 0;
        task.AddOnEnterAction(delegate
        {
            m.transform.position = startV2; // 对初始坐标进行进一步修正
            m.AddCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc); // 不可作为选取的目标
            m.AddCanBlockFunc(noBlockFunc); // 不可被阻挡
            m.AddCanHitFunc(noHitFunc); // 不可被子弹击中
            m.SetAlpha(0); // 0透明度
        });
        task.AddTaskFunc(delegate
        {
            if (currentTime <= totalTime)
            {
                currentTime++;
                float t = (float)currentTime / totalTime;
                m.SetPosition(Vector2.Lerp(startV2, endV2, t));
                m.SetAlpha(t);
                return false;
            }
            return true;
        });
        task.AddOnExitAction(delegate
        {
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
        });
        m.AddTask(task);
    }
    #endregion
}
