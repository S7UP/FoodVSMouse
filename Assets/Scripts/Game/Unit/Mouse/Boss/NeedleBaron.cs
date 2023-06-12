using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

/// <summary>
/// 针头男爵
/// </summary>
public class NeedleBaron : BossUnit
{
    private FloatModifier DecDamageMod; // 自身减伤标签
    private FloatModifier AttackMod; // 攻击力标签
    private FloatModifier AttackSpeedMod; // 攻速标签
    private FloatModifier MoveSpeedMod; // 移动速度标签
    private static BoolModifier IgnoreDownMod = new BoolModifier(true); // 免疫控制与减速标签
    private static RuntimeAnimatorController Curse_Run;
    private static RuntimeAnimatorController Bat_Run;
    private static RuntimeAnimatorController Needle_Run;
    private static RuntimeAnimatorController Break_Run;

    private const string Cursekey = "NeedleBaron_Curse";
    private const string OccupyKey = "NeedleBaron_Bat_Occupy";

    private int S2Count; // 崩坏的使用次数
    private List<BaseUnit> curseUnitList = new List<BaseUnit>();
    private List<BaseUnit> batUnitList = new List<BaseUnit>();

    public override void Awake()
    {
        DecDamageMod = new FloatModifier(0);
        AttackMod = new FloatModifier(0);
        AttackSpeedMod = new FloatModifier(0);
        MoveSpeedMod = new FloatModifier(0);
        if(Curse_Run == null)
        {
            Curse_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/9/Curse");
            Bat_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/9/Bat");
            Needle_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/9/Needle");
            Break_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/9/Break");
        }
        base.Awake();
    }

    public override void MInit()
    {
        DecDamageMod = new FloatModifier(0);
        AttackMod = new FloatModifier(0);
        AttackSpeedMod = new FloatModifier(0);
        MoveSpeedMod = new FloatModifier(0);
        curseUnitList.Clear();
        batUnitList.Clear();
        S2Count = 0;
        base.MInit();
        // 添加出现的技能
        {
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            Func<BaseUnit, BaseUnit, bool> noSelcetedFunc = delegate { return false; };

            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 60;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Idle");
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
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
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in curseUnitList)
        {
            if(!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            curseUnitList.Remove(u);
        }

        delList.Clear();
        foreach (var bat in batUnitList)
        {
            if (!bat.IsAlive())
                delList.Add(bat);
        }
        foreach (var bat in delList)
        {
            batUnitList.Remove(bat);
        }

        base.MUpdate();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.NeedleBaron, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public override void AfterDeath()
    {
        foreach (var bat in batUnitList)
        {
            bat.DeathEvent();
        }
        base.AfterDeath();
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
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
        UpdatePassive();
    }

    /// <summary>
    /// 更新自身被动技能
    /// </summary>
    private void UpdatePassive()
    {
        NumericBox.DamageRate.RemoveModifier(DecDamageMod);
        DecDamageMod.Value = 1 - 0.01f * GetParamValue("dec_dmg_percent");
        NumericBox.DamageRate.AddModifier(DecDamageMod);
        AttackMod.Value = GetParamValue("add_attack_percent");
        AttackSpeedMod.Value = GetParamValue("add_attackSpeed_percent");
        MoveSpeedMod.Value = GetParamValue("add_moveSpeed_percent");
        // 刷新一次所操控单位BUFF
        foreach (var u in curseUnitList)
        {
            u.NumericBox.Attack.RemovePctAddModifier(AttackMod);
            u.NumericBox.AttackSpeed.RemovePctAddModifier(AttackSpeedMod);
            u.NumericBox.MoveSpeed.RemovePctAddModifier(MoveSpeedMod);
            u.NumericBox.DamageRate.RemoveModifier(DecDamageMod);

            u.NumericBox.Attack.AddPctAddModifier(AttackMod);
            u.NumericBox.AttackSpeed.AddPctAddModifier(AttackSpeedMod);
            u.NumericBox.MoveSpeed.AddPctAddModifier(MoveSpeedMod);
            u.NumericBox.DamageRate.AddModifier(DecDamageMod);
        }
    }

    /// <summary>
    /// 为某个单位添加血族仆从效果
    /// </summary>
    /// <param name="unit"></param>
    private void AddCurse(BaseUnit unit)
    {
        if (IsCurse(unit) || !MouseManager.IsGeneralMouse(unit) || (unit is MouseUnit && (unit as MouseUnit).IsBoss()))
            return;

        // 添加特效
        BaseEffect e = BaseEffect.CreateInstance(Curse_Run, "Appear", "Idle", "Disappear", true);
        GameController.Instance.AddEffect(e);
        unit.mEffectController.AddEffectToDict(Cursekey, e, Vector2.zero);
        // 添加BUFF
        unit.NumericBox.Attack.AddPctAddModifier(AttackMod);
        unit.NumericBox.AttackSpeed.AddPctAddModifier(AttackSpeedMod);
        unit.NumericBox.MoveSpeed.AddPctAddModifier(MoveSpeedMod);
        unit.NumericBox.DamageRate.AddModifier(DecDamageMod);
        // 目标被炸了会直接被处决
        unit.AddActionPointListener(ActionPointType.PreReceiveDamage, BombBurnInstanceKillAction);
        // 立即移除当前的定身效果与减速效果
        StatusManager.RemoveAllSettleDownDebuff(unit);
        StatusManager.RemoveAllSlowDownDebuff(unit);
        // 获得减速类debuff与定身类debuff免疫效果
        StatusManager.AddIgnoreSettleDownBuff(unit, IgnoreDownMod);
        StatusManager.AddIgnoreSlowDownBuff(unit, IgnoreDownMod);
        // 每秒损失生命值
        int interval = 60;
        int timeLeft = interval;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            timeLeft--;
            if(timeLeft <= 0)
            {
                DamageAction ac = new DamageAction(CombatAction.ActionType.RealDamage, null, unit, 0.01f*GetParamValue("lost_hp_percent") *unit.mMaxHp);
                ac.ApplyAction();
                new CureAction(CombatAction.ActionType.GiveCure, unit, this, ac.RealCauseValue).ApplyAction();
                timeLeft += interval;
            }
            return false;
        });
        unit.AddTask(t);
        curseUnitList.Add(unit);
    }

    private bool IsCurse(BaseUnit unit)
    {
        return unit.mEffectController.IsContainEffect(Cursekey) || curseUnitList.Contains(unit);
    }

    private void BombBurnInstanceKillAction(CombatAction action)
    {
        if (action is DamageAction)
        {
            DamageAction d = (action as DamageAction);
            if (d.IsDamageType(DamageAction.DamageType.BombBurn))
            {
                new DamageAction(CombatAction.ActionType.BurnDamage, null, d.Target, d.Target.mCurrentHp).ApplyAction();
            }
        }
    }

    /// <summary>
    /// 产生一只啮齿蝠
    /// </summary>
    /// <param name="g"></param>
    private MouseUnit CreateBat(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(Bat_Run);
        m.transform.position = pos;
        m.SetBaseAttribute(GetParamValue("bat_hp"), 10, 1, 0, 0, 0.5f, 1);
        m.AddActionPointListener(ActionPointType.PreReceiveDamage, BombBurnInstanceKillAction);
        m.animatorController.Play("Move", true);
        m.typeAndShapeValue = 5;
        GameController.Instance.AddMouseUnit(m);

        CustomizationTask t = new CustomizationTask();
        t.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("recycle_time")*60));
        t.AddOnExitAction(delegate {
            m.SetActionState(CreateRecycleState(m));
        });
        m.AddTask(t);

        batUnitList.Add(m);

        return m;
    }

    private bool CanOccupy(BaseGrid g)
    {
        if (g == null || !g.IsAlive() || g.IsContainUnit(OccupyKey))
            return false;
        return true;
    }

    /// <summary>
    /// 尝试占据一个格子
    /// </summary>
    /// <param name="bat"></param>
    /// <param name="g"></param>
    /// <returns></returns>
    private bool TryOccupy(BaseUnit bat, BaseGrid g)
    {
        if (!CanOccupy(g))
            return false;

        bat.SetActionState(CreateOccupyState(bat, g));
        return true;
    }

    private bool CanAdsorpte(BaseUnit m)
    {
        if (IsCurse(m) || !(m is MouseUnit) || (m as MouseUnit).IsBoss() || m == null || !m.IsAlive() || m.IsContainUnit(OccupyKey) || !MouseManager.IsGeneralMouse(m) || !UnitManager.CanBeSelectedAsTarget(null, m) || m.mHeight < 0)
            return false;
        return true;
    }

    /// <summary>
    /// 尝试吸附一个单位
    /// </summary>
    /// <param name="bat"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool TryAdsorpte(BaseUnit bat, BaseUnit m)
    {
        if (!CanAdsorpte(m))
            return false;

        bat.SetActionState(CreateAdsorptionState(bat, m));
        AddCurse(m);
        return true;
    }

    /// <summary>
    /// 生成一个占据状态
    /// </summary>
    /// <returns></returns>
    private CustomizationState CreateOccupyState(BaseUnit bat, BaseGrid g)
    {
        int interval = 60;
        int timeLeft = interval;
        CustomizationState s = new CustomizationState();
        s.AddOnEnterAction(delegate {
            g.AddUnitToDict(OccupyKey, bat);
            bat.animatorController.Play("Idle", true);
        });
        s.AddOnUpdateAction(delegate {
            if (!g.IsAlive())
                bat.SetActionState(CreateRecycleState(bat));
            timeLeft--;
            bat.transform.position = g.transform.position;
            if(timeLeft <= 0)
            {
                timeLeft += interval;
                BaseUnit target = g.GetHighestAttackPriorityUnit(bat);
                if(target != null && !(target is CharacterUnit))
                {
                    DamageAction da = new DamageAction(CombatAction.ActionType.RealDamage, bat, target, GetParamValue("attack_percent") * 0.01f * target.mMaxHp);
                    da.ApplyAction();
                    // new CureAction(CombatAction.ActionType.GiveCure, bat, this, da.RealCauseValue).ApplyAction();
                }
            }
            // 检测该格有没有其他非BOSS敌方单位，如果有则吸附在它身上
            List<MouseUnit> mList = new List<MouseUnit>();
            foreach (var m in g.GetMouseUnitList())
            {
                if (!m.IsBoss())
                    mList.Add(m);
            }
            if (mList.Count > 0)
            {
                mList.Sort((m1, m2) => { return m1.transform.position.x.CompareTo(m2.transform.position.x); });
                foreach (var m in mList)
                {
                    if (TryAdsorpte(bat, m))
                        break;
                }
            }
        });
        s.AddOnExitAction(delegate {
            if(g.IsAlive())
                g.RemoveUnitFromDict(OccupyKey);
        });
        return s;
    }

    /// <summary>
    /// 生成一个吸附状态
    /// </summary>
    /// <param name="bat"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private CustomizationState CreateAdsorptionState(BaseUnit bat, BaseUnit target)
    {
        bool isCreateCheckRect = false;
        RetangleAreaEffectExecution r = null;
        CustomizationState s = new CustomizationState();
        s.AddOnEnterAction(delegate {
            target.AddUnitToDict(OccupyKey, bat);
            bat.animatorController.Play("Idle", true);
        });
        s.AddOnUpdateAction(delegate {
            if (!target.IsAlive())
            {
                // 如果被吸附目标已死亡
                if (!isCreateCheckRect)
                {
                    isCreateCheckRect = true;
                    r = RetangleAreaEffectExecution.GetInstance(bat.transform.position, 0.5f, 0.5f, "CollideGrid");
                    r.isAffectGrid = true;
                    r.AddGridEnterConditionFunc((g) => {
                        return !g.IsContainUnit(OccupyKey);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                else
                {
                    bool isSuccessOccupy = false;
                    if(r.gridList.Count > 0)
                    {
                        // 有的话就取一个最靠左上的格子
                        List<BaseGrid> l1 = new List<BaseGrid>();
                        BaseGrid leftGrid = r.gridList[0];
                        foreach (var g in r.gridList)
                        {
                            if(g.transform.position.x < leftGrid.transform.position.x)
                            {
                                leftGrid = g;
                                l1.Clear();
                                l1.Add(g);
                            }else if (g.transform.position.x == leftGrid.transform.position.x)
                            {
                                l1.Add(g);
                            }
                        }
                        if (l1.Count == 1)
                            isSuccessOccupy = TryOccupy(bat, l1[0]);
                        else
                        {
                            l1.Sort((g1, g2) => { return -g1.transform.position.y.CompareTo(g2.transform.position.y); });
                            foreach (var g in l1)
                            {
                                if(TryOccupy(bat, g))
                                {
                                    isSuccessOccupy = true;
                                    break;
                                }    
                            }
                        }
                        r.MDestory();
                    }
                    if(!isSuccessOccupy)
                    {
                        // 没有找到合适的格子的话就被回收
                        bat.SetActionState(CreateRecycleState(bat));
                    }
                }
            }
            else
            {
                // 如果吸附目标未死亡则跟随
                bat.transform.position = target.transform.position;
            }
        });
        s.AddOnExitAction(delegate {
            if(target.IsAlive())
                target.RemoveUnitFromDict(OccupyKey);
        });
        return s;
    }

    /// <summary>
    /// 生成一个被BOSS回收的状态
    /// </summary>
    /// <param name="bat"></param>
    /// <returns></returns>
    private CustomizationState CreateRecycleState(BaseUnit bat)
    {
        float v = TransManager.TranToVelocity(GetParamValue("recycle_velocity"));
        Vector2 currentPos = bat.transform.position;
        CustomizationState s = new CustomizationState();
        s.AddOnEnterAction(delegate {
            bat.animatorController.Play("Move", true);
        });
        s.AddOnUpdateAction(delegate {
            Vector2 rot = ((Vector2)transform.position - currentPos).normalized;
            currentPos += v*rot;
            if (rot.x > 0)
                bat.SetSpriteLocalScale(new Vector2(-1, 1));
            else
                bat.SetSpriteLocalScale(new Vector2(1, 1));
            bat.transform.position = currentPos;
            if(((Vector2)transform.position - currentPos).magnitude <= 2*v)
            {
                new CureAction(CombatAction.ActionType.GiveCure, bat, this, GetParamValue("heal_percent")*0.01f*mMaxHp).ApplyAction();
                EffectManager.AddHealEffectToUnit(this);
                bat.DeathEvent();
            }
        });
        return s;
    }

    private void FindC0(int rowIndex, out int colIndex)
    {
        int _colIndex = 9 - Mathf.FloorToInt(GetParamValue("left_col0"));
        colIndex = _colIndex;
        List<BaseUnit> list = UnitManager.GetSortedListByXPos(
            (u)=> {
                if (u.transform.position.x <= MapManager.GetColumnX(_colIndex) && u is FoodUnit && FoodManager.IsAttackableFoodType(u as FoodUnit) && UnitManager.CanBeSelectedAsTarget(this, u) && CanOccupy(u.GetGrid()))
                    return true;
                return false;
            }, GameController.Instance.GetSpecificRowAllyList(rowIndex));
        list.Reverse();

        if (list.Count > 0)
        {
            colIndex = Mathf.Min(colIndex, list[0].GetColumnIndex());
        }else
        {
            
            for (int i = colIndex; i >= 0; i--)
            {
                if (CanOccupy(GameController.Instance.mMapController.GetGrid(i, rowIndex)))
                {
                    colIndex = i;
                    break;
                }
            }
        }
    }

    private void Skill0Bat(Vector2 startPos, Vector2 endPos, int timeLeft)
    {
        FloatModifier xMod = new FloatModifier(0);
        FloatModifier yMod = new FloatModifier(0);
        int moveTime = 60;
        System.Random ran = GameManager.Instance.rand;
        Vector2 ranSpritePos = new Vector2(((float)ran.NextDouble() - 0.5f) * MapManager.gridWidth, ((float)ran.NextDouble() - 0.5f) * MapManager.gridHeight);
        MouseUnit m = CreateBat(startPos);
        m.CloseCollision();
        xMod.Value = ranSpritePos.x;
        yMod.Value = ranSpritePos.y;
        m.AddSpriteOffsetX(xMod);
        m.AddSpriteOffsetY(yMod);
        // 移动任务
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.animatorController.Play("Move", true);
            });
            t.AddTaskFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    timeLeft = moveTime;
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                timeLeft--;
                float rate = (1 - (float)timeLeft / moveTime);
                Vector2 SpritePos = Vector2.Lerp(ranSpritePos, Vector2.zero, rate);
                m.RemoveSpriteOffsetX(xMod);
                m.RemoveSpriteOffsetY(yMod);
                xMod.Value = SpritePos.x;
                yMod.Value = SpritePos.y;
                m.AddSpriteOffsetX(xMod);
                m.AddSpriteOffsetY(yMod);
                m.transform.position = Vector2.Lerp(startPos, endPos, rate);
                if (timeLeft <= 0)
                {
                    // 抵达目的地后尝试占据格子，同时打开判定
                    if(!TryOccupy(m, GameController.Instance.mMapController.GetGrid(m.GetColumnIndex(), m.GetRowIndex())))
                    {
                        // 如果占据失败则返回
                        m.SetActionState(CreateRecycleState(m));
                    }
                    m.OpenCollision();
                    return true;
                }
                return false;
            });
            m.AddTask(t);
        }
    }

    /// <summary>
    /// 召唤啮齿蝠
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int num = Mathf.FloorToInt(GetParamValue("num0", mHertIndex));
        int moveTime = Mathf.FloorToInt(60 * GetParamValue("t0_0", mHertIndex));
        int totalTime = Mathf.FloorToInt(60 * GetParamValue("t0_1", mHertIndex));
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("t0_2", mHertIndex));
        int interval = totalTime / num;
        // 变量
        int timeLeft = 0;
        List<int> rowIndexList = new List<int>();

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle");
        };
        {
            // 移动到中间
            {
                //Func<CustomizationTask> func;
                //CompoundSkillAbilityManager.GetCreateMoveToFunc(transform, MapManager.GetGridLocalPosition(8, 3), moveTime, out func);
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetMoveToTask(transform, MapManager.GetGridLocalPosition(8, 3), moveTime, out task);
                    return task;
                });
            }
            // 播放动画
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreDefence", out task);
                    return task;
                });
            }
            // 提前计算所召唤蝙蝠经过的行
            {
                c.AddAction(delegate {
                    animatorController.Play("Defence", true);
                    rowIndexList.Clear();
                    rowIndexList.Add(GetRowIndex());
                    int count = 1;
                    while (count <= 6)
                    {
                        int r = GetRowIndex() + count;
                        if (r >= 0 && r <= 6)
                        {
                            rowIndexList.Add(r);
                            if (rowIndexList.Count >= num)
                                break;
                        }
                        r = GetRowIndex() - count;
                        if (r >= 0 && r <= 6)
                        {
                            rowIndexList.Add(r);
                            if (rowIndexList.Count >= num)
                                break;
                        }
                        count++;
                    }
                });
            }
            // 一只只召唤蝙蝠
            for (int i = 0; i < num; i++)
            {
                int j = i;
                c.AddSpellingFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        timeLeft += interval;
                        int rowIndex = rowIndexList[j];
                        int colIndex;
                        FindC0(rowIndex, out colIndex);
                        Skill0Bat(transform.position, MapManager.GetGridLocalPosition(colIndex, rowIndex), totalTime - interval * j);
                        return true;
                    }
                    return false;
                });
            }
            // 播放动画
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostDefence", out task);
                    return task;
                });
                c.AddAction(delegate {
                    animatorController.Play("Idle");
                });
            }
            // 自身停滞
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(waitTime, out task);
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private BaseUnit FindS1Target()
    {
        BaseUnit target = null;
        // 先找老鼠
        foreach (var u in GameController.Instance.GetEachEnemy())
        {
            if(u is MouseUnit && u.IsAlive())
            {
                MouseUnit m = u as MouseUnit;
                if(!m.IsBoss() && CanAdsorpte(m))
                {
                    if(target == null || m.mMaxHp > target.mMaxHp)
                    {
                        target = m;
                    }
                }
            }
        }
        if (target != null)
            return target;
        // 再找美食
        foreach (var u in GameController.Instance.GetEachAlly())
        {
            if (u is FoodUnit && u.IsAlive())
            {
                FoodUnit f = u as FoodUnit;
                if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f))
                {
                    if (target == null || f.mMaxHp > target.mMaxHp)
                    {
                        target = f;
                    }
                }
            }
        }
        return target;
    }

    private void CreateS1Area(Vector2 pos)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 1, 0.5f, "BothCollide");
        r.SetAffectHeight(0);
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.AddEnemyEnterConditionFunc((m)=>{ return CanAdsorpte(m); });
        r.SetInstantaneous();
        r.AddBeforeDestoryAction(delegate {
            float dmg = 0;
            foreach (var f in r.foodUnitList)
            {
                dmg += UnitManager.Execute(this, f).RealCauseValue;
            }
            foreach (var m in r.mouseUnitList)
            {
                if (CanAdsorpte(m))
                {
                    float cureValue = m.GetLostHp();
                    new CureAction(CombatAction.ActionType.GiveCure, this, m, cureValue).ApplyAction();
                    EffectManager.AddHealEffectToUnit(m);
                    dmg += cureValue;

                    MouseUnit bat = CreateBat(m.transform.position);
                    if(!TryAdsorpte(bat, m))
                    {
                        bat.SetActionState(CreateRecycleState(bat));
                    }
                }
            }
            // 自身损失生命值
            dmg = Mathf.Min(dmg, GetCurrentHp()- 0.01f*GetParamValue("hp_percent1") *mMaxHp);
            if (dmg > 0)
                new DamageAction(CombatAction.ActionType.RealDamage, null, this, dmg).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 瘟疫异变
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("wait1"));
        int moveTime = Mathf.FloorToInt(60 * GetParamValue("move_time1"));
        int num = Mathf.FloorToInt(GetParamValue("num1"));
        // 变量

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Idle");
        };
        for (int _i = 0; _i < num; _i++)
        {
            int i = _i;
            // 移动到目标处
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    BaseUnit target = FindS1Target();
                    Vector2 pos = MapManager.GetGridLocalPosition(GetRandomNext(0, 9), GetRandomNext(0, 7));
                    if(target != null && target.IsAlive())
                        CompoundSkillAbilityManager.GetMoveToTask(transform, target, 12, moveTime, out task);
                    else
                        CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime, out task);
                    return task;
                });
            }
            
            // 吸血
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(80, out task);
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("Suck", true);
                        BaseEffect e = BaseEffect.CreateInstance(Needle_Run, "PreAttack", "Attack", null, true);
                        e.transform.position = transform.position;
                        CustomizationTask t = new CustomizationTask();
                        t.AddTaskFunc(delegate {
                            if (!animatorController.GetCurrentAnimatorStateRecorder().aniName.Equals("Suck"))
                                return true;
                            return false;
                        });
                        t.AddOnExitAction(delegate {
                            e.ExecuteDeath();
                        });
                        e.AddTask(t);
                        GameController.Instance.AddEffect(e);
                    });
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Idle", true);
                        CreateS1Area(transform.position);
                    });
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
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 崩坏
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int waitTime = Mathf.FloorToInt(60 * GetParamValue("wait_time2_0"));
        int moveTime = Mathf.FloorToInt(60 * GetParamValue("move_time2"));
        int waitTime2 = Mathf.FloorToInt(60 * GetParamValue("wait_time2_1"));
        int num = Mathf.FloorToInt(GetParamValue("num2"));
        // 变量
        Vector2 pos = Vector2.zero;
        Vector2 rot = Vector2.right;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        for (int _i = 0; _i < num; _i++)
        {
            int i = _i;
            // 移动到目标处
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        S2Count++;
                        if (S2Count % 2 == 1)
                        {
                            pos = MapManager.GetGridLocalPosition(8, 0);
                            rot = new Vector2(-1, -1).normalized;
                        }
                        else
                        {
                            pos = MapManager.GetGridLocalPosition(8, 6);
                            rot = new Vector2(-1, 1).normalized;
                        }
                        animatorController.Play("Idle");
                    });
                    return task;
                });

                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime, out task);
                    return task;
                });
            }
            // 发呆
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(waitTime, out task);
                    return task;
                });
            }
            // 崩坏动作
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Break", out task);
                    task.AddOnExitAction(delegate {
                        CreateS2BreakArea(rot);
                        animatorController.Play("Idle", true);
                    });
                    return task;
                });
            }
            // 结束等待
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddTimeTaskFunc(waitTime2);
                    return task;
                });
            }
        }

        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private RetangleAreaEffectExecution CreateS2BreakArea(Vector2 rotate)
    {
        Vector2 startPos = transform.position;
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(startPos, 0, 0.5f, "BothCollide");
        r.SetRight(rotate);
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.AddFoodEnterConditionFunc((f) => {
            return FoodManager.IsAttackableFoodType(f) && !SkyGridType.IsIgnoreDrop(f);
        });
        r.AddEnemyEnterConditionFunc((m) => {
            return m.GetHeight() <= 0 && !SkyGridType.IsIgnoreDrop(m) && !m.IsBoss();
        });
        r.SetOnFoodEnterAction((u) => { u.ExecuteDrop(); });
        r.SetOnEnemyEnterAction((u) => { u.ExecuteDrop(); });
        GameController.Instance.AddAreaEffectExecution(r);

        {
            BaseEffect e = BaseEffect.CreateInstance(Break_Run, "Attack", "Idle", "Disappear", true);
            e.SetSpriteRight(rotate);
            e.transform.position = startPos;
            GameController.Instance.AddEffect(e);

            int time = 35;
            int count = Mathf.FloorToInt(GetParamValue("count2"));
            int interval = time / count;
            int timeLeft = interval;
            CustomizationTask t = new CustomizationTask();
            t.AddTimeTaskFunc(time, null, (leftTime, totalTime) => {
                float rate = 1 - (float)leftTime/totalTime;
                float len = MapManager.gridWidth * 6 * rate;
                r.SetBoxCollider2D(Vector2.zero, new Vector2(len, MapManager.gridHeight * 0.5f));
                r.transform.position = startPos + rotate * len / 2;

                timeLeft--;
                if(timeLeft <= 0)
                {
                    MouseUnit bat = CreateBat(startPos + rotate * len);
                    bat.AddTask(GetS2BatTask(startPos + rotate * len, bat));
                    timeLeft += interval;
                }
            }, null);
            t.AddTimeTaskFunc(Mathf.FloorToInt(60 * GetParamValue("break_aliveTime2")), null, null, delegate {
                r.MDestory();
                e.ExecuteDeath();
            });
            e.AddTask(t);
        }
        return r;
    }

    private CustomizationTask GetS2BatTask(Vector2 pos, BaseUnit bat)
    {
        CustomizationTask task = new CustomizationTask();
        task.AddTimeTaskFunc(30, delegate {
            BaseGrid g = GameController.Instance.mMapController.GetGrid(MapManager.GetXIndex(pos.x), MapManager.GetYIndex(pos.y));
            if (!TryOccupy(bat, g))
            {
                bat.SetActionState(CreateRecycleState(bat));
            }
        }, (leftTime, totalTime) => { bat.SetAlpha(1-(float)leftTime/totalTime); }, null);
        return task;
    }
}
