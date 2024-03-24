using System;
using System.Collections.Generic;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 列车一阶
/// </summary>
public class RatTrain : BaseRatTrain
{
    private static RuntimeAnimatorController SummonSoldier_RuntimeAnimatorController;
    private static RuntimeAnimatorController MissileAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController Missile_RuntimeAnimatorController;
    private FloatModifier moveSpeedModifier = new FloatModifier(0); // 移速提升标签
    private List<BaseUnit> retinueList = new List<BaseUnit>(); // 自身随从表（在死亡时同时销毁所有随从）

    private List<int> attack_orderList = new List<int>(); // 攻击顺序表

    private Queue<float> lost_hp_percent_queue = new Queue<float>();
    private float current_lost_hp_percent;
    private float dmgRecord; // 受到的伤害总和
    private int max_stun_time; // 最大晕眩时间
    private int stun_timeLeft; // 晕眩时间
    private Action<CombatAction> burnAction; // 被炸事件
    private FloatModifier extra_burn_rate = new FloatModifier(0); // 额外灰烬抗性

    private RingUI FinalSkillRingUI;
    private RingUI WaitRingUI;

    public override void Awake()
    {
        if(SummonSoldier_RuntimeAnimatorController == null)
        {
            SummonSoldier_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/SummonSoldier");
            MissileAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/MissileAttacker");
            Missile_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Missile");
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

    public override void MInit()
    {
        extra_burn_rate.Value = 0;
        attack_orderList.Clear();
        dmgRecord = 0;
        stun_timeLeft = 0;
        max_stun_time = 0;
        retinueList.Clear();
        lost_hp_percent_queue.Clear();
        current_lost_hp_percent = 9999;
        base.MInit();
        // 设置横向缩放
        hscale = 1.11f;
        // 创建10节车厢
        CreateHeadAndBody(10);
        // 设置车头、车身、车尾的伤害倍率
        GetHead().SetDmgRate(GetParamValue("head_normal"), GetParamValue("head_burn"));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal"), GetParamValue("body_burn"));
        }
        // 车尾处理
        {
            GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal"), GetParamValue("tail_burn"));
        }
        
        // 默认速度为2.4纵格/每秒
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridHeight/60);
        SetAllComponentNoBeSelectedAsTargetAndHited();
        SetActionState(new MoveState(this));

        // 大招UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, GetHead(), 0.25f * MapManager.gridHeight * Vector3.down));
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(0);

            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(1);
            task.AddOnExitAction(delegate {
                if (current_lost_hp_percent != 9999)
                    FinalSkillRingUI.Show();
            });
            taskController.AddTask(task);
        }
        // 停滞UI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            taskController.AddTask(TaskManager.GetWaitRingUITask(rUI, GetHead(), 0.25f * MapManager.gridHeight * Vector3.up));
            AddOnDestoryAction(delegate { if (rUI.IsValid()) rUI.MDestory(); });
            rUI.Show();
            rUI.SetPercent(0);
            WaitRingUI = rUI;
        }
        // 受击事件
        {
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) =>
            {
                if (combatAction is DamageAction)
                {
                    float triggerFinalSkillDamage = mMaxHp * GetParamValue("p_lost_hp_percent") / 100;
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
                        dmgRecord -= triggerFinalSkillDamage;
                        CustomizationSkillAbility s = GetUltimateSkill();
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
        }
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in retinueList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            retinueList.Remove(u);
        }
        base.MUpdate();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.RatTrain, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // 特殊参数处理
        // 获取大招损失生命值条件
        {
            Action<float[]> action = delegate
            {
                float[] arr = GetParamArray("p_lost_hp_percent");
                lost_hp_percent_queue.Clear();
                foreach (var val in arr)
                    lost_hp_percent_queue.Enqueue(val / 100);
                if (lost_hp_percent_queue.Count > 0)
                    current_lost_hp_percent = lost_hp_percent_queue.Dequeue();
                else
                {
                    current_lost_hp_percent = 9999;
                    FinalSkillRingUI.Hide();
                }
            };
            AddParamChangeAction("p_lost_hp_percent", action);
            action(null);
        }
        {
            Action<float[]> action = delegate
            {
                max_stun_time = Mathf.FloorToInt(GetParamValue("p_max_stun_time") * 60);
            };
            AddParamChangeAction("p_max_stun_time", action);
            action(null);
        }
        {
            Action<float[]> action = delegate {
                if (NumericBox.BurnRate.Contains(extra_burn_rate))
                {
                    NumericBox.BurnRate.RemoveModifier(extra_burn_rate);
                    extra_burn_rate.Value = 1 - GetParamValue("p_extra_burn_defence")/100;
                    NumericBox.BurnRate.AddModifier(extra_burn_rate);
                }
                else
                {
                    extra_burn_rate.Value = 1 - GetParamValue("p_extra_burn_defence") / 100;
                }
            };
            AddParamChangeAction("p_extra_burn_defence", action);
            action(null);
        }
        {
            // 攻击顺序
            attack_orderList.Clear();
            float[] orderArray = GetParamArray("attack_order");
            // 如果没有自定义攻击顺序，则按默认的攻击顺序来
            foreach (var item in orderArray)
            {
                int i = Mathf.Max(0, (int)item - 1);
                attack_orderList.Add(i);
            }
        }
    }

    /// <summary>
    /// 切换阶段、加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        // 设置横向缩放
        hscale = 1.087f;

        // 设置获取技能组的方法
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(delegate { return attack_orderList; });

        LoadP3SkillAbility();

        // 移速变化
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
    }

    /// <summary>
    /// 获取终极技能
    /// </summary>
    /// <returns></returns>
    private CustomizationSkillAbility GetUltimateSkill()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("extra_t0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("extra_t1", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type1"));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape1"));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // 炮台数据
        float hp0 = GetParamValue("hp0");
        float extra_burn_defence0 = GetParamValue("extra_burn_defence0")/100;
        // 传送带数据
        float hp1 = GetParamValue("hp1");
        float extra_burn_defence1 = GetParamValue("extra_burn_defence1")/100;

        return Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, extra_burn_defence0, t0_0)); // 指令-炮击
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, extra_burn_defence1)); // 指令-奇袭
                return actionList;
            }
            );
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // 炮台数据
        float hp0 = GetParamValue("hp0");
        float burn_defence0 = GetParamValue("burn_defence0")/100;
        float extra_burn_defence0 = GetParamValue("extra_burn_defence0")/100;
        // 传送带数据
        float hp1 = GetParamValue("hp1");
        float burn_defence1 = GetParamValue("burn_defence1")/100;
        float extra_burn_defence1 = GetParamValue("extra_burn_defence1")/100;
        // 是否反向炮台
        int isReverse = Mathf.FloorToInt(GetParamValue("isReverse"));

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
                return actionList;
            }
            ));
        list.Add(GetUltimateSkill());
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               if(isReverse==0)
                   actionList.Add(CreateMissileAttackAction2(hp0, burn_defence0, t0_0)); // 指令-炮击
               else
                   actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
               return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
                return actionList;
            }
            ));
        list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               if (isReverse == 0)
                   actionList.Add(CreateMissileAttackAction2(hp0, burn_defence0, t0_0)); // 指令-炮击
               else
                   actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // 指令-炮击
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // 指令-奇袭
               return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }


    public override void BeforeDeath()
    {
        foreach (var u in retinueList)
        {
            u.taskController.Initial();
            u.ExecuteDeath();
        }
        retinueList.Clear();
        base.BeforeDeath();
    }

    /// <summary>
    /// 在进入移动状态时，附加一个效果，如果被炸则会停止一段时间
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if(GetHead()!=null)
            GetHead().AddActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        NumericBox.BurnRate.AddModifier(extra_burn_rate);
        base.OnMoveStateEnter();
    }

    public override void OnMoveState()
    {
        if(stun_timeLeft > 0)
        {
            WaitRingUI.Show();
            WaitRingUI.SetPercent((float)stun_timeLeft/max_stun_time);
            DisableMove(true);
            stun_timeLeft--;
        }
        else
        {
            WaitRingUI.Hide();
            DisableMove(false);
        }
        base.OnMoveState();
    }

    public override void OnMoveStateExit()
    {
        if (GetHead() != null)
            GetHead().RemoveActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        NumericBox.BurnRate.RemoveModifier(extra_burn_rate);
        base.OnMoveStateExit();
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    private void SpawnEnemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape, int stun_time)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.moveRotate = rot;
        if (m.moveRotate.x > 0)
            m.transform.localScale = new Vector2(-1, 1);
        // 一些初始出现动画不能被击中的效果
        Func<BaseUnit, BaseUnit, bool> noSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        BoolModifier noUseBearMod = new BoolModifier(true);

        CustomizationTask task = new CustomizationTask();
        int totalTime = 90;
        int currentTime = 0;
        task.AddOnEnterAction(delegate
        {
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
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
            
            Environment.SkyManager.RemoveNoAffectBySky(m, noUseBearMod);
            // 自我晕眩
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        });
        m.AddTask(task);
    }

    /// <summary>
    /// 创建一个敌人生成器
    /// </summary>
    private void CreateEnemySwpaner(RatTrainComponent master, bool isLeft, int waitTime, int type, int shape, int stun_time, float hp, float burn_defence)
    {
        MouseModel m = MouseModel.GetInstance(SummonSoldier_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(1 - burn_defence));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate * master.moveRotate.y;
            m.transform.localScale = new Vector2(hscale, 1);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, (isLeft?-1:1)*0.98f * MapManager.gridHeight);
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
                    if(isLeft)
                        m.animatorController.Play("PreSpawn1");
                    else
                        m.animatorController.Play("PreSpawn0");
                });
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        if (isLeft)
                            m.animatorController.Play("Spawn1", true);
                        else
                            m.animatorController.Play("Spawn0", true);
                        return true;
                    }
                    return false;
                });
                t.AddTaskFunc(delegate
                {
                    if (waitTime > 0)
                    {
                        waitTime--;
                        return false;
                    }
                    {
                        if (isLeft)
                            m.animatorController.Play("PostSpawn1");
                        else
                            m.animatorController.Play("PostSpawn0");
                        // 召唤一只小怪
                        if (m.IsAlive())
                        {
                            Vector2 rot = isLeft ? Vector2.left : Vector2.right;
                            Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                            SpawnEnemy(pos + 0.5f * rot * MapManager.gridWidth, pos + 1.0f * rot * MapManager.gridWidth, rot, type, shape, stun_time);
                        }
                        m.CloseCollision();
                        return true;
                    }
                });
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                t.AddOnExitAction(delegate {
                    m.MDestory(); // 直接回收，不附带死亡动画也不触发亡语（如果有）
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 2; // 图层-2
            // m.UpdateRenderLayer(master.spriteRenderer.sortingOrder - 2); // 图层-2
        }
    }


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
    private void CreateMissileAttacker(RatTrainComponent master, bool isAttackLeft, int waitTime, float hp, float burn_defence)
    {
        MouseModel m = MouseModel.GetInstance(MissileAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(1-burn_defence));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            if(isAttackLeft)
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

            // 动作
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
                t.AddTaskFunc(delegate
                {
                    if (waitTime > 0)
                    {
                        waitTime--;
                        return false;
                    }
                    {
                        m.animatorController.Play("PostAttack");
                        // 发射一枚炮弹
                        if (m.IsAlive())
                        {
                            Vector2 rot = isAttackLeft ? Vector2.left : Vector2.right;
                            Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                            CreateMissile(master, pos + rot * MapManager.gridWidth, rot);
                        }
                        m.CloseCollision();
                        return true;
                    }
                });
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                t.AddOnExitAction(delegate {
                    m.MDestory(); // 直接回收，不附带死亡动画也不触发亡语（如果有）
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 1; // 图层-1
        }
    }

    /// <summary>
    /// 指令-炮击
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateMissileAttackAction(float hp, float burn_defence, int wait_time)
    {
        int index = 0;
        int j = 0;
        int interval = 1;
        Action<int, int> action = (timeLeft, totalTime) => {
            if ((totalTime - timeLeft) == interval * (j + 1))
            {
                bool flag = false;
                RatTrainBody body = GetBody(index);
                while (!flag)
                {
                    flag = (body!=null && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f));
                    if (flag)
                    {
                        if (body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateMissileAttacker(body, true, wait_time, hp, burn_defence);
                        }
                        else if (body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateMissileAttacker(body, false, wait_time, hp, burn_defence);
                        }
                        else
                        {
                            CreateMissileAttacker(body, true, wait_time, hp, burn_defence);
                            CreateMissileAttacker(body, false, wait_time, hp, burn_defence);
                        }
                    }
                    index++;
                    body = GetBody(index);
                    if (flag || body == null)
                        break;
                }
                j++;
            }
        };
        return action;
    }

    /// <summary>
    /// 指令-炮击2（给P3两侧停靠用的）
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateMissileAttackAction2(float hp, float burn_defence, int wait_time)
    {
        int index = 0;
        int j = 0;
        int interval = 1;
        Action<int, int> action = (timeLeft, totalTime) => {
            if ((totalTime - timeLeft) == interval * (j + 1))
            {
                bool flag = false;
                RatTrainBody body = GetBody(index);
                while (!flag)
                {
                    flag = (body != null && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f));
                    if (flag)
                    {
                        if (body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateMissileAttacker(body, false, wait_time, hp, burn_defence);
                        }
                        else if (body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateMissileAttacker(body, true, wait_time, hp, burn_defence);
                        }
                        else
                        {
                            CreateMissileAttacker(body, true, wait_time, hp, burn_defence);
                            CreateMissileAttacker(body, false, wait_time, hp, burn_defence);
                        }
                    }
                    index++;
                    body = GetBody(index);
                    if (flag || body == null)
                        break;
                }
                j++;
            }
        };
        return action;
    }

    /// <summary>
    /// 指令-奇袭
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateEnemySpawnerAction(int waitTime, int type, int shape, int stun_time, float hp, float burn_defence)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 1)
            {
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f))
                    {
                        if(body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateEnemySwpaner(body, true, waitTime, type, shape, stun_time, hp, burn_defence);
                        }else if(body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateEnemySwpaner(body, false, waitTime, type, shape, stun_time, hp, burn_defence);
                        }
                        else
                        {
                            CreateEnemySwpaner(body, true, waitTime, type, shape, stun_time, hp, burn_defence);
                            CreateEnemySwpaner(body, false, waitTime, type, shape, stun_time, hp, burn_defence);
                        }
                    }
                }
            }
        };
        return action;
    }

    /// <summary>
    /// 产生一个移动相关的技能（给下面的方法用）
    /// </summary>
    /// <param name="pointList">起始点,结束点 对</param>
    /// <param name="waitDist">停靠前需要移动的距离</param>
    /// <param name="wait">停靠时间</param>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility CreateMovementFunc(List<Vector2[]> pointList, float waitDist, int wait, Func<List<Action<int, int>>> createActionListFunc)
    {
        // 变量
        int timeLeft = 0;
        float distLeft = 0;
        List<Action<int, int>> WaitTaskActionList = null;
        
        
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            distLeft = waitDist;
            SetHeadMoveDestination();
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
                list.Add(new Vector2[] { p[0], p[1] });
            WaitTaskActionList = createActionListFunc();
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
            // 等待
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    foreach (var action in WaitTaskActionList)
                        action(timeLeft, wait); // 执行指令
                    timeLeft--;
                    return false;
                }
                else
                {
                    SetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new MoveState(this));
                    return true;
                }
            });
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (IsHeadMoveToDestination())
                {
                    SetActionState(new IdleState(this));
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
    /// 右侧停靠·奇数行
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2]{ new Vector2(8f, 8f), new Vector2(8f, -22f) } }, // 起始点与终点
            MapManager.GetRowY(0) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            ); 
    }

    /// <summary>
    /// 右侧停靠·偶数行
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2]{ new Vector2(8f, 8f), new Vector2(8f, -22f) } }, // 起始点与终点
            MapManager.GetRowY(1) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 中场停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(Func<List<Action<int, int>>> createActionListFunc)
    {
        int wait = Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60);
        List<Vector2[]> pointList = new List<Vector2[]>() { new Vector2[2] { new Vector2(4f, -3f), new Vector2(4f, 27f) } };

        // 变量初始化
        float dist = 0;
        int timeLeft = 0;
        List<Action<int, int>> WaitTaskActionList = null;
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            FinalSkillRingUI.Hide();
            timeLeft = 0;
            SetHeadMoveDestination();
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
                list.Add(new Vector2[] { p[0], p[1] });
            dist = 9*MapManager.gridHeight + GetHeadToBodyLength(); // 计算出第一次车厢第一节停在第7行所需要的路程
            WaitTaskActionList = createActionListFunc();
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // 是否抵达停靠点
            c.AddSpellingFunc(delegate {
                dist -= GetMoveSpeed();
                if (dist <= 0)
                {
                    CancelSetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new IdleState(this));
                    timeLeft = wait;
                    return true;
                }
                return false;
            });
            // 等待
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    foreach (var action in WaitTaskActionList)
                        action(timeLeft, wait); // 执行指令
                    timeLeft--;
                    return false;
                }
                else
                {
                    SetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new MoveState(this));
                    // 再来一次，这次要计算车尾到第二行的距离
                    dist += 9 * GetBodyLength() - 4.5f*MapManager.gridHeight;
                    WaitTaskActionList = createActionListFunc(); // 重置等待任务
                    return true;
                }
            });
            // 是否抵达停靠点
            c.AddSpellingFunc(delegate {
                dist -= GetMoveSpeed();
                if (dist <= 0)
                {
                    CancelSetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new IdleState(this));
                    timeLeft = wait;
                    return true;
                }
                return false;
            });
            // 等待
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    foreach (var action in WaitTaskActionList)
                        action(timeLeft, wait); // 执行指令
                    timeLeft--;
                    return false;
                }
                else
                {
                    SetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new MoveState(this));
                    return true;
                }
            });
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (IsHeadMoveToDestination())
                {
                    if(current_lost_hp_percent < 9999)
                        FinalSkillRingUI.Show();
                    SetActionState(new IdleState(this));
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;





        //return CreateMovementFunc(
        //    new List<Vector2[]>() { new Vector2[2]{ new Vector2(4f, -3f), new Vector2(4f, 27f) } }, // 起始点与终点
        //    MapManager.GetRowY(-3) - MapManager.GetRowY(6) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在第七行所需要走的路程
        //    Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // 获取停靠时间
        //    createActionListFunc // 产生（停靠时执行的指令集合）的方法
        //    );
    }

    /// <summary>
    /// 两侧停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement3(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(6f, 9f), new Vector2(6f, -3.5f) },
                new Vector2[2]{ new Vector2(1f, -3.5f), new Vector2(1f, 27f) }
            }, // 起始点与终点
            22*MapManager.gridHeight + GetHeadToBodyLength(),
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 左侧停靠·奇数行
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement4(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2] { new Vector2(0f, 8f), new Vector2(0f, -22f) } }, // 起始点与终点
            MapManager.GetRowY(0) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 左侧停靠·偶数行
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement5(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2] { new Vector2(0f, 8f), new Vector2(0f, -22f) } }, // 起始点与终点
            MapManager.GetRowY(1) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 两侧停靠·镜像
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement6(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(2f, 9f), new Vector2(2f, -3.5f) },
                new Vector2[2]{ new Vector2(7f, -3.5f), new Vector2(7f, 27f) }
            }, // 起始点与终点
            22 * MapManager.gridHeight + GetHeadToBodyLength(),
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }
}
