using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

public class MistyJulie : BossUnit
{
    private static RuntimeAnimatorController Missile_Run;

    public override void Awake()
    {
        if (Missile_Run == null)
            Missile_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/10/Missile");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        mHeight = 1;
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
            c.AddCreateTaskFunc(delegate{
                CustomizationTask t;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Appear", out t);
                t.AddTaskFunc(delegate {
                    animatorController.Play("Idle", true);
                    return true;
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
    public override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.MistyJulie, 0))
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
        list.Add(SKill0Init(infoList[0]));
        list.Add(SKill1Init(infoList[1]));
        list.Add(SKill2Init(infoList[2]));
        list.Add(SKill1Init(infoList[1]));
        list.Add(SKill3Init(infoList[3]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void CreateFog(Vector2 pos, int fog_alive_time)
    {
        FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos);
        e.SetOpen();
        CustomizationTask fog_task = new CustomizationTask();
        fog_task.AddOnEnterAction(delegate { e.transform.position = pos; });
        fog_task.AddTimeTaskFunc(fog_alive_time);
        fog_task.AddOnExitAction(delegate { e.SetDisappear(); });
        e.AddTask(fog_task);
        GameController.Instance.AddAreaEffectExecution(e);
    }

    /// <summary>
    /// 制造迷雾
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("wait1"));
        int moveTime0 = Mathf.FloorToInt(60 * GetParamValue("move_time0_0"));
        int moveTime1 = Mathf.FloorToInt(60 * GetParamValue("move_time0_1"));
        int fog_alive_time = Mathf.FloorToInt(60 * GetParamValue("fog_alive_time0"));
        int startRowIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col0"));
        int interval = moveTime1 / 6;
        // 变量
        int timeLeft = 0;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            animatorController.Play("Idle", true);
        };
        // 移动到第一行
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(8, 0);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Fog", true);
                });
                return task;
            });
        }

        // 移动到第七行
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(8, 6);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime1, out task);
                // 添加一个检测移动然后放雾的任务
                {
                    CustomizationTask t = new CustomizationTask();
                    for (int _i = 0; _i < 7; _i++)
                    {
                        int i = _i;
                        t.AddTaskFunc(delegate {
                            timeLeft--;
                            if (timeLeft<=0)
                            {
                                for (int rowIndex = startRowIndex; rowIndex <= 9; rowIndex++)
                                {
                                    CreateFog(MapManager.GetGridLocalPosition(rowIndex, i), fog_alive_time);
                                }
                                timeLeft += interval;
                                return true;
                            }
                            return false;
                        });
                    }
                    task.taskController.AddTask(t);
                }
                
                return task;
            });
        }

        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(waitTime, out task);
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 燃烧弹
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("wait_time1"));
        int moveTime0 = Mathf.FloorToInt(60 * GetParamValue("move_time1_0"));
        int moveTime1 = Mathf.FloorToInt(60 * GetParamValue("move_time1_1"));
        int fog_alive_time = Mathf.FloorToInt(60 * GetParamValue("fog_alive_time1"));
        int spr_num = Mathf.FloorToInt(GetParamValue("num1"));

        // 变量
        int rowIndex = 3;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            rowIndex = 3;
            animatorController.Play("Idle", true);
        };
        // 移动到第R1行
        {
            c.AddAction(delegate {
                FindR1(out rowIndex);
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(8, rowIndex);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime0, out task);
                return task;
            });
        }

        // 落地，等待，然后升空
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Land", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle1", true);
                    mHeight = 0;
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(waitTime, out task);
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Takeoff", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle", true);
                    mHeight = 1;
                });
                return task;
            });
        }

        // 移动到同一行右五列
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(4, rowIndex);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime1, out task);
                return task;
            });
        }

        // 投放燃烧弹
        {
            c.AddCreateTaskFunc(delegate {
                bool flag = true;
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("CauseFire");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        return true;
                    else if(flag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5f)
                    {
                        CreateMissile(transform.position, fog_alive_time, spr_num);
                        flag = false;
                    }
                    return false;
                });
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle", true);
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void FindR1(out int rowIndex)
    {
        rowIndex = 3; // 默认值
        int colIndex = 4;
        List<BaseGrid> list = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(),
            MapManager.GetColumnX(colIndex - 0.5f), MapManager.GetColumnX(colIndex + 0.5f), MapManager.GetRowY(-0.5f), MapManager.GetRowY(6.5f));
        if(list.Count < 0)
            return;
        List<BaseGrid> list2 = GridManager.GetGridListWhichHasMaxCondition(list, (g) => {
            float hp = float.MinValue;
            foreach (var u in g.GetAttackableFoodUnitList())
            {
                if (!UnitManager.CanBeSelectedAsTarget(this, u))
                    continue;
                if (u.mCurrentHp > hp)
                    hp = u.mCurrentHp;
            }
            return hp;
        });
        if (list2.Count < 0)
            return;
        BaseGrid g = list2[GetRandomNext(0, list2.Count)];
        rowIndex = g.GetRowIndex();
    }

    private void CreateMissile(Vector2 pos, int fog_alive_time, int spr_count)
    {
        BaseEffect e = BaseEffect.CreateInstance(Missile_Run, null, "Drop", null, false);
        e.SetSpriteRendererSorting("Unit", LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, MapManager.GetYIndex(pos.y), 5, 0));
        Vector2 startPos = pos + Vector2.up * MapManager.gridHeight * 0.5f;
        e.transform.position = startPos;
        e.SetSpriteRight(Vector2.down);
        e.AddBeforeDeathAction(delegate {
            CreateFire(pos, Vector2.right, fog_alive_time, spr_count);
            CreateFire(pos, Vector2.left, fog_alive_time, spr_count);
            CreateFire(pos, Vector2.up, fog_alive_time, spr_count);
            CreateFire(pos, Vector2.down, fog_alive_time, spr_count);
        });
        GameController.Instance.AddEffect(e);

        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(15, null, (leftTime, totalTime)=> {
            float rate = Mathf.Pow(1 - (float)leftTime / totalTime, 2);
            e.transform.position = Vector2.Lerp(startPos, pos, rate);
        }, null);
        e.AddTask(t);
    }

    private void CreateFire(Vector2 pos, Vector2 rot, int fog_alive_time, int spr_count)
    {
        if (pos.x < MapManager.GetColumnX(-0.5f) && pos.x > MapManager.GetColumnX(8.5f) && pos.y > MapManager.GetRowY(-0.5f) && pos.y < MapManager.GetRowY(6.5f))
            return;

        BaseEffect e = EffectManager.GetFireEffect(false);
        e.transform.position = pos;
        GameController.Instance.AddEffect(e);

        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(15);
        t.AddOnExitAction(delegate {
            CreateFog(pos, fog_alive_time);
            // 火焰扩散一次
            if(spr_count > 0)
            {
                CreateFire(pos + new Vector2(rot.x * MapManager.gridWidth, rot.y * MapManager.gridHeight), rot, fog_alive_time, spr_count - 1);
            }
        });
        e.AddTask(t);

        float dmg = GetParamValue("dmg1");
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "BothCollide");
        r.SetInstantaneous();
        r.SetAffectHeight(0);
        r.isAffectFood = true;
        r.SetOnFoodEnterAction((u)=>{
            new BombDamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
        });
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u) => {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 恢复信号
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int moveTime = Mathf.FloorToInt(60 * GetParamValue("move_time2"));
        int healTime = Mathf.FloorToInt(60 * GetParamValue("t2_0"));
        float heal_percent = GetParamValue("heal2_0")*0.01f;

        // 变量
        int rowIndex = 3;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            rowIndex = 3;
            animatorController.Play("Idle", true);
        };
        // 移动到第R2行
        {
            c.AddAction(delegate {
                FindR2(out rowIndex);
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(6, rowIndex);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime, out task);
                return task;
            });
        }

        // 落地
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Land", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle1", true);
                    mHeight = 0;
                });
                return task;
            });
        }

        // 架起信号塔，回血
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreCast", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Cast", true);
                });
                return task;
            });

            c.AddCreateTaskFunc(delegate {
                int interval = 60;
                int timeLeft = interval;
                CustomizationTask task = new CustomizationTask();
                task.AddTimeTaskFunc(healTime, null, delegate {
                    timeLeft--;
                    if(timeLeft <= 0)
                    {
                        foreach (var unit in GameController.Instance.GetEachEnemy())
                        {
                            // 必须是老鼠单位
                            if (!MouseManager.IsGeneralMouse(unit))
                                continue;
                            // 对BOSS单位无效
                            if (unit is MouseUnit && (unit as MouseUnit).IsBoss())
                                continue;
                            // 需要目标存活
                            if (!unit.IsAlive())
                                continue;
                            // 检测治疗有没有超过上限
                            float realAdd = unit.GetRealCureValue(unit.mMaxHp*heal_percent);
                            float leftAdd = realAdd - unit.mMaxHp + unit.mCurrentHp;
                            if (leftAdd > 0)
                            {
                                // 当治疗溢出时，剩余的治疗量转化为护盾
                                new CureAction(CombatAction.ActionType.GiveCure, this, unit, unit.mMaxHp - unit.mCurrentHp).ApplyAction();
                                new ShieldAction(CombatAction.ActionType.GiveShield, this, unit, leftAdd).ApplyAction();
                            }
                            else
                            {
                                // 未溢出时全额转化为治疗
                                new CureAction(CombatAction.ActionType.GiveCure, this, unit, realAdd).ApplyAction();
                            }
                            // 回复特效添加给单位上
                            EffectManager.AddHealEffectToUnit(unit);
                        }
                        timeLeft += interval;
                    }
                }, null);
                return task;
            });

            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostCast", out task);
                return task;
            });
        }

        // 升空
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Takeoff", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle", true);
                    mHeight = 1;
                });
                return task;
            });
        }

        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void FindR2(out int rowIndex)
    {
        rowIndex = 3; // 默认值
        List<int> list = FoodManager.GetRowListWhichHasMinConditionAllyCount((unit) =>
        {
            if (unit is FoodUnit && UnitManager.CanBeSelectedAsTarget(this, unit))
            {
                FoodUnit f = unit as FoodUnit;
                return FoodManager.IsAttackableFoodType(f);
            }
            return false;
        });
        if (list.Count > 0)
        {
            rowIndex = list[GetRandomNext(0, list.Count)];
        }
        else
            GetRandomNext(0, 1);
    }

    /// <summary>
    /// 支援信号
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill3Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int moveTime = Mathf.FloorToInt(60 * GetParamValue("move_time3"));
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("wait_time3"));
        int buffTime = Mathf.FloorToInt(60 * GetParamValue("t3_0"));
        FloatModifier AttackMod = new FloatModifier(GetParamValue("add_attack_percent3"));
        FloatModifier AttackSpeedMod = new FloatModifier(GetParamValue("add_attackSpeed_percent3"));
        FloatModifier DefenceMod = new FloatModifier(1-0.01f*GetParamValue("add_defence_percent3"));

        // 变量
        int rowIndex = 3;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            rowIndex = 3;
            animatorController.Play("Idle", true);
        };
        // 移动到第R3行
        {
            c.AddAction(delegate {
                FindR3(out rowIndex);
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                Vector2 pos = MapManager.GetGridLocalPosition(7, rowIndex);
                CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime, out task);
                return task;
            });
        }

        // 落地
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Land", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle1", true);
                    mHeight = 0;
                });
                return task;
            });
        }

        // 架起信号塔，强化
        {
            c.AddCreateTaskFunc(delegate {
                bool flag = true;
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Summon");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        return true;
                    else if (flag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.63f)
                    {
                        foreach (var unit in GameController.Instance.GetEachEnemy())
                        {
                            // 必须是老鼠单位
                            if (!MouseManager.IsGeneralMouse(unit))
                                continue;
                            // 对BOSS单位无效
                            if (unit is MouseUnit && (unit as MouseUnit).IsBoss())
                                continue;
                            // 需要目标存活
                            if (!unit.IsAlive())
                                continue;

                            CustomizationTask t = new CustomizationTask();
                            t.AddOnEnterAction(delegate {
                                unit.NumericBox.Attack.AddPctAddModifier(AttackMod);
                                unit.NumericBox.AttackSpeed.AddPctAddModifier(AttackSpeedMod);
                                unit.NumericBox.DamageRate.AddModifier(DefenceMod);
                            });
                            t.AddTimeTaskFunc(buffTime);
                            t.AddOnExitAction(delegate {
                                unit.NumericBox.Attack.RemovePctAddModifier(AttackMod);
                                unit.NumericBox.AttackSpeed.RemovePctAddModifier(AttackSpeedMod);
                                unit.NumericBox.DamageRate.RemoveModifier(DefenceMod);
                            });
                            unit.AddTask(t);
                        }
                        flag = false;
                    }
                    return false;
                });
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle1", true);
                });
                return task;
            });
        }

        // 停滞
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(waitTime, out task);
                return task;
            });
        }

        // 升空
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Takeoff", out task);
                task.AddOnExitAction(delegate {
                    animatorController.Play("Idle", true);
                    mHeight = 1;
                });
                return task;
            });
        }

        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void FindR3(out int rowIndex)
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
}
