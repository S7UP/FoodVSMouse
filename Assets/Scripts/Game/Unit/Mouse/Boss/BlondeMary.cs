using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 金发玛丽
/// </summary>
public class BlondeMary : BossUnit
{
    private static RuntimeAnimatorController Bullet_AnimatorController;

    public override void Awake()
    {
        if (Bullet_AnimatorController == null)
        {
            Bullet_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/6/Bullet");
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

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.BlondeMary, 0))
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
        list.Add(SKill0Init(infoList[0]));
        list.Add(SKill2Init(infoList[2]));
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

    private void FindR0C0(out int R0, out int C0)
    {
        int colIndex = Mathf.FloorToInt(GetParamValue("col0", mHertIndex)) - 1;
        C0 = colIndex;
        List<int> rowList = FoodManager.GetRowListWhichHasMaxConditionAllyCount(
            (u)=> { 
                if(u is FoodUnit)
                {
                    FoodUnit f = u as FoodUnit;
                    return FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, u) && u.transform.position.x >= MapManager.GetColumnX(-0.5f) && u.transform.position.x <= MapManager.GetColumnX(colIndex + 0.5f);
                }
                return false;
            }
        );
        // 然后随机选一行吧
        if (rowList.Count > 0)
            R0 = rowList[GetRandomNext(0, rowList.Count)];
        else
            R0 = 3;
    }

    private BaseUnit CreateCheckArea()
    {
        float dmg0 = GetParamValue("dmg0", mHertIndex);

        // 生成一个隐形挨炸的老鼠
        MouseUnit m = MouseManager.GetBombedToolMouse();
        m.SetMaxHpAndCurrentHp(mMaxHp);
        m.transform.position = transform.position;
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (this.IsAlive())
                    m.transform.position = transform.position;
                else
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                m.ExecuteDeath();
            });
            m.AddTask(t);
        }


        // 产生一个踩踏检测区域
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.1f, 0.5f, "EnemyAllyGrid");
            r.isAffectMouse = true;
            r.AddExcludeMouseUnit(this);
            r.AddExcludeMouseUnit(m);
            r.SetOnEnemyEnterAction((u) => {
                if(UnitManager.CanBeSelectedAsTarget(this, u))
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg0).ApplyAction();
            });
            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeDamage(this, dmg0, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (m.IsAlive())
                    r.transform.position = m.transform.position;
                else
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
        return m;
    }

    private void FindR2(out int R2)
    {
        List<int> rowList = FoodManager.GetRowListWhichHasMaxConditionAllyCount(
            (u)=> {
                if (u is FoodUnit)
                {
                    FoodUnit f = u as FoodUnit;
                    return FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, u);
                }
                return false;
            }
        );
        // 然后随机选一行吧
        if (rowList.Count > 0)
            R2 = rowList[GetRandomNext(0, rowList.Count)];
        else
            R2 = 3;
    }

    private BaseBullet CreateBullet()
    {
        int time = Mathf.FloorToInt(60 * GetParamValue("flyTime1", mHertIndex)); ; // 一次回旋需要的时间
        float deltaAngle = 2 * Mathf.PI / time;
        float a1 = GetParamValue("a1", mHertIndex) * MapManager.gridWidth; // 长轴
        float b1 = GetParamValue("b1", mHertIndex) * MapManager.gridHeight; // 短轴
        float px = MapManager.GetColumnX(8);
        float py = MapManager.GetRowY(3);

        EnemyBullet b = EnemyBullet.GetInstance(Bullet_AnimatorController, this, 0);
        b.CloseCollision();

        // 控制运动
        {
            int currentTime = 0;
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                currentTime++;
                b.transform.position = new Vector2(px + a1 * (Mathf.Cos(currentTime * deltaAngle) - 1), py + b1 * Mathf.Sin(currentTime * deltaAngle));
                if (currentTime >= time || !b.IsAlive())
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate {
                b.ExecuteRecycle();
            });
            b.AddTask(t);
            GameController.Instance.AddBullet(b);
        }


        // 携带一个判定区域
        {
            float dmg1 = GetParamValue("dmg1", mHertIndex);
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 0.1f, 0.1f, "EnemyAllyGrid");
            r.isAffectGrid = true;
            Action<BaseGrid> enterAction = (g) => {
                g.TakeDamage(this, dmg1, false);
                r.AddExcludeGrid(g); // 真的只能一次！
            };
            r.SetOnGridEnterAction(enterAction);
            r.isAffectMouse = true;
            r.AddExcludeMouseUnit(this);
            r.SetOnEnemyEnterAction((u) => {
                if (UnitManager.CanBeSelectedAsTarget(this, u))
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg1).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (b.IsAlive())
                {
                    r.transform.position = b.transform.position;
                    return false;
                }
                return true;
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
        return b;
    }

    /// <summary>
    /// 踩踏
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int t0 = Mathf.FloorToInt(60*GetParamValue("t0", mHertIndex));
        int timeLeft = 0;
        float v = TransManager.TranToVelocity(6); // 移动速度
        BaseUnit toolUnit = null; // 用来检测移动时被炸的单位

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
            toolUnit = null;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 去R0行C0列
                    int R0;
                    int C0;
                    FindR0C0(out R0, out C0);
                    transform.position = MapManager.GetGridLocalPosition(C0, R0);
                    animatorController.Play("Appear");
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
                    // 计算自身到左侧一列的x距离
                    float s = Mathf.Abs(transform.position.x - MapManager.GetColumnX(0));
                    timeLeft = Mathf.FloorToInt(s / v);
                    // 开踩
                    toolUnit = CreateCheckArea();
                    animatorController.Play("Move", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                transform.position += Vector3.left*v;
                if (timeLeft <= 0 || transform.position.x <= MapManager.GetColumnX(0) || !toolUnit.IsAlive())
                {
                    if (toolUnit != null && toolUnit.IsAlive())
                        toolUnit.ExecuteDeath();
                    animatorController.Play("Idle", true);
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
    /// 回旋镖
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        int t1 = Mathf.FloorToInt(60 * GetParamValue("t1", mHertIndex));
        int timeLeft = 0;
        BaseBullet bullet = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
            bullet = null;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    transform.position = MapManager.GetGridLocalPosition(8, 3);
                    animatorController.Play("Appear");
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
                    animatorController.Play("PreAttack");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Attack", true);
                    // 丢回旋镖
                    bullet = CreateBullet();
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (bullet == null || !bullet.IsAlive())
                {
                    animatorController.Play("PostAttack", true);
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
    /// 催眠曲
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int t2 = Mathf.FloorToInt(60 * GetParamValue("t2", mHertIndex));
        int stun2 = Mathf.FloorToInt(60 * GetParamValue("stun2", mHertIndex));
        int timeLeft = 0;

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
                    // 去R0行C0列
                    int R2;
                    FindR2(out R2);
                    transform.position = MapManager.GetGridLocalPosition(8, R2);
                    animatorController.Play("Appear");
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
                    animatorController.Play("Cast");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 停一会儿
                    timeLeft = stun2;
                    foreach (var unit in GameController.Instance.GetEachAlly())
                    {
                        if(unit is FoodUnit)
                        {
                            FoodUnit f = unit as FoodUnit;
                            if (FoodManager.IsAttackableFoodType(f))
                                f.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(f, stun2, false));
                        }else if(unit is CharacterUnit)
                        {
                            unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, stun2, false));
                        }
                    }
                    foreach (var unit in GameController.Instance.GetEachEnemy())
                    {
                        unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, stun2/2, false));
                    }
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
