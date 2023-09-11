using System;
using System.Collections.Generic;

using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 酷帅小明
/// </summary>
public class XiaoMing : BossUnit
{
    private static List<XiaoMing> bossList = new List<XiaoMing>();

    private List<int>[] rowListArray;
    private List<int> avaliableIndexList = new List<int>();

    private List<BaseUnit> bulletList = new List<BaseUnit>();
    private Vector2 start;
    private int[][] valueArray;
    private float dmgRecord; // 受到的伤害总和

    private RingUI FinalSkillRingUI;

    private static RuntimeAnimatorController IceShield_Run;
    private static RuntimeAnimatorController RedPackage_Run;
    private static RuntimeAnimatorController IceBreak_Run;
    private static Sprite Ice_Icon_Sprite;
    private static Sprite IceShield_Icon_Sprite;
    private static Sprite Wait_Icon_Sprite;
    public override void Awake()
    {
        base.Awake();
        if(IceShield_Run == null)
        {
            IceShield_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/14/IceShield");
            RedPackage_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/14/RedPackage");
            IceBreak_Run = GameManager.Instance.GetRuntimeAnimatorController("Effect/IceBreak");
            Ice_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Ice");
            IceShield_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/IceShield");
            Wait_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Wait");
        }
    }

    public override void MInit()
    {
        if(bossList.Count == 0)
        {
            GameController.Instance.mTaskController.AddTask(GetDisplayAllyIceValueTask());
        }
        bossList.Add(this);

        dmgRecord = 0;
        rowListArray = null;
        avaliableIndexList.Clear();
        bulletList.Clear();
        base.MInit();
        // 大招UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this));
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(1);
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // 受击事件
        {
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) => {
                if(combatAction is DamageAction)
                {
                    float triggerFinalSkillDamage = mMaxHp * GetParamValue("p_lost_hp_percent") / 100;
                    DamageAction dmgAction = combatAction as DamageAction;
                    dmgRecord += dmgAction.RealCauseValue;
                    FinalSkillRingUI.SetPercent(1 - dmgRecord / triggerFinalSkillDamage);
                    if (dmgRecord >= triggerFinalSkillDamage)
                    {
                        FinalSkillRingUI.Hide();
                        dmgRecord -= triggerFinalSkillDamage;
                        CustomizationSkillAbility s = SKill2Init(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[2]);
                        mSkillQueueAbilityManager.SetNextSkill(s);
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
        foreach (var b in bulletList)
        {
            if (!b.IsAlive())
                delList.Add(b);
        }
        foreach (var b in delList)
        {
            bulletList.Remove(b);
        }
        base.MUpdate();
    }

    public override void AfterDeath()
    {
        foreach (var b in bulletList)
        {
            b.ExecuteDeath();
        }
        bulletList.Clear();
        // 遍历所有BOSS
        foreach (var u in GameController.Instance.GetEachEnemy())
        {
            if (u is XiaoHong)
            {
                // 进入下一P
                BossUnit boss = u as XiaoHong;
                boss.SkipStage();
            }
        }
        base.AfterDeath();
    }

    public override void MDestory()
    {
        base.MDestory();
        bossList.Remove(this);
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.XiaoMing, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // 特殊参数初始化
        // 获取Rset
        {
            Action<float[]> action = (arr) =>
            {
                rowListArray = new List<int>[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    rowListArray[i] = new List<int>();
                    int val = Mathf.FloorToInt(arr[i]);
                    while (val > 0)
                    {
                        rowListArray[i].Insert(0, val % 10 - 1);
                        val = val / 10;
                    }
                }
                for (int i = 0; i < rowListArray.Length; i++)
                    avaliableIndexList.Add(i);
            };
            AddParamChangeAction("RSet0", action);
            action(GetParamArray("RSet0"));
        }

        // 获取点集
        {
            Action<float[]> action = delegate 
            {
                int w = Mathf.FloorToInt(GetParamValue("w1"));
                int h = Mathf.FloorToInt(GetParamValue("h1"));

                start = new Vector2((float)w / 2 - 0.5f, (float)h / 2 - 0.5f);
                // 确定左上角和右下角点的格子坐标
                Vector2 left_up = start;
                Vector2 right_down = new Vector2(8 - (float)w / 2 + 0.5f, 6 - (float)h / 2 + 0.5f);
                // 求数组宽高
                int x = Mathf.Max(1, Mathf.FloorToInt(right_down.x - left_up.x));
                int y = Mathf.Max(1, Mathf.FloorToInt(right_down.y - left_up.y));

                valueArray = new int[x][];
                for (int i = 0; i < valueArray.Length; i++)
                {
                    valueArray[i] = new int[y];
                    for (int j = 0; j < valueArray[i].Length; j++)
                    {
                        valueArray[i][j] = 0;
                    }
                }
            };
            AddParamChangeAction("w1", action);
            AddParamChangeAction("h1", action);
            action(null);
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
        mBoxCollider2D.size = new Vector2(0.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    #region 一技能
    /// <summary>
    /// 产生一个红包
    /// </summary>
    private void CreateBullet(Vector2 pos, Vector2 rot)
    {
        float dmg = GetParamValue("dmg0");
        float ice_val = GetParamValue("ice_val0");
        float aoe_ice_val = GetParamValue("aoe_ice_val0");

        MouseModel m = MouseModel.GetInstance(RedPackage_Run);
        bulletList.Add(m);
        m.transform.position = pos;
        m.SetBaseAttribute(10000, 10, 1.0f, 12.0f, 100, 0.5f, 1);
        m.SetMoveRoate(rot);
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        m.IdleClipName = "Fly";
        m.MoveClipName = "Fly";
        m.AttackClipName = "Fly";
        m.DieClipName = "Hit";
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));
        m.AddAfterDeathEvent((m) => {
            BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
            eff.SetSpriteRendererSorting("Effect", 2);
            eff.transform.position = m.transform.position;
            GameController.Instance.AddEffect(eff);

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 3f, 3f, "BothCollide");
            r.SetInstantaneous();
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodEnterAction((u)=> {
                if (FoodManager.IsAttackableFoodType(u))
                    EnvironmentFacade.AddIceDebuff(u, aoe_ice_val);
            });
            r.SetOnEnemyEnterAction((u)=> {
                if(!u.IsBoss())
                    EnvironmentFacade.AddIceDebuff(u, aoe_ice_val);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        });
        GameController.Instance.AddMouseUnit(m);

        // 产生一个区域:会检测美食单位并触发击中效果
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "ItemCollideAlly");
            r.isAffectFood = true;
            r.AddFoodEnterConditionFunc((u) => {
                return UnitManager.CanBeSelectedAsTarget(m, u);
            });
            Action<BaseUnit> action = (u) =>
            {
                EnvironmentFacade.AddIceDebuff(u, ice_val);
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
                m.ExecuteDeath();
            };
            r.SetOnFoodEnterAction(action);
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (!m.IsAlive())
                    return true;
                else
                {
                    r.transform.position = m.transform.position;
                    return false;
                }
            });
            t.AddOnExitAction(delegate
            {
                r.MDestory();
            });
            r.AddTask(t);
        }
    }
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0_0", mHertIndex) * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait0_1", mHertIndex) * 60);
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col0", mHertIndex));
        float num = GetParamValue("num0", mHertIndex);
        float spr = GetParamValue("spr0", mHertIndex)/180*Mathf.PI;

        // 变量
        List<int> rowIndexList = null;
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        // 一些变量初始化
        c.BeforeSpellFunc = delegate
        {
            if(avaliableIndexList.Count <= 0)
                for (int i = 0; i < rowListArray.Length; i++)
                    avaliableIndexList.Add(i);

            //int ranIndex = GetRandomNext(0, avaliableIndexList.Count);
            int ranIndex = 0; // 固定成0
            int index = avaliableIndexList[ranIndex];
            avaliableIndexList.RemoveAt(ranIndex);
            rowIndexList = rowListArray[index];
        };
        {
            // 瞬移+丢红包
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < rowIndexList.Count; _i++)
                {
                    int i = _i;
                    task.AddTaskFunc(delegate {
                        animatorController.Play("Disappear");
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndexList[i]);
                            animatorController.Play("Appear");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            timeLeft = wait0;
                            animatorController.Play("Idle", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Attack");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.59f)
                        {                            
                            // 产生弹体
                            if (num == 1)
                                CreateBullet(transform.position + Vector3.left * MapManager.gridWidth, new Vector2(Mathf.Cos(Mathf.PI), Mathf.Sin(Mathf.PI)));
                            else
                                for (float j = -0.5f; j <= 0.5f; j += 1 / (num-1))
                                    CreateBullet(transform.position + Vector3.left * MapManager.gridWidth, new Vector2(Mathf.Cos(Mathf.PI + spr * j), Mathf.Sin(Mathf.PI + spr * j)));
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                            return true;
                        return false;
                    });
                }
                return task;
            });
            // 末尾等待
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
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
    private Vector2 S1FindPosition()
    {
        float w = GetParamValue("w1");
        float h = GetParamValue("h1");
        // 先初始化一次
        for (int i = 0; i < valueArray.Length; i++)
            for (int j = 0; j < valueArray[i].Length; j++)
                valueArray[i][j] = 0;
        // 遍历所有（可被攻击的）美食单位
        foreach (var u in GameController.Instance.GetEachAlly())
        {
            // 剔除人物单位和不可被攻击的美食单位
            if (!(u is FoodUnit) || !FoodManager.IsAttackableFoodType(u as FoodUnit))
                continue;

            // 获得格子坐标
            Vector2 p = new Vector2(MapManager.GetXIndexF(u.transform.position.x), MapManager.GetYIndexF(u.transform.position.y));

            List<int> xList = new List<int>();
            for (float i = Mathf.CeilToInt(p.x - w/2 - start.x); i <= p.x + w/2 - start.x; i++)
            {
                if (i < 0)
                    continue;
                else if (i >= valueArray.Length)
                    break;
                else
                    xList.Add(Mathf.FloorToInt(i));
            }

            List<int> yList = new List<int>();
            for (float i = Mathf.CeilToInt(p.y - h / 2 - start.y); i <= p.y + h / 2 - start.y; i++)
            {
                if (i < 0)
                    continue;
                else if (i >= valueArray[0].Length)
                    break;
                else
                    yList.Add(Mathf.FloorToInt(i));
            }

            // 计算权值
            int val = (Environment.EnvironmentFacade.GetIceDebuff(u) != null ? 100 : 1);

            // 为范围内的点加权
            for (int i = 0; i < xList.Count; i++)
                for (int j = 0; j < yList.Count; j++)
                    valueArray[xList[i]][yList[j]] += val;
        }

        // 最后统计一次，取出权重最大的几个点
        List<Vector2> maxV2 = new List<Vector2>();
        int max = 0;

        for (int i = 0; i < valueArray.Length; i++)
            for (int j = 0; j < valueArray[i].Length; j++)
            {
                if(valueArray[i][j] > max)
                {
                    maxV2.Clear();
                    maxV2.Add(new Vector2(i, j));
                    max = valueArray[i][j];
                }else if(valueArray[i][j] == max)
                {
                    maxV2.Add(new Vector2(i, j));
                }
            }

        // 在这些候选点里随机取一个点
        Vector2 _p = maxV2[GetRandomNext(0, maxV2.Count)];
        // 转成实际坐标并返回
        return MapManager.GetGridLocalPosition(start.x + _p.x, start.y + _p.y);
    }
    
    
    private void S1CreateIceArea(Vector2 pos, float tran_rate)
    {
        float w = GetParamValue("w1");
        float h = GetParamValue("h1");
        float ice_val = GetParamValue("ice_val1");
        float max_ice_val = GetParamValue("max_ice_val1");

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2((w - 0.5f) * MapManager.gridWidth, (h - 0.5f) * MapManager.gridHeight), "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        Action<BaseUnit> action = (u) => {
            ITask t = EnvironmentFacade.GetIceDebuff(u);
            if(t == null)
            {
                if(ice_val > 0)
                    EnvironmentFacade.AddIceDebuff(u, ice_val);
            }
            else
            {
                IceTask iceTask = t as IceTask;
                if (iceTask.IsForzen())
                {
                    // 若已触发凝结，则直接加至最大值
                    float add = max_ice_val - iceTask.GetValue();
                    if (add > 0)
                        iceTask.AddValue(add);
                }
                else
                {
                    iceTask.AddValue(Mathf.Min(iceTask.GetValue() * tran_rate, max_ice_val - iceTask.GetValue()));
                }
            }
        };
        r.SetOnFoodEnterAction(action);
        r.SetOnEnemyEnterAction(action);
        GameController.Instance.AddAreaEffectExecution(r);
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);
        float tran_rate = GetParamValue("tran_rate1") / 100;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
        };
        {
            // 瞬移
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Disappear", out task);
                task.AddOnExitAction(delegate {
                    transform.position = S1FindPosition();
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Appear", out task);
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Step");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.40f)
                    {
                        // 产生大量冰特效
                        for (int i = 0; i < 5; i++)
                        {
                            BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
                            eff.SetSpriteRendererSorting("Effect", 2);
                            eff.transform.position = transform.position;
                            float a = UnityEngine.Random.Range(0, 2 * Mathf.PI);
                            eff.SetSpriteRight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                            GameController.Instance.AddEffect(eff);
                        }
                        {
                            BaseEffect eff = BaseEffect.CreateInstance(IceShield_Run, "Appear", "Idle", "Disapear", false);
                            eff.SetSpriteRendererSorting("Effect", 1);
                            eff.transform.position = transform.position;
                            GameController.Instance.AddEffect(eff);
                        }

                        // 产生判定检测区域
                        S1CreateIceArea(transform.position, tran_rate);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        return true;
                    return false;
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
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
    private void CreateS2TriggerIce(Vector2 pos, float ice_val)
    {
        // 冰冻特效
        for (int i = 0; i < 5; i++)
        {
            BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
            eff.SetSpriteRendererSorting("Effect", 2);
            eff.transform.position = pos;
            float a = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            eff.SetSpriteRight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            GameController.Instance.AddEffect(eff);
        }
        // 范围冰冻效果
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(GetParamValue("w2") * MapManager.gridWidth, GetParamValue("h2") * MapManager.gridHeight), "BothCollide");
            r.SetInstantaneous();
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodEnterAction((u) => {
                if (FoodManager.IsAttackableFoodType(u))
                {
                    Environment.EnvironmentFacade.AddIceDebuff(u, ice_val);
                }
            });
            r.SetOnEnemyEnterAction((u) => {
                if (!u.IsBoss())
                {
                    Environment.EnvironmentFacade.AddIceDebuff(u, ice_val);
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    /// <summary>
    /// 获取给自身P3阶段施法的UI任务
    /// </summary>
    /// <param name="ru"></param>
    /// <returns></returns>
    private CustomizationTask GetS2WaitRingUITask(RingUI ru)
    {
        float r = 238f / 255;
        float g = 255f / 255;
        float b = 197f / 255;

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            ru.Show();
            ru.SetIcon(Wait_Icon_Sprite);
            ru.SetPercent(0);
            ru.SetColor(new Color(r, g, b, 0.5f));
        });
        task.AddTaskFunc(delegate {
            ru.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.down;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            ru.MDestory();
        });
        return task;
    }

    /// <summary>
    /// 获取给自身P3阶段冰反盾的UI任务
    /// </summary>
    /// <param name="ru"></param>
    /// <returns></returns>
    private CustomizationTask GetS2IceShieldRingUITask(RingUI ru)
    {
        float r = 199f / 255;
        float g = 233f / 255;
        float b = 255f / 255;

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            ru.Show();
            ru.SetIcon(IceShield_Icon_Sprite);
            ru.SetPercent(1);
            ru.SetColor(new Color(r, g, b, 1));
        });
        task.AddTaskFunc(delegate {
            if (!IsAlive())
                return true;
            ru.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.down;
            return false;
        });
        task.AddOnExitAction(delegate {
            ru.MDestory();
        });
        return task;
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int startColIndex = 9 - Mathf.FloorToInt(GetParamValue("start_right_col2"));
        int endColIndex = 9 - Mathf.FloorToInt(GetParamValue("end_right_col2"));
        float startX = MapManager.GetColumnX(startColIndex);
        float endX = MapManager.GetColumnX(endColIndex);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        int interval = Mathf.FloorToInt(GetParamValue("interval2") * 60);
        int totalTime = Mathf.FloorToInt(GetParamValue("time2") * 60);
        float v = TransManager.TranToVelocity(GetParamValue("v2"));
        float dmg_rate = 1 - GetParamValue("defence2")/100;
        int dmg_interval = 15;
        float dmg_trans = GetParamValue("dmg_trans2") / 100;
        float dmg_percent = GetParamValue("dmg_percent2", mHertIndex)/100;
        float trigger_percent = GetParamValue("trigger_percent2") / 100;
        float aoe_ice_val0 = GetParamValue("aoe_ice_val2_0");

        // 变量
        RingUI rUI = null;
        CustomizationTask waitTask = null;
        CustomizationTask iceShieldTask = null;
        FloatModifier DamageRateMod = new FloatModifier(dmg_rate);

        int changeTimeLeft = 0;
        int totalTimeLeft = 0;
        float targetYPos = 0;
        float dmgLeft = float.MaxValue;
        RetangleAreaEffectExecution dmgArea = null;
        Action<CombatAction> reboundAction = (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                dmgLeft -= dmgAction.RealCauseValue;
                if(dmgLeft <= 0)
                {
                    dmgLeft += mMaxHp * trigger_percent;
                    CreateS2TriggerIce(transform.position, aoe_ice_val0);
                }
                rUI.SetPercent(dmgLeft / (mMaxHp * trigger_percent));
            }
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Disappear", out task);
                task.AddOnExitAction(delegate {
                    List<int> list = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                    int rowIndex = list[GetRandomNext(0, list.Count)];
                    transform.position = MapManager.GetGridLocalPosition(startColIndex, rowIndex);
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitClipTask(animatorController, "Appear", out task);
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait0, (timeLeft)=> {
                    rUI.SetPercent(1-(float)timeLeft/wait0);
                }, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Drink");
                    rUI = RingUI.GetInstance(0.3f * Vector2.one);
                    GameNormalPanel.Instance.AddUI(rUI);
                    waitTask = GetS2WaitRingUITask(rUI);
                    rUI.mTaskController.AddTask(waitTask);
                    // 获得减伤
                    NumericBox.DamageRate.AddModifier(DamageRateMod);
                });
                task.AddOnExitAction(delegate {
                    totalTimeLeft = totalTime;
                    changeTimeLeft = 0;
                    targetYPos = transform.position.y;
                    // 移除减伤
                    NumericBox.DamageRate.RemoveModifier(DamageRateMod);
                    // 播放旋风动画
                    animatorController.Play("Cyclone");
                    // UI改成冰反UI
                    {
                        rUI.mTaskController.RemoveTask(waitTask);

                        rUI = RingUI.GetInstance(0.3f * Vector2.one);
                        GameNormalPanel.Instance.AddUI(rUI);
                        waitTask = null;
                        iceShieldTask = GetS2IceShieldRingUITask(rUI);
                        rUI.mTaskController.AddTask(iceShieldTask);
                    }
                    // 产生冰反效果
                    dmgLeft = mMaxHp * trigger_percent;
                    actionPointController.AddListener(ActionPointType.PostReceiveDamage, reboundAction);
                    // 产生伤害光环
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1f, 1f, "BothCollide");
                        r.isAffectFood = true;
                        r.isAffectMouse = true;
                        r.AddEnemyEnterConditionFunc((m) => {
                            return !m.IsBoss();
                        });
                        GameController.Instance.AddAreaEffectExecution(r);

                        int timeLeft = dmg_interval;
                        CustomizationTask t = new CustomizationTask();
                        t.AddTaskFunc(delegate {
                            timeLeft--;
                            if(timeLeft <= 0)
                            {
                                timeLeft += dmg_interval;
                                foreach (var u in r.foodUnitList)
                                {
                                    DamageAction d = new DamageAction(CombatAction.ActionType.RealDamage, this, u, u.mMaxHp * dmg_percent);
                                    d.ApplyAction();
                                    new DamageAction(CombatAction.ActionType.CauseDamage, null, this, d.RealCauseValue * dmg_trans).ApplyAction(); // 伤害反弹给自己
                                } 
                                foreach (var u in r.mouseUnitList)
                                    new DamageAction(CombatAction.ActionType.RealDamage, this, u, u.mMaxHp * dmg_percent).ApplyAction();
                            }
                            r.transform.position = transform.position;
                            return !IsAlive();
                        });
                        r.AddTask(t);
                        dmgArea = r;
                    }
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTaskFunc(delegate {
                    totalTimeLeft--;
                    changeTimeLeft--;
                    if (changeTimeLeft <= 0)
                    {
                        List<int> oriList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
                        oriList.Remove(MapManager.GetYIndex(targetYPos));

                        List<int> list = FoodManager.GetRowListWhichHasMaxConditionAllyCount(oriList, (unit) =>
                        {
                            if (unit is FoodUnit)
                            {
                                FoodUnit f = unit as FoodUnit;
                                return FoodManager.IsAttackableFoodType(f);
                            }
                            return false;
                        });
                        targetYPos = MapManager.GetRowY(list[GetRandomNext(0, list.Count)]);
                        changeTimeLeft += interval;
                    }
                    float rate = 1 - (float)totalTimeLeft / totalTime;
                    // 同步y坐标
                    float deltaY = targetYPos - transform.position.y;
                    float dy = Mathf.Sign(deltaY) * Mathf.Min(v, Mathf.Abs(deltaY));
                    transform.position = new Vector2(startX + rate * (endX - startX), transform.position.y + dy);
                    if (totalTimeLeft <= 0)
                    {
                        return true;
                    }
                    return false;
                });
                return task;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
                    // 移除冰反效果
                    rUI.mTaskController.RemoveTask(iceShieldTask);
                    actionPointController.RemoveListener(ActionPointType.PostReceiveDamage, reboundAction);
                    // 移除伤害光环
                    if (dmgArea != null)
                        dmgArea.MDestory();
                    // 显示大招UI
                    FinalSkillRingUI.Show();
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion

    #region 外显UI任务
    private static CustomizationTask GetDisplayAllyIceValueTask()
    {
        GameNormalPanel panel = GameNormalPanel.Instance;
        Action<BaseCardBuilder> action = (b) =>
        {
            RingUI ru = RingUI.GetInstance(0.2f * Vector2.one);
            BaseUnit u = b.mProduct;
            u.taskController.AddUniqueTask("IceDebuffDisplayer", GetRingUITask(ru, u));
            u.AddOnDestoryAction(delegate { if (ru.IsValid()) ru.MDestory(); });
            panel.AddUI(ru);
        };


        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            foreach (var u in GameController.Instance.GetEachAlly())
            {
                RingUI ru = RingUI.GetInstance(0.2f * Vector2.one);
                u.taskController.AddUniqueTask("IceDebuffDisplayer", GetRingUITask(ru, u));
                u.AddOnDestoryAction(delegate { if (ru.IsValid()) ru.MDestory(); });
                panel.AddUI(ru);
            }

            // 除此之外，后续生成的美食单位也会附加这个UI
            foreach (var builder in GameController.Instance.mCardController.mCardBuilderList)
            {
                builder.AddAfterBuildAction(action);
            }
        });
        task.AddTaskFunc(delegate {
            return bossList.Count <= 0;
        });
        task.AddOnExitAction(delegate 
        {
            foreach (var builder in GameController.Instance.mCardController.mCardBuilderList)
                builder.RemoveAfterBuildAction(action);

            foreach (var u in GameController.Instance.GetEachAlly())
                u.taskController.RemoveUniqueTask("IceDebuffDisplayer");
        });
        return task;
    }

    /// <summary>
    /// 获取给友方卡片添加冰冻损伤UI显示的任务
    /// </summary>
    /// <param name="ru"></param>
    /// <param name="u"></param>
    /// <returns></returns>
    private static CustomizationTask GetRingUITask(RingUI ru, BaseUnit u)
    {
        float maxVal = 100;
        float r = 199f / 255;
        float g = 233f / 255;
        float b = 255f / 255;

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            ru.Hide();
            ru.SetIcon(Ice_Icon_Sprite);
            ru.SetPercent(0);
            ru.SetColor(new Color(r, g, b, 0.5f));
        });
        task.AddTaskFunc(delegate {
            if (u == null || !u.IsAlive())
                return true;
            ru.transform.position = u.transform.position + 0.25f*MapManager.gridHeight * Vector3.down;
            ITask t = EnvironmentFacade.GetIceDebuff(u);
            if(t != null)
            {
                ru.Show();
                IceTask iceTask = t as IceTask;
                if (iceTask.IsForzen())
                {
                    maxVal = Mathf.Max(100, Mathf.Max(maxVal, iceTask.GetValue()));
                    ru.SetColor(new Color(r, g, b, 1));
                }else
                {
                    maxVal = 100;
                    ru.SetColor(new Color(r, g, b, 0.5f));
                }
                ru.SetPercent(iceTask.GetValue() / maxVal);
            }
            else
            {
                ru.Hide();
            }
            return false;
        });
        task.AddOnExitAction(delegate {
            ru.MDestory();
        });
        return task;
    }
    #endregion
}
