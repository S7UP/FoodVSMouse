using System;
using System.Collections.Generic;

using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 列车终极
/// </summary>
public class RatTrain2 : BaseRatTrain
{
    private static RuntimeAnimatorController SummonSoldier_RuntimeAnimatorController;
    private static RuntimeAnimatorController MissileAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController Missile_RuntimeAnimatorController;
    private static RuntimeAnimatorController FogCreator_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserEffect_RuntimeAnimatorController;
    private static RuntimeAnimatorController Bomb_RuntimeAnimatorController;
    private static RuntimeAnimatorController Oil_Run;
    private static Sprite Wait_Icon_Sprite;
    private static Sprite Speed_Icon_Sprite;


    public override void Awake()
    {
        if (SummonSoldier_RuntimeAnimatorController == null)
        {
            SummonSoldier_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/SummonSoldier");
            MissileAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/MissileAttacker");
            Missile_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Missile");
            FogCreator_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FogCreator");
            LaserAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserAttacker");
            LaserEffect_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserEffect");
            Bomb_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Bomb");
            Oil_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Oil");
            Speed_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Speed");
            Wait_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Wait");
        }
        // 被炸事件定义
        {
            burnAction = (combat) =>
            {
                if (combat is DamageAction)
                {
                    DamageAction dmgAction = combat as DamageAction;
                    if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    {
                        stun_timeLeft = Mathf.Min(max_stun_time, stun_timeLeft + Mathf.FloorToInt(GetParamValue("p_stun_time") * 60));
                    }
                }
            };
        }
        base.Awake();
    }

    private List<BaseUnit> retinueList = new List<BaseUnit>(); // 自身随从表（在死亡时同时销毁所有随从）

    private int max_stun_time; // 最大晕眩时间
    private int stun_timeLeft; // 晕眩时间
    private Action<CombatAction> burnAction; // 被炸事件

    public override void MInit()
    {
        stun_timeLeft = 0;
        max_stun_time = 0;
        retinueList.Clear();
        base.MInit();

        SetActionState(new IdleState(this));
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in retinueList)
            if (!u.IsAlive())
                delList.Add(u);
        foreach (var u in delList)
            retinueList.Remove(u);
        base.MUpdate();
    }

    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.875f, 0.75f, 0.625f, 0.5f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.RatTrain, 2))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility> list = new List<SkillAbility>();
        if (mHertIndex == 0)
            list.Add(Movement0());
        else if (mHertIndex == 1)
            list.Add(Movement1());
        else if (mHertIndex == 2)
            list.Add(Movement2());
        else if (mHertIndex == 3)
            list.Add(Movement3());
        else if (mHertIndex == 4)
            list.Add(Movement4());
        // list.Add(Movement4());
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    #region 指令-奇袭
    private CustomizationTask GetUnitAppearTask(MouseUnit m, Vector2 startV2, Vector2 endV2, int stun_time)
    {
        // 一些初始出现动画不能被击中的效果
        Func<BaseUnit, BaseUnit, bool> noSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        BoolModifier noUseBearMod = new BoolModifier(true);

        CustomizationTask task = new CustomizationTask();
        int totalTime = 60;
        int currentTime = 0;
        task.AddOnEnterAction(delegate
        {
            m.DisableMove(true);
            m.transform.position = startV2; // 对初始坐标进行进一步修正
            m.AddCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc); // 不可作为选取的目标
            m.AddCanBlockFunc(noBlockFunc); // 不可被阻挡
            m.AddCanHitFunc(noHitFunc); // 不可被子弹击中
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.NoBearInSky, noUseBearMod); // 不占用承载数
            Environment.SkyManager.AddNoAffectBySky(m, noUseBearMod); // 不会掉下去
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
            else
            {
                m.NumericBox.RemoveDecideModifierToBoolDict(StringManager.NoBearInSky, noUseBearMod);
                return true;
            }
        });
        task.AddOnExitAction(delegate
        {
            m.DisableMove(false);
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
            Environment.SkyManager.RemoveNoAffectBySky(m, noUseBearMod);
            // 自我晕眩
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        });
        return task;
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    private void SpawnEnemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape, int stun_time)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.NumericBox.AddDecideModifierToBoolDict("isSummon", new BoolModifier(true)); // 由三车召唤出来的老鼠的标记
        m.moveRotate = rot;
        if (m.moveRotate.x > 0)
            m.transform.localScale = new Vector2(-1, 1);

        m.AddTask(GetUnitAppearTask(m, startV2, endV2, stun_time));
    }

    /// <summary>
    /// 创建一个敌人生成器
    /// </summary>
    private MouseModel CreateEnemySwpaner(RatTrainComponent master, bool isLeft)
    {
        MouseModel m = MouseModel.GetInstance(SummonSoldier_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(master.GetCurrentHp(), 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate * master.moveRotate.y;
            m.transform.localScale = new Vector2(hscale, 1);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, (isLeft ? -1 : 1) * 0.98f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            m.canTriggerCat = false;
            // 收纳为自身随从
            retinueList.Add(m);

            // 动作
            {
                CustomizationTask t = new CustomizationTask();
                t.AddOnEnterAction(delegate
                {
                    if (isLeft)
                        m.animatorController.Play("PreSpawn1");
                    else
                        m.animatorController.Play("PreSpawn0");
                });
                t.AddTaskFunc(delegate
                {
                    return m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                });
                t.AddOnExitAction(delegate {
                    if (isLeft)
                        m.animatorController.Play("Spawn1", true);
                    else
                        m.animatorController.Play("Spawn0", true);
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 2; // 图层-2
        }
        return m;
    }
    #endregion

    #region 指令-炮击
    /// <summary>
    /// 发射一枚炮弹
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateMissile(MouseUnit master, Vector2 pos, Vector2 rotate)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Missile_RuntimeAnimatorController, master, 0);
        b.CloseCollision();
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(24);
        b.transform.position = pos;
        b.SetRotate(rotate);
        GameController.Instance.AddBullet(b);

        // 添加范围检测
        {
            Action<BaseUnit> hitAction = delegate
            {
                // 伤害格子
                DamageGrid(b.transform.position);

                // 伤害老鼠
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1f, 1f, "ItemCollideEnemy");
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    r.AddExcludeMouseUnit(master);
                    r.SetOnEnemyEnterAction((u) => {
                        if (!u.IsBoss())
                            BurnManager.BurnDamage(master, u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                b.KillThis(); // 子弹爆炸
            };

            bool isHit = false;
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 0.25f, 0.25f, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.AddFoodEnterConditionFunc((u) => {
                return UnitManager.CanBulletHit(u, b);
            });
            r.SetOnFoodEnterAction(delegate {
                if (!isHit)
                {
                    isHit = true;
                    hitAction(null);
                }
            });
            r.AddEnemyEnterConditionFunc((u) => {
                return UnitManager.CanBulletHit(u, b);
            });
            r.SetOnEnemyEnterAction(delegate {
                if (!isHit)
                {
                    isHit = true;
                    hitAction(null);
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = b.transform.position;
                return !b.IsAlive() || isHit;
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }

        return b;
    }

    private void DamageGrid(Vector2 pos)
    {
        // 伤害最近的格子
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "CollideGrid");
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.AddBeforeDestoryAction(delegate {
                BaseGrid targetGrid = null;
                float minDist = float.MaxValue;
                foreach (var g in r.gridList.ToArray())
                {
                    float dist = (r.transform.position - g.transform.position).magnitude;
                    if (dist < minDist)
                    {
                        targetGrid = g;
                        minDist = dist;
                    }
                }
                if (targetGrid != null)
                {
                    targetGrid.TakeAction(this, (u) => { BurnManager.BurnDamage(this, u); }, false);
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    /// <summary>
    /// 创建一个炮弹发射器
    /// </summary>
    private MouseModel CreateMissileAttacker(RatTrainComponent master, bool isAttackLeft)
    {
        MouseModel m = MouseModel.GetInstance(MissileAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(master.GetCurrentHp(), 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            if (isAttackLeft)
                m.transform.right = master.moveRotate * Mathf.Sign(master.moveRotate.y);
            else
                m.transform.right = -master.moveRotate * Mathf.Sign(master.moveRotate.y);
            m.transform.localScale = new Vector2(hscale, 1);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, 0.98f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            m.canTriggerCat = false;
            // 收纳为自身随从
            retinueList.Add(m);
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 1; // 图层-1
        }
        return m;
    }
    #endregion

    #region 指令-雾袭
    /// <summary>
    /// 创建一个喷雾器
    /// </summary>
    private MouseModel CreateFogCreator(MouseUnit master, bool isLeft)
    {
        MouseModel m = MouseModel.GetInstance(FogCreator_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(master.mMaxHp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = master.transform.right;
            m.transform.localScale = new Vector2(hscale, (isLeft ? 1 : -1));
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, 1f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // 收纳为自身随从
            retinueList.Add(m);
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 2; // 图层-2
            m.CloseCollision();
        }
        return m;
    }
    #endregion

    #region 指令-冷凝激光
    /// <summary>
    /// 创建一个激光发射器
    /// </summary>
    private MouseModel CreateLaserAttacker(RatTrainComponent master, bool isLeft)
    {
        MouseModel m = MouseModel.GetInstance(LaserAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
            m.SetBaseAttribute(master.mMaxHp, 1, 1f, 0, 100, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = master.transform.right;
            m.transform.localScale = new Vector2(hscale, (isLeft ? 1 : -1));
            m.AddCanHitFunc(delegate { return false; });
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.mBoxCollider2D.offset = new Vector2(0, 1.0f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // 收纳为自身随从
            retinueList.Add(m);
        }
        GameController.Instance.AddMouseUnit(m);
        m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 1; // 图层-1
        return m;
    }
    #endregion

    #region 指令-定时炸弹
    private MouseModel CreateBomb(RatTrainComponent master)
    {
        MouseModel m = MouseModel.GetInstance(Bomb_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(0));
            m.SetBaseAttribute(master.mMaxHp, 1, 1f, 0, 100, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = master.transform.right;
            m.AddCanHitFunc(delegate { return false; });
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.mBoxCollider2D.offset = new Vector2(0, 1.0f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // 收纳为自身随从
            retinueList.Add(m);
        }
        GameController.Instance.AddMouseUnit(m);
        m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder + 1; // 图层-1
        return m;
    }
    #endregion

    #region 通用
    /// <summary>
    /// 产生一个移动相关的技能（给下面的方法用）
    /// </summary>
    /// <param name="pointList">起始点,结束点 对</param>
    /// <param name="waitDist">停靠前需要移动的距离</param>
    /// <param name="wait">停靠时间</param>
    /// <param name="OnEnterWaitAction">停靠一刻做的事</param>
    /// <param name="OnExitWaitAction">停靠结束时做的事</param>
    /// <param name="stage">阶段数</param>
    /// <returns></returns>
    private CompoundSkillAbility CreateMovementFunc(List<Vector2[]> pointList, float waitDist, int wait, Action OnEnterMoveAction, Action OnEnterWaitAction, Action OnExitWaitAction, int stage)
    {
        // 变量
        int timeLeft = 0;
        float distLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            OnEnterMoveAction();
            timeLeft = 0;
            distLeft = waitDist;
            SetHeadMoveDestination();
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
                list.Add(new Vector2[] { p[0], p[1] });
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // 是否抵达停靠点
            c.AddSpellingFunc(delegate {
                distLeft -= GetMoveSpeed();
                if (distLeft <= 0)
                {
                    CancelSetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new IdleState(this));
                    
                    timeLeft = wait;
                    return true;
                }
                return false;
            });
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTimeTaskFunc(30);
                task.AddOnExitAction(delegate {
                    OnEnterWaitAction();
                });
                return task;
            });
            // 等待
            c.AddSpellingFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0 || mHertIndex > stage)
                {
                    SetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new MoveState(this));
                    OnExitWaitAction();
                    return true;
                }
                return false;
            });
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (IsHeadMoveToDestination())
                {
                    SetActionState(new IdleState(this));
                    GetHead().MDestory();
                    ClearRouteComponetPosInfoDict();
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
    /// 创建撞击区域
    /// </summary>
    /// <param name="master"></param>
    private void CreateHitArea(MouseUnit master, string collide_group)
    {
        float re_rate = GetParamValue("p_rebound_dmg")/100;

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(master.transform.position, new Vector2(1f * MapManager.gridWidth, 1f * MapManager.gridHeight), collide_group);
        //r.isAffectFood = true;
        r.isAffectMouse = true;
        r.AddEnemyEnterConditionFunc((u) => {
            return !u.IsBoss() && MouseManager.IsGeneralMouse(u) && u.GetHeight()==0;
        });
        //r.SetOnFoodEnterAction((u)=> {
        //    DamageAction d = UnitManager.Execute(master, u);
        //    new DamageAction(CombatAction.ActionType.CauseDamage, null, this, d.DamageValue * re_rate).ApplyAction();
        //});
        r.SetOnEnemyEnterAction((u) => {
            // UnitManager.Execute(master, u);

            KnockFlyingMouse(u, master.DeltaPosition.normalized * UnityEngine.Random.Range(15, 20)*MapManager.gridWidth);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        // 跟随master 并 读取master的CanHit来决定是否消亡
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = master.transform.position;
                return !master.IsAlive() || !master.NumericBox.GetBoolNumericValue("CanHit");
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
    }

    /// <summary>
    /// 创建传送区域（P1P2）
    /// </summary>
    private RetangleAreaEffectExecution CreateTpArea(Vector2 startPos, Vector2 endPos, Vector2 moveRotate, int stun_time)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(startPos, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.name = "TpArea";
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.AddEnemyEnterConditionFunc((u) => {
            return !u.NumericBox.GetBoolNumericValue("isSummon") && !u.IsBoss() && MouseManager.IsGeneralMouse(u) && u.GetHeight() == 0 && !u.NumericBox.GetBoolNumericValue("isTp") && !UnitManager.IsFlying(u);
        });
        r.SetOnEnemyEnterAction((u) => {
            u.NumericBox.AddDecideModifierToBoolDict("isTp", new BoolModifier(true));
            u.SetMoveRoate(moveRotate);
            u.transform.position = endPos;
            u.transform.localScale = new Vector2(-moveRotate.x, 1);
            // 自我晕眩
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        GameController.Instance.AddAreaEffectExecution(r);
        return r;
    }

    /// <summary>
    /// 创建传送区域（P3,P4）
    /// </summary>
    private RetangleAreaEffectExecution CreateTpArea2(Vector2 startPos, Vector2 endPos, Vector2 size, int stun_time)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(startPos, size, "ItemCollideEnemy");
        r.name = "TpArea";
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.AddEnemyEnterConditionFunc((u) => {
            return !u.NumericBox.GetBoolNumericValue("isSummon") && !u.IsBoss() && MouseManager.IsGeneralMouse(u) && u.GetHeight() == 0 && !u.NumericBox.GetBoolNumericValue("isTp") && !UnitManager.IsFlying(u);
        });
        r.SetOnEnemyEnterAction((u) => {
            u.NumericBox.AddDecideModifierToBoolDict("isTp", new BoolModifier(true));
            u.SetMoveRoate(Vector2.left);
            u.transform.position = endPos;
            u.transform.localScale = new Vector2(1, 1);
            //// 自我晕眩
            //u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
            int rowIndex = u.GetRowIndex();
            Vector2 move_pos = Vector2.zero;
            if (rowIndex == 0)
                move_pos = new Vector2(u.transform.position.x, MapManager.GetRowY(1));
            else if(rowIndex == 6)
                move_pos = new Vector2(u.transform.position.x, MapManager.GetRowY(5));
            else
            {
                int up_count = 0;
                int down_count = 0;
                foreach (var m in GameController.Instance.GetSpecificRowEnemyList(rowIndex - 1))
                    if (MouseManager.IsGeneralMouse(m))
                        up_count++;
                foreach (var m in GameController.Instance.GetSpecificRowEnemyList(rowIndex + 1))
                    if (MouseManager.IsGeneralMouse(m))
                        down_count++;

                List<int> list = new List<int>() { rowIndex - 1, rowIndex + 1 };
                int ran_rowIndex = list[GetRandomNext(0, list.Count)];
                if(up_count < down_count)
                    move_pos = new Vector2(u.transform.position.x, MapManager.GetRowY(rowIndex - 1));
                else if(up_count > down_count)
                    move_pos = new Vector2(u.transform.position.x, MapManager.GetRowY(rowIndex + 1));
                else
                {
                    move_pos = new Vector2(u.transform.position.x, MapManager.GetRowY(ran_rowIndex));
                }
            }
            u.AddTask(GetUnitAppearTask(u, endPos, move_pos, stun_time));
        });
        GameController.Instance.AddAreaEffectExecution(r);
        return r;
    }

    /// <summary>
    /// 创建加速油滴
    /// </summary>
    /// <returns></returns>
    private RetangleAreaEffectExecution CreateOilArea(Vector2 pos)
    {
        float move_speed_rate = GetParamValue("p_add_move_speed")/100 + 1;

        // 判定部分
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        {
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && MouseManager.IsGeneralMouse(u);
            });
            r.SetOnEnemyEnterAction((m) => {
                OilTask oil_task;
                ITask t = m.GetTask("RatTran2_OilTask");
                if (t != null)
                {
                    oil_task = t as OilTask;
                    oil_task.AddCount();
                }
                else
                {
                    oil_task = new OilTask(m, move_speed_rate);
                    m.taskController.AddUniqueTask("RatTran2_OilTask", oil_task);
                }
            });
            r.SetOnEnemyExitAction((m) => {
                OilTask oil_task;
                ITask t = m.GetTask("RatTran2_OilTask");
                if (t != null)
                {
                    oil_task = t as OilTask;
                    oil_task.DecCount();
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // 特效部分
        {
            BaseEffect e = BaseEffect.CreateInstance(Oil_Run, "Appear", "Idle", "Disappear", true);
            e.transform.position = pos;
            e.SetSpriteRendererSorting("Grid", 1);
            GameController.Instance.AddEffect(e);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return !r.IsValid();
            });
            task.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.taskController.AddTask(task);
        }
        return r;
    }

    #endregion

    #region P1
    private void CreateS0EnemySpawner(RatTrainComponent master, bool isLeft)
    {
        MouseModel m = CreateEnemySwpaner(master, isLeft);

        // 几秒后生成一只摔角
        {
            int start_wait = Mathf.FloorToInt(GetParamValue("sta_wait0") * 60);
            int start_type = Mathf.FloorToInt(GetParamValue("sta_type0"));
            int start_shape = Mathf.FloorToInt(GetParamValue("sta_shape0"));
            int start_stun = Mathf.FloorToInt(GetParamValue("sta_stun0") * 60);

            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(start_wait);
            task.AddOnExitAction(delegate {
                Vector2 rot = isLeft ? Vector2.left : Vector2.right;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                SpawnEnemy(pos + 0f * rot * MapManager.gridWidth, pos + 0.25f * rot * MapManager.gridWidth, rot, start_type, start_shape, start_stun);
            });
            m.taskController.AddTask(task);
        }
        // 每隔一定时间间隔后生成铁锅
        {
            int interval = Mathf.FloorToInt(GetParamValue("interval0") * 60);
            int sus_type = Mathf.FloorToInt(GetParamValue("sus_type0"));
            int sus_shape = Mathf.FloorToInt(GetParamValue("sus_shape0"));
            int sus_stun = Mathf.FloorToInt(GetParamValue("sus_stun0") * 60);

            int timeLeft = interval;
            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                timeLeft = interval;
            });
            task.AddTaskFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                { 
                    Vector2 rot = isLeft ? Vector2.left : Vector2.right;
                    Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                    SpawnEnemy(pos + 0f * rot * MapManager.gridWidth, pos + 0.25f * rot * MapManager.gridWidth, rot, sus_type, sus_shape, sus_stun);
                    timeLeft += interval;
                }
                return false;
            });
            m.taskController.AddTask(task);
        }
        // 结束时损毁事件
        {
            int des_type = Mathf.FloorToInt(GetParamValue("des_type0"));
            int des_shape = Mathf.FloorToInt(GetParamValue("des_shape0"));
            int des_stun = Mathf.FloorToInt(GetParamValue("des_stun0") * 60);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return !master.IsAlive();
            });
            task.AddOnExitAction(delegate {
                Vector2 rot = isLeft ? Vector2.left : Vector2.right;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                SpawnEnemy(pos, pos, rot, des_type, des_shape, des_stun);
                m.ExecuteDeath();
            });
            m.taskController.AddTask(task);
        }
    }

    /// <summary>
    /// P1
    /// </summary>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0()
    {
        float[] colIndexArray = new float[] { 8.5f, 7f, 1f, -0.5f };


        BoolModifier mod = new BoolModifier(true);
        int wait = Mathf.FloorToInt(GetParamValue("sus_wait0") * 60);
        int stun = Mathf.FloorToInt(GetParamValue("tp_stun0") * 60);

        List<Vector2[]> list = new List<Vector2[]>() {
            new Vector2[2] { new Vector2(colIndexArray[0], 7), new Vector2(colIndexArray[0], -1) },
            new Vector2[2] { new Vector2(colIndexArray[1], -2), new Vector2(colIndexArray[1], 8) },
            new Vector2[2] { new Vector2(colIndexArray[2], 8), new Vector2(colIndexArray[2], -2) },
            new Vector2[2] { new Vector2(colIndexArray[3], -1), new Vector2(colIndexArray[3], 7 + 26) },
        };

        List<RetangleAreaEffectExecution> tpAreaList = new List<RetangleAreaEffectExecution>(); // 传送区域表
        Action<BaseUnit> rmTpAreaAction = delegate {
            foreach (var r in tpAreaList)
                r.MDestory();
        };

        return CreateMovementFunc(
            list, // 起始点与终点
            35*MapManager.gridHeight + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            wait, // 获取停靠时间
            // 开始移动时
            delegate 
            {
                // 设置横向缩放
                hscale = 1.11f;
                // 创建14节车厢
                CreateHeadAndBody(18);
                // 设置车头、车身、车尾的伤害倍率
                GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
                foreach (var item in GetBodyList())
                    item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
                GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
                // 默认速度为2.4纵格/每秒
                NumericBox.MoveSpeed.SetBase(4.8f * MapManager.gridHeight / 60);
                SetAllComponentNoBeSelectedAsTargetAndHited();
                SetActionState(new MoveState(this));
                // 添加创人判定
                GetHead().NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                CreateHitArea(GetHead(), "BothCollide");
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                        CreateHitArea(u, "ItemCollideEnemy");
                    }
            },
            // 停靠时
            delegate
            {
                // 移除创人判定
                GetHead().NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                    }

                // 添加传送带车厢
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.y >= MapManager.GetRowY(7f) && body.transform.position.y <= MapManager.GetRowY(-1f))
                    {
                        if (body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateS0EnemySpawner(body, true);
                        }
                        else if (body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateS0EnemySpawner(body, false);
                        }
                        else
                        {
                            CreateS0EnemySpawner(body, true);
                            CreateS0EnemySpawner(body, false);
                        }
                    }
                }
                
                // 添加传送区域
                // 1 3 5 7
                for (int i = 0; i <= 6; i+=2)
                    tpAreaList.Add(CreateTpArea(MapManager.GetGridLocalPosition(colIndexArray[0], i), MapManager.GetGridLocalPosition(colIndexArray[3], i), Vector2.right, stun));
                // 2 4 6
                for (int i = 1; i <= 5; i += 2)
                    tpAreaList.Add(CreateTpArea(MapManager.GetGridLocalPosition(colIndexArray[1], i), MapManager.GetGridLocalPosition(colIndexArray[2], i), Vector2.right, stun));
                AddBeforeDeathEvent(rmTpAreaAction);
            },
            // 重新移动时
            delegate {
                // 所有车厢爆炸
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                        u.ExecuteDeath();
                // 移除传送区域
                rmTpAreaAction(this);
                RemoveBeforeDeathEvent(rmTpAreaAction);
                // 全场老鼠晕眩
                int stun_time = Mathf.FloorToInt(GetParamValue("des_stun0") * 60);
                foreach (var u in GameController.Instance.GetEachEnemy())
                {
                    if (!(u is MouseUnit))
                        continue;
                    MouseUnit m = u as MouseUnit;
                    if(!m.IsBoss() && MouseManager.IsGeneralMouse(m))
                    {
                        m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
                        BurnManager.BurnDamage(this, m);
                    }
                }
                // 强制下一p
                float[] arr = GetParamArray("hpRate");
                if (arr.Length > 0 && mHertIndex < 1)
                {
                    mHertIndex = 1;
                    OnHertStageChanged();
                }
            },
            0);
    }
    #endregion

    #region P2
    private void CreateS1Attacker(RatTrainComponent master, bool isAttackLeft, bool isAttackOnce)
    {
        MouseModel m = CreateMissileAttacker(master, isAttackLeft);

        // 初始
        {
            int wait = Mathf.FloorToInt(GetParamValue("sta_wait1") * 60);
            int interval = Mathf.FloorToInt(GetParamValue("interval1") * 60);
            int timeLeft = interval;

            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                m.animatorController.Play("PreAttack");
            });
            task.AddTimeTaskFunc(wait, null, null, delegate {
                Vector2 rot = isAttackLeft ? Vector2.left : Vector2.right;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                CreateMissile(master, pos + rot * MapManager.gridWidth, rot);
            });
            if (isAttackOnce)
            {
                // 只攻击一次就收回
                task.AddTaskFunc(delegate {
                    m.animatorController.Play("PostAttack");
                    return true;
                });
                task.AddTaskFunc(delegate {
                    return m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                });
                task.AddOnExitAction(delegate {
                    m.MDestory();
                });
            }
            else
            {
                // 攻击数次
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        Vector2 rot = isAttackLeft ? Vector2.left : Vector2.right;
                        Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                        CreateMissile(master, pos + rot * MapManager.gridWidth, rot);
                        timeLeft += interval;
                    }
                    return false;
                });
            }
            m.taskController.AddTask(task);
        }


        // 结束时损毁事件
        {
            int des_type = Mathf.FloorToInt(GetParamValue("des_type1"));
            int des_shape = Mathf.FloorToInt(GetParamValue("des_shape1"));
            int des_stun = Mathf.FloorToInt(GetParamValue("des_stun1") * 60);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return !master.IsAlive();
            });
            task.AddOnExitAction(delegate {
                Vector2 rot = isAttackLeft ? Vector2.left : Vector2.right;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                SpawnEnemy(pos, pos, rot, des_type, des_shape, des_stun);
                m.ExecuteDeath();
            });
            m.taskController.AddTask(task);
        }
    }

    /// <summary>
    /// P2
    /// </summary>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1()
    {
        float[] colIndexArray = new float[] { 6f, 4f, 2f };


        BoolModifier mod = new BoolModifier(true);
        int wait = Mathf.FloorToInt(GetParamValue("sus_wait1") * 60);
        int stun = Mathf.FloorToInt(GetParamValue("tp_stun1") * 60);

        List<Vector2[]> list = new List<Vector2[]>() {
            new Vector2[2] { new Vector2(colIndexArray[0], 7), new Vector2(colIndexArray[0], -1) },
            new Vector2[2] { new Vector2(colIndexArray[1], -2), new Vector2(colIndexArray[1], 7) },
            new Vector2[2] { new Vector2(colIndexArray[2], 8), new Vector2(colIndexArray[2], -20) },
        };

        List<RetangleAreaEffectExecution> tpAreaList = new List<RetangleAreaEffectExecution>(); // 传送区域表
        Action<BaseUnit> rmTpAreaAction = delegate {
            foreach (var r in tpAreaList)
                r.MDestory();
        };

        return CreateMovementFunc(
            list, // 起始点与终点
            25 * MapManager.gridHeight + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            wait, // 获取停靠时间
                  // 开始移动时
            delegate
            {
                // 设置横向缩放
                hscale = 1.11f;
                // 创建14节车厢
                CreateHeadAndBody(19);
                // 设置车头、车身、车尾的伤害倍率
                GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
                foreach (var item in GetBodyList())
                    item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
                GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
                // 设置速度
                NumericBox.MoveSpeed.SetBase(4.8f * MapManager.gridHeight / 60);
                SetAllComponentNoBeSelectedAsTargetAndHited();
                SetActionState(new MoveState(this));
                // 添加创人判定
                GetHead().NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                CreateHitArea(GetHead(), "BothCollide");
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                        CreateHitArea(u, "ItemCollideEnemy");
                    }
            },
            // 停靠时
            delegate
            {
                // 移除创人判定
                GetHead().NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                    }

                // 添加炮台车厢
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.y >= MapManager.GetRowY(7f) && body.transform.position.y <= MapManager.GetRowY(-1f))
                    {
                        if (body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateS1Attacker(body, false, false);
                        }
                        else if (body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateS1Attacker(body, true, false);
                        }
                        else
                        {
                            CreateS1Attacker(body, true, true);
                            CreateS1Attacker(body, false, true);
                        }
                    }
                }

                //添加传送区域
                // 1 3 5 7
                for (int i = 0; i <= 6; i += 2)
                {
                    tpAreaList.Add(CreateTpArea(MapManager.GetGridLocalPosition(colIndexArray[0], i), MapManager.GetGridLocalPosition(colIndexArray[2], i), Vector2.left, stun));

                }

                // 2 4 6
                for (int i = 1; i <= 5; i += 2)
                {
                    tpAreaList.Add(CreateTpArea(MapManager.GetGridLocalPosition(colIndexArray[1] - 0.5f, i), MapManager.GetGridLocalPosition(colIndexArray[1] + 0.75f, i), Vector2.left, stun));
                    tpAreaList.Add(CreateTpArea(MapManager.GetGridLocalPosition(colIndexArray[1] + 0.5f, i), MapManager.GetGridLocalPosition(colIndexArray[1] - 0.75f, i), Vector2.left, stun));
                }
                AddBeforeDeathEvent(rmTpAreaAction);
            },
            // 重新移动时
            delegate {
                // 所有车厢爆炸
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                        u.ExecuteDeath();
                // 移除传送区域
                rmTpAreaAction(this);
                RemoveBeforeDeathEvent(rmTpAreaAction);
                // 全场老鼠晕眩
                int stun_time = Mathf.FloorToInt(GetParamValue("des_stun1") * 60);
                foreach (var u in GameController.Instance.GetEachEnemy())
                {
                    if (!(u is MouseUnit))
                        continue;
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss() && MouseManager.IsGeneralMouse(m))
                    {
                        m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
                        BurnManager.BurnDamage(this, m);
                    }
                }
                // 强制下一p
                float[] arr = GetParamArray("hpRate");
                if (arr.Length > 0 && mHertIndex < 2)
                {
                    mHertIndex = 2;
                    OnHertStageChanged();
                }
            },
            1);
    }
    #endregion

    #region P3
    private void CreateS2FogCreator(RatTrainComponent master, bool isAttackLeft)
    {
        MouseModel m = CreateFogCreator(master, isAttackLeft);

        // 初始
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate
            {
                m.animatorController.Play("PreAttack");
            });
            t.AddTaskFunc(delegate
            {
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.animatorController.Play("Attack", true);
                    return true;
                }
                return false;
            });
            t.AddOnExitAction(delegate {
                m.animatorController.Play("PostAttack");
                Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                Vector2 pos = new Vector2(m.transform.position.x, m.transform.position.y);
                for (int i = 0; i <= 1; i++)
                {
                    RetangleAreaEffectExecution oil_r = CreateOilArea(pos + MapManager.gridHeight * rot + MapManager.gridWidth * Vector2.right * i);
                    FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + MapManager.gridHeight * rot + MapManager.gridWidth * Vector2.right * i);
                    e.SetOpen();
                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate
                    {
                        return !m.IsAlive();
                    });
                    t.AddOnExitAction(delegate
                    {
                        oil_r.MDestory();
                        e.SetDisappear();
                    });
                    e.AddTask(t);
                    GameController.Instance.AddAreaEffectExecution(e);
                }
            });
            m.AddTask(t);
        }

        // 结束时损毁事件
        {
            int des_type = Mathf.FloorToInt(GetParamValue("des_type2"));
            int des_shape = Mathf.FloorToInt(GetParamValue("des_shape2"));
            int des_stun = Mathf.FloorToInt(GetParamValue("des_stun2") * 60);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate
            {
                return !master.IsAlive();
            });
            task.AddOnExitAction(delegate
            {
                Vector2 rot = Vector2.left;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                SpawnEnemy(pos, pos, rot, des_type, des_shape, des_stun);
                m.ExecuteDeath();
            });
            m.taskController.AddTask(task);
        }
    }

    /// <summary>
    /// P3
    /// </summary>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2()
    {
        //float[] colIndexArray = new float[] { 6.5f, 4f, 1.5f };


        BoolModifier mod = new BoolModifier(true);
        int wait = Mathf.FloorToInt(GetParamValue("sus_wait2") * 60);

        List<Vector2[]> list = new List<Vector2[]>() {
            new Vector2[2] { new Vector2(11, 1), new Vector2(1, 1) },
            new Vector2[2] { new Vector2(1, 3), new Vector2(8.5f, 3) },
            new Vector2[2] { new Vector2(9, 5), new Vector2(-1, 5) },
        };

        List<RetangleAreaEffectExecution> tpAreaList = new List<RetangleAreaEffectExecution>(); // 传送区域表
        Action<BaseUnit> rmTpAreaAction = delegate
        {
            foreach (var r in tpAreaList)
                r.MDestory();
        };

        return CreateMovementFunc(
            list, // 起始点与终点
            24.5f * MapManager.gridWidth + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            wait, // 获取停靠时间
            // 开始移动时
            delegate
            {
                // 设置横向缩放
                hscale = 1f;
                // 创建14节车厢
                CreateHeadAndBody(18);
                // 设置车头、车身、车尾的伤害倍率
                GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
                foreach (var item in GetBodyList())
                    item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
                GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
                // 设置速度
                NumericBox.MoveSpeed.SetBase(4.8f * MapManager.gridHeight / 60);
                SetAllComponentNoBeSelectedAsTargetAndHited();
                SetActionState(new MoveState(this));
                // 添加创人判定
                GetHead().NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                CreateHitArea(GetHead(), "BothCollide");
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                        CreateHitArea(u, "ItemCollideEnemy");
                    }
            },
            // 停靠时
            delegate
            {
                // 移除创人判定
                GetHead().NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                    }

                // 添加喷雾车厢
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.y >= MapManager.GetRowY(7f) && body.transform.position.y <= MapManager.GetRowY(-1f))
                    {
                        if (body.transform.position.y > MapManager.GetRowY(2.5f))
                        {
                            CreateS2FogCreator(body, true);
                        }
                        else if (body.transform.position.y < MapManager.GetRowY(3.5f))
                        {
                            CreateS2FogCreator(body, false);
                        }
                        else
                        {
                            CreateS2FogCreator(body, true);
                            CreateS2FogCreator(body, false);
                        }
                    }
                }

                int stun_time = Mathf.FloorToInt(GetParamValue("sus_stun2") * 60);
                // 2 4 6
                for (int i = 1; i <= 5; i += 2)
                    tpAreaList.Add(CreateTpArea2(MapManager.GetGridLocalPosition(5f, i),  MapManager.GetGridLocalPosition(2, i), new Vector2(7 * MapManager.gridWidth, 0.5f * MapManager.gridHeight), stun_time));
                AddBeforeDeathEvent(rmTpAreaAction);
            },
            // 重新移动时
            delegate {
                // 所有车厢爆炸
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                        u.ExecuteDeath();
                // 移除传送区域
                rmTpAreaAction(this);
                RemoveBeforeDeathEvent(rmTpAreaAction);
                // 全场老鼠晕眩
                int stun_time = Mathf.FloorToInt(GetParamValue("des_stun2") * 60);
                foreach (var u in GameController.Instance.GetEachEnemy())
                {
                    if (!(u is MouseUnit))
                        continue;
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss() && MouseManager.IsGeneralMouse(m))
                    {
                        m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
                        BurnManager.BurnDamage(this, m);
                    }
                }
                // 强制下一p
                float[] arr = GetParamArray("hpRate");
                if (arr.Length > 0 && mHertIndex < 3)
                {
                    mHertIndex = 3;
                    OnHertStageChanged();
                }
            },
            2);
    }
    #endregion

    #region P4

    private void CreateS3LaserAttacker(RatTrainComponent master, bool isAttackLeft, bool isAttackOnce)
    {
        MouseModel m = CreateLaserAttacker(master, isAttackLeft);
        m.transform.localScale = new Vector2(0.75f*m.transform.localScale.x, 0.75f * m.transform.localScale.y);

        // 开始时
        {
            float sta_ice_trans = GetParamValue("sta_ice_trans3");
            float sta_ice_val = GetParamValue("sta_ice_val3");

            float sus_ice_trans = GetParamValue("sus_ice_trans3");
            float sus_ice_val = GetParamValue("sus_ice_val3");

            int stun = Mathf.FloorToInt(GetParamValue("sus_stun3") * 60);
            int interval = Mathf.FloorToInt(GetParamValue("interval3") * 60);

            int timeLeft = 0;

            // 一次激光攻击
            Action<float, float> attack = (ice_val, ice_trans)=>
            {
                Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                Vector2 pos = new Vector2(MapManager.GetColumnX(MapManager.GetXIndex(m.transform.position.x)), m.transform.position.y + rot.y * MapManager.gridHeight);
                // 产生一道激光
                {
                    BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PostAttack", null, false);
                    e.spriteRenderer.material = GameManager.Instance.GetMaterial("LinearDodge");
                    e.transform.right = rot;
                    e.transform.position = pos; // 对x进行补正
                    e.SetSpriteRendererSorting("Effect", 10);
                    GameController.Instance.AddEffect(e);
                }
                // 产生真正的激光判定
                {
                    Action<BaseUnit> action = (u) =>
                    {
                        ITask t = EnvironmentFacade.GetIceDebuff(u);
                        if (t != null)
                            new DamageAction(CombatAction.ActionType.BurnDamage, this, u, (t as IceTask).GetValue() * ice_trans).ApplyAction();

                        if (ice_val > 0)
                            EnvironmentFacade.AddIceDebuff(u, ice_val);
                    };

                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos + rot * 1.5f * MapManager.gridHeight, 0.5f, 3f, "BothCollide");
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    r.isAffectFood = true;
                    r.SetOnFoodEnterAction(action);
                    r.SetOnEnemyEnterAction(action);
                    foreach (var laserUnit in retinueList)
                    {
                        if (laserUnit is MouseUnit)
                        {
                            MouseUnit m = laserUnit as MouseUnit;
                            r.AddExcludeMouseUnit(m); // 不能毁掉自身的组件
                        }
                    }
                    GameController.Instance.AddAreaEffectExecution(r);
                }
            };

            // 任务本身
            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate
            {
                m.animatorController.Play("PreAttack");
            });
            task.AddTaskFunc(delegate
            {
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.animatorController.Play("Attack", true);
                    Vector2 rot = m.transform.position.y > MapManager.GetRowY(3f) ? Vector2.down : Vector2.up;
                    BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PreAttack", "Disappear", true);
                    e.SetSpriteRendererSorting("Effect", 9);
                    GameController.Instance.AddEffect(e);
                    m.mEffectController.AddEffectToDict("LaserEffect", e, 1f * Vector2.up * MapManager.gridHeight);
                    e.transform.right = rot;
                    timeLeft = 120;
                    return true;
                }
                return false;
            });
            task.AddTaskFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    // 移除蓄力特效
                    m.mEffectController.RemoveEffectFromDict("LaserEffect");
                    attack(sta_ice_val, sta_ice_trans);
                    return true;
                }
                return false;
            });
            if (isAttackOnce)
            {
                task.AddTaskFunc(delegate {
                    m.animatorController.Play("PostAttack");
                    return true;
                });
                task.AddTaskFunc(delegate
                {
                    return m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
                });
                task.AddOnExitAction(delegate {
                    m.MDestory();
                });
            }
            else
            {
                task.AddTaskFunc(delegate {
                    timeLeft = interval;
                    return true;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    if(timeLeft <= 0)
                    {
                        attack(sus_ice_val, sus_ice_trans);
                        timeLeft += interval;
                    }
                    return false;
                });
            }
            m.taskController.AddTask(task);
        }

        // 结束时损毁事件
        {
            int des_type = Mathf.FloorToInt(GetParamValue("des_type3"));
            int des_shape = Mathf.FloorToInt(GetParamValue("des_shape3"));
            int des_stun = Mathf.FloorToInt(GetParamValue("des_stun3") * 60);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate
            {
                return !master.IsAlive();
            });
            task.AddOnExitAction(delegate
            {
                Vector2 rot = Vector2.left;
                Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                SpawnEnemy(pos, pos, rot, des_type, des_shape, des_stun);
                m.ExecuteDeath();
            });
            m.taskController.AddTask(task);
        }
    }

    /// <summary>
    /// P4
    /// </summary>
    /// <returns></returns>
    private CustomizationSkillAbility Movement3()
    {
        //float[] colIndexArray = new float[] { 6.5f, 4f, 1.5f };
        float dx = -0.25f;

        BoolModifier mod = new BoolModifier(true);
        int wait = Mathf.FloorToInt(GetParamValue("sus_wait3") * 60);

        List<Vector2[]> list = new List<Vector2[]>() {
            new Vector2[2] { new Vector2(12f + dx, 0), new Vector2(2f + dx, 0) },
            new Vector2[2] { new Vector2(0.675f + dx, 2), new Vector2(8.675f + dx, 2) },
            new Vector2[2] { new Vector2(9.5f + dx, 4), new Vector2(1.5f + dx, 4) },
            new Vector2[2] { new Vector2(1 + dx, 6), new Vector2(12 + dx, 6) },
        };

        List<RetangleAreaEffectExecution> tpAreaList = new List<RetangleAreaEffectExecution>(); // 传送区域表
        Action<BaseUnit> rmTpAreaAction = delegate
        {
            foreach (var r in tpAreaList)
                r.MDestory();
        };

        return CreateMovementFunc(
            list, // 起始点与终点
            33f * MapManager.gridWidth + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            wait, // 获取停靠时间
                  // 开始移动时
            delegate
            {
                // 设置横向缩放
                hscale = 1f;
                // 创建14节车厢
                CreateHeadAndBody(16);
                // 设置车头、车身、车尾的伤害倍率
                GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
                foreach (var item in GetBodyList())
                    item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
                GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
                // 设置速度
                NumericBox.MoveSpeed.SetBase(4.8f * MapManager.gridHeight / 60);
                SetAllComponentNoBeSelectedAsTargetAndHited();
                SetActionState(new MoveState(this));
                // 添加创人判定
                GetHead().NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                CreateHitArea(GetHead(), "BothCollide");
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                        CreateHitArea(u, "ItemCollideEnemy");
                    }
            },
            // 停靠时
            delegate
            {
                // 移除创人判定
                GetHead().NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                    }

                // 添加炮台车厢
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.x >= MapManager.GetColumnX(0) && body.transform.position.x <= MapManager.GetColumnX(9))
                    {
                        if (body.transform.position.y > MapManager.GetRowY(0.5f) && body.transform.position.y < MapManager.GetRowY(-0.5f))
                        {
                            // 1
                            CreateS3LaserAttacker(body, false, false);
                            CreateS3LaserAttacker(body, true, true);
                        }
                        else if (body.transform.position.y > MapManager.GetRowY(2.5f) && body.transform.position.y < MapManager.GetRowY(1.5f))
                        {
                            // 3
                            CreateS3LaserAttacker(body, true, false);
                            CreateS3LaserAttacker(body, false, true);
                        }
                        else if (body.transform.position.y > MapManager.GetRowY(4.5f) && body.transform.position.y < MapManager.GetRowY(3.5f))
                        {
                            // 5
                            CreateS3LaserAttacker(body, false, false);
                            CreateS3LaserAttacker(body, true, true);
                        }
                        else if (body.transform.position.y > MapManager.GetRowY(6.5f) && body.transform.position.y < MapManager.GetRowY(5.5f))
                        {
                            // 7
                            CreateS3LaserAttacker(body, true, false);
                            CreateS3LaserAttacker(body, false, true);
                        }
                    }
                }

                int stun_time = Mathf.FloorToInt(GetParamValue("sus_stun3") * 60);
                // 1 3 5 7
                for (int i = 0; i <= 6; i += 2)
                    tpAreaList.Add(CreateTpArea2(MapManager.GetGridLocalPosition(5f, i), MapManager.GetGridLocalPosition(2, i), new Vector2(7 * MapManager.gridWidth, 0.5f * MapManager.gridHeight), stun_time));
                AddBeforeDeathEvent(rmTpAreaAction);
            },
            // 重新移动时
            delegate {
                // 所有车厢爆炸
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                        u.ExecuteDeath();
                // 移除传送区域
                rmTpAreaAction(this);
                RemoveBeforeDeathEvent(rmTpAreaAction);
                // 全场老鼠晕眩
                int stun_time = Mathf.FloorToInt(GetParamValue("des_stun3") * 60);
                foreach (var u in GameController.Instance.GetEachEnemy())
                {
                    if (!(u is MouseUnit))
                        continue;
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss() && MouseManager.IsGeneralMouse(m))
                    {
                        m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
                        BurnManager.BurnDamage(this, m);
                    }
                }
                // 强制下一p
                float[] arr = GetParamArray("hpRate");
                if (arr.Length > 0 && mHertIndex < 4)
                {
                    mHertIndex = 4;
                    OnHertStageChanged();
                }
            },
            3);
    }
    #endregion

    #region P5
    private void CreateS4Bomb(RatTrainComponent master)
    {
        MouseModel m = CreateBomb(master);

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate 
        {
            m.animatorController.Play("Cast");
        });
        task.AddTaskFunc(delegate {
            return m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        task.AddOnExitAction(delegate {
            // 产生3*3爆破效果
            {
                GameManager.Instance.audioSourceController.PlayEffectMusic("Boom");
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 3, 3, "BothCollide");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.isAffectFood = true;
                r.SetOnFoodEnterAction((u) => {
                    BurnManager.BurnInstanceKill(this, u);
                });
                r.AddEnemyEnterConditionFunc((u) => {
                    return !u.IsBoss() && MouseManager.IsGeneralMouse(u);
                });
                r.SetOnEnemyEnterAction((u) => {
                    BurnManager.BurnInstanceKill(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            m.ExecuteDeath();
        });
        m.taskController.AddTask(task);
    }

    private void CreateS4HitArea(Vector2 pos)
    {
        float lost_hp_percent = GetParamValue("hit_percent4") / 100;
        int stun_time = Mathf.FloorToInt(GetParamValue("stun4") * 60);
        float min_x = MapManager.GetColumnX(GetParamValue("hit_limit_left_col4") - 1);
        float max_x = MapManager.GetColumnX(8.5f);

        // 判定
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "BothCollide");
            r.name = "CrackArea";
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.AddFoodEnterConditionFunc((u) => {
                return u.IsAlive() && FoodManager.IsAttackableFoodType(u) && !NumericBox.GetBoolNumericValue("isStop");
            });
            r.SetOnFoodEnterAction((u)=> {
                KnockFlyingFood(u, new Vector2(-transform.localScale.x * UnityEngine.Random.Range(15, 20) * MapManager.gridWidth, UnityEngine.Random.Range(0, 2) * MapManager.gridHeight));
            });
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && !UnitManager.IsFlying(u) && MouseManager.IsGeneralMouse(u) && 
                ((moveRotate.x < 0 && u.transform.position.x > min_x)|| (moveRotate.x > 0 && u.transform.position.x < max_x)) &&
                u.GetHeight() == 0 && !NumericBox.GetBoolNumericValue("isStop");
            });
            r.SetOnEnemyEnterAction((u) => {
                // 添加一个弹起的任务 
                float dist;
                if (moveRotate.x < 0)
                    dist = Mathf.Min(mCurrentMoveSpeed * 30, u.transform.position.x - min_x);
                else
                    dist = Mathf.Min(mCurrentMoveSpeed * 30, max_x - u.transform.position.x);
                int time = Mathf.Max(40, Mathf.FloorToInt(dist/ mCurrentMoveSpeed) + 5);
                CustomizationTask t = TaskManager.GetParabolaTask(u, dist / time, dist / 2, u.transform.position, u.transform.position + dist * Vector3.right * moveRotate.x, false, true);
                // 且禁止移动
                u.DisableMove(true);
                t.AddOnExitAction(delegate {
                    u.DisableMove(false);
                    new DamageAction(CombatAction.ActionType.CauseDamage, null, u, lost_hp_percent * u.mCurrentHp).ApplyAction(); // 落地伤害
                    u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false)); // 晕眩
                });
                u.AddTask(t);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = transform.position;
                return !IsAlive();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
    }

    private CustomizationSkillAbility Movement4()
    {

        BoolModifier mod = new BoolModifier(true);

        List<Vector2[]> list = new List<Vector2[]>() {
            new Vector2[2] { new Vector2(10, 1), new Vector2(-1, 1) },
            new Vector2[2] { new Vector2(2, -2), new Vector2(2, 8) },
            new Vector2[2] { new Vector2(-1, 5), new Vector2(10, 5) },
            new Vector2[2] { new Vector2(6, 8), new Vector2(6, -2-10) },
        };

        CompoundSkillAbility s = CreateMovementFunc(
            list, // 起始点与终点
            22f * MapManager.gridWidth + 20 * MapManager.gridHeight + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            600, // 获取停靠时间
            // 开始移动时
            delegate
            {
                // 设置横向缩放
                hscale = 1.03f;
                // 创建14节车厢
                CreateHeadAndBody(23);
                // 设置车头、车身、车尾的伤害倍率
                GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
                foreach (var item in GetBodyList())
                    item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
                GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
                // 设置速度
                NumericBox.MoveSpeed.SetBase(4.8f * MapManager.gridHeight / 60);
                SetAllComponentNoBeSelectedAsTargetAndHited();
                SetActionState(new MoveState(this));
                // 添加创人判定
                GetHead().NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                CreateHitArea(GetHead(), "BothCollide");
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.AddDecideModifierToBoolDict("CanHit", mod);
                        CreateHitArea(u, "ItemCollideEnemy");
                    }
            },
            // 停靠时
            delegate
            {
                // 移除创人判定
                GetHead().NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                foreach (var u in GetBodyList())
                    if (u.IsAlive())
                    {
                        u.NumericBox.RemoveDecideModifierToBoolDict("CanHit", mod);
                    }

                // 添加炮台车厢
                foreach (var body in GetBodyList())
                {
                    CreateS4Bomb(body);
                    body.MDestory();
                }
            },
            // 重新移动时
            delegate {

            },
            -1);

        // 再等等，车头不能这么急
        {
            s.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTimeTaskFunc(360);
                return task;
            });
        }


        // 开始乱创人！
        {
            float colIndex_left = GetParamValue("left_col4") - 1;
            float colIndex_right = 9 - GetParamValue("right_col4");
            Queue<int> rowQueue = new Queue<int>();
            {
                float[] arr = GetParamArray("row_set4");
                for (int i = 0; i < arr.Length; i++)
                {
                    rowQueue.Enqueue(Mathf.FloorToInt(arr[i] - 1));
                }
            }

            s.AddCreateTaskFunc(delegate {
                // 常量
                float start_pos_x = MapManager.GetColumnX(colIndex_right);
                float end_pos_x = MapManager.GetColumnX(colIndex_left);
                float dist = Mathf.Abs(start_pos_x - end_pos_x);
                CustomizationState move_state = new CustomizationState();
                CustomizationState disappear_state = new CustomizationState();


                // 变量
                int move_sign = (end_pos_x - start_pos_x < 0 ? -1 : 1);
                float move_dist = 0; // 当前移动距离
                float current_y = 0; // 当前纵坐标
                // 和倒油有关的变量
                List<int> rowLeftList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
                int currentRow = -1;
                Queue<float> x_queue = new Queue<float>(); // 需要途径的横坐标
                List<RetangleAreaEffectExecution> oilList = new List<RetangleAreaEffectExecution>();
                Action<BaseUnit> ClearAllOilWhenDeath = delegate {
                    foreach (var r in oilList)
                    {
                        r.MDestory();
                    }
                    oilList.Clear();
                };

                // 当前状态（有移动态和消失态）
                CustomizationState current_state = new CustomizationState();
                {
                    CustomizationState s = move_state;
                    s.AddOnEnterAction(delegate {
                        animatorController.Play("Appear");
                        if (rowLeftList.Contains(currentRow))
                        {
                            rowLeftList.Remove(currentRow);
                            x_queue.Clear();
                            if(move_sign < 0)
                                for (int i = Mathf.FloorToInt(colIndex_right); i > Mathf.FloorToInt(colIndex_left); i--)
                                {
                                    x_queue.Enqueue(MapManager.GetColumnX(i));
                                }
                            else
                                for (int i = Mathf.CeilToInt(colIndex_left); i < Mathf.CeilToInt(colIndex_right); i++)
                                {
                                    x_queue.Enqueue(MapManager.GetColumnX(i));
                                }
                        }
                            
                    });
                    s.AddOnUpdateAction(delegate {
                        if(x_queue.Count > 0)
                        {
                            if((move_sign < 0 && transform.position.x < x_queue.Peek()) || (move_sign > 0 && transform.position.x > x_queue.Peek()))
                                oilList.Add(CreateOilArea(new Vector2(x_queue.Dequeue(), current_y)));
                        }
                        if (move_dist >= dist)
                        {
                            current_state = disappear_state;
                            current_state.OnEnter();
                        }
                    });
                }
                {
                    CustomizationState s = disappear_state;
                    s.AddOnEnterAction(delegate {
                        animatorController.Play("Disappear");
                    });
                    s.AddOnUpdateAction(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            move_dist = 0;
                            // 换行
                            currentRow = rowQueue.Dequeue();
                            current_y = MapManager.GetRowY(currentRow);
                            rowQueue.Enqueue(currentRow);
                            // 对调
                            float temp = start_pos_x;
                            start_pos_x = end_pos_x;
                            end_pos_x = temp;
                            move_sign = (end_pos_x - start_pos_x < 0 ? -1 : 1);
                            // 更新车头朝向
                            SetMoveRoate(new Vector2(move_sign, 0));
                            transform.localScale = new Vector2(-move_sign, 1);
                            current_state = move_state;
                            current_state.OnEnter();
                        }
                    });
                }

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    // 如果本体就是头呢
                    OpenCollision();
                    GetSpriteRenderer().enabled = true;
                    // 添加撞击判定
                    CreateS4HitArea(transform.position);
                    // 添加加速、减防任务
                    {
                        RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
                        GameNormalPanel.Instance.AddUI(rUI);
                        AddOnDestoryAction(delegate { if (rUI.IsValid()) rUI.MDestory(); });
                        rUI.Show();
                        rUI.SetPercent(0);

                        int totalTime = Mathf.FloorToInt(GetParamValue("t4") * 60);
                        int max_stun_time = Mathf.FloorToInt(GetParamValue("max_burn_stun4") *60);
                        int add_stun_time = Mathf.FloorToInt(GetParamValue("burn_stun4") * 60); // 被炸一次增加的时间
                        float v0 = GetParamValue("v4_0") * MapManager.gridWidth/60;
                        float v1 = GetParamValue("v4_1") * MapManager.gridWidth / 60;
                        float burn_rate0 = GetParamValue("head_burn");
                        float burn_rate1 = GetParamValue("head_burn4");
                        float dmg_rate0 = GetParamValue("head_normal");
                        float dmg_rate1 = GetParamValue("head_normal4");
                        float lost_hp_percent = GetParamValue("lost_hp_percent4")/100;
                        int add_time = Mathf.FloorToInt(totalTime * GetParamValue("lost_speed_percent4") / 100);
                        int interval = 60;

                        int timeLeft = 0; // 加速剩余时间
                        int dmgTimeLeft = 0; // 受伤剩余时间
                        int stun_timeLeft = 0; // 晕眩剩余时间

                        FloatModifier burn_rate_mod = new FloatModifier(burn_rate0);
                        FloatModifier dmg_rate_mod = new FloatModifier(dmg_rate0);
                        BoolModifier isStop_mod = new BoolModifier(true);

                        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
                            if (!(combat is DamageAction))
                                return;
                            DamageAction d = combat as DamageAction;
                            if (d.IsDamageType(DamageAction.DamageType.BombBurn))
                                stun_timeLeft = Mathf.Min(max_stun_time, stun_timeLeft + add_stun_time);
                        });
                        CustomizationTask task = new CustomizationTask();
                        task.AddOnEnterAction(delegate {
                            NumericBox.BurnRate.AddModifier(burn_rate_mod);
                            NumericBox.DamageRate.AddModifier(dmg_rate_mod);
                            timeLeft = totalTime;
                            dmgTimeLeft = interval;
                            stun_timeLeft = 0;
                        });
                        task.AddTaskFunc(delegate {
                            rUI.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.up;
                            if (stun_timeLeft > 0)
                            {
                                float r = 238f / 255;
                                float g = 255f / 255;
                                float b = 197f / 255;
                                rUI.SetIcon(Wait_Icon_Sprite);
                                rUI.SetColor(new Color(r, g, b, 0.5f));
                                stun_timeLeft--;
                                rUI.SetPercent((float)stun_timeLeft/max_stun_time);
                                NumericBox.MoveSpeed.SetBase(0);
                                if(stun_timeLeft == 0)
                                {
                                    timeLeft = Mathf.Min(timeLeft + add_time, totalTime);
                                }
                                if (!NumericBox.GetBoolNumericValue("isStop"))
                                    NumericBox.AddDecideModifierToBoolDict("isStop", isStop_mod);
                                return false;
                            }
                            if (timeLeft > 0)
                            {
                                float r = 199f / 255;
                                float g = 233f / 255;
                                float b = 255f / 255;
                                rUI.SetIcon(Speed_Icon_Sprite);
                                rUI.SetColor(new Color(r, g, b, 0.5f));
                                timeLeft--;
                                dmgTimeLeft--;
                                if(dmgTimeLeft <= 0)
                                {
                                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, lost_hp_percent * mCurrentHp).ApplyAction();
                                    dmgTimeLeft += interval;
                                }
                                float rate = 1 - (float)timeLeft / totalTime;
                                rUI.SetPercent(rate);
                                NumericBox.MoveSpeed.SetBase(Mathf.Lerp(v0, v1, rate));
                                NumericBox.BurnRate.RemoveModifier(burn_rate_mod);
                                NumericBox.DamageRate.RemoveModifier(dmg_rate_mod);
                                burn_rate_mod.Value = Mathf.Lerp(burn_rate0, burn_rate1, rate);
                                dmg_rate_mod.Value = Mathf.Lerp(dmg_rate0, dmg_rate1, rate);
                                NumericBox.BurnRate.AddModifier(burn_rate_mod);
                                NumericBox.DamageRate.AddModifier(dmg_rate_mod);
                                if (NumericBox.GetBoolNumericValue("isStop"))
                                    NumericBox.RemoveDecideModifierToBoolDict("isStop", isStop_mod);
                            }
                            return false;
                        });
                        taskController.AddTask(task);
                    }
                    // 添加在死亡时移除所有油
                    AddBeforeDeathEvent(ClearAllOilWhenDeath);

                    currentRow = rowQueue.Dequeue();
                    current_y = MapManager.GetRowY(currentRow);
                    rowQueue.Enqueue(currentRow);
                    current_state = move_state;
                    current_state.OnEnter();
                });
                task.AddTaskFunc(delegate {
                    move_dist += mCurrentMoveSpeed;
                    current_state.OnUpdate();
                    transform.position = new Vector2(Mathf.Lerp(start_pos_x, end_pos_x, move_dist / dist), current_y);
                    return false;
                });
                return task;
            });
        }


        return s;
    }

    #endregion

    #region 其他
    /// <summary>
    /// 击飞美食单位
    /// </summary>
    /// <param name="u"></param>
    private void KnockFlyingFood(FoodUnit u, Vector2 offset)
    {
        int totalTime = 120;
        int timeLeft = totalTime;
        Sprite s = u.GetSpirte();
        if (s == null)
        {
            List<Sprite> list = u.GetSpriteList();
            if (list.Count > 0)
                s = list[0];
        }
        u.DeathEvent();

        if (s == null)
            return;

        Vector2 startPos = u.transform.position;
        Vector2 endPos = startPos + offset;

        BaseEffect e = BaseEffect.CreateInstance(s);
        e.SetSpriteRendererSorting("Effect", 2);
        e.transform.position = u.transform.position;
        GameController.Instance.AddEffect(e);

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            e.transform.position = startPos;
        });
        task.AddTaskFunc(delegate {
            timeLeft--;
            float rate = 1 - ((float)timeLeft / totalTime);
            e.transform.position = Vector2.Lerp(startPos, endPos, Mathf.Sin(rate * Mathf.PI / 2));
            if (timeLeft <= 0)
                return true;
            else
                return false;
        });
        task.AddOnExitAction(delegate {
            e.ExecuteDeath();
        });
        e.AddTask(task);
    }

    private void KnockFlyingMouse(MouseUnit u, Vector2 offset)
    {
        int totalTime = 120;
        int timeLeft = totalTime;
        Sprite s = u.GetSpirte();
        if (s == null)
        {
            List<Sprite> list = u.GetSpriteList();
            if (list != null && list.Count > 0)
                s = list[0];
        }
        u.MDestory();

        if (s == null)
            return;

        Vector2 startPos = u.transform.position;
        Vector2 endPos = startPos + offset;

        BaseEffect e = BaseEffect.CreateInstance(s);
        e.SetSpriteRendererSorting("Effect", 2);
        e.transform.position = u.transform.position;
        GameController.Instance.AddEffect(e);

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            e.transform.position = startPos;
        });
        task.AddTaskFunc(delegate {
            timeLeft--;
            float rate = 1 - ((float)timeLeft / totalTime);
            e.transform.position = Vector2.Lerp(startPos, endPos, Mathf.Sin(rate * Mathf.PI / 2));
            if (timeLeft <= 0)
                return true;
            else
                return false;
        });
        task.AddOnExitAction(delegate {
            e.ExecuteDeath();
        });
        e.AddTask(task);
    }
    #endregion

    #region 油滴加速任务
    private class OilTask : ITask
    {
        private BaseUnit master;
        private FloatModifier mod = new FloatModifier(100);
        private BoolModifier Ignore_mod = new BoolModifier(true);
        private int count = 0; // 处于的油滴数

        public OilTask(BaseUnit master, float add_move_speed_rate)
        {
            // 0.1->-90
            // 1->0
            // 2->100
            this.master = master;
            count = 1;
            mod.Value = (add_move_speed_rate - 1)*100;
        }

        public void OnEnter()
        {
            master.NumericBox.MoveSpeed.AddPctAddModifier(mod);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, Ignore_mod);
        }

        public void OnUpdate()
        {
            
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {
            master.NumericBox.MoveSpeed.RemovePctAddModifier(mod);
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, Ignore_mod);
        }

        public bool IsClearWhenDie()
        {
            return true;
        }

        public void ShutDown()
        {
            
        }

        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }
    }
    #endregion
}
