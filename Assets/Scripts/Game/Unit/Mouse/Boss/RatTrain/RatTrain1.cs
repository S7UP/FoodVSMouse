using System;
using System.Collections.Generic;

using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 列车二阶
/// </summary>
public class RatTrain1 : BaseRatTrain
{
    private static RuntimeAnimatorController FogCreator_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserEffect_RuntimeAnimatorController;
    private static RuntimeAnimatorController FireBulletAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController Head_RuntimeAnimatorController;
    private static RuntimeAnimatorController Body_RuntimeAnimatorController;
    private static RuntimeAnimatorController FireBullet_RuntimeAnimatorController;
    private static RuntimeAnimatorController Bomb_RuntimeAnimatorController;


    private static string LaserEffectKey = "RatTrain1_LaserEffectKey";

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
        if(FogCreator_RuntimeAnimatorController == null)
        {
            FogCreator_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FogCreator");
            LaserAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserAttacker");
            LaserEffect_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserEffect");
            FireBulletAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FireBulletAttacker");
            Head_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Head");
            Body_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Body");
            FireBullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FireBullet");
            Bomb_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Bomb");
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
        retinueList.Clear();
        lost_hp_percent_queue.Clear();
        current_lost_hp_percent = 9999;
        base.MInit();
        // 创建13节车厢
        CreateHeadAndBody(13);
        // 设置车头、车身、车尾的伤害倍率
        GetHead().SetDmgRate(GetParamValue("head_normal"), GetParamValue("head_burn"));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal"), GetParamValue("body_burn"));
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal"), GetParamValue("tail_burn"));
        // 默认速度为2.4横格/每秒
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridWidth/60);
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
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.RatTrain, 1))
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
                    extra_burn_rate.Value = 1 - GetParamValue("p_extra_burn_defence") / 100;
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
        // 设置获取技能组的方法
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(delegate { return attack_orderList; });

        if (mHertIndex == 0)
        {
            LoadP3SkillAbility();
        }
        else if (mHertIndex == 1)
        {
            LoadP3SkillAbility();
        }
        else
        {
            LoadP3SkillAbility();
        }

        // 移速变化
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        float hp = GetParamValue("hp0", mHertIndex);
        float burn_rate0 = 1 - GetParamValue("burn_defence0") / 100;
        float burn_rate1 = 1 - GetParamValue("burn_defence1") / 100;

        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, false, soldier_type, soldier_shape, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, false, soldier_type, soldier_shape, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, false, soldier_type, soldier_shape, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement3(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, false, soldier_type, soldier_shape, fog0_0, hp, burn_rate0));
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
    /// 创建一个激光发射器
    /// </summary>
    private BaseUnit CreateLaserAttacker(RatTrainComponent master, bool isLeft, float burn_rate)
    {
        float hp = GetParamValue("hp1");
        int wait = Mathf.FloorToInt(GetParamValue("t1_0")*60);
        float dmg_rate = GetParamValue("dmg_rate1");
        float ice_val = GetParamValue("ice_val1");

        MouseModel m = MouseModel.GetInstance(LaserAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
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

        int timeLeft = 0;
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
                Vector2 rot = m.transform.position.y > MapManager.GetRowY(3f) ? Vector2.down : Vector2.up;
                BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PreAttack", "Disappear", true);
                e.spriteRenderer.material = GameManager.Instance.GetMaterial("LinearDodge");
                e.SetSpriteRendererSorting("Effect", 9);
                GameController.Instance.AddEffect(e);
                m.mEffectController.AddEffectToDict(LaserEffectKey, e, 1f * Vector2.up * MapManager.gridHeight);
                e.transform.right = rot;
                timeLeft = wait;
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            timeLeft--;
            if (timeLeft <= 0)
            {
                m.animatorController.Play("PostAttack");
                // 移除蓄力特效
                m.mEffectController.RemoveEffectFromDict(LaserEffectKey);
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
                    //m.mEffectController.AddEffectToDict(LaserEffectKey, e, 1f * Vector2.up * MapManager.gridHeight);
                }
                // 产生真正的激光判定
                {
                    Action<BaseUnit> action = (u) => 
                    {
                        ITask t = EnvironmentFacade.GetIceDebuff(u);
                        if (t != null)
                            new DamageAction(CombatAction.ActionType.BurnDamage, this, u, (t as IceTask).GetValue() * dmg_rate).ApplyAction();

                        if (ice_val > 0)
                            EnvironmentFacade.AddIceDebuff(u, ice_val);
                    };

                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos + rot*1.5f*MapManager.gridHeight, 0.4f, 3f, "BothCollide");
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
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            return m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        t.AddOnExitAction(delegate {
            m.MDestory();
        });
        m.taskController.AddTask(t);

        return m;
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    private void SpawnEnemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.moveRotate = rot;
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
        });
        m.AddTask(task);
    }

    /// <summary>
    /// 创建一个喷雾器
    /// </summary>
    private BaseUnit CreateFogCreator(MouseUnit master, float hp, bool isLeft, int waitTime, bool use_type_and_shape, int type, int shape, int fog_time, float burn_rate)
    {
        MouseModel m = MouseModel.GetInstance(FogCreator_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
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
                        // 召唤一只小怪并释放迷雾
                        if (m.IsAlive())
                        {
                            Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                            Vector2 pos = new Vector2(m.transform.position.x, m.transform.position.y);
                            for (int i = -1; i <= 1; i+=2)
                            {
                                FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + MapManager.gridHeight * rot + MapManager.gridWidth * Vector2.right * i/2);
                                e.SetOpen();
                                int timeLeft = fog_time;
                                CustomizationTask t = new CustomizationTask();
                                t.AddOnEnterAction(delegate {
                                    if (e.transform.position.x < MapManager.GetColumnX(GetParamValue("left_col0") - 1) || e.transform.position.x > MapManager.GetColumnX(9 - GetParamValue("right_col0")))
                                        timeLeft = Mathf.Min(timeLeft, 60);
                                });
                                t.AddTaskFunc(delegate {
                                    if (timeLeft > 0)
                                        timeLeft--;
                                    else
                                        return true;
                                    return false;
                                });
                                t.AddOnExitAction(delegate {
                                    e.SetDisappear();
                                });
                                e.AddTask(t);
                                GameController.Instance.AddAreaEffectExecution(e);
                            }

                            Vector2 startV2 = pos + 0.5f * rot * MapManager.gridWidth;
                            if (startV2.x <= MapManager.GetColumnX(8.5f) && startV2.x >= MapManager.GetColumnX(0))
                            {
                                if (!use_type_and_shape)
                                {
                                    // 根据startV2的横坐标来决定召唤的老鼠
                                    if (startV2.x > MapManager.GetColumnX(6.5f))
                                    {
                                        type = Mathf.FloorToInt(GetParamValue("type0_0"));
                                        shape = Mathf.FloorToInt(GetParamValue("shape0_0"));
                                    }
                                    else if (startV2.x > MapManager.GetColumnX(4.5f))
                                    {
                                        type = Mathf.FloorToInt(GetParamValue("type0_1"));
                                        shape = Mathf.FloorToInt(GetParamValue("shape0_1"));
                                    }
                                    else if (startV2.x > MapManager.GetColumnX(2.5f))
                                    {
                                        type = Mathf.FloorToInt(GetParamValue("type0_2"));
                                        shape = Mathf.FloorToInt(GetParamValue("shape0_2"));
                                    }
                                    else
                                    {
                                        type = Mathf.FloorToInt(GetParamValue("type0_3"));
                                        shape = Mathf.FloorToInt(GetParamValue("shape0_3"));
                                    }
                                }
                                SpawnEnemy(startV2, pos + 1.0f * rot * MapManager.gridWidth, Vector2.left, type, shape);
                            }
                            m.CloseCollision();
                        }
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
        }
        return m;
    }


    /// <summary>
    /// 发射一枚火炮
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateFireBullet(MouseUnit master, Vector2 pos, Vector2 rotate)
    {
        EnemyBullet b = EnemyBullet.GetInstance(FireBullet_RuntimeAnimatorController, master, 0);
        b.CloseCollision();
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(36);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.mCircleCollider2D.radius = 0.25f*MapManager.gridWidth;
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
            r.SetOnFoodEnterAction(delegate {
                if(!isHit)
                {
                    isHit = true;
                    hitAction(null);
                }
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
    /// 创建一个投影
    /// </summary>
    private MouseModel CreateProjection(RuntimeAnimatorController runtimeAnimatorController, bool isFireAttacker)
    {
        Vector2 pos = MapManager.GetGridLocalPosition(8, 3);
        MouseModel m = MouseModel.GetInstance(runtimeAnimatorController);
        {
            BossUnit.AddBossIgnoreDebuffEffect(m);
            m.transform.position = pos;
            m.SetBaseAttribute(mMaxHp, 1, 1f, 0, 0, 0, 0);
            m.SetGetBurnRateFunc(delegate { return mBurnRate; });
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
            m.currentYIndex = MapManager.GetYIndex(pos.y);
            m.mBoxCollider2D.offset = new Vector2(0, 0);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            // 伤害转移给本体的方法
            Action<CombatAction> transToMaster = (action) => {
                if(action is DamageAction)
                {
                    var damageAction = action as DamageAction;
                    if (isFireAttacker)
                    {
                        if (damageAction.IsDamageType(DamageAction.DamageType.BombBurn))
                        {
                            DamageAction d = new DamageAction(action.mActionType, action.Creator, this, damageAction.DamageValue * GetParamValue("tail_burn", mHertIndex));
                            d.AddDamageType(DamageAction.DamageType.BombBurn);
                            d.ApplyAction();
                        }
                        else
                        {
                            DamageAction d = new DamageAction(action.mActionType, action.Creator, this, damageAction.DamageValue * GetParamValue("tail_normal", mHertIndex));
                            d.ApplyAction();
                        }
                    }
                    else
                    {
                        if (damageAction.IsDamageType(DamageAction.DamageType.BombBurn))
                        {
                            DamageAction d = new DamageAction(action.mActionType, action.Creator, this, damageAction.DamageValue * GetParamValue("body_burn", mHertIndex));
                            d.AddDamageType(DamageAction.DamageType.BombBurn);
                            d.ApplyAction();
                        }
                        else
                        {
                            DamageAction d = new DamageAction(action.mActionType, action.Creator, this, damageAction.DamageValue * GetParamValue("body_normal", mHertIndex));
                            d.ApplyAction();
                        }
                    }
                }
            };
            m.AddActionPointListener(ActionPointType.PreReceiveDamage, transToMaster);
            // 收纳为自身随从
            retinueList.Add(m);
            // 不触发猫，进家也不判输
            m.canTriggerCat = false;
            m.canTriggerLoseWhenEnterLoseLine = false;
            m.isBoss = true;

            // 动作
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate
                {
                    m.mCurrentHp = mCurrentHp; // 生命值与BOSS本体同步
                    return false;
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            // m.UpdateRenderLayer(GetBody(0).spriteRenderer.sortingOrder - 1); // 图层-1
        }
        return m;
    }

    /// <summary>
    /// 生成带炮台的列车
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTrain()
    {
        int wait = Mathf.FloorToInt(GetParamValue("t2_0") * 60);
        int num = Mathf.FloorToInt(GetParamValue("num2_0"));

        CustomizationTask fogTask = null;

        List<BaseUnit> compList = new List<BaseUnit>();
        // 生成车头投影
        BaseUnit head = CreateProjection(Head_RuntimeAnimatorController, false);
        compList.Add(head);
        // 生成车身与炮台投影
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));
        MouseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);
        compList.Add(fireBulletAttacker);
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        // 以下为其行为控制
        foreach (var u in compList)
        {
            u.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // 不可作为选取的目标
            u.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            u.AddCanHitFunc(noHitFunc); // 不可被子弹击中
            u.CloseCollision();
            u.moveRotate = Vector2.left;
            u.transform.position = MapManager.GetGridLocalPosition(8, -3); // 直接藏在屏幕外边
        }
        float maxDist = GetHeadToBodyLength();
        float distLeft = GetHeadToBodyLength();
        int count = 1; // 出现的车组件数量（头和炮台也算的）
        int timeLeft = wait;
        int currentAttackCount = 0;
        Vector2 start = MapManager.GetGridLocalPosition(9, 3);
        Vector2 stop = MapManager.GetGridLocalPosition(5, 3);
        head.transform.position = start;

        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            distLeft -= mCurrentMoveSpeed;
            for (int i = 0; i < count; i++)
            {
                BaseUnit u = compList[i];
                u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
                if (i == count - 1)
                {
                    u.animatorController.Play("Appear", false, 0.75f*(1-distLeft/maxDist));
                }
                else
                {
                    u.animatorController.Play("Move", true);
                }
            }
            if(distLeft <= 0)
            {
                if(count < compList.Count)
                {
                    compList[count].transform.position = start - compList[count].moveRotate * distLeft;
                    compList[count].OpenCollision();
                }
                count++;
                maxDist = GetBodyLength();
                distLeft += maxDist;
            }
            return count > compList.Count;
        });
        t.AddTaskFunc(delegate {
            // 继续前进
            foreach (var u in compList)
            {
                u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
            }
            if (fireBulletAttacker.transform.position.x < stop.x)
            {
                head.moveRotate = Vector2.left;
                compList[1].moveRotate = Vector2.left;
                compList[3].moveRotate = Vector2.right;
                foreach (var u in compList)
                {
                    if (u != fireBulletAttacker)
                    {
                        u.animatorController.Play("Disappear");
                    } 
                }
                return true;
            }
            return false;
        });
        // 停靠时分类讨论：车头和第一节车厢继续前进并消失，车尾倒退消失
        t.AddTaskFunc(delegate {
            // 继续前进
            bool flag = false;
            foreach (var u in compList)
            {
                if (u != fireBulletAttacker)
                {
                    u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
                    if (u.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                // 炮台架起，变得可被攻击
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                // 清理掉其他车厢
                foreach (var u in compList)
                {
                    if (u != fireBulletAttacker)
                        u.MDestory();
                }
                return true;
            }
            return false;
        });
        // 打开炮台
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // 等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos(90*i*Mathf.PI/180), Mathf.Sin(90 * i * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f*MapManager.gridWidth*rot, rot);
                }
                fireBulletAttacker.animatorController.Play("Attack0");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < num)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                    }
                    fireBulletAttacker.animatorController.Play("Attack0", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // 在转了
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // 再等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                }
                fireBulletAttacker.animatorController.Play("Attack1");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < num)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                    }
                    fireBulletAttacker.animatorController.Play("Attack1", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // 收起来，并追加喷雾器攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.animatorController.Play("Move", true);
                fogTask = GetFogAreaToFireBulletAttackerTask(fireBulletAttacker, fireBulletAttacker.GetSpriteRenderer().sortingOrder - 1);
                t.taskController.AddTask(fogTask);
                return true;
            }
            return false;
        });
        // 自爆！
        t.AddTaskFunc(delegate {
            if (fogTask.IsEnd())
            {
                // 爆炸特效
                {
                    BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "Die", null, false);
                    e.transform.position = fireBulletAttacker.transform.position;
                    GameController.Instance.AddEffect(e);
                }
                fireBulletAttacker.MDestory();
                // 产生3*3爆破效果
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(fireBulletAttacker.transform.position, 3, 3, "BothCollide");
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    r.isAffectFood = true;
                    r.SetOnFoodEnterAction((u) => {
                        BurnManager.BurnDamage(this, u);
                    });
                    r.SetOnEnemyEnterAction((u) => {
                        BurnManager.BurnDamage(this, u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                return true;
            }
            return false;
        });
        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// 生成雾域给P3的炮台
    /// </summary>
    private CustomizationTask GetFogAreaToFireBulletAttackerTask(MouseUnit master, int sortingOrder)
    {
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex) * 60);
        int fog2_0 = Mathf.FloorToInt(GetParamValue("fog2_0", mHertIndex) * 60);
        int soldier_type2_0 = Mathf.FloorToInt(GetParamValue("soldier_type2_0", mHertIndex));
        int soldier_shape2_0 = Mathf.FloorToInt(GetParamValue("soldier_shape2_0", mHertIndex));
        float fog_hp2_0 = GetParamValue("fog_hp2_0", mHertIndex);
        float burn_rate = 1 - GetParamValue("burn_defence2")/100;


        BaseUnit[] arr = new BaseUnit[2];
        arr[0] = CreateFogCreator(master, fog_hp2_0, true, t2_1, false, soldier_type2_0, soldier_shape2_0, fog2_0, burn_rate);
        arr[0].NumericBox.BurnRate.AddModifier(new FloatModifier(0.25f));
        arr[0].GetSpriteRenderer().sortingOrder = sortingOrder;
        arr[1] = CreateFogCreator(master, fog_hp2_0, false, t2_1, false, soldier_type2_0, soldier_shape2_0, fog2_0, burn_rate);
        arr[1].NumericBox.BurnRate.AddModifier(new FloatModifier(0.25f));
        arr[1].GetSpriteRenderer().sortingOrder = sortingOrder;
        
        int timeLeft = 67 + t2_1;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            timeLeft--;
            if (timeLeft <= 0)
            {
                Vector2[] v2List = new Vector2[4] {
                    new Vector2(MapManager.gridWidth, MapManager.gridHeight),
                    new Vector2(MapManager.gridWidth, 0),
                    new Vector2(-MapManager.gridWidth, 0),
                    new Vector2(-MapManager.gridWidth, MapManager.gridHeight)
                };
                // 填充其他地方的雾和老鼠
                foreach (var m in arr)
                {
                    if (m.IsAlive())
                    {
                        int r = m.transform.localScale.y > 0 ? 1 : -1;
                        Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                        foreach (var v2 in v2List)
                        {
                            FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + new Vector2(v2.x, v2.y * r));
                            e.SetOpen();

                            CustomizationTask t = new CustomizationTask();
                            int fog_time = fog2_0;
                            t.AddTaskFunc(delegate {
                                if (fog_time > 0)
                                    fog_time--;
                                else
                                    return true;
                                return false;
                            });
                            t.AddOnExitAction(delegate {
                                e.SetDisappear();
                            });
                            e.AddTask(t);
                            GameController.Instance.AddAreaEffectExecution(e);

                            Vector2 startV2 = pos + 0.5f * new Vector2(v2.x, v2.y * r);
                            if (startV2.x <= MapManager.GetColumnX(8.5f) && startV2.x >= MapManager.GetColumnX(0))
                            {
                                int type, shape;
                                // 根据startV2的横坐标来决定召唤的老鼠
                                if (startV2.x > MapManager.GetColumnX(6.5f))
                                {
                                    type = Mathf.FloorToInt(GetParamValue("type0_0"));
                                    shape = Mathf.FloorToInt(GetParamValue("shape0_0"));
                                }
                                else if (startV2.x > MapManager.GetColumnX(4.5f))
                                {
                                    type = Mathf.FloorToInt(GetParamValue("type0_1"));
                                    shape = Mathf.FloorToInt(GetParamValue("shape0_1"));
                                }
                                else if (startV2.x > MapManager.GetColumnX(2.5f))
                                {
                                    type = Mathf.FloorToInt(GetParamValue("type0_2"));
                                    shape = Mathf.FloorToInt(GetParamValue("shape0_2"));
                                }
                                else
                                {
                                    type = Mathf.FloorToInt(GetParamValue("type0_3"));
                                    shape = Mathf.FloorToInt(GetParamValue("shape0_3"));
                                }
                                SpawnEnemy(startV2, pos + 1.0f * new Vector2(v2.x, v2.y * r), Vector2.left, type, shape);
                            }
                            // SpawnEnemy(pos + 0.5f * new Vector2(v2.x, v2.y * r), pos + 1.0f * new Vector2(v2.x, v2.y * r), Vector2.left, soldier_type2_0, soldier_shape2_0);
                        }
                    }
                }
                return true;
            }
            return false;
        });
        t.AddTimeTaskFunc(105);
        return t;
    }

    /// <summary>
    /// 只生成一个带炮台的车尾（用于P3的雾攻击）
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTail(Vector2 start)
    {
        int wait = Mathf.FloorToInt(GetParamValue("t2_0") * 60);
        int num = Mathf.FloorToInt(GetParamValue("num2_0"));
        float colIndex = 9 - GetParamValue("right_col3");

        MouseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // 不可作为选取的目标
        fireBulletAttacker.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
        fireBulletAttacker.AddCanHitFunc(noHitFunc); // 不可被子弹击中
        fireBulletAttacker.moveRotate = Vector2.left;
        fireBulletAttacker.transform.position = start;
        float maxDist = GetBodyLength();
        int timeLeft = wait;
        int currentAttackCount = 0;
        Vector2 stop = MapManager.GetGridLocalPosition(colIndex, 3);
        
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            fireBulletAttacker.animatorController.Play("Appear");
        });
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.transform.position.x > stop.x)
                fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * GetMoveSpeed();
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.animatorController.Play("Move");
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.transform.position.x <= stop.x)
            {
                // 炮台架起，变得可被攻击
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                return true;
            }
            else
            {
                // 继续前进
                fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            }
            return false;
        });
        // 打开炮台
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // 等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                }
                fireBulletAttacker.animatorController.Play("Attack0");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < num)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                    }
                    fireBulletAttacker.animatorController.Play("Attack0", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // 在转了
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // 再等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                }
                fireBulletAttacker.animatorController.Play("Attack1");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < num)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot);
                    }
                    fireBulletAttacker.animatorController.Play("Attack1", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // 自爆！
        t.AddTaskFunc(delegate {
            // 爆炸特效
            {
                BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "Die", null, false);
                e.SetSpriteRendererSorting("Effect", 10);
                e.transform.position = fireBulletAttacker.transform.position;
                GameController.Instance.AddEffect(e);
            }
            fireBulletAttacker.MDestory();
            // 产生3*3爆破效果
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(fireBulletAttacker.transform.position, 3, 3, "BothCollide");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.isAffectFood = true;
                r.SetOnFoodEnterAction((u) => {
                    BurnManager.BurnDamage(this, u);
                });
                r.SetOnEnemyEnterAction((u) => {
                    BurnManager.BurnDamage(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            return true;
        });
        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// 指令-对流激光
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateLaserAttackerAction(float burn_rate)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                // 场上满足条件的车厢生成激光发射器
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.x >= MapManager.GetColumnX(-1) && body.transform.position.x <= MapManager.GetColumnX(9))
                    {
                        if (body.transform.position.y > MapManager.GetRowY(0.5f))
                        {
                            CreateLaserAttacker(body, false, burn_rate);
                        }
                        else if (body.transform.position.y < MapManager.GetRowY(5.5f))
                        {
                            CreateLaserAttacker(body, true, burn_rate);
                        }
                        else
                        {
                            CreateLaserAttacker(body, false, burn_rate);
                            CreateLaserAttacker(body, true, burn_rate);
                        }
                    }
                }
            }
        };
        return action;
    }

    /// <summary>
    /// 指令-雾袭
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateFogCreatorAction(int waitTime, bool use_type_and_shape, int type, int shape, int fog0_0, float fog_hp0_0, float burn_rate)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f))
                    {
                        if(body.transform.position.y > MapManager.GetRowY(0.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, use_type_and_shape, type, shape, fog0_0, burn_rate);
                        }else if(body.transform.position.y < MapManager.GetRowY(5.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, use_type_and_shape, type, shape, fog0_0, burn_rate);
                        }
                        else
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, use_type_and_shape, type, shape, fog0_0, burn_rate);
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, use_type_and_shape, type, shape, fog0_0, burn_rate);
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
        // 变量初始化
        float distLeft = 0;
        int timeLeft = 0;
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
                Debug.Log("distLeft=" + distLeft);
                if (distLeft <= 0)
                {
                    AddHeadMoveDist(distLeft); // 坐标补正一次
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
    /// 对流停靠·奇数列
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 1, x3 = 1, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 6f), new Vector2(x2, 6f) },
                new Vector2[2] { new Vector2(x3, 0f), new Vector2(x4, 0f) }
            }, // 起始点与终点
            (x1 - x2 - x3 + 8) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // 计算停靠前的移动距离
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 对流停靠·偶数列
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0, x3 = 0, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(x1, 0f), new Vector2(x2, 0f) }, 
                new Vector2[2] { new Vector2(x3, 6f), new Vector2(x4, 6f) }
            }, // 起始点与终点
            (x1 - x2 - x3 + 7)*MapManager.gridWidth + GetHeadToBodyLength() + 5*GetBodyLength(), // 计算停靠前的移动距离
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            ); 
    }

    /// <summary>
    /// 二六停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0.5f, x3 = 0.5f, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 1f), new Vector2(x2, 1f) },
                new Vector2[2] { new Vector2(x3, 5f), new Vector2(x4, 5f) }
            }, // 起始点与终点
            (x1 - x2 - x3 + 7) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // 计算停靠前的移动距离
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 六二停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement3(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0.5f, x3 = 0.5f, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 5f), new Vector2(x2, 5f) },
                new Vector2[2] { new Vector2(x3, 1f), new Vector2(x4, 1f) }
            }, // 起始点与终点
            (x1 - x2 - x3 + 7) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // 计算停靠前的移动距离
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            );
    }

    /// <summary>
    /// 两面包夹芝士停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility GetUltimateSkill()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("extra_type0", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("extra_shape0", mHertIndex));
        float hp = GetParamValue("hp0", mHertIndex);
        float burn_rate0 = 1 - GetParamValue("extra_burn_defence0") / 100;
        float burn_rate1 = 1 - GetParamValue("extra_burn_defence1") / 100;

        float x1 = 9, x2 = 1, x3 = 1, x4 = 9, x5 = 10, x6 = -3;




        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            FinalSkillRingUI.Hide();
            c.ActivateChildAbility(CreateMovementFunc(
                new List<Vector2[]>() {
                    new Vector2[2]{ new Vector2(x1, 0f), new Vector2(x2, 0f) },
                    new Vector2[2] { new Vector2(x3, 6f), new Vector2(x4, 6f) },
                    new Vector2[2] { new Vector2(x5, 3f), new Vector2(x6, 3f) },
                }, // 起始点与终点
                (x1 - x2 - x3 + x4 + x5 - 1) * MapManager.gridWidth + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
                Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // 获取停靠时间
                delegate {
                    List<Action<int, int>> actionList = new List<Action<int, int>>();
                    actionList.Add(CreateLaserAttackerAction(burn_rate1));
                    actionList.Add(CreateFogCreatorAction(t0_0, true, soldier_type, soldier_shape, fog0_0, hp, burn_rate0));
                    return actionList;
                } // 产生（停靠时执行的指令集合）的方法
                ));
        };
        {
            c.AddSpellingFunc(delegate
            {
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                isMoveToDestination = false;
                AddRouteListByGridIndex(new List<Vector2[]>() {
                    new Vector2[2] { new Vector2(0f, -4f), new Vector2(26f, -4f) },
                });
                SetActionState(new MoveState(this));
                return true;
            });
            {
                BaseUnit u = null;
                c.AddSpellingFunc(delegate
                {
                    BaseUnit tail = GetLastBody();
                    if (MapManager.GetYIndex(tail.transform.position.y) == 3 && tail.transform.position.x <= MapManager.GetColumnX(8) - GetBodyLength())
                    {
                        u = CreateFireBulletAttackerTail((Vector2)tail.transform.position - moveRotate * GetBodyLength());
                        return true;
                    }
                    return false;
                });
                // 判断炮台是否消失，消失则为该技能结束的标志
                c.AddSpellingFunc(delegate
                {
                    return u == null || !u.IsAlive();
                });
            }
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (IsHeadMoveToDestination())
                {
                    SetActionState(new IdleState(this));
                    if (current_lost_hp_percent < 9999)
                        FinalSkillRingUI.Show();
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
    /// 在进入移动状态时，附加一个效果，如果被炸则会停止一段时间
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if (GetHead() != null)
            GetHead().AddActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        NumericBox.BurnRate.AddModifier(extra_burn_rate);
        base.OnMoveStateEnter();
    }

    public override void OnMoveState()
    {
        if (stun_timeLeft > 0)
        {
            WaitRingUI.Show();
            WaitRingUI.SetPercent((float)stun_timeLeft / max_stun_time);
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
}
