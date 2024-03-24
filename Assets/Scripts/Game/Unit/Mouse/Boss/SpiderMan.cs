using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;

public class SpiderMan : BossUnit
{
    private static RuntimeAnimatorController Smoke_Run;
    private static RuntimeAnimatorController Swing_Run;
    private static RuntimeAnimatorController WebBullet;
    private static RuntimeAnimatorController Trap_Run;
    private static Sprite FireWeight_Icon_Sprite;
    private const string TrappedKey = "SpiderMan_Trapped"; // 被蜘蛛陷阱困住的key
    
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
    private static Func<BaseUnit, BaseUnit, bool> noSelcetedFunc = delegate { return false; };

    public override void Awake()
    {
        if (Smoke_Run == null)
        {
            Smoke_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/18/Smoke");
            Swing_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/18/Swing");
            WebBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/18/WebBullet");
            Trap_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/18/Trap");
            FireWeight_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/FireWeight");
        }
        base.Awake();
    }

    private List<BaseUnit> trapList = new List<BaseUnit>(); // 陷阱表
    private List<FoodUnit> trappedUnitList = new List<FoodUnit>(); // 中了陷阱的卡片表
    private RingUI FireWeightRUI; // 火苗重量图标
    private RingUI FinalSkillRingUI; // 大招条图标

    private float dmgRecord; // 受到的伤害总和
    private Queue<float> lost_hp_percent_queue = new Queue<float>();
    private float current_lost_hp_percent;
    private int finalSkillLeft = 0; // 剩余大招数

    public override void MInit()
    {
        FireWeightRUI = null;
        FinalSkillRingUI = null;
        trapList.Clear();
        trappedUnitList.Clear();
        finalSkillLeft = 0;
        base.MInit();

        // 火苗UI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            // 添加绑定任务
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    rUI.Show();
                    rUI.SetIcon(FireWeight_Icon_Sprite);
                    rUI.SetPercent(0);
                    rUI.SetColor(new Color(1, 1, 1, 0.75f));
                });
                task.AddTaskFunc(delegate {
                    rUI.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.up;
                    float per = rUI.GetPercent();
                    if (per < 1)
                        rUI.SetColor(new Color(1, 1 - per, 1 - per, 0.75f));
                    else
                    {
                        float rate = (GameController.Instance.GetCurrentStageFrame() / 8) % 2;
                        rUI.SetColor(new Color(1, 0.5f * rate, 0.5f * rate, 1));
                    }
                    return !IsAlive();
                });
                task.AddOnExitAction(delegate {
                    rUI.MDestory();
                });
                taskController.AddTask(task);
            }
            rUI.Hide();
            rUI.SetPercent(0);
            AddOnDestoryAction(delegate { if (rUI.IsValid()) rUI.MDestory(); });
            FireWeightRUI = rUI;
        }
        // 大招UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this, 0.25f * MapManager.gridHeight * Vector3.down));
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(1);
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // 挨打与大招事件
        actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) =>
        {
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
                    finalSkillLeft++;
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

        // 界定可被选为攻击目标的范围
        AddCanBeSelectedAsTargetFunc(delegate {
            return transform.position.y < MapManager.GetRowY(-1) && transform.position.y > MapManager.GetRowY(7) && transform.position.x < MapManager.GetColumnX(9) && transform.position.x > MapManager.GetColumnX(0);
        });
        // 添加出现的技能
        {
            SetAlpha(0);
            mHeight = 1;
            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 0;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Move", true);
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                // 第一帧强制把纵坐标同步至-3行
                if(current_lost_hp_percent != 9999)
                {
                    FinalSkillRingUI.Show();
                    FinalSkillRingUI.SetPercent(0);
                }
                transform.position = MapManager.GetGridLocalPosition(8, -3);
                SetAlpha(1);
                timeLeft = 120;
                return true;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                    return true;
                }
                return false;
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    public override void MUpdate()
    {
        {
            List<BaseUnit> delList = new List<BaseUnit>();
            foreach (var u in trapList)
            {
                if (!u.IsAlive())
                    delList.Add(u);
            }
            foreach (var u in delList)
            {
                trapList.Remove(u);
            }
        }

        {
            List<FoodUnit> delList = new List<FoodUnit>();
            foreach (var u in trappedUnitList)
            {
                if (!u.IsAlive())
                    delList.Add(u);
            }
            foreach (var u in delList)
            {
                trappedUnitList.Remove(u);
            }
        }
        base.MUpdate();

    }

    public override void BeforeDeath()
    {
        SetAlpha(1);
        foreach (var u in trapList)
            u.ExecuteDeath();
        trapList.Clear();
        base.BeforeDeath();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.SpiderMan, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        // 获取大招损失生命值条件
        {
            Action<float[]> action = delegate
            {
                float[] arr = GetParamArray("p_lost_hp_percent");
                lost_hp_percent_queue.Clear();
                foreach (var val in arr)
                    lost_hp_percent_queue.Enqueue(val / 100);
                if (lost_hp_percent_queue.Count > 0)
                {
                    current_lost_hp_percent = lost_hp_percent_queue.Dequeue();
                }
                else
                {
                    current_lost_hp_percent = 9999;
                }
            };
            AddParamChangeAction("p_lost_hp_percent", action);
            action(null);
        }
    }
    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
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
        //list.Add(SKill2Init(infoList[2]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 生成一个蜘蛛网
    /// </summary>
    /// <param name="pos"></param>
    private BaseUnit CreateTrap(Vector2 pos, float burn_rate)
    {
        // 实体部分
        MouseModel m = MouseModel.GetInstance(Trap_Run);
        {
            trapList.Add(m);
            m.transform.position = pos;
            m.SetBaseAttribute(10000, 10, 1.0f, 0.0f, 100, 0.5f, 1);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
            m.canTriggerCat = false;
            m.canTriggerLoseWhenEnterLoseLine = false;
            m.isIgnoreRecordDamage = true;
            StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
            StatusManager.AddIgnoreSlowDownBuff(m, new BoolModifier(true));
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
            m.AddCanBlockFunc(delegate { return false; });
            m.AddCanHitFunc(delegate { return false; });
            m.IdleClipName = "Idle";
            m.MoveClipName = "Idle";
            m.AttackClipName = "Idle";
            m.DieClipName = "Disappear";
            m.currentYIndex = MapManager.GetYIndex(pos.y);
            m.SetActionState(new MoveState(m));
            GameController.Instance.AddMouseUnit(m);

            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                m.animatorController.Play("Appear");
            });
            task.AddTaskFunc(delegate {
                return (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
            });
            task.AddOnExitAction(delegate {
                m.animatorController.Play("Idle");
            });
            m.taskController.AddTask(task);
        }

        // 判定部分
        RetangleAreaEffectExecution r = null;
        {
            FloatModifier moveSpeedMod = new FloatModifier(-GetParamValue("p_dec_move_speed"));
            FloatModifier attackMod = new FloatModifier(-GetParamValue("p_dec_attack"));
            IntModifier trapMod = new IntModifier(1);

            Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };

            Action<BaseUnit> unitEnterAction = (u) =>
            {
                u.NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedMod);
                u.NumericBox.Attack.AddFinalPctAddModifier(attackMod);
            };

            Action<BaseUnit> unitExitAction = (u) =>
            {
                u.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedMod);
                u.NumericBox.Attack.RemoveFinalPctAddModifier(attackMod);
            };

            r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "BothCollide");
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodEnterAction((u)=> {
                unitEnterAction(u);
                if(!u.NumericBox.IntDict.ContainsKey(TrappedKey))
                    u.NumericBox.IntDict.Add(TrappedKey, new IntNumeric());
                if (u.NumericBox.IntDict[TrappedKey].Value <= 0)
                    trappedUnitList.Add(u);
                u.NumericBox.IntDict[TrappedKey].AddAddModifier(trapMod);
                u.AddCanBlockFunc(noBlockFunc);
            });
            r.SetOnFoodExitAction((u)=> {
                unitExitAction(u);
                if (u.NumericBox.IntDict.ContainsKey(TrappedKey))
                {
                    u.NumericBox.IntDict[TrappedKey].RemoveAddModifier(trapMod);
                    if (u.NumericBox.IntDict[TrappedKey].Value <= 0)
                        trappedUnitList.Remove(u);
                }
                u.RemoveCanBlockFunc(noBlockFunc);
            });
            r.SetOnEnemyEnterAction(unitEnterAction);
            r.SetOnEnemyStayAction((u) => {
                if (u.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
                {
                    if (u.NumericBox.MoveSpeed.FinalPctAddCollector.GetModifierList().Contains(moveSpeedMod))
                        u.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedMod);
                }
                else
                {
                    if (!u.NumericBox.MoveSpeed.FinalPctAddCollector.GetModifierList().Contains(moveSpeedMod))
                        u.NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedMod);
                }
            });
            r.SetOnEnemyExitAction(unitExitAction);
            r.AddExcludeMouseUnit(m);
            GameController.Instance.AddAreaEffectExecution(r);


            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = m.transform.position;
                return !m.IsAlive();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
        return m;
    }


    #region 一技能
    /// <summary>
    /// 生成一个蜘蛛线
    /// </summary>
    private void CreateS0Web(Vector2 pos)
    {
        float min_x = MapManager.GetColumnX(GetParamValue("left_col0") - 1);
        int num = Mathf.FloorToInt(GetParamValue("num0"));
        float burn_rate = 1 - GetParamValue("p_burn_defence") / 100;
        Vector3[] vArray = new Vector3[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down };

        // 显示部分
        BaseEffect e = BaseEffect.CreateInstance(WebBullet, null, "Fly", "Hit", true);
        {
            e.transform.position = pos;
            e.AddBeforeDeathAction(delegate {
                if(IsAlive())
                {
                    CreateTrap(e.transform.position, burn_rate);
                    for (int i = 1; i <= num; i++)
                    {
                        foreach (var v in vArray)
                        {
                            Vector3 pos = e.transform.position + v * MapManager.gridHeight * i;
                            if(pos.x >= MapManager.GetColumnX(-0.5f) && pos.x <= MapManager.GetColumnX(9) && pos.y <= MapManager.GetRowY(0) && pos.y >= MapManager.GetRowY(6))
                                CreateTrap(pos, burn_rate);
                        }
                    }
                }
            });
            GameController.Instance.AddEffect(e);

            float v = MapManager.gridWidth / 15;
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                e.transform.position += v*Vector3.left;
                return e.transform.position.x <= min_x;
            });
            task.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.taskController.AddTask(task);
        }

        // 判定部分
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
            r.isAffectFood = true;
            r.AddFoodEnterConditionFunc((u) => {
                return !u.NumericBox.IntDict.ContainsKey(TrappedKey) || u.NumericBox.IntDict[TrappedKey].Value <= 0;
            });
            r.SetOnFoodEnterAction((u) => {
                e.ExecuteDeath();
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = e.transform.position;
                return !e.IsValid();
            });
            r.taskController.AddTask(task);
        }
    }

    private List<int> FindS0RowIndexList()
    {
        float max = float.MinValue;
        List<int> rowIndexlist = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        List<int> list = new List<int>();
        foreach (var rowIndex in rowIndexlist)
        {
            BaseUnit u = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, MapManager.GetColumnX(-1), false);
            if (u != null)
            {
                if(u.transform.position.x > max)
                {
                    max = u.transform.position.x;
                    list.Clear();
                    list.Add(rowIndex);
                }else if(u.transform.position.x == max)
                {
                    list.Add(rowIndex);
                }
            }
            else if(max == float.MinValue)
            {
                list.Add(rowIndex);
            }
        }

        int max_index = list[GetRandomNext(0, list.Count)];
        rowIndexlist.Remove(max_index);

        float min = float.MaxValue;
        foreach (var rowIndex in rowIndexlist)
        {
            BaseUnit u = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, MapManager.GetColumnX(-1), false);
            if (u != null)
            {
                if (u.transform.position.x < min)
                {
                    min = u.transform.position.x;
                    list.Clear();
                    list.Add(rowIndex);
                }
                else if (u.transform.position.x == min)
                {
                    list.Add(rowIndex);
                }
            }
            else
            {
                if(min > float.MinValue)
                {
                    min = float.MinValue;
                    list.Clear();
                    list.Add(rowIndex);
                }else if(min == float.MinValue)
                    list.Add(rowIndex);
            }
        }
        int min_index = list[GetRandomNext(0, list.Count)];
        List<int> l = new List<int>() { max_index, min_index };
        l.Sort();
        return l;
    }

    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        float colIndex = 9 - GetParamValue("right_col0");
        int move_time = Mathf.FloorToInt(GetParamValue("move_time0") * 60);
        int wait = Mathf.FloorToInt(GetParamValue("wait0") * 60);

        // 变量
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        List<int> rowIndexList = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate 
        {
            rowIndexList = FindS0RowIndexList();
            transform.position = MapManager.GetGridLocalPosition(colIndex, -3);
        };
        {
            // 放两次网
            for (int _i = 0; _i < 2; _i++)
            {
                int i = _i;
                c.AddCreateTaskFunc(delegate
                {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate
                    {
                        startPos = transform.position;
                        endPos = MapManager.GetGridLocalPosition(colIndex, rowIndexList[i]);
                        timeLeft = move_time;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        float rate = 1 - (float)timeLeft / move_time;
                        transform.position = Vector2.Lerp(startPos, endPos, rate);
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Idle", true);
                            timeLeft = wait;
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Shoot");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.41f)
                        {
                            CreateS0Web(transform.position);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                    });
                    return task;
                });
            }
            // 结束，润走
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Move", true);
                    startPos = transform.position;
                    endPos = MapManager.GetGridLocalPosition(colIndex, -3);
                    timeLeft = move_time;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate = 1 - (float)timeLeft / move_time;
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    return timeLeft <= 0;
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion

    #region 二技能
    private List<BaseUnit> FindS1NearestTrapList()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        float min = float.MaxValue;
        foreach (var u in trapList)
        {
            float dist = (u.transform.position - transform.position).magnitude;
            if (dist < min)
            {
                min = dist;
                list.Clear();
                list.Add(u);
            }
            else if (dist == min)
                list.Add(u);
        }
        return list;
    }

    private void CreateS1ChangeArea()
    {
        int type = Mathf.FloorToInt(GetParamValue("type1"));
        int shape = Mathf.FloorToInt(GetParamValue("shape1"));
        int stun_time = Mathf.FloorToInt(GetParamValue("enemy_stun1")*60);

        // 保底生成一只老鼠
        List<MouseUnit> baodiList = new List<MouseUnit>();
        for (int i = 0; i < 1; i++)
        {
            // 召唤特效
            BaseEffect e = BaseEffect.CreateInstance(Smoke_Run, null, "Appear", null, false);
            e.SetSpriteRendererSorting("Effect", 2);
            e.transform.position = transform.position;
            GameController.Instance.AddEffect(e);

            MouseUnit new_mouse = GameController.Instance.mMouseFactory.GetMouse(type, shape);
            new_mouse.transform.position = transform.position;
            new_mouse.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(new_mouse, stun_time, false));
            GameController.Instance.AddMouseUnit(new_mouse);
        }
        // 对其他老鼠单位
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
            r.SetInstantaneous();
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            foreach (var m in baodiList)
                r.AddExcludeMouseUnit(m);
            r.AddEnemyEnterConditionFunc((m) => {
                return !m.IsBoss() && MouseManager.IsGeneralMouse(m);
            });
            r.AddBeforeDestoryAction(delegate {
                foreach (var m in r.mouseUnitList.ToArray())
                {
                    // 召唤特效
                    BaseEffect e = BaseEffect.CreateInstance(Smoke_Run, null, "Appear", null, false);
                    e.SetSpriteRendererSorting("Effect", 2);
                    e.transform.position = m.transform.position;
                    GameController.Instance.AddEffect(e);

                    MouseUnit new_mouse = GameController.Instance.mMouseFactory.GetMouse(type, shape);
                    new_mouse.transform.position = m.transform.position;
                    new_mouse.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(new_mouse, stun_time, false));
                    GameController.Instance.AddMouseUnit(new_mouse);
                    m.MDestory();
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int rowIndex = Mathf.FloorToInt(GetParamValue("row1")-1);
        int colIndex = Mathf.FloorToInt(9 - GetParamValue("right_col1"));
        float totalCost = GetParamValue("cost1");
        int move_time = Mathf.FloorToInt(GetParamValue("move_time1") * 60);
        int wait = Mathf.FloorToInt(GetParamValue("wait1")*60);
        int stun_time = Mathf.FloorToInt(GetParamValue("stun1") * 60);

        // 变量
        float cost = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        BaseUnit target = null;
        bool canExit = false;
        CustomizationState state = null;

        // 找、移动、偷三个状态定义
        Func<CustomizationState> FindTargetStateFunc = null;
        Func<CustomizationState> MoveToTargetStateFunc = null;
        Func<CustomizationState> WaitStateFunc = null;
        Func<CustomizationState> StealStateFunc = null;
        Func<CustomizationState> DropStateFunc = null;
        Func<CustomizationState> StunStateFunc = null;
        Func<CustomizationState> PostStunStateFunc = null;
        {
            FindTargetStateFunc = delegate {
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    if(cost >= totalCost)
                    {
                        CustomizationTask task = TaskManager.GetRingUIChangeTask(FireWeightRUI, 60, 1, 0);
                        task.AddOnExitAction(delegate {
                            FireWeightRUI.Hide();
                        });
                        FireWeightRUI.mTaskController.AddTask(task);
                        state = DropStateFunc();
                        state.OnEnter();
                    }
                    else
                    {
                        List<BaseUnit> targetList = FindS1NearestTrapList();
                        if (targetList.Count <= 0 || finalSkillLeft > 0)
                        {
                            canExit = true;
                        }
                        else
                        {
                            target = targetList[GetRandomNext(0, targetList.Count)];
                            state = MoveToTargetStateFunc();
                            state.OnEnter();
                        }
                    }
                });
                return s;
            };

            MoveToTargetStateFunc = delegate {
                Vector2 startPos = Vector2.zero;
                Vector2 endPos = Vector2.zero;
                animatorController.Play("Move", true);
                int timeLeft = move_time;
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    startPos = transform.position;
                    endPos = target.transform.position;
                });
                s.AddOnUpdateAction(delegate {
                    timeLeft--;
                    float rate = 1 - (float)timeLeft / move_time;
                    if (target != null && target.IsAlive())
                        endPos = target.transform.position;
                    else
                        target = null;
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        state = WaitStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };

            WaitStateFunc = delegate {
                int timeLeft = wait;
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
                });
                s.AddOnUpdateAction(delegate {
                    timeLeft--;
                    if(timeLeft <= 0)
                    {
                        state = StealStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };

            StealStateFunc = delegate {
                bool flag = true;
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    animatorController.Play("Steal");
                });
                s.AddOnUpdateAction(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5f && flag)
                    {
                        flag = false;
                        // 调换本格的东西
                        CreateS1ChangeArea();
                        if (target != null)
                        {
                            target.ExecuteDeath();
                            trapList.Remove(target);
                        }
                        // 处决一格的美食单位
                        {
                            Vector2 pos = new Vector2(transform.position.x, MapManager.GetRowY(MapManager.GetYIndex(transform.position.y)));
                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f*MapManager.gridWidth, 0.5f*MapManager.gridHeight), "ItemCollideAlly");
                            r.SetInstantaneous();
                            r.isAffectFood = true;
                            r.AddFoodEnterConditionFunc((f) => {
                                return FoodManager.IsAttackableFoodType(f) && f.GetFoodInGridType() != FoodInGridType.Bomb;
                            });
                            r.SetOnFoodEnterAction((f) => 
                            {
                                cost += (float)GameManager.Instance.attributeManager.GetCardBuilderAttribute(f.mType, f.mShape).GetCost(f.mLevel);
                                f.ExecuteDeath();
                                FireWeightRUI.SetPercent(cost/totalCost);
                            });
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        state = FindTargetStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };

            DropStateFunc = delegate {
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    animatorController.Play("Drop");
                });
                s.AddOnUpdateAction(delegate {
                    if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        state = StunStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };

            StunStateFunc = delegate {
                int timeLeft = stun_time;
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    animatorController.Play("Stun");
                    mHeight = 0;
                });
                s.AddOnUpdateAction(delegate {
                    timeLeft--;
                    if (timeLeft <= 0 || finalSkillLeft > 0)
                    {
                        state = PostStunStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };

            PostStunStateFunc = delegate {
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    animatorController.Play("AfterStun");
                });
                s.AddOnUpdateAction(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        mHeight = 1;
                        canExit = true;
                    }
                });
                return s;
            };
        }


        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            startPos = MapManager.GetGridLocalPosition(colIndex, -3);
            endPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
            animatorController.Play("Move", true);
            canExit = false;
            cost = 0;
            FireWeightRUI.Show();
            FireWeightRUI.SetPercent(0);
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTimeTaskFunc(60, null, (leftTime, totalTime) => {
                    float rate = 1 - (float)leftTime / totalTime;
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                }, null);
                task.AddOnExitAction(delegate {
                    state = FindTargetStateFunc();
                    state.OnEnter();
                });
                return task;
            });
            c.AddSpellingFunc(delegate {
                state.OnUpdate();
                return canExit;
            });
            // 结束，润走
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Move", true);
                    startPos = transform.position;
                    endPos = MapManager.GetGridLocalPosition(colIndex, -3);
                });
                task.AddTimeTaskFunc(move_time, null, (leftTime, totalTime) => {
                    float rate = 1 - (float)leftTime / totalTime;
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                }, null);
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate {
            FireWeightRUI.Hide();
        };
        return c;
    }
    #endregion

    #region 三技能
    private Queue<int> GetS2RowIndexQueue()
    {
        int count = Mathf.FloorToInt(GetParamValue("num2"));
        

        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        // 根据卡的数量升序排序
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

        while (list.Count > count)
            list.RemoveAt(list.Count - 1);

        // 再根据行数升序排序
        list.Sort((x, y) => {
            return x.CompareTo(y);
        });
        Queue<int> queue = new Queue<int>();
        foreach (var item in list)
            queue.Enqueue(item);
        return queue;
    }

    private BaseEffect CreateS2Model()
    {
        BaseEffect e = BaseEffect.CreateInstance(Swing_Run, null, "Idle", null, true);
        e.transform.position = MapManager.GetGridLocalPosition(0, -3);
        GameController.Instance.AddEffect(e);

        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            return !IsAlive();
        });
        e.taskController.AddTask(task);
        return e;
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        float colIndex = GetParamValue("left_col2") - 1;
        float radius = 7*MapManager.gridHeight;
        int fly_time = 120;
        int type = Mathf.FloorToInt(GetParamValue("type2"));
        int shape = Mathf.FloorToInt(GetParamValue("shape2"));
        float burn_rate = 1 - GetParamValue("burn_defence2")/100;
        int stun_time = Mathf.FloorToInt(GetParamValue("enemy_stun2") * 60);

        // 变量
        BaseEffect eff = null;
        Queue<int> queue = null;
        CustomizationState state = null;
        BaseUnit target = null;
        Vector2 center = Vector2.zero;
        int sign = 1;
        bool canExit = false;

        Func<CustomizationState> FindTargetStateFunc = null;
        Func<CustomizationState> MoveToTargetStateFunc = null;
        {
            FindTargetStateFunc = delegate {
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    if(queue.Count > 0)
                    {
                        int rowIndex = queue.Dequeue();
                        target = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(rowIndex, float.MaxValue, false);
                        if (target != null)
                            center = new Vector3(Mathf.Max(MapManager.GetColumnX(colIndex), target.transform.position.x), MapManager.GetRowY(rowIndex))  + radius*Vector3.up;
                        else
                            center = MapManager.GetGridLocalPosition(colIndex, rowIndex) + radius * Vector3.up;
                        eff.transform.position = center;
                        state = MoveToTargetStateFunc();
                        state.OnEnter();
                    }
                    else
                    {
                        canExit = true;
                    }
                });
                return s;
            };

            MoveToTargetStateFunc = delegate 
            {
                float start = 0;
                float end = 0;
                int timeLeft = fly_time;
                bool flag = true;
                CustomizationState s = new CustomizationState();
                s.AddOnEnterAction(delegate {
                    if(sign == -1)
                    {
                        start = 0;
                        end = -Mathf.PI;
                    }
                    else
                    {
                        start = -Mathf.PI;
                        end = 0;
                    }
                    flag = true;
                    sign = -sign;
                    timeLeft = fly_time;
                });
                s.AddOnUpdateAction(delegate {
                    timeLeft--;
                    float rate = 1 - (float)timeLeft / fly_time;
                    float rad = Mathf.Lerp(start, end, rate);
                    transform.position = center + radius * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    eff.transform.right = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    if(rate >= 0.5f && flag)
                    {
                        flag = false;
                        Vector2 pos = new Vector2(transform.position.x, MapManager.GetRowY(MapManager.GetYIndex(transform.position.y)));
                        CreateTrap(pos, burn_rate);
                        {
                            // 召唤特效
                            BaseEffect e = BaseEffect.CreateInstance(Smoke_Run, null, "Appear", null, false);
                            e.SetSpriteRendererSorting("Effect", 2);
                            e.transform.position = transform.position;
                            GameController.Instance.AddEffect(e);

                            MouseUnit new_mouse = GameController.Instance.mMouseFactory.GetMouse(type, shape);
                            new_mouse.transform.position = transform.position;
                            new_mouse.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(new_mouse, stun_time, false));
                            GameController.Instance.AddMouseUnit(new_mouse);
                        }
                    }
                    
                    if (timeLeft <= 0)
                    {
                        state = FindTargetStateFunc();
                        state.OnEnter();
                    }
                });
                return s;
            };
        }

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            eff = CreateS2Model();
            queue = GetS2RowIndexQueue();
            canExit = false;
            SetAlpha(0);

            state = FindTargetStateFunc();
            state.OnEnter();
            finalSkillLeft--;
            AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
        };
        {
            c.AddSpellingFunc(delegate {
                state.OnUpdate();
                return canExit;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate 
        {
            SetAlpha(1);
            if (current_lost_hp_percent < 9999)
                FinalSkillRingUI.Show();
            if (eff != null)
                eff.ExecuteDeath();
            RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
        };
        return c;
    }
    #endregion

    #region 其他
    #endregion
}
