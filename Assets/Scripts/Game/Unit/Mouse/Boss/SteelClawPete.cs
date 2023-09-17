using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 钢爪皮特
/// </summary>
public class SteelClawPete : BossUnit
{
    private static RuntimeAnimatorController Bullet_AnimatorController;
    private static RuntimeAnimatorController Barrier_AnimatorController;
    private static RuntimeAnimatorController Trap_AnimatorController;

    private const string Trap_Key = "SteelClawPete_Trap";
    private List<BaseUnit> trapList = new List<BaseUnit>();

    public override void Awake()
    {
        if (Bullet_AnimatorController == null)
        {
            Bullet_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/7/Bullet");
            Barrier_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/7/Barrier");
            Trap_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/7/Trap");
        }
        base.Awake();
    }

    public override void MInit()
    {
        trapList.Clear();
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
                animatorController.Play("Appear");
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
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
        foreach (var u in trapList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            trapList.Remove(u);
        }
        base.MUpdate();
    }

    public override void BeforeDeath()
    {
        foreach (var u in trapList)
        {
            u.ExecuteDeath();
        }
        trapList.Clear();
        base.BeforeDeath();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.SteelClawPete, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
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
        list.Add(SKill3Init(infoList[3]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 生成一个钢爪陷阱
    /// </summary>
    private void CreateTrap(BaseGrid g)
    {
        if (g == null || g.IsContainUnit(Trap_Key))
            return;

        bool isTrigger = false;

        MouseModel m = MouseModel.GetInstance(Trap_AnimatorController);
        trapList.Add(m);
        m.transform.position = g.transform.position;
        m.SetBaseAttribute(1, 10, 1.0f, 0, 100, 0.5f, 0);
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.canDrivenAway = false;
        m.isIgnoreRecordDamage = true;
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        m.MoveClipName = "Prepare";
        m.DieClipName = "Attack";
        m.IdleClipName = "Prepare";
        m.AttackClipName = "Attack";
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // 标记免疫水蚀
        m.SetActionState(new IdleState(m));
        GameController.Instance.AddMouseUnit(m);
        g.AddUnitToDict(Trap_Key, m);

        // 产生一个美食判定区域，检测到该区域的第一个美食会触发陷阱
        Action createCheckArea = delegate
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 0.5f, 0.5f, "ItemCollideAlly");
            r.isAffectFood = true;
            Action<BaseUnit> stayAction = (u) =>
            {
                if (!isTrigger && UnitManager.CanBeSelectedAsTarget(m, u))
                {
                    isTrigger = true;
                }
            };
            r.SetOnFoodEnterAction(stayAction);
            r.SetOnFoodStayAction(stayAction);
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (m.IsAlive())
                {
                    r.transform.position = m.transform.position;
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

        // 自身行为
        {
            int timeLeft = Mathf.FloorToInt(GetParamValue("prepareTime", mHertIndex) * 60);
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.animatorController.Play("Prepare", true);
            });
            t.AddTaskFunc(delegate {
                m.transform.position = g.transform.position;
                timeLeft--;
                if (timeLeft <= 0)
                {
                    createCheckArea();
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                m.transform.position = g.transform.position;
                if (isTrigger)
                {
                    m.animatorController.Play("Attack");
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                m.transform.position = g.transform.position;
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 处决本格所有可攻击美食
                    foreach (var f in g.GetAttackableFoodUnitList())
                    {
                        if (FoodManager.IsAttackableFoodType(f))
                            new DamageAction(CombatAction.ActionType.RealDamage, m, f, f.GetCurrentHp()).ApplyAction();
                    }
                    // 处决老鼠
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 1, 1, "ItemCollideEnemy");
                        r.isAffectMouse = true;
                        r.SetAffectHeight(0);
                        r.SetOnEnemyEnterAction((mouse) =>{
                            if (!UnitManager.CanBeSelectedAsTarget(m, mouse))
                                return;
                            if(mouse.IsBoss())
                                new DamageAction(CombatAction.ActionType.CauseDamage, m, mouse, GetParamValue("dmg", mHertIndex)).ApplyAction();
                            else
                                new DamageAction(CombatAction.ActionType.CauseDamage, m, mouse, mouse.GetCurrentHp()).ApplyAction();
                        });
                        r.SetInstantaneous();
                        GameController.Instance.AddAreaEffectExecution(r);
                    }
                    // 如果本格有人物则对人物造成晕眩
                    if (g.IsContainCharacter())
                    {
                        CharacterUnit c = g.GetCharacterUnit();
                        c.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(c, Mathf.FloorToInt(GetParamValue("stun", mHertIndex) * 60), false));
                    }
                    return true;
                }
                return false;
            });
            t.AddOnExitAction(delegate {
                m.DeathEvent();
            });
            m.AddTask(t);
        }
    }

    /// <summary>
    /// 发射一发导弹
    /// </summary>
    private void CreateMissile(BaseUnit target)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Bullet_AnimatorController, this, 0);
        b.isnDelOutOfBound = true;
        // 修改攻击优先级，这种投掷攻击优先攻击护罩里的东西
        b.GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetThrowHighestAttackPriorityUnitInclude(this);
            }
            return unit;
        };
        b.AddHitAction((b, u) => {
            if (u is CharacterUnit)
            {
                // 如果击中人物，则为人物施加晕眩效果
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, Mathf.FloorToInt(GetParamValue("stun2", mHertIndex) * 60), false));
            }
            else
            {
                // new DamageAction(CombatAction.ActionType.BurnDamage, this, u, GetParamValue("dmg2", mHertIndex)).ApplyAction();
                if(u!=null && u.IsAlive())
                    BurnManager.BurnDamage(this, u);
            }
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        });
        int t = 120;
        Vector2 startPos = transform.position + 2*MapManager.gridHeight*Vector3.up;
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, t, 2.5f, startPos, target, true));
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// 生成金刚之刺
    /// </summary>
    private BaseUnit CreateBarrier(BaseGrid g)
    {
        if (g == null)
            return null;

        MouseModel m = MouseModel.GetInstance(Barrier_AnimatorController);
        m.transform.position = g.transform.position;
        m.SetBaseAttribute(mMaxHp * mBurnRate, 10.0f, 1.0f, 0, 100, 0.5f, 0);
        m.NumericBox.BurnRate.AddModifier(new FloatModifier(1-0.01f*GetParamValue("burn_defence3")));
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.canDrivenAway = false;
        m.isIgnoreRecordDamage = true;
        m.AddCanBlockFunc(delegate { return false; });
        m.IdleClipName = "Idle";
        m.AttackClipName = "Idle";
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // 标记免疫水蚀
        m.SetActionState(new IdleState(m));
        Action<CombatAction> hitAction = (com) => {
            if (com is DamageAction)
            {
                DamageAction d = com as DamageAction;
                new DamageAction(CombatAction.ActionType.CauseDamage, d.Creator, this, GetParamValue("tran_rate3", mHertIndex) * 0.01f * d.DamageValue).ApplyAction();
            }
        };
        m.AddActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
        GameController.Instance.AddMouseUnit(m);

        // 自身的行为动作
        {
            int timeLeft = Mathf.FloorToInt(GetParamValue("t3", mHertIndex) * 60);

            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.animatorController.Play("Appear");
            });
            t.AddTaskFunc(delegate {
                m.animatorController.Play("Appear");
                return true;
            });
            t.AddTaskFunc(delegate {
                m.transform.position = g.transform.position;
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                m.transform.position = g.transform.position;
                timeLeft--;
                if (timeLeft <= 0)
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                m.ExecuteDeath();
            });
            m.AddTask(t);
        }

        // 生成的瞬间对其上的美食与老鼠造成伤害
        {
            DamageAreaEffectExecution d = DamageAreaEffectExecution.GetInstance(m, g.transform.position, 1, 1, CombatAction.ActionType.CauseDamage, GetParamValue("dmg3", mHertIndex));
            d.AddExcludeMouseUnit(m);
            d.AddExcludeMouseUnit(this);
            d.isAffectFood = true;
            d.isAffectMouse = true;
            d.isAffectCharacter = false;
            GameController.Instance.AddAreaEffectExecution(d);
        }
        return m;
    }

    private void FindR0(out int R0)
    {
        // 获取右侧四列非17行的所有格子
        List<BaseGrid> gList = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(),
            MapManager.GetColumnX(4.5f), MapManager.GetColumnX(5.5f), MapManager.GetRowY(0.5f), MapManager.GetRowY(5.5f));
        // bool flag = false;
        if(gList.Count <= 0)
        {
            R0 = GetRandomNext(1, 6);
            return;
        }
        // 获取含有生命值最高卡片的格子
        List<BaseGrid> gList2 = GridManager.GetGridListWhichHasMaxCondition(gList, (g) => {
            float max = float.MinValue;
            foreach (var unit in g.GetAttackableFoodUnitList())
            {
                if (UnitManager.CanBeSelectedAsTarget(this, unit) && unit.GetCurrentHp() > max)
                    max = unit.GetCurrentHp();
            }
            return max;
        });
        // 从几个中挑一个吧
        R0 = gList2[GetRandomNext(0, gList2.Count)].GetRowIndex();
    }

    private void FindR1_0(out int R1_0)
    {
        FindR0(out R1_0);
    }

    private void FindR1_1(int col, out int R1_1)
    {
        // 获取该列所有格子
        List<BaseGrid> gList = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(),
            MapManager.GetColumnX(col - 0.5f), MapManager.GetColumnX(col+0.5f), MapManager.GetRowY(-0.5f), MapManager.GetRowY(6.5f));
        // 找出满足 没有美食单位 没有钢爪陷阱 的格子
        List<BaseGrid> gList2 = GridManager.GetGridListWhichHasMinCondition(gList, (g) => {
            int min = int.MaxValue;
            if (g.IsContainUnit(Trap_Key))
                return min;

            foreach (var f in g.GetAttackableFoodUnitList())
            {
                if(FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f))
                {
                    min = int.MaxValue - 1;
                    return min;
                }
            }

            min = 0;
            return min;
        });
        // 随机取一个吧
        if (gList2.Count > 0)
            R1_1 = gList2[GetRandomNext(0, gList2.Count)].GetRowIndex();
        else
        {
            GetRandomNext(0, 1);
            R1_1 = 3;
        }
    }

    private void FindR2(out int R2)
    {
        List<int> list = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
        if (list.Count > 0)
            R2 = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            R2 = 3;
        }
    }

    /// <summary>
    /// 寻找导弹的攻击目标集合
    /// </summary>
    /// <returns></returns>
    private Queue<List<BaseUnit>> FindMissileTargetQueue()
    {
        // 先找这几行的所有可攻击，可被选取的美食单位
        int startRow = Mathf.Max(0, GetRowIndex() - 1);
        int endRow = Mathf.Min(6, GetRowIndex() + 1);
        List<BaseUnit> list = new List<BaseUnit>();
        for (int i = startRow; i <= endRow; i++)
        {
            foreach (var u in GameController.Instance.GetSpecificRowAllyList(i))
            {
                if(u is FoodUnit)
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
                if(list[j].GetCurrentHp() > maxUnit.GetCurrentHp())
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
            while(index < list.Count)
            {
                if(list[index].GetCurrentHp() == hp)
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

    private void FindR3(out int R3)
    {
        List<int> list = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
        if (list.Count > 0)
            R3 = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            R3 = 3;
        }
    }

    private List<int> FindBarrierRowList()
    {
        int count = Mathf.Min(7, Mathf.Max(0, Mathf.FloorToInt(GetParamValue("num3", mHertIndex))));
        List<int> unSelectedRowList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        List<int> rowList = new List<int>();

        while(count > 0)
        {
            List<int> l = FoodManager.GetRowListWhichHasMaxConditionAllyCount(unSelectedRowList, (u) => {
                if (u is FoodUnit)
                {
                    FoodUnit f = u as FoodUnit;
                    if (FoodManager.IsAttackableFoodType(f))
                        return true;
                }
                return false;
            });
            if (l.Count <= 0)
                break;
            foreach (var rowIndex in l)
            {
                rowList.Add(rowIndex);
                unSelectedRowList.Remove(rowIndex);
                count--;
                if (count <= 0)
                {
                    break;
                }
            }
        }
        return rowList;
    }

    /// <summary>
    /// 大荒星陨
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int t0 = Mathf.FloorToInt(60 * GetParamValue("t0", mHertIndex));
        int timeLeft = t0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Jump");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R0行右四列
                    int R0;
                    FindR0(out R0);
                    transform.position = MapManager.GetGridLocalPosition(5, R0);
                    animatorController.Play("Drop");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 停一会儿
                    timeLeft = t0;
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    animatorController.Play("Attack");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    float dmg = GetParamValue("dmg0", mHertIndex);
                    // 对3*3格子造成范围伤害
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 2.5f, 2.5f, "EnemyAllyGrid");
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
                    animatorController.Play("PreCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    int col = GetColumnIndex();
                    int row = GetRowIndex();
                    // 对前方四格召唤钢爪陷阱
                    for (int i = 0; i < 4; i++)
                        CreateTrap(GameController.Instance.mMapController.GetGrid(col - (i + 1), row));
                    animatorController.Play("PostCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 召唤钢爪陷阱
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        int t1 = Mathf.FloorToInt(60 * GetParamValue("t1", mHertIndex));
        int timeLeft = t1;
        int startRowIndex = 9 - Mathf.FloorToInt(GetParamValue("right_row1", mHertIndex));

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Jump");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R1_0行右四列
                    int R1_0;
                    FindR1_0(out R1_0);
                    transform.position = MapManager.GetGridLocalPosition(5, R1_0);
                    animatorController.Play("Drop");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 停一会儿
                    timeLeft = t1;
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    animatorController.Play("PreCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    for (int i = startRowIndex; i < 9; i++)
                    {
                        int R1_1;
                        FindR1_1(i, out R1_1);
                        CreateTrap(GameController.Instance.mMapController.GetGrid(i, R1_1));
                    }
                    animatorController.Play("PostCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 导弹轰炸
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int t2 = Mathf.FloorToInt(60 * GetParamValue("t2", mHertIndex));
        int num2 = Mathf.FloorToInt(GetParamValue("num2", mHertIndex));
        int timeLeft = t2;
        Queue<List<BaseUnit>> queue = null;
        List<BaseUnit> list = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Jump");
            queue = null;
            list = null;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R2行右一列
                    int R2;
                    FindR2(out R2);
                    transform.position = MapManager.GetGridLocalPosition(8, R2);
                    animatorController.Play("Drop");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 停一会儿
                    timeLeft = t2;
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    animatorController.Play("PreMissile");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    queue = FindMissileTargetQueue();
                    if(queue.Count > 0)
                        list = queue.Peek();
                    return true;
                }
                return false;
            });
            for (int i = 0; i < num2; i++)
            {
                int index = i;
                c.AddSpellingFunc(delegate
                {
                    animatorController.Play("Missile");
                    foreach (var l in queue)
                    {
                        List<BaseUnit> delList = new List<BaseUnit>();
                        foreach (var u in l)
                        {
                            if (!u.IsAlive())
                                delList.Add(u);
                        }
                        foreach (var u in delList)
                        {
                            l.Remove(u);
                        }
                    }

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
                    if (list != null)
                    {
                        // 从list里随机取一个目标出来
                        BaseUnit target = list[GetRandomNext(0, list.Count)];
                        CreateMissile(target);
                        list.Remove(target);
                    }
                    else
                    {
                        GetRandomNext(0, 1);
                    }
                    return true;
                });
                c.AddSpellingFunc(delegate
                {
                    foreach (var l in queue)
                    {
                        List<BaseUnit> delList = new List<BaseUnit>();
                        foreach (var u in l)
                        {
                            if (!u.IsAlive())
                                delList.Add(u);
                        }
                        foreach (var u in delList)
                        {
                            l.Remove(u);
                        }
                    }

                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }else if(list == null)
                    {
                        return true;
                    }
                    return false;
                });
            }
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("PostMissile");
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 金钢之刺
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill3Init(SkillAbility.SkillAbilityInfo info)
    {
        int t3 = Mathf.FloorToInt(60 * GetParamValue("t3", mHertIndex));
        int timeLeft = t3;
        List<BaseUnit> barrierList = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Jump");
            barrierList = null;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R3行右四列
                    int R3;
                    FindR3(out R3);
                    transform.position = MapManager.GetGridLocalPosition(5, R3);
                    animatorController.Play("Drop");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("PreCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    timeLeft = t3;
                    animatorController.Play("Cast", true);
                    // 生成刺
                    List<int> rowList = FindBarrierRowList();
                    barrierList = new List<BaseUnit>();
                    foreach (var rowIndex in rowList)
                    {
                        BaseUnit u = CreateBarrier(GameController.Instance.mMapController.GetGrid(6, rowIndex));
                        if (u != null)
                            barrierList.Add(u);
                    }
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                List<BaseUnit> delList = new List<BaseUnit>();
                foreach (var u in barrierList)
                {
                    if (!u.IsAlive())
                        delList.Add(u);
                }
                foreach (var u in delList)
                {
                    barrierList.Remove(u);
                }

                timeLeft--;
                if(timeLeft <= 0 || barrierList.Count <= 0)
                {
                    animatorController.Play("PostCast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
