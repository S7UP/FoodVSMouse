using System;
using System.Collections.Generic;

using Environment;

using S7P.Numeric;

using UnityEngine;

/// <summary>
/// 地狱屎者
/// </summary>
public class HellShiter : BossUnit
{
    private static RuntimeAnimatorController Fog_Run;
    private static RuntimeAnimatorController GhostFire_Run;
    private static RuntimeAnimatorController Sceptre_Run;
    private static RuntimeAnimatorController Shit_Run;

    private const string GhostKey = "HellShiter_GhostKey";
    private List<BaseUnit> ghostFireList = new List<BaseUnit>();
    private BaseUnit mSceptre;

    public override void Awake()
    {
        if(Fog_Run == null)
        {
            Fog_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/8/Fog");
            GhostFire_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/8/GhostFire");
            Sceptre_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/8/Sceptre");
            Shit_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/8/Shit");
        }
        base.Awake();
    }

    public override void MInit()
    {
        transform.localScale = Vector2.one;
        mSceptre = null;
        ghostFireList.Clear();
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
                CreateCheckArea(); // 创建检测区域
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
        foreach (var u in ghostFireList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            ghostFireList.Remove(u);
        }
        base.MUpdate();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        RecycleSceptre();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.HellShiter, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.25f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
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
    /// 创建被动范围效果检测范围圈
    /// </summary>
    private void CreateCheckArea()
    {
        FloatModifier speedMod = new FloatModifier(GetParamValue("AddSpeedPercent", mHertIndex));
        FloatModifier P_AttackSpeedMod = new FloatModifier(-GetParamValue("P_DecAttackSpeedPercent", mHertIndex));

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 2.75f, 2.75f, "BothCollide");
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((m) => {
            m.NumericBox.MoveSpeed.AddPctAddModifier(speedMod);
            m.NumericBox.AttackSpeed.AddPctAddModifier(speedMod);
        });
        r.SetOnEnemyExitAction((m) => {
            m.NumericBox.MoveSpeed.RemovePctAddModifier(speedMod);
            m.NumericBox.AttackSpeed.RemovePctAddModifier(speedMod);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        // 绑定任务
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (IsAlive())
                    r.transform.position = transform.position;
                else
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                r.ExecuteRecycle();
            });
            r.AddTask(t);
        }
    }

    private MouseModel CreateGhostFire(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(GhostFire_Run);
        m.AttackClipName = "Idle";
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        ghostFireList.Add(m);
        m.transform.position = pos;
        m.SetBaseAttribute(1, 10, 1.0f, 0f, 100, 0.5f, 1);
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // 标记免疫水蚀
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));
        Action<BaseUnit> deathAction = (u) => {
            GhostFireDeathAction(m.transform.position, m.aliveTime);
        };
        m.AddBeforeDeathEvent(deathAction);
        GameController.Instance.AddMouseUnit(m);
        return m;
    }

    private void GhostFireDeathAction(Vector2 pos, int aliveTime)
    {
        int frozenValue = Mathf.FloorToInt(GetParamValue("FrozenValue", mHertIndex));
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        Action<BaseUnit> frozenAction = (u) => {
            //u.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(u, time, false));
            EnvironmentFacade.AddIceDebuff(u, frozenValue);
        };
        r.SetOnFoodEnterAction(frozenAction);
        r.SetOnEnemyEnterAction(frozenAction);
        r.SetOnCharacterEnterAction(frozenAction);
        // 原地生成一只幽灵鼠
        if (pos.x >= MapManager.GetColumnX(3))
        {
            MouseUnit m = GameController.Instance.CreateMouseUnit(MapManager.GetXIndex(pos.x), MapManager.GetYIndex(pos.y), new BaseEnemyGroup.EnemyInfo() { type = (int)MouseNameTypeMap.GhostMouse, shape = 0 });
            m.transform.position = new Vector2(pos.x, m.transform.position.y);
            m.SetMaxHpAndCurrentHp(m.mMaxHp * NumberManager.GetCurrentEnemyHpRate()); // 对老鼠最大生命值进行修正
        }
        // 回复玩家能量
        SmallStove.CreateAddFireEffect(pos, GetParamValue("ReturnFire", mHertIndex));
        GameController.Instance.AddAreaEffectExecution(r);
    }

    private void Skill0GhostFire(Vector2 startPos, int rowIndex, int timeLeft)
    {
        FloatModifier xMod = new FloatModifier(0);
        FloatModifier yMod = new FloatModifier(0);
        int moveTime = 60;
        System.Random ran = GameManager.Instance.rand;
        Vector2 ranSpritePos = new Vector2(((float)ran.NextDouble()-0.5f)*MapManager.gridWidth, ((float)ran.NextDouble() - 0.5f)*MapManager.gridHeight);
        Vector2 endPos = new Vector2(startPos.x, MapManager.GetRowY(rowIndex));
        MouseModel m = CreateGhostFire(startPos);
        m.CloseCollision();
        xMod.Value = ranSpritePos.x;
        yMod.Value = ranSpritePos.y;
        m.AddSpriteOffsetX(xMod);
        m.AddSpriteOffsetY(yMod);
        // 移动任务
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.animatorController.Play("Appear");
            });
            t.AddTaskFunc(delegate {
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.animatorController.Play("Idle");
                    return true;
                }
                return false;
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
                    // 产生检测区域并向前推进
                    m.NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(2.0f));
                    m.OpenCollision();
                    CreateSkill0GhostFireCheckArea(m);
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                if(m.transform.position.x <= MapManager.GetColumnX(0))
                {
                    return true;
                }
                return false;
            });
            t.AddOnExitAction(delegate {
                m.ExecuteDeath();
            });
            m.AddTask(t);
        }
    }

    private void CreateSkill0GhostFireCheckArea(MouseModel ghostFire)
    {
        bool isHasTarget = false;
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(ghostFire.transform.position, 0.25f, 0.25f, "ItemCollideAlly");
        r.isAffectFood = true;
        r.SetOnFoodEnterAction((f) => {
            if(!isHasTarget && FoodManager.IsAttackableFoodType(f) && !f.IsContainUnit(GhostKey))
            {
                isHasTarget = true;
                r.MDestory();
                if (ghostFire.IsAlive())
                {
                    GhostFireAttach(ghostFire, f);
                }
            }
        });
        GameController.Instance.AddAreaEffectExecution(r);
        // 检测范围跟随鬼火
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (ghostFire.IsAlive())
                {
                    r.transform.position = ghostFire.transform.position;
                    return false;
                }
                else
                    return true;
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
    }

    private void Skill1GhostFire(Vector2 startPos, BaseUnit target)
    {
        bool isTargetAlive = true;
        int totalTime = 30;
        int timeLeft = totalTime;
        MouseModel m = CreateGhostFire(startPos);
        Vector2 endPos = target.transform.position;

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            m.animatorController.Play("Appear");
        });
        t.AddTaskFunc(delegate {
            timeLeft--;
            if (isTargetAlive)
            {
                if (target.IsAlive())
                    endPos = target.transform.position;
                else
                    isTargetAlive = false;
            }
            float rate = 1-((float)timeLeft / totalTime);
            m.transform.position = Vector2.Lerp(startPos, endPos, rate);
            if (timeLeft <= 0)
            {
                if (target.IsAlive() && isTargetAlive)
                    GhostFireAttach(m, target);
                return true;
            }
            else
                return false;
        });
        t.AddTaskFunc(delegate {
            if (isTargetAlive && target.IsAlive())
            {
                m.transform.position = target.transform.position;
                return false;
            }
            else
                return true;
        });
        t.AddOnExitAction(delegate {
            m.ExecuteDeath();
        });
        m.AddTask(t);
    }

    /// <summary>
    /// 使鬼火吸附到某个单位上
    /// </summary>
    /// <param name="ghostFire"></param>
    /// <param name="u"></param>
    private void GhostFireAttach(MouseModel ghostFire, BaseUnit u)
    {
        if(u.IsContainUnit(GhostKey))
        {
            ghostFire.ExecuteDeath();
            return;
        }

        FloatModifier decMod = new FloatModifier(-GetParamValue("DecAttackSpeedPercent", mHertIndex));
        u.AddUnitToDict(GhostKey, ghostFire);
        u.NumericBox.AttackSpeed.AddFinalPctAddModifier(decMod);
        Action<BaseUnit> action = delegate {
            if(u!=null && u.IsAlive())
            {
                u.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decMod);
            }
        };
        ghostFire.AddBeforeDeathEvent(action);
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (u.IsAlive())
            {
                ghostFire.transform.position = u.transform.position;
                return false;
            }
            else
            {
                u = null;
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            ghostFire.ExecuteDeath();
        });
        ghostFire.AddTask(t);
    }

    private void CreateSceptre(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(Sceptre_Run);
        m.AttackClipName = "Idle";
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        mSceptre = m;
        m.transform.position = pos;
        m.SetBaseAttribute(mMaxHp * mBurnRate, 10, 1.0f, 0f, 100, 0.5f, 0);
        m.NumericBox.BurnRate.AddModifier(new FloatModifier(1 - 0.01f * GetParamValue("burn_defence1")));
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBlockFunc(delegate { return false; });
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // 标记免疫水蚀
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new IdleState(m));
        //m.AddBeforeBurnEvent(delegate { TriggerAllGhostFire(); });
        m.AddOnDestoryAction(delegate { TriggerAllGhostFire(); });
        Action<CombatAction> hitAction = (com) => {
            if (com is DamageAction)
            {
                DamageAction d = com as DamageAction;
                new DamageAction(CombatAction.ActionType.CauseDamage, d.Creator, this, GetParamValue("tran_rate1", mHertIndex) * 0.01f * d.DamageValue).ApplyAction();
            }
        };
        m.AddActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
        GameController.Instance.AddMouseUnit(m);

        Action<BaseCardBuilder> action = (builder) => {
            BaseUnit u = builder.GetResult();
            Skill1GhostFire(m.transform.position, u);
        };
        m.animatorController.Play("Appear");
        // 添加种下卡片后的监听
        foreach (var builder in GameController.Instance.mCardController.mCardBuilderList)
            builder.AddAfterBuildAction(action);
        m.AddAfterDeathEvent(delegate {
            foreach (var builder in GameController.Instance.mCardController.mCardBuilderList)
                builder.RemoveAfterBuildAction(action);
        });
    }

    private void RecycleSceptre()
    {
        if (mSceptre != null && mSceptre.IsAlive())
            mSceptre.ExecuteDeath();
    }

    private void TriggerAllGhostFire()
    {
        foreach (var u in ghostFireList)
        {
            u.ExecuteDeath();
        }
        ghostFireList.Clear();
    }

    private void Eat(BaseUnit u)
    {
        if (u == null || !u.IsAlive())
            return;

        int totalTime = 60;
        int timeLeft = totalTime;
        Sprite s = u.GetSpirte();
        if (s == null)
        {
            List<Sprite> list = u.GetSpriteList();
            if(list.Count > 0)
                s = list[0];
        }

        float reboundDamage = 0.01f*GetParamValue("rebound_rate2", mHertIndex) *u.mCurrentHp;
        // 使单位直接死亡（不触发死亡特效）
        u.DeathEvent();

        if (s == null)
            return;
        BaseEffect e = BaseEffect.CreateInstance(s);
        Vector2 startPos = u.transform.position;
        e.transform.position = startPos;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            timeLeft--;
            float rate = 1 - ((float)timeLeft / totalTime);
            float s = Mathf.Min(1, (float)timeLeft / 15);
            e.transform.position = Vector2.Lerp(startPos, transform.position, rate);
            e.transform.localScale = new Vector2(s, s);
            if (timeLeft <= 0)
                return true;
            else
                return false;
        });
        t.AddOnExitAction(delegate {
            new DamageAction(CombatAction.ActionType.CauseDamage, null, this, reboundDamage).ApplyAction();
            e.ExecuteDeath();
        });
        e.AddTask(t);
        GameController.Instance.AddEffect(e);
    }

    private BaseEffect CreateShit(Vector2 pos)
    {
        List<BaseEffect> fogEffectList = new List<BaseEffect>();
        BaseEffect e = BaseEffect.CreateInstance(Shit_Run, null, "Idle", "Die", true);
        e.SetSpriteRendererSorting("Effect", 2);
        e.transform.position = pos;
        e.AddBeforeDeathAction(delegate {
            foreach (var e in fogEffectList)
                e.ExecuteDeath();
        });
        GameController.Instance.AddEffect(e);
        // 持续时间任务
        {
            int timeLeft = Mathf.FloorToInt(60 * GetParamValue("t3_1", mHertIndex));
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.AddTask(t);
        }

        // 挂上一个检测区域,检测本格存在的单位并作对应处理
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "EnemyAllyGrid");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectGrid = true;
            r.isAffectCharacter = true;
            Action<BaseUnit> action0 = (u) => {
                new DamageAction(CombatAction.ActionType.RealDamage, null, u, u.GetCurrentHp()).ApplyAction();
            };
            Action<MouseUnit> action1 = (m) => {
                if(!m.IsBoss())
                    new DamageAction(CombatAction.ActionType.RealDamage, null, m, m.GetCurrentHp()).ApplyAction();
            };
            r.SetOnFoodEnterAction(action0);
            r.SetOnEnemyEnterAction(action1);
            r.SetOnFoodStayAction(action0);
            r.SetOnEnemyStayAction(action1);
            r.SetOnCharacterStayAction((u) => {
                StatusAbility s = u.GetNoCountUniqueStatus(StringManager.Stun);
                if (s == null)
                {
                    s = new StunStatusAbility(u, 2, false);
                }
                else
                {
                    s.leftTime = Mathf.Max(s.leftTime, 2);
                }
            });
            Func<BaseGrid, FoodInGridType, bool> nobuildFunc = delegate { return false; };
            r.SetOnGridEnterAction((g) => {
                g.AddCanBuildFuncListener(nobuildFunc);
            });
            r.SetOnGridExitAction((g) => {
                g.RemoveCanBuildFuncListener(nobuildFunc);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            e.AddBeforeDeathAction(delegate {
                r.MDestory();
            });
        }
        // 初始在原地生成四个方向的毒气
        {
            CreateFog(fogEffectList, pos, Vector2.left);
            CreateFog(fogEffectList, pos, Vector2.right);
            CreateFog(fogEffectList, pos, Vector2.up);
            CreateFog(fogEffectList, pos, Vector2.down);
        }

        return e;
    }

    private void CreateFog(List<BaseEffect> fogEffectList, Vector2 pos, Vector2 rotate)
    {
        // 越界检测
        if (pos.x <= MapManager.GetColumnX(-0.5f) || pos.x >= MapManager.GetColumnX(8.5f) || pos.y >= MapManager.GetRowY(-0.5f) || pos.y <= MapManager.GetRowY(6.5f))
            return;

        float min_dmg = GetParamValue("min_dmg3", mHertIndex);
        float percent_dmg = GetParamValue("percent_dmg3", mHertIndex)*0.01f;

        RetangleAreaEffectExecution r = null;
        int timeLeft = Mathf.FloorToInt(60 * GetParamValue("spreadTime", mHertIndex));
        
        BaseEffect e = BaseEffect.CreateInstance(Fog_Run, "Appear", "Idle", "Disappear", true);
        e.SetSpriteRendererSorting("Effect", 1);
        e.transform.position = pos;
        GameController.Instance.AddEffect(e);
        fogEffectList.Add(e);
        e.AddBeforeDeathAction(delegate { r.MDestory(); });

        // 毒气检测区域
        {
            r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "EnemyAllyGrid");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;
            r.isAffectGrid = true;
            Action<BaseUnit> stunAction = (u) =>
            {
                StatusAbility s = u.GetNoCountUniqueStatus(StringManager.Stun);
                if (s == null)
                {
                    s = new StunStatusAbility(u, 2, false);
                }
                else
                {
                    s.leftTime = Mathf.Max(s.leftTime, 2);
                }
            };
            r.SetOnEnemyStayAction(stunAction);
            r.SetOnCharacterStayAction(stunAction);
            GameController.Instance.AddAreaEffectExecution(r);

            // 添加扩散计时器
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    if (r.foodUnitList.Count <= 0)
                        timeLeft--;
                    if (timeLeft <= 0)
                        return true;
                    else
                        return false;
                });
                t.AddOnExitAction(delegate {
                    CreateFog(fogEffectList, pos + new Vector2(rotate.x * MapManager.gridWidth, rotate.y * MapManager.gridHeight), rotate);
                });
                r.AddTask(t);
            }
            // 添加毒伤计时器
            {
                int interval = 60;
                int left = interval;
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    left--;
                    if(left <= 0)
                    {
                        foreach (var g in r.gridList)
                        {
                            g.TakeAction(null, (u) => { new DamageAction(CombatAction.ActionType.CauseDamage, null, u, Mathf.Max(min_dmg, percent_dmg * u.mMaxHp)).ApplyAction(); }, false);
                        }
                        left += interval;
                    }
                    return false;
                });
                r.AddTask(t);
            }
        }
        
    }

    private void FindR0(out int rowIndex)
    {
        List<int> list = FoodManager.GetRowListWhichHasMinConditionAllyCount((u) => {
            if (UnitManager.CanBeSelectedAsTarget(this, u) && u is FoodUnit && FoodManager.IsAttackableFoodType((u as FoodUnit)))
                return true;
            return false;
        });
        if (list.Count > 0)
            rowIndex = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 4;
        }
    }

    private void FindR1(out int rowIndex)
    {
        List<int> list = FoodManager.GetRowListWhichHasMaxConditionAllyCount((u) => {
            if (UnitManager.CanBeSelectedAsTarget(this, u) && u is FoodUnit && FoodManager.IsAttackableFoodType((u as FoodUnit)))
                return true;
            return false;
        });
        if (list.Count > 0)
            rowIndex = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 4;
        }
    }

    private void FindR2(out int rowIndex)
    {
        List<int> list = FoodManager.GetRowListBySpecificConditions((standUnit, u2)=> {
            if (standUnit == null)
                return true;
            if (u2.transform.position.x > standUnit.transform.position.x)
                return true;
            else
                return false;
        }, (compareUnit, rowStandUnit)=> {
            if (rowStandUnit == null)
                return -1;
            if (compareUnit == null)
                return 1;
            if (rowStandUnit.transform.position.x < compareUnit.transform.position.x)
                return 1;
            else if (rowStandUnit.transform.position.x == compareUnit.transform.position.x)
                return 0;
            else
                return -1;
        });
        if (list.Count > 0)
            rowIndex = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 4;
        }
    }

    private void FindR3C3(out int rowIndex, out int colIndex)
    {
        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
        float maxDist = 0;
        for (int i = 0; i < 7; i++)
        {
            BaseUnit left = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(i, float.MaxValue, false);
            BaseUnit right = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(i, float.MinValue, false);
            if(left != null)
            {
                float dist = right.transform.position.x - left.transform.position.x;
                if(dist > maxDist)
                {
                    list.Clear();
                    list.Add(i);
                    maxDist = dist;
                }else if(dist == maxDist)
                {
                    list.Add(i);
                }
            }
        }

        //List<int> list = FoodManager.GetRowListWhichHasMaxConditionAllyCount((u) => {
        //    if (u is FoodUnit && FoodManager.IsAttackableFoodType((u as FoodUnit)))
        //        return true;
        //    return false;
        //});
        if (list.Count > 0)
            rowIndex = list[GetRandomNext(0, list.Count)];
        else
        {
            GetRandomNext(0, 1);
            rowIndex = 4;
        }

        BaseUnit leftUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(rowIndex, float.MaxValue, false);
        BaseUnit rightUnit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, false);
        if (leftUnit == null || rightUnit == null)
            colIndex = 4;
        else
            colIndex = MapManager.GetXIndex((leftUnit.transform.position.x + rightUnit.transform.position.x) / 2);
    }

    /// <summary>
    /// 地狱召唤
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int num = Mathf.FloorToInt(GetParamValue("num0", mHertIndex));
        int totalTime = Mathf.FloorToInt(60 * GetParamValue("t0", mHertIndex));
        int interval = totalTime / num;
        // 变量
        int timeLeft = 0;
        List<int> rowIndexList = new List<int>();

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R0行右一列
                    int rowIndex = 3;
                    FindR0(out rowIndex);
                    transform.position = MapManager.GetGridLocalPosition(8, rowIndex);
                    rowIndexList.Clear();
                    rowIndexList.Add(rowIndex);
                    int count = 1;
                    while(count <= 6)
                    {
                        int r = rowIndex + count;
                        if(r >= 0 && r <= 6)
                        {
                            rowIndexList.Add(r);
                            if (rowIndexList.Count >= num)
                                break;
                        }
                        r = rowIndex - count;
                        if (r >= 0 && r <= 6)
                        {
                            rowIndexList.Add(r);
                            if (rowIndexList.Count >= num)
                                break;
                        }
                        count ++;
                    }
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Cast", true);
                    timeLeft = interval;
                    return true;
                }
                return false;
            });
            for (int i = 0; i < num; i++)
            {
                int j = i;
                c.AddSpellingFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        timeLeft += interval;
                        Skill0GhostFire(transform.position, rowIndexList[j], totalTime - interval * j);
                        return true;
                    }
                    return false;
                });
            }
            c.AddSpellingFunc(delegate {
                timeLeft = 35 + interval;
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    animatorController.Play("Summon");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
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
    /// 冥火权杖
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int totalTime = Mathf.FloorToInt(60 * GetParamValue("t1", mHertIndex));
        int colIndex = 4;
        // 变量
        int timeLeft = 0;
        int rowIndex = 3;
        

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R1行
                    FindR1(out rowIndex);
                    transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    timeLeft = totalTime;
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    animatorController.Play("Throw");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.44f)
                {
                    CreateSceptre(transform.position);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
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
    /// 大快朵颐
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int t2_0 = Mathf.FloorToInt(60 * GetParamValue("t2_0", mHertIndex));
        int t2_1 = Mathf.FloorToInt(60 * GetParamValue("t2_1", mHertIndex));
        int num = Mathf.FloorToInt(GetParamValue("num2", mHertIndex));
        int healInterval = 60;
        // 变量
        int timeLeft = 0;
        int sleepTimeLeft = 0;
        int healTimeLeft = 0;
        FoodUnit[] unitArray = new FoodUnit[num];

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R2行
                    int rowIndex = 3;
                    FindR2(out rowIndex);
                    transform.position = MapManager.GetGridLocalPosition(8, rowIndex);
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("PreThink");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Think", true);
                    timeLeft = t2_0;
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    // 去找离自己近的美食
                    for (int i = 0; i < unitArray.Length; i++)
                    {
                        unitArray[i] = null;
                    }
                    foreach (var u in GameController.Instance.GetEachAlly())
                    {
                        if(u.transform.position.x > MapManager.GetColumnX(1.5f) && u is FoodUnit)
                        {
                            FoodUnit f = u as FoodUnit;
                            if (FoodManager.IsAttackableFoodType(f))
                            {
                                for (int i = 0; i < unitArray.Length; i++)
                                {
                                    BaseUnit other = unitArray[i];
                                    if (other == null)
                                    {
                                        unitArray[i] = f;
                                        break;
                                    }else if (((Vector2)u.transform.position - (Vector2)transform.position).magnitude < ((Vector2)other.transform.position - (Vector2)transform.position).magnitude)
                                    {
                                        for (int j = unitArray.Length - 1; j > i; j--)
                                        {
                                            unitArray[j] = unitArray[j-1];
                                        }
                                        unitArray[i] = f;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // 计算T并对表中所有单位执行Eat
                    float totalCost = 0;
                    for (int i = 0; i < unitArray.Length; i++)
                    {
                        FoodUnit f = unitArray[i];
                        if (f == null)
                            break;
                        BaseCardBuilder builder = f.GetCardBuilder();
                        if (builder != null)
                        {
                            totalCost += (float)builder.attr.GetCost(f.mLevel);
                        }
                        Eat(f);
                    }
                    sleepTimeLeft = Mathf.Min(600, Mathf.Max(80, Mathf.FloorToInt(totalCost / (15 * num) * 60)));
                    animatorController.Play("Eat", true);
                    timeLeft = t2_1;
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    animatorController.Play("PreSleep");
                    healTimeLeft = healInterval;
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                sleepTimeLeft--;
                healTimeLeft--;
                if(healTimeLeft <= 0)
                {
                    //EffectManager.AddHealEffectToUnit(this);
                    //new CureAction(CombatAction.ActionType.GiveCure, this, this, heal_percent * GetLostHp()).ApplyAction();
                    healTimeLeft += healInterval;
                }

                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Sleep", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                sleepTimeLeft--;
                healTimeLeft--;
                if (healTimeLeft <= 0)
                {
                    //EffectManager.AddHealEffectToUnit(this);
                    //new CureAction(CombatAction.ActionType.GiveCure, this, this, heal_percent * GetLostHp()).ApplyAction();
                    healTimeLeft += healInterval;
                }

                if (sleepTimeLeft <= 0)
                {
                    animatorController.Play("PostSleep");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
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
    /// 冥界美味
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill3Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int totalTime = Mathf.FloorToInt(60 * GetParamValue("t3_0", mHertIndex));
        // 变量
        int timeLeft = 0;
        BaseEffect shit = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
            shit = null;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R3行C3列
                    int rowIndex = 3;
                    int colIndex = 4;
                    FindR3C3(out rowIndex, out colIndex);
                    transform.position = MapManager.GetGridLocalPosition(colIndex + 1, rowIndex);
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 转向拉屎
                    //spriteRenderer.flipX = true;
                    transform.localScale = new Vector2(-1, 1);
                    animatorController.Play("PreShit");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Shit", true);
                    timeLeft = totalTime;
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    shit = CreateShit(transform.position - MapManager.gridWidth*Vector3.right);
                    animatorController.Play("Idle2", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (!shit.IsValid())
                {
                    animatorController.Play("Fetch");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.56f)
                {
                    RecycleSceptre();
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    transform.localScale = Vector2.one;
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
