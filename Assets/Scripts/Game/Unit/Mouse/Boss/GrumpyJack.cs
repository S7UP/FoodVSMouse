using S7P.Numeric;
using S7P.State;

using System;
using System.Collections.Generic;

using UnityEngine;

public class GrumpyJack : BossUnit
{
    private static RuntimeAnimatorController Body_Run;
    private static RuntimeAnimatorController FireBullet_Run;
    private static RuntimeAnimatorController Head_Run;
    private static RuntimeAnimatorController Heart_Run;

    private List<BaseUnit> bodyList = new List<BaseUnit>();
    private int count; // 一技能使用计数器

    public override void Awake()
    {
        if(Body_Run == null)
        {
            Body_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/12/Body");
            FireBullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/12/FireBullet");
            Head_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/12/Head");
            Heart_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/12/Heart");
        }
        base.Awake();
    }

    public override void MInit()
    {
        bodyList.Clear();
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
                    animatorController.Play("Combine");
                });
                t.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        return true;
                    return false;
                });
                t.AddOnExitAction(delegate
                {
                    count = 0;
                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                });
                return t;
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in bodyList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            bodyList.Remove(u);
        }
        base.MUpdate();
    }

    public override void BeforeDeath()
    {
        foreach (var u in bodyList)
        {
            u.ExecuteDeath();
        }
        base.BeforeDeath();
    }
    public override void AfterDeath()
    {
        foreach (var u in bodyList)
        {
            u.ExecuteDeath();
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
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.GrumpyJack, 0))
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
        list.Add(Skill0Init(infoList[0]));
        list.Add(Skill2Init(infoList[2]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    #region 通用技能
    private BaseBullet CreateFireBullet(Vector2 pos)
    {
        EnemyBullet b = EnemyBullet.GetInstance(FireBullet_Run, this, GetParamValue("p_dmg0"));
        b.transform.position = pos;
        b.SetStandardVelocity(0);
        b.CloseCollision();
        // 产生一个燃烧特效与岩浆
        b.AddHitAction(delegate {
            BaseEffect e = EffectManager.GetFireEffect(false);
            e.transform.position = b.transform.position;
            GameController.Instance.AddEffect(e);
            CreateLavaArea(b.transform.position, Mathf.FloorToInt(GetParamValue("p_lava_time0")*60));
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    private void CreateLavaArea(Vector2 pos, int time)
    {
        if (pos.x >= MapManager.GetColumnX(8.5f) || pos.x <= MapManager.GetColumnX(-0.5f) || pos.y >= MapManager.GetRowY(-0.5f) || pos.y <= MapManager.GetRowY(6.5f))
            return;
        LavaAreaEffectExecution lava = LavaAreaEffectExecution.GetInstance(pos);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            lava.SetOpen();
        });
        t.AddTaskFunc(delegate {
            time--;
            if(time <= 0)
            {
                return true;
            }
            return false;
        });
        t.AddOnExitAction(delegate {
            lava.SetDisappear();
        });
        lava.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(lava);
    }
    
    private void AddFireBulletMoveTask(BaseBullet b, Vector2 firstPosition, Vector2 targetPosition)
    {
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, 60, MapManager.gridHeight, firstPosition, targetPosition, true));
    }

    /// <summary>
    /// 生成十字炎弹
    /// </summary>
    private void CreateFourFireBullet(Vector2 pos, float radius)
    {
        Vector2[] arr = new Vector2[] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, 1) };
        foreach (var v2 in arr)
        {
            AddFireBulletMoveTask(CreateFireBullet(pos), pos, pos + radius * new Vector2(v2.x*MapManager.gridWidth, v2.y*MapManager.gridHeight));
        }
    }
    
    /// <summary>
    /// 生成身体部分组件
    /// </summary>
    private BaseUnit CreateComponent(RuntimeAnimatorController Run, Vector2 pos, float hp, float defence, float dmg, float dmg_trans)
    {
        MouseModel m = MouseModel.GetInstance(Run);
        bodyList.Add(m);
        m.SetBaseAttribute(hp, 10, 1.0f, 2.0f, 0, 0.5f, 0);
        m.NumericBox.DamageRate.AddModifier(new FloatModifier(1 - 0.01f * defence));
        m.SetIsMouseUnit(false);
        m.isBoss = true;
        m.transform.position = pos;
        m.AttackClipName = "Move";
        m.DieClipName = null;
        m.AddCanBlockFunc(delegate { return false; });
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
        m.AddAfterDeathEvent(delegate {
            bodyList.Remove(m);
            // 产生一个燃烧特效与岩浆
            BaseEffect e = EffectManager.GetFireEffect(false);
            e.transform.position = m.transform.position;
            GameController.Instance.AddEffect(e);
            CreateLavaArea(m.transform.position, Mathf.FloorToInt(GetParamValue("p_lava_time1") * 60));
        });
        Action<CombatAction> hitAction = (com) => {
            if (com is DamageAction)
            {
                DamageAction d = com as DamageAction;
                DamageAction new_d = DamageActionManager.Copy(d, d.Creator, this);
                foreach (var item in new_d.GetDamageTypeList())
                {
                    Debug.Log("new_d:" + item);
                }
                new_d.DamageValue = dmg_trans * new_d.DamageValue;
                new_d.ApplyAction();
                // new DamageAction(CombatAction.ActionType.CauseDamage, d.Creator, this, dmg_trans * d.DamageValue).ApplyAction();
            }
        };
        m.AddActionPointListener(ActionPointType.PreReceiveDamage, hitAction);
        m.SetActionState(new MoveState(m));
        m.DisableMove(true);

        // 阻挡检测区域
        RetangleAreaEffectExecution r;
        bool isCollide = false; // 是否有判定
        bool isBlock = false; // 是否被阻挡
        {
            r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.AddExcludeMouseUnit(m);

            // 对阻挡检测区域添加任务
            {
                int interval = 15;
                int timeLeft = interval;
                CustomizationTask task = new CustomizationTask();
                task.AddTaskFunc(delegate {
                    if (m.IsAlive())
                        r.transform.position = m.transform.position;
                    else
                        return true;

                    if (!isCollide)
                    {
                        timeLeft = interval;
                        return false;
                    }
                    timeLeft--;
                    if (r.foodUnitList.Count > 0)
                        isBlock = true;
                    else
                        isBlock = false;
                    if (timeLeft <= 0)
                    {
                        List<BaseUnit> dmgList = new List<BaseUnit>();
                        foreach (var u in r.foodUnitList)
                        {
                            if (u.GetHeight() == 0)
                                //new DamageAction(CombatAction.ActionType.BurnDamage, m, u, dmg).ApplyAction();
                                dmgList.Add(u);
                        }
                        foreach (var u in r.mouseUnitList)
                        {
                            if(u.GetHeight() == 0)
                                //new DamageAction(CombatAction.ActionType.BurnDamage, m, u, dmg).ApplyAction();
                                dmgList.Add(u);
                        }
                        foreach (var u in dmgList)
                        {
                            new DamageAction(CombatAction.ActionType.BurnDamage, m, u, dmg).ApplyAction();
                        }
                        timeLeft += interval;
                    }
                    return false;
                });
                task.AddOnExitAction(delegate {
                    r.MDestory();
                });
                r.AddTask(task);
            }
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 状态定义
        {
            m.mStateController.AddCreateStateFunc("Droping", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    isCollide = false;
                    m.animatorController.Play("Idle", true);
                });
                return s;
            });
            m.mStateController.AddCreateStateFunc("PostDrop", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    m.animatorController.Play("Drop");
                    isCollide = true;
                });
                return s;
            });
            m.mStateController.AddCreateStateFunc("Idle", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    m.animatorController.Play("Idle", true);
                });
                return s;
            });
            m.mStateController.AddCreateStateFunc("Move", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    m.animatorController.Play("Move", true);
                    m.DisableMove(false);
                });
                s.AddOnUpdateAction(delegate {
                    if (isBlock)
                        m.DisableMove(true);
                    else
                        m.DisableMove(false);
                });
                s.AddOnExitAction(delegate {
                    m.DisableMove(true);
                });
                return s;
            });
        }
        GameController.Instance.AddMouseUnit(m);
        return m;
    }
    
    private BaseUnit CreateBody(Vector2 pos)
    {
        BaseUnit u = CreateComponent(Body_Run, pos, GetParamValue("p_hp1"), GetParamValue("p_defence1"), GetParamValue("p_dmg1"), 0.01f * GetParamValue("p_dmg_trans1"));
        u.NumericBox.BurnRate.AddModifier(new FloatModifier(1-GetParamValue("p_burn_defence1")*0.01f));
        return u;
    } 

    private BaseUnit CreateHead(Vector2 pos)
    {
        BaseUnit u = CreateComponent(Head_Run, pos, GetParamValue("p_hp1"), GetParamValue("p_defence1"), GetParamValue("p_dmg1"), 0.01f * GetParamValue("p_dmg_trans1"));
        u.NumericBox.BurnRate.AddModifier(new FloatModifier(1 - GetParamValue("p_burn_defence1") * 0.01f));
        return u;
    }

    private BaseUnit CreateHeart(Vector2 pos)
    {
        BaseUnit u = CreateComponent(Heart_Run, pos, mMaxHp, 100, GetParamValue("p_dmg2"), 0.01f * GetParamValue("p_dmg_trans2"));
        u.NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true)); // 核心是无敌的
        u.AddActionPointListener(ActionPointType.PreReceiveDamage, (action) => {
            (u as MouseUnit).FlashWhenHited();
        });
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            foreach (var mod in NumericBox.BurnRate.GetModifierList())
            {
                u.NumericBox.BurnRate.AddModifier(mod);
            }
        });
        t.AddTaskFunc(delegate {
            u.mCurrentHp = mCurrentHp;
            return false;
        });
        u.AddTask(t);
        return u;
    }

    #endregion

    #region 一技能
    private CompoundSkillAbility Skill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int moveTime = Mathf.FloorToInt(GetParamValue("move_time0") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait0_1") * 60);
        int num = Mathf.Max(0, Mathf.Min(1, Mathf.FloorToInt(GetParamValue("num0"))));

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("PreJump");
        };
        {
            // 移动
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreJump", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Jumping", true);
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    int rowIndex;
                    int colIndex;
                    FindR0C0(out rowIndex, out colIndex);
                    Vector2 pos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    CompoundSkillAbilityManager.GetMoveToTask(transform, pos, moveTime, out task);
                    return task;
                });
            }

            // 落下、停滞、砸击、停滞
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostJump", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Idle", true);
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitTimeTask(wait0, out task);
                    return task;
                });
                // 砸人
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("Attack");
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5f)
                        {
                            CreateS0DamageArea();
                            CreateS0StunArea();
                            for (int i = 0; i < num; i++)
                            {
                                CreateFourFireBullet(transform.position + MapManager.gridWidth * Vector3.left, 2);
                            }
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


    private void FindR0C0(out int rowIndex, out int colIndex)
    {
        count = count % 2;
        if (count == 0)
            rowIndex = 1;
        else
            rowIndex = 5;
        count++;
        colIndex = 3;
    }
    
    private void CreateS0DamageArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 2.5f, 0.5f, "EnemyAllyGrid");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u) => {
            if (u.IsBoss())
                return;
            UnitManager.Execute(this, u);
        });

        r.isAffectGrid = true;
        r.SetOnGridEnterAction((g) => {
            g.TakeAction(this, (u) => {
                DamageAction action = UnitManager.Execute(this, u);
                new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans0") / 100).ApplyAction();
            }, false);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    private void CreateS0StunArea()
    {
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time0")*60);
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*MapManager.gridWidth * Vector3.left, 3.5f, 2.5f, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction((u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        r.SetOnEnemyEnterAction((m) => {
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        });
        r.SetOnCharacterEnterAction((c) => {
            c.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(c, stun_time, false));
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
    #endregion

    #region 二技能
    private CompoundSkillAbility Skill1Init(SkillAbility.SkillAbilityInfo info)
    {
        int move_attack_time = Mathf.FloorToInt(GetParamValue("move_attack_time1") * 60);
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);
        float v = TransManager.TranToVelocity(GetParamValue("v1"));
        float colIndex = 9 - GetParamValue("right_column1");

        FloatModifier mod = new FloatModifier(0);
        BaseUnit heart = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("PreJump");
        };
        {
            // 移动
            {
                // 飞天（实际上只有贴图在飞）
                mod.Value = 0;
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreJump", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Jumping", true);
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        CloseCollision();
                    });
                    task.AddTimeTaskFunc(45, null, (leftTime, totalTime) => {
                        float rate = 1 - (float)leftTime / totalTime;
                        mod.Value = MapManager.gridHeight * 20 * rate;
                        RemoveSpriteOffsetY(mod);
                        AddSpriteOffsetY(mod);
                    }, null);
                    task.AddTimeTaskFunc(120);
                    return task;
                });

            }

            // 滚石，合体
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        heart = CreateHeart(MapManager.GetGridLocalPosition(colIndex, 3));
                        heart.AddTask(GetS1ComponentTask(heart, v));
                        int rowIndex = GetRandomNext(0, 6);
                        if (rowIndex >= 3)
                            rowIndex++;
                        // 头部随机取一行，其他都是身体
                        BaseUnit head = CreateHead(MapManager.GetGridLocalPosition(colIndex, rowIndex));
                        head.AddTask(GetS1ComponentTask(head, v));
                        for (int i = 0; i < 7; i++)
                        {
                            if (i == rowIndex || i == 3)
                                continue;
                            BaseUnit u = CreateBody(MapManager.GetGridLocalPosition(colIndex, i));
                            u.AddTask(GetS1ComponentTask(u, v));
                        }
                    });
                    task.AddTimeTaskFunc(move_attack_time + 89);
                    return task;
                });

                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Combine", out task);
                    task.AddOnEnterAction(delegate {
                        transform.position = heart.transform.position;
                        RemoveSpriteOffsetY(mod);
                        OpenCollision();
                    });
                    task.AddTimeTaskFunc(wait, delegate { animatorController.Play("Idle", true); }, null, null);
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    
    private CustomizationTask GetS1ComponentTask(BaseUnit u, float v)
    {
        float dx = MapManager.gridWidth*2;
        float dy = MapManager.gridHeight*10;

        FloatModifier XMod = new FloatModifier(dx);
        FloatModifier YMod = new FloatModifier(dy);

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            u.mStateController.ChangeState("Droping");
            u.AddSpriteOffsetX(XMod);
            u.AddSpriteOffsetY(YMod);
        });
        task.AddTimeTaskFunc(30, null, (leftTime, totalTime) => {
            float rate = (float)leftTime / totalTime;
            XMod.Value = dx * rate;
            YMod.Value = dy * rate;
            u.RemoveSpriteOffsetX(XMod);
            u.AddSpriteOffsetX(XMod);
            u.RemoveSpriteOffsetY(YMod);
            u.AddSpriteOffsetY(YMod);
        }, delegate {
            u.RemoveSpriteOffsetX(XMod);
            u.RemoveSpriteOffsetY(YMod);
            u.mStateController.ChangeState("PostDrop");
            // 创建十字炎弹与落地伤害
            CreateFourFireBullet(u.transform.position, 1);
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(u.transform.position, 0.5f, 0.5f, "EnemyAllyGrid");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    if (u.IsBoss())
                        return;
                    UnitManager.Execute(this, u);
                });

                r.isAffectGrid = true;
                r.SetOnGridEnterAction((g) => {
                    g.TakeAction(this, (u) => {
                        DamageAction action = UnitManager.Execute(this, u);
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans1") / 100).ApplyAction();
                    }, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            //DamageAreaEffectExecution d = DamageAreaEffectExecution.GetInstance(u, u.transform.position, 0.75f, 0.5f, CombatAction.ActionType.CauseDamage, GetParamValue("dmg1"));
            //d.isAffectFood = true;
            //d.isAffectMouse = true;
            //d.isAffectCharacter = false;
            //d.AddExcludeMouseUnit((MouseUnit)u);
            //GameController.Instance.AddAreaEffectExecution(d);
        });
        task.AddTaskFunc(delegate {
            if (u.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                // 可以开滚了
                u.mStateController.ChangeState("Move");
                u.NumericBox.MoveSpeed.SetBase(v);
                return true;
            }
            return false;
        });
        task.AddTimeTaskFunc(Mathf.FloorToInt(GetParamValue("move_attack_time1")*60));
        task.AddOnExitAction(delegate {
            u.ExecuteDeath();
        });
        return task;
    }
    #endregion

    #region 三技能
    private CompoundSkillAbility Skill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int interval = Mathf.FloorToInt(GetParamValue("interval2") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        List<Vector2> posList = null;

        FloatModifier mod = new FloatModifier(0);
        BaseUnit heart = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("PreJump");
        };
        {
            // 移动
            {
                // 飞天（实际上只有贴图在飞）
                mod.Value = 0;
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreJump", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Jumping", true);
                    });
                    return task;
                });
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        CloseCollision();
                    });
                    task.AddTimeTaskFunc(45, null, (leftTime, totalTime) => {
                        float rate = 1 - (float)leftTime / totalTime;
                        mod.Value = MapManager.gridHeight * 20 * rate;
                        RemoveSpriteOffsetY(mod);
                        AddSpriteOffsetY(mod);
                    }, null);
                    task.AddTimeTaskFunc(120);
                    return task;
                });

            }

            // 落石，合体
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        posList = GetS2PosList();
                    });
                    task.AddTimeTaskFunc(interval, delegate {
                        BaseUnit u = CreateHead(MapManager.GetGridLocalPosition(posList[0].x, posList[0].y));
                        u.AddTask(GetS2ComponentTask(u, false, interval*6 + wait0));
                    }, null, null);
                    for (int _i = 1; _i < 6; _i++)
                    {
                        int i = _i;
                        task.AddTimeTaskFunc(interval, delegate {
                            BaseUnit u = CreateBody(MapManager.GetGridLocalPosition(posList[i].x, posList[i].y));
                            u.AddTask(GetS2ComponentTask(u, false, interval * (6 - i) + wait0));
                        }, null, null);
                    }
                    task.AddTimeTaskFunc(wait0 + 89, delegate {
                        heart = CreateHeart(MapManager.GetGridLocalPosition(posList[6].x, posList[6].y));
                        heart.AddTask(GetS2ComponentTask(heart, true, wait0));
                    }, null, null);
                    return task;
                });

                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Combine", out task);
                    task.AddOnEnterAction(delegate {
                        transform.position = heart.transform.position;
                        RemoveSpriteOffsetY(mod);
                        OpenCollision();
                    });
                    task.AddTimeTaskFunc(wait1, delegate { animatorController.Play("Idle", true); }, null, null);
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private List<Vector2> GetS2PosList()
    {
        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        list.Sort((x, y) => {
            int xRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[x])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    xRowAllyCount++;
            }
            int yRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[y])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    yRowAllyCount++;
            }
            return xRowAllyCount.CompareTo(yRowAllyCount);
        });

        List<Vector2> posList = new List<Vector2>();
        foreach (var rowIndex in list)
        {
            BaseUnit leftUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(rowIndex, float.MaxValue, false);
            BaseUnit rightUnit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, false);
            int colIndex;
            if (leftUnit == null)
                colIndex = 4;
            else
                //colIndex = MapManager.GetXIndex((leftUnit.transform.position.x + rightUnit.transform.position.x) / 2);
                colIndex = MapManager.GetXIndex(rightUnit.transform.position.x);
            posList.Add(new Vector2(colIndex, rowIndex));
        }
        return posList;
    }

    private CustomizationTask GetS2ComponentTask(BaseUnit u, bool isHeart, int waitTime)
    {
        float dx = MapManager.gridWidth * 0;
        float dy = MapManager.gridHeight * 10;

        FloatModifier XMod = new FloatModifier(dx);
        FloatModifier YMod = new FloatModifier(dy);

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            u.mStateController.ChangeState("Droping");
            u.AddSpriteOffsetX(XMod);
            u.AddSpriteOffsetY(YMod);
            u.NumericBox.DamageRate.AddModifier(new FloatModifier(1-GetParamValue("defence2")*0.01f)); // 可额外获得减伤
        });
        task.AddTimeTaskFunc(30, null, (leftTime, totalTime) => {
            float rate = (float)leftTime / totalTime;
            XMod.Value = dx * rate;
            YMod.Value = dy * rate;
            u.RemoveSpriteOffsetX(XMod);
            u.AddSpriteOffsetX(XMod);
            u.RemoveSpriteOffsetY(YMod);
            u.AddSpriteOffsetY(YMod);
        }, delegate {
            u.RemoveSpriteOffsetX(XMod);
            u.RemoveSpriteOffsetY(YMod);
            u.mStateController.ChangeState("PostDrop");
            // 创建十字炎弹与落地伤害
            if (isHeart)
            {
                CreateFourFireBullet(u.transform.position, 1);
                CreateFourFireBullet(u.transform.position, 2);
            }
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(u.transform.position, 0.5f, 0.5f, "EnemyAllyGrid");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    if (u.IsBoss())
                        return;
                    UnitManager.Execute(this, u);
                });

                r.isAffectGrid = true;
                r.SetOnGridEnterAction((g) => {
                    g.TakeAction(this, (u) => {
                        DamageAction action = UnitManager.Execute(this, u);
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans2") / 100).ApplyAction();
                    }, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            //DamageAreaEffectExecution d = DamageAreaEffectExecution.GetInstance(u, u.transform.position, 0.75f, 0.5f, CombatAction.ActionType.CauseDamage, GetParamValue("dmg1"));
            //d.isAffectFood = true;
            //d.isAffectMouse = true;
            //d.isAffectCharacter = false;
            //d.AddExcludeMouseUnit((MouseUnit)u);
            //GameController.Instance.AddAreaEffectExecution(d);
        });
        task.AddTaskFunc(delegate {
            if (u.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                u.mStateController.ChangeState("Idle");
                return true;
            }
            return false;
        });
        task.AddTimeTaskFunc(waitTime);
        task.AddOnExitAction(delegate {
            u.ExecuteDeath();
        });
        return task;
    }
    #endregion 
}
