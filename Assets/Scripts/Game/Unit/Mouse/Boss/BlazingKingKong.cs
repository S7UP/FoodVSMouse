using Environment;

using S7P.Numeric;
using S7P.State;

using System;
using System.Collections.Generic;

using UnityEngine;

public class BlazingKingKong : BossUnit
{
    private static RuntimeAnimatorController Stab_Run;
    private static RuntimeAnimatorController FireBullet_Run;
    private static Sprite Lightning_Sprite;

    private BossUnit Pete;
    private BossUnit Julie;
    private List<LavaAreaEffectExecution> lavaList = new List<LavaAreaEffectExecution>();
    private Action<MouseUnit> mouseProcessAction;

    public override void Awake()
    {
        if (Stab_Run == null)
        {
            Stab_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/13/Stab");
            FireBullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/12/FireBullet");
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        }
        base.Awake();
    }

    public override void MInit()
    {
        mouseProcessAction = (u) => {
            // 场上存在的老鼠获得岩浆抗性
            FloatModifier lavaRateMod = new FloatModifier(1 - GetParamValue("lava_defence4") * 0.01f);
            LavaTask.AddUnitLavaRate(u, lavaRateMod);
        };
        Pete = null;
        Julie = null;
        lavaList.Clear();
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

    public override void MUpdate()
    {
        List<LavaAreaEffectExecution> delList = new List<LavaAreaEffectExecution>();
        foreach (var lava in lavaList)
        {
            if (!lava.IsValid())
                delList.Add(lava);
        }
        foreach (var lava in delList)
        {
            lavaList.Remove(lava);
        }
        base.MUpdate();
    }

    public override void BeforeDeath()
    {
        if (Pete != null && Pete.IsAlive())
            Pete.ExecuteDeath();
        if (Julie != null && Julie.IsAlive())
            Julie.ExecuteDeath();
        base.BeforeDeath();
    }

    public override void AfterDeath()
    {
        if (Pete != null && Pete.IsAlive())
            Pete.ExecuteDeath();
        if (Julie != null && Julie.IsAlive())
            Julie.ExecuteDeath();

        if(GetParamValue("lava_disappear4") == 1.0f)
            foreach (var lava in lavaList)
                lava.SetDisappear();
        lavaList.Clear();

        GameController.Instance.mMouseFactory.RemoveProcessAction(mouseProcessAction);
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.8f, 0.6f, 0.4f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.BlazingKingKong, 0))
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
        if(mHertIndex == 0)
        {
            list.Add(SKill0Init(infoList[0]));
        }else if(mHertIndex == 1)
        {
            list.Add(SKill1Init(infoList[1]));
        }
        else if (mHertIndex == 2)
        {
            list.Add(SKill2Init(infoList[2]));
        }
        else if (mHertIndex == 3)
        {
            list.Add(SKill3Init(infoList[3]));
        }
        else
        {
            list.Add(SKill4Init(infoList[4]));
        }
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    #region 通用机制
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
            CreateLavaArea(b.transform.position, Mathf.FloorToInt(GetParamValue("p_lava_time0") * 60));
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
            if (time <= 0)
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
        lavaList.Add(lava);
    }

    private void AddFireBulletMoveTask(BaseBullet b, Vector2 firstPosition, Vector2 targetPosition)
    {
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, (targetPosition - firstPosition).magnitude / 60, MapManager.gridHeight, firstPosition, targetPosition, true));
    }
    #endregion

    #region 一技能
    /// <summary>
    /// 震撼登场
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        float[] right_colArray = GetParamArray("right_col0");
        float rowIndex = GetParamValue("row0") - 1;

        float dmg0 = GetParamValue("dmg0_0");
        float dmg1 = GetParamValue("dmg0_1");
        int stun_time = Mathf.FloorToInt(60 * GetParamValue("stun_time0", mHertIndex));
        int num0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex));
        int num1 = Mathf.FloorToInt(GetParamValue("num0_1", mHertIndex));
        int interval = Mathf.FloorToInt(60 * GetParamValue("interval0", mHertIndex));
        int wait = Mathf.FloorToInt(60 * GetParamValue("wait0", mHertIndex));

        float size = GetParamValue("size0");

        FloatModifier mod = new FloatModifier(0);
        BoolModifier mod2 = new BoolModifier(true);
        int timeLeft = interval;
        int totalTimeLeft = wait;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            OpenCollision();
            SetAlpha(1);
            NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod2);
        };
        {
            // 移动
            {
                // 飞天、降落
                mod.Value = 0;
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreFly", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Flying", true);
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
                    }, delegate { animatorController.Play("Idle", true); });
                    task.AddTimeTaskFunc(120);
                    task.AddTimeTaskFunc(45, delegate {
                        float colIndex = 9 - right_colArray[GetRandomNext(0, right_colArray.Length)];
                        transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    }, 
                        (leftTime, totalTime) => {
                        float rate = (float)leftTime / totalTime;
                        mod.Value = MapManager.gridHeight * 20 * rate;
                        RemoveSpriteOffsetY(mod);
                        AddSpriteOffsetY(mod);
                    }, null);
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod2);
                        OpenCollision();
                        RemoveSpriteOffsetY(mod);
                        S0CreateDamageEffectExecution(transform.position, size-0.5f, size-0.5f);
                        S0CreateStunEffectExecution(MapManager.GetGridLocalPosition(4, 3), 10, 7, stun_time);
                        S0CreateFireBullet(num0);
                    });
                    return task;
                });
            }

            // 持续站场
            {
                string taskKey = "S0Task";

                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        timeLeft = interval;
                        totalTimeLeft = wait;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        totalTimeLeft--;
                        if (timeLeft <= 0 && task.taskController.GetTask(taskKey)==null)
                        {
                            timeLeft += interval;
                            task.taskController.AddUniqueTask(taskKey, GetS0AttackTask());
                        }
                        if ((totalTimeLeft <= 0 || mHertIndex > 0) && task.taskController.GetTask(taskKey) == null)
                            return true;
                        else
                            return false;
                    });
                    task.AddOnExitAction(delegate {
                        float[] arr = GetParamArray("hpRate");
                        if (arr.Length > 0)
                        {
                            mHertIndex = 1;
                            OnHertStageChanged();
                        }
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void S0CreateDamageEffectExecution(Vector2 pos, float col, float row)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, col, row, "EnemyAllyGrid");
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

    private void S0CreateStunEffectExecution(Vector2 pos, float col, float row, int stun_time)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, col, row, "EnemyAllyGrid");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction((u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        r.SetOnEnemyEnterAction((u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        r.SetOnCharacterEnterAction((u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 寻找熔岩弹的攻击目标集合
    /// </summary>
    /// <returns></returns>
    private Queue<List<BaseUnit>> FindFireBulletTargetQueue()
    {
        // 先找这几行的所有可攻击，可被选取的美食单位
        int startRow = 0;
        int endRow = 6;
        List<BaseUnit> list = new List<BaseUnit>();
        for (int i = startRow; i <= endRow; i++)
        {
            foreach (var u in GameController.Instance.GetSpecificRowAllyList(i))
            {
                if (u is FoodUnit)
                {
                    FoodUnit f = u as FoodUnit;
                    if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f) && f.IsAlive() && u.transform.position.x < MapManager.GetColumnX(7))
                    {
                        list.Add(f);
                    }
                }
            }
        }
        // 按当前生命值降序排序
        for (int i = 0; i < list.Count; i++)
        {
            BaseUnit maxUnit = list[i];
            int index = i;
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[j].GetCurrentHp() > maxUnit.GetCurrentHp())
                {
                    maxUnit = list[j];
                    index = j;
                }
            }
            list[index] = list[i];
            list[i] = maxUnit;
        }
        // 降序完之后把生命值相同的类归并，然后放入队列
        Queue<List<BaseUnit>> queue = new Queue<List<BaseUnit>>();
        if (list.Count > 0)
        {
            float hp = list[0].GetCurrentHp();
            int index = 0;
            List<BaseUnit> l = new List<BaseUnit>();
            while (index < list.Count)
            {
                if (list[index].GetCurrentHp() == hp)
                {
                    l.Add(list[index]);
                }
                else
                {
                    queue.Enqueue(l);
                    l = new List<BaseUnit>();
                    hp = list[index].GetCurrentHp();
                    l.Add(list[index]);
                }
                index++;
            }
            queue.Enqueue(l);
        }
        return queue;
    }

    private void S0CreateFireBullet(int num)
    {
        Queue<List<BaseUnit>> queue = null;
        List<BaseUnit> list = null;

        queue = FindFireBulletTargetQueue();
        if (queue.Count > 0)
            list = queue.Peek();

        for (int i = 0; i < num; i++)
        {
            while (list != null && list.Count == 0)
            {
                if (queue.Count > 1)
                {
                    queue.Dequeue();
                    list = queue.Peek();
                }
                else
                    list = null;
            }
            Vector2 targetPosition = Vector2.zero;
            if (list != null)
            {
                // 从list里随机取一个目标出来
                BaseUnit target = list[GetRandomNext(0, list.Count)];
                GetRandomNext(0, 1);
                targetPosition = target.transform.position;
                list.Remove(target);
            }
            else
            {
                targetPosition = MapManager.GetGridLocalPosition(GetRandomNext(0, 8), GetRandomNext(0, 6));
            }
            AddFireBulletMoveTask(CreateFireBullet(transform.position), transform.position, targetPosition);
        }
    }

    private CustomizationTask GetS0AttackTask()
    {
        int num1 = Mathf.FloorToInt(GetParamValue("num0_1", mHertIndex));
        bool flag = true;
        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            animatorController.Play("Attack");
        });
        task.AddTaskFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.38f && flag)
            {
                S0CreateDamageEffectExecution(transform.position, 2.5f, 0.5f);
                S0CreateFireBullet(num1);
                flag = false;
            }
            else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                return true;
            return false;
        });
        task.AddOnExitAction(delegate {

            animatorController.Play("Idle", true);
        });
        return task;
    }
    #endregion

    #region 二技能
    /// <summary>
    /// 第一次解体
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        float dmg_trans = GetParamValue("dmg_trans1")*0.01f;
        int wait = Mathf.FloorToInt(60 * GetParamValue("wait1", mHertIndex));

        int totalTimeLeft = wait;
        BoolModifier mod = new BoolModifier(true);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            // 消失
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Disappear", out task);
                    task.AddOnEnterAction(delegate {
                        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
                    });
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
                        CloseCollision();
                        SetAlpha(0);
                        CreateS1Boss0(dmg_trans);
                        CreateS1Boss1(dmg_trans);
                    });
                    return task;
                });
            }

            // 等待
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        totalTimeLeft = wait;
                    });
                    task.AddTaskFunc(delegate {
                        totalTimeLeft--;
                        if ((totalTimeLeft <= 0 || mHertIndex > 1))
                            return true;
                        else
                            return false;
                    });
                    task.AddOnExitAction(delegate {
                        if(Pete != null)
                        {
                            Pete.mSkillQueueAbilityManager.Initial();
                            Pete.mSkillQueueAbilityManager.SetCurrentSkill(GetBossDisappearSkill(Pete));
                        }
                            //Pete.AddTask(GetBossDisappearTask(Pete));
                        if (Julie != null)
                        {
                            Julie.mSkillQueueAbilityManager.Initial();
                            Julie.mSkillQueueAbilityManager.SetCurrentSkill(GetBossDisappearSkill(Julie));
                        }
                            //Julie.AddTask(GetBossDisappearTask(Julie));
                    });
                    return task;
                });
                // 等双BOSS都消失后结束本阶段
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddTaskFunc(delegate {
                        if ((Pete == null || !Pete.IsAlive()) && (Julie == null || !Julie.IsAlive()))
                        {
                            Pete = null;
                            Julie = null;
                            return true;
                        }
                        else
                            return false;
                    });
                    task.AddOnExitAction(delegate {
                        float[] arr = GetParamArray("hpRate");
                        if (arr.Length > 1)
                        {
                            mHertIndex = 2;
                            OnHertStageChanged();
                        }
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void CreateS1Boss0(float dmg_trans)
    {
        BossUnit b = GameController.Instance.CreateBossUnit(6, 3, new BaseEnemyGroup.EnemyInfo { type = (int)BossNameTypeMap.SteelClawPete,shape = 0 }, mMaxHp);
        b.transform.position = transform.position;
        b.SetUseDefaultRecieveDamageActionMethod(false); // 不受正常受击逻辑
        // 受击伤害传递
        b.actionPointController.AddListener(ActionPointType.WhenReceiveDamage, (c) => {
            DamageAction d = DamageActionManager.Copy((c as DamageAction), c.Creator, this);
            d.DamageValue = d.DamageValue * dmg_trans;
            d.ApplyAction();
        });
        // 重要属性同步
        b.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        b.NumericBox.Defense.AddGetValueFunc(999, delegate { return NumericBox.Defense.Value; });
        b.SetGetAoeRateFunc(delegate { return mAoeRate; });
        b.SetGetBurnRateFunc(delegate { return mBurnRate; });
        b.SetGetDamageRateFunc(delegate { return mDamgeRate; });
        // 重置种子，使之与金刚初始位置相关
        b.SetRandSeedByRowIndex(GetSeedValue()); // 设置BOSS的种子生成器
        // 设置阶段点
        b.AddParamArray("hpRate", new float[] { -1f });
        Pete = b;
    }

    private void CreateS1Boss1(float dmg_trans)
    {
        BossUnit b = GameController.Instance.CreateBossUnit(6, 1, new BaseEnemyGroup.EnemyInfo { type = (int)BossNameTypeMap.MistyJulie, shape = 0 }, mMaxHp);
        b.transform.position = transform.position + new Vector3(0, MapManager.gridHeight*1.5f);
        b.SetUseDefaultRecieveDamageActionMethod(false); // 不受正常受击逻辑
        // 受击伤害传递
        b.actionPointController.AddListener(ActionPointType.WhenReceiveDamage, (c) => {
            DamageAction d = DamageActionManager.Copy((c as DamageAction), c.Creator, this);
            d.DamageValue = d.DamageValue * dmg_trans;
            d.ApplyAction();
        });
        // 重要属性同步
        b.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        b.NumericBox.Defense.AddGetValueFunc(999, delegate { return NumericBox.Defense.Value; });
        b.SetGetAoeRateFunc(delegate { return mAoeRate; });
        b.SetGetBurnRateFunc(delegate { return mBurnRate; });
        b.SetGetDamageRateFunc(delegate { return mDamgeRate; });
        // 重置种子，使之与金刚初始位置相关
        b.SetRandSeedByRowIndex(GetSeedValue()); // 设置BOSS的种子生成器
        // 设置阶段点
        b.AddParamArray("hpRate", new float[] { -1f });
        Julie = b;
    }

    private CompoundSkillAbility GetBossDisappearSkill(BossUnit b)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
        };
        c.AddCreateTaskFunc(delegate {
            CustomizationTask task;
            CompoundSkillAbilityManager.GetWaitClipTask(b.animatorController, "Disappear", out task);
            task.AddOnEnterAction(delegate {
                b.CloseCollision();
            });
            task.AddOnExitAction(delegate {
                b.MDestory();
            });
            return task;
        });
        return c;
    }
    #endregion

    #region 三技能
    /// <summary>
    /// 雷电尖刺
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        float[] right_colArray = GetParamArray("right_col2");
        float rowIndex = GetParamValue("row2") - 1;

        float burn_rate = 1 - GetParamValue("burn_defence2") * 0.01f;
        float dmg_rate = 1 - GetParamValue("normal_defence2") * 0.01f;
        int reborn_time = Mathf.FloorToInt(60 * GetParamValue("reborn_time2"));
        int interval = Mathf.FloorToInt(60 * GetParamValue("interval2"));
        int wait = Mathf.FloorToInt(60 * GetParamValue("wait2"));

        int totalTimeLeft = wait;
        BoolModifier mod = new BoolModifier(true);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
            float colIndex = 9 - right_colArray[GetRandomNext(0, right_colArray.Length)];
            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
            OpenCollision();
            SetAlpha(1);
        };
        {
            {
                // 出现
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Appear", out task);
                    return task;
                });
                // 施法
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreCast", out task);
                    task.AddOnExitAction(delegate {
                        animatorController.Play("Casting", true);
                    });
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
                    });
                    return task;
                });
            }

            // 等待
            {
                c.AddCreateTaskFunc(delegate { return S2GetStabsControllerTask(wait, reborn_time, interval, dmg_rate, burn_rate); });
                // 施法结束
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostCast", out task);
                    task.AddOnEnterAction(delegate {
                        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
                    });
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
                        animatorController.Play("Idle", true);
                        float[] arr = GetParamArray("hpRate");
                        if (arr.Length > 2)
                        {
                            mHertIndex = 3;
                            OnHertStageChanged();
                        }  
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private MouseUnit S2CreateStab(Vector2 pos, float dmg_rate, float burn_rate)
    {
        MouseModel m = MouseModel.GetInstance(Stab_Run);
        m.transform.position = pos;
        m.SetBaseAttribute(9999, 10.0f, 1.0f, 0, 100, 0.5f, 0);
        m.NumericBox.DamageRate.AddModifier(new FloatModifier(dmg_rate));
        m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.canDrivenAway = false;
        m.isIgnoreRecordDamage = true;
        m.AddCanBlockFunc(delegate { return false; });
        m.IdleClipName = "Idle";
        m.AttackClipName = "Idle";
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // 标记免疫水蚀
        LavaTask.AddUnitLavaRate(m, new FloatModifier(0)); // 使目标完全免疫岩浆伤害
        m.SetActionState(new IdleState(m));
        GameController.Instance.AddMouseUnit(m);

        // 状态定义
        {
            m.mStateController.AddCreateStateFunc("Appear", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    m.animatorController.Play("Appear");
                });
                s.AddOnUpdateAction(delegate {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        m.mStateController.ChangeState("Idle");
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

            m.mStateController.ChangeState("Appear"); // 设置为初始状态
        }
        return m;
    }

    private CustomizationTask S2GetStabsControllerTask(int wait, int reborn_time, int interval, float dmg_rate, float burn_rate)
    {
        Vector2[][] posArray = new Vector2[2][];
        MouseUnit[][] stabArray = new MouseUnit[2][];
        int[][] rebornArray = new int[2][];
        for (int i = 0; i < stabArray.Length; i++)
        {
            posArray[i] = new Vector2[3];
            stabArray[i] = new MouseUnit[3];
            rebornArray[i] = new int[3];
        }
        // 枚举出六根电刺的坐标
        posArray[0][0] = MapManager.GetGridLocalPosition(0, 3);
        posArray[0][1] = MapManager.GetGridLocalPosition(6, 0);
        posArray[0][2] = MapManager.GetGridLocalPosition(6, 6);
        posArray[1][0] = MapManager.GetGridLocalPosition(8, 3);
        posArray[1][1] = MapManager.GetGridLocalPosition(2, 0);
        posArray[1][2] = MapManager.GetGridLocalPosition(2, 6);

        int timeLeft = interval;
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            // 产生六根电刺
            for (int i = 0; i < stabArray.Length; i++)
            {
                for (int j = 0; j < stabArray[i].Length; j++)
                {
                    stabArray[i][j] = S2CreateStab(posArray[i][j], dmg_rate, burn_rate);
                    rebornArray[i][j] = reborn_time;
                }
            }
            timeLeft = interval;
        });
        t.AddTaskFunc(delegate {
            timeLeft--;
            if(timeLeft <= 0)
            {
                timeLeft += interval;
                List<BaseUnit> hitedUnitList = new List<BaseUnit>();
                // 连锁电击
                for (int i = 0; i < stabArray.Length; i++)
                {
                    for (int j = 0; j < stabArray[i].Length; j++)
                    {
                        MouseUnit u1 = stabArray[i][j];
                        if(u1 != null && u1.IsAlive())
                        {
                            for (int k = j + 1; k < stabArray[i].Length; k++)
                            {
                                MouseUnit u2 = stabArray[i][k];
                                if (u2 != null && u2.IsAlive())
                                    S2CreateLighting(u1, u2, hitedUnitList, stabArray);
                            }
                        }
                    }
                }
            }
            // 检查电刺的状态
            for (int i = 0; i < stabArray.Length; i++)
            {
                for (int j = 0; j < stabArray[i].Length; j++)
                {
                    MouseUnit u = stabArray[i][j];
                    if(u == null || !u.IsAlive())
                    {
                        stabArray[i][j] = null;
                        rebornArray[i][j]--;
                        if(rebornArray[i][j] <= 0)
                        {
                            stabArray[i][j] = S2CreateStab(posArray[i][j], dmg_rate, burn_rate);
                            rebornArray[i][j] += reborn_time;
                        }
                    }
                }
            }

            wait--;
            if ((wait <= 0 || mHertIndex > 2))
                return true;
            else
                return false;
        });
        t.AddOnExitAction(delegate {
            for (int i = 0; i < stabArray.Length; i++)
            {
                for (int j = 0; j < stabArray[i].Length; j++)
                {
                    MouseUnit u = stabArray[i][j];
                    if (u != null && u.IsAlive())
                    {
                        u.ExecuteDeath();
                    }
                }
            }
        });
        return t;
    }

    private void S2CreateLighting(BaseUnit u1, BaseUnit u2, List<BaseUnit> hitedUnitList, MouseUnit[][] stabArray)
    {
        // 计算距离与方向向量
        float dist = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).magnitude;
        Vector2 rot = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).normalized;
        // 产生伤害判定域
        {
            Action<BaseUnit> action = (u) =>
            {
                if (hitedUnitList.Contains(u))
                    return;
                BurnManager.BurnDamage(null, u, GetParamValue("burn_rate2") * 0.01f);
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, Mathf.FloorToInt(60 * GetParamValue("stun_time2")), false));
                // 记录已被电击过一次
                hitedUnitList.Add(u);
            };

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(Vector2.Lerp(u1.transform.position, u2.transform.position, 0.5f), dist / MapManager.gridWidth, 0.5f, "BothCollide");
            r.transform.right = rot;
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetOnFoodEnterAction(action);
            r.SetOnEnemyEnterAction(action);
            for (int i = 0; i < stabArray.Length; i++)
            {
                for (int j = 0; j < stabArray[i].Length; j++)
                {
                    r.AddExcludeMouseUnit(stabArray[i][j]);
                }
            }
            r.AddExcludeMouseUnit(this);
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
    #endregion

    #region 四技能
    /// <summary>
    /// 第二次解体
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill3Init(SkillAbility.SkillAbilityInfo info)
    {
        float dmg_trans = GetParamValue("dmg_trans3") * 0.01f;
        int wait = Mathf.FloorToInt(60 * GetParamValue("wait3", mHertIndex));

        int totalTimeLeft = wait;
        BoolModifier mod = new BoolModifier(true);
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            // 消失
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Disappear", out task);
                    task.AddOnEnterAction(delegate {
                        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
                    });
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
                        CloseCollision();
                        SetAlpha(0);
                        CreateS3Boss0(dmg_trans);
                        CreateS3Boss1(dmg_trans);
                    });
                    return task;
                });
            }

            // 等待
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        totalTimeLeft = wait;
                    });
                    task.AddTaskFunc(delegate {
                        totalTimeLeft--;
                        if ((totalTimeLeft <= 0 || mHertIndex > 3))
                            return true;
                        else
                            return false;
                    });
                    task.AddOnExitAction(delegate {
                        if (Pete != null)
                        {
                            Pete.mSkillQueueAbilityManager.Initial();
                            Pete.mSkillQueueAbilityManager.SetCurrentSkill(GetBossDisappearSkill(Pete));
                        }
                        //Pete.AddTask(GetBossDisappearTask(Pete));
                        if (Julie != null)
                        {
                            Julie.mSkillQueueAbilityManager.Initial();
                            Julie.mSkillQueueAbilityManager.SetCurrentSkill(GetBossDisappearSkill(Julie));
                        }
                    });
                    return task;
                });
                // 等双BOSS都消失后结束本阶段
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddTaskFunc(delegate {
                        if ((Pete == null || !Pete.IsAlive()) && (Julie == null || !Julie.IsAlive()))
                        {
                            Pete = null;
                            Julie = null;
                            return true;
                        }
                        else
                            return false;
                    });
                    task.AddOnExitAction(delegate {
                        float[] arr = GetParamArray("hpRate");
                        if (arr.Length > 3)
                        {
                            mHertIndex = 4;
                            OnHertStageChanged();
                        }
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    private void CreateS3Boss0(float dmg_trans)
    {
        BossUnit b = GameController.Instance.CreateBossUnit(6, 3, new BaseEnemyGroup.EnemyInfo { type = (int)BossNameTypeMap.SteelClawPete, shape = 0 }, mMaxHp);
        b.transform.position = transform.position;
        b.SetUseDefaultRecieveDamageActionMethod(false); // 不受正常受击逻辑
        // 受击伤害传递
        b.actionPointController.AddListener(ActionPointType.WhenReceiveDamage, (c) => {
            DamageAction d = DamageActionManager.Copy((c as DamageAction), c.Creator, this);
            d.DamageValue = d.DamageValue * dmg_trans;
            d.ApplyAction();
        });
        // 重要属性同步
        b.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        b.NumericBox.Defense.AddGetValueFunc(999, delegate { return NumericBox.Defense.Value; });
        b.SetGetAoeRateFunc(delegate { return mAoeRate; });
        b.SetGetBurnRateFunc(delegate { return mBurnRate; });
        b.SetGetDamageRateFunc(delegate { return mDamgeRate; });
        // 重置种子，使之与金刚初始位置相关
        b.SetRandSeedByRowIndex(GetSeedValue()); // 设置BOSS的种子生成器
        // 设置阶段点
        b.mHertIndex = 0;
        b.AddParamArray("hpRate", new float[] { 1f, -1f });
        b.UpdateHertMap();
        Pete = b;
    }

    private void CreateS3Boss1(float dmg_trans)
    {
        BossUnit b = GameController.Instance.CreateBossUnit(6, 1, new BaseEnemyGroup.EnemyInfo { type = (int)BossNameTypeMap.MistyJulie, shape = 0 }, mMaxHp);
        b.transform.position = transform.position + new Vector3(0, MapManager.gridHeight * 1.5f);
        b.SetUseDefaultRecieveDamageActionMethod(false); // 不受正常受击逻辑
        // 受击伤害传递
        b.actionPointController.AddListener(ActionPointType.WhenReceiveDamage, (c) => {
            DamageAction d = DamageActionManager.Copy((c as DamageAction), c.Creator, this);
            d.DamageValue = d.DamageValue * dmg_trans;
            d.ApplyAction();
        });
        // 重要属性同步
        b.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        b.NumericBox.Defense.AddGetValueFunc(999, delegate { return NumericBox.Defense.Value; });
        b.SetGetAoeRateFunc(delegate { return mAoeRate; });
        b.SetGetBurnRateFunc(delegate { return mBurnRate; });
        b.SetGetDamageRateFunc(delegate { return mDamgeRate; });
        // 重置种子，使之与金刚初始位置相关
        b.SetRandSeedByRowIndex(GetSeedValue()); // 设置BOSS的种子生成器
        // 设置阶段点
        b.mHertIndex = 0;
        b.AddParamArray("hpRate", new float[] { 1f, -1f });
        b.UpdateHertMap();
        Julie = b;
    }
    #endregion

    #region 五技能
    /// <summary>
    /// 火山爆发
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill4Init(SkillAbility.SkillAbilityInfo info)
    {
        float[] right_colArray = GetParamArray("right_col4");
        float rowIndex = GetParamValue("row4") - 1;

        int interval = Mathf.FloorToInt(60 * GetParamValue("interval4"));
        int left_columnIndex = Mathf.FloorToInt(GetParamValue("left_column4")) - 1; // 最左下标限制

        int timeLeft = interval;
        int lava_columnIndex = 8;

        float dmg = GetParamValue("dmg4");
        BoolModifier mod = new BoolModifier(true);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
            float colIndex = 9 - right_colArray[GetRandomNext(0, right_colArray.Length)];
            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
            OpenCollision();
            SetAlpha(1);
        };
        {
            {
                // 出现
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Appear", out task);
                    return task;
                });
                // 施法
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PreCast", out task);
                    task.AddOnExitAction(delegate {
                        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
                        animatorController.Play("Casting", true);
                        // 场上存在的老鼠获得岩浆抗性
                        FloatModifier lavaRateMod = new FloatModifier(1 - GetParamValue("lava_defence4") * 0.01f);
                        foreach (var u in GameController.Instance.GetEachEnemy())
                        {
                            if (u.IsAlive())
                                LavaTask.AddUnitLavaRate(u, lavaRateMod);
                        }
                        // 以后出现的老鼠也会获得岩浆抗性
                        GameController.Instance.mMouseFactory.AddProcessAction(mouseProcessAction);
                    });
                    return task;
                });
            }

            
            {
                // 持续放置岩浆
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        timeLeft = 0;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            if(lava_columnIndex >= left_columnIndex)
                            {
                                // 生成一列岩浆
                                for (int i = 0; i < 7; i++)
                                {
                                    CreateLavaArea(MapManager.GetGridLocalPosition(lava_columnIndex, i), int.MaxValue);
                                }
                                if(lava_columnIndex == left_columnIndex)
                                    timeLeft += 30;
                                else
                                    timeLeft += interval;
                                lava_columnIndex--;
                            }
                            else
                            {
                                foreach (var u in GameController.Instance.GetEachAlly())
                                {
                                    if (u is CottonCandy)
                                        new DamageAction(CombatAction.ActionType.BurnDamage, null, u, dmg).ApplyAction();
                                }
                                timeLeft += 30;
                            }
                        }
                        return false;
                    });
                    return task;
                });
                // 施法结束
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task;
                    CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "PostCast", out task);
                    task.AddOnExitAction(delegate {
                        GameController.Instance.mMouseFactory.RemoveProcessAction(mouseProcessAction);
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion
}
