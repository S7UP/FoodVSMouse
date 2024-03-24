using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 美队鼠
/// </summary>
public class CaptainAmerica : BossUnit
{
    private static Sprite Wait_Icon_Sprite;
    private static Sprite AttackLeft_Icon_Sprite;

    private RuntimeAnimatorController ShieldBullet_Run;
    private RuntimeAnimatorController StrikeEffect_Run;
    private RuntimeAnimatorController RainBow_Run;
    private Sprite Shield_Spr;

    private float dmgRecord; // 受到的伤害总和
    private Queue<float> lost_hp_percent_queue = new Queue<float>();
    private float current_lost_hp_percent;

    private FloatModifier p_dmgRateMod = new FloatModifier(1.0f);
    private RetangleAreaEffectExecution ShieldArea;

    private RingUI AttackLeftRingUI;
    private RingUI FinalSkillRingUI;

    public override void Awake()
    {
        if (Wait_Icon_Sprite == null)
        {
            Wait_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Wait");
            AttackLeft_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/AttackLeft");
        }
        // 默认皮肤
        {
            ShieldBullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/0/ShieldBullet");
            StrikeEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/0/Strike");
            RainBow_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/0/RainBow");
            Shield_Spr = GameManager.Instance.GetSprite("Boss/16/0/Shield");
        }
        base.Awake();
    }

    public override void MInit()
    {
        ShieldArea = null;
        p_dmgRateMod.Value = 1;
        dmgRecord = 0;
        lost_hp_percent_queue.Clear();
        current_lost_hp_percent = 9999;
        base.MInit();

        // 大招UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this, 0.25f * MapManager.gridHeight * Vector3.down));
            FinalSkillRingUI.SetPercent(0);
            FinalSkillRingUI.Hide();
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // 攻击次数显示UI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            // 添加绑定任务
            {
                float r = 199f / 255;
                float g = 233f / 255;
                float b = 255f / 255;

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    rUI.Show();
                    rUI.SetIcon(AttackLeft_Icon_Sprite);
                    rUI.SetPercent(0);
                    rUI.SetColor(new Color(r, g, b, 0.75f));
                });
                task.AddTaskFunc(delegate {
                    rUI.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.up;
                    float per = rUI.GetPercent();
                    rUI.SetColor(new Color(r, g, b, 0.75f));
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
            AttackLeftRingUI = rUI;
        }
        // 受击事件
        {
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) => {
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
                animatorController.Play("PostMove0");
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (current_lost_hp_percent != 9999)
                        FinalSkillRingUI.Show();
                    animatorController.Play("Idle0", true);
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
                    // 先移除一次被动减伤（保险措施）
                    RemoveP();
                    // 添加被动减伤
                    AddP();
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
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.CaptainAmerica, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // 特殊参数初始化
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
        // 被动减伤
        {
            Action<float[]> action = delegate
            {
                RemoveP();
                p_dmgRateMod.Value = 1 - GetParamValue("p_defence") / 100;
                AddP();
            };
            AddParamChangeAction("p_defence", action);
            action(null);
        }
        // 根据传入的skin换皮（0为原皮，1为变异）
        {
            Action<float[]> action = delegate
            {
                int skin = Mathf.FloorToInt(GetParamValue("skin"));
                ShieldBullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/" + skin + "/ShieldBullet");
                StrikeEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/" + skin + "/Strike");
                RainBow_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/" + skin + "/RainBow");
                Shield_Spr = GameManager.Instance.GetSprite("Boss/16/" + skin + "/Shield");
                animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/" + skin+"/0");
            };
            action(null);
            AddParamChangeAction("skin", action);
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
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////


    /// <summary>
    /// 添加被动技能
    /// </summary>
    private void AddP()
    {
        NumericBox.DamageRate.AddModifier(p_dmgRateMod);

        float w = GetParamValue("p_w") - 0.25f;
        float h = GetParamValue("p_h") - 0.25f;

        if (w <= 0 || h <= 0 || (ShieldArea != null && ShieldArea.IsValid()))
            return;

        float dmg_trans = GetParamValue("p_dmg_trans")/100;

        FloatModifier dmgRateMod = new FloatModifier(1 - GetParamValue("p_give_defence")/100);
        Action<CombatAction> action = (combat) => 
        { 
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.mActionType.Equals(DamageAction.ActionType.CauseDamage))
                    new DamageAction(CombatAction.ActionType.CauseDamage, dmgAction.Creator, this, dmgAction.DamageValue * dmg_trans).ApplyAction();
            }
        };

        int skin = Mathf.FloorToInt(GetParamValue("skin"));

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, w, h, "ItemCollideEnemy");
        r.name = "ShieldArea";
        r.isAffectMouse = true;
        r.AddEnemyEnterConditionFunc((m) => {
            return !m.IsBoss() && MouseManager.IsGeneralMouse(m);
        });
        r.SetOnEnemyEnterAction((m) => {
            m.NumericBox.DamageRate.AddModifier(dmgRateMod);
            m.actionPointController.AddListener(ActionPointType.PreReceiveDamage, action);
            // 添加庇护特效
            {
                BaseEffect e = BaseEffect.CreateInstance(Shield_Spr);
                string name;
                int order;
                if (m.TryGetSpriteRenternerSorting(out name, out order))
                    e.SetSpriteRendererSorting(name, order+1);
                GameController.Instance.AddEffect(e);
                // m.mEffectController.AddEffectToGroup("MeiDui_Shield" + skin, 0, e);
                m.mEffectController.AddEffectToDict("MeiDui_Shield" + skin, e, Vector2.zero);
            }

        });
        r.SetOnEnemyExitAction((m) => {
            m.NumericBox.DamageRate.RemoveModifier(dmgRateMod);
            m.actionPointController.RemoveListener(ActionPointType.PreReceiveDamage, action);
            // 移除庇护特效
            // m.mEffectController.RemoveEffectFromGroup("MeiDui_Shield" + skin, e);
            m.mEffectController.RemoveEffectFromDict("MeiDui_Shield" + skin);
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
                r.MDestory();
            });
            r.AddTask(t);
        }
        ShieldArea = r;
    }

    /// <summary>
    /// 移除被动技能
    /// </summary>
    private void RemoveP()
    {
        NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);

        if (ShieldArea != null && ShieldArea.IsValid())
        {
            ShieldArea.MDestory();
            ShieldArea = null;
        }
    }

    #region 一技能
    private Vector2 FindS1Pos(List<int> rowIndexList, out int rowIndex)
    {
        float xMax = MapManager.GetColumnX(9 - GetParamValue("right_col0_0"));
        float xMin = MapManager.GetColumnX(9 - GetParamValue("right_col0_1"));

        BaseUnit target = null;
        float min = float.MaxValue;
        foreach (var u in GameController.Instance.GetEachEnemy())
        {
            if(u.transform.position.x < min && (u is MouseUnit && !(u as MouseUnit).IsBoss()) && 
                MouseManager.IsGeneralMouse(u) && u.transform.position.x >= xMin &&
                u.transform.position.x <= xMax && rowIndexList.Contains(u.GetRowIndex()) &&
                u.GetHeight() == 0 && !UnitManager.IsFlying(u))
            {
                min = u.transform.position.x;
                target = u;
            }
        }

        rowIndex = rowIndexList[GetRandomNext(0, rowIndexList.Count)];
        if(target != null)
        {
            rowIndex = target.GetRowIndex();
            return new Vector2(target.transform.position.x, MapManager.GetRowY(rowIndex));
        }
        else
        {
            return MapManager.GetGridLocalPosition(9 - GetParamValue("right_col0_0"), rowIndex);
        }
    }

    private void CreateS0Area(float dmg_trans, float dmg, float dist,  bool isLeft, int stun_time, float min_x, float max_x)
    {
        // 处决格子
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*MapManager.gridWidth*Vector3.left*(isLeft?1:-1), 1f, 0.5f, "CollideGrid");
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction action = new DamageAction(CombatAction.ActionType.RealDamage, this, u, dmg);
                    action.ApplyAction();
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * dmg_trans).ApplyAction();
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 助推老鼠
        RetangleAreaEffectExecution push_area = CreateS0PushArea(isLeft, stun_time, min_x, max_x);

        // 自身也会位移！
        {
            Vector2 v2 = Vector2.zero;
            if (isLeft)
                v2 = new Vector2(-Mathf.Max(0, Mathf.Min(dist, transform.position.x - min_x)), 0);
            else
                v2 = new Vector2(Mathf.Max(0, Mathf.Min(-dist, max_x - transform.position.x)), 0);
            CustomizationTask task = TaskManager.GetAccDecMoveTask(transform, v2, 30);
            task.AddOnExitAction(delegate {
                if (push_area != null && push_area.IsValid())
                    push_area.MDestory();
            });
            taskController.AddTask(task);
        }

    }

    private RetangleAreaEffectExecution CreateS0PushArea(bool isLeft, int stun_time, float min_x, float max_x)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 2f, 0.5f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetOnEnemyEnterAction((u) =>
        {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
        });
        r.AddEnemyEnterConditionFunc((m) => {
            return !m.IsBoss() && MouseManager.IsGeneralMouse(m) && (!m.NumericBox.IntDict.ContainsKey(StringManager.Flying) || m.NumericBox.IntDict[StringManager.Flying].Value <= 0);
        });
        if(isLeft)
        {
            r.SetOnEnemyStayAction((m) => {
                if (m.transform.position.x > r.transform.position.x && m.transform.position.x > min_x)
                {
                    m.transform.position += (r.transform.position.x - m.transform.position.x) * Vector3.right;
                }
            });
        }
        else
        {
            r.SetOnEnemyStayAction((m) => {
                if (m.transform.position.x < r.transform.position.x && m.transform.position.x < max_x)
                {
                    m.transform.position += (r.transform.position.x - m.transform.position.x) * Vector3.right;
                }
            });
        }
        GameController.Instance.AddAreaEffectExecution(r);

        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate
        {
            r.transform.position = transform.position;
            return !IsAlive();
        });
        t.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.AddTask(t);
        
        return r;
    }

    /// <summary>
    /// 圆盾拍击技能定义
    /// </summary>
    private CustomizationSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col0", mHertIndex));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60);
        int num = Mathf.FloorToInt(GetParamValue("num0"));
        float dmg = GetParamValue("dmg0");
        float dmg_trans = GetParamValue("dmg_trans0") / 100;
        float dist = GetParamValue("dist0") * MapManager.gridWidth;
        bool isLeft = GetParamValue("dist0") >= 0? true:false;
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time0") * 60);
        float min_x = MapManager.GetColumnX(GetParamValue("left_col0") - 1);
        float max_x = MapManager.GetColumnX(9 - GetParamValue("max_right_col0"));

        // 变量
        List<int> rowIndexList = new List<int>();
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        // 一些变量初始化
        c.BeforeSpellFunc = delegate 
        {
            rowIndexList.Clear();
            for (int i = 0; i < 7; i++)
                rowIndexList.Add(i);
            AttackLeftRingUI.SetPercent(0);
            AttackLeftRingUI.Show();
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < num; _i++)
                {
                    int i = _i;
                    // 先瞬移
                    task.AddTaskFunc(delegate {
                        animatorController.Play("PreMove0");
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            int selected_rowIndex = -1;
                            transform.position = FindS1Pos(rowIndexList, out selected_rowIndex);
                            rowIndexList.Remove(selected_rowIndex);
                            if(rowIndexList.Count <= 0)
                                for (int k = 0; k < 7; k++)
                                    rowIndexList.Add(k);
                            if (!isLeft)
                                transform.localScale = new Vector2(-1, 1);
                            animatorController.Play("PostMove0");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Attack");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.47f)
                        {
                            // 特效
                            {
                                BaseEffect e = BaseEffect.CreateInstance(StrikeEffect_Run, null, "StrikeEffect", null, false);
                                e.transform.position = transform.position;
                                if (!isLeft)
                                    e.transform.localScale = new Vector2(-1, 1);
                                GameController.Instance.AddEffect(e);
                            }
                            // 对前方两格造成伤害以及对老鼠的推进效果
                            CreateS0Area(dmg_trans, dmg, dist, isLeft, stun_time, min_x, max_x);
                            // 环形UI+1
                            AttackLeftRingUI.SetPercent((float)(i+1)/num);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            timeLeft = wait0;
                            animatorController.Play("Idle0", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                            return true;
                        return false;
                    });
                }
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { AttackLeftRingUI.Hide(); };
        return c;
    }
    #endregion

    #region 二技能
    private int FindR1()
    {
        int rowIndex = 3; // 默认值
        List<int> rowList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        List<int> list = new List<int>();
        int min = int.MaxValue;
        foreach (var index in rowList)
        {
            int count = 0;
            foreach (var unit in GameController.Instance.GetSpecificRowEnemyList(index))
            {
                // 如果目标在传入的条件判断中为真，则计数+1
                if (unit is MouseUnit && !(unit as MouseUnit).IsBoss() && MouseManager.IsGeneralMouse(unit))
                    count++;
            }
            if (count == min)
            {
                list.Add(index);
            }
            else if (count < min)
            {
                list.Clear();
                list.Add(index);
                min = count;
            }
        }

        if (list.Count > 0)
        {
            rowIndex = list[GetRandomNext(0, list.Count)];
        }
        else
            GetRandomNext(0, 1);
        return rowIndex;
    }

    /// <summary>
    /// 获取召唤的行表
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    private List<int> GetS1SpawnRowIndexList(int num, int rowIndex)
    {
        List<int> rowIndexList = new List<int>();
        rowIndexList.Add(rowIndex);
        int count = 1;
        while (count <= 6)
        {
            int r = rowIndex + count;
            if (r >= 0 && r <= 6)
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
            count++;
        }
        return rowIndexList;
    }

    /// <summary>
    /// 产生一个敌人生成器
    /// </summary>
    private void CreateS1EnemySpawner(int type, int shape, int wait, Vector2 pos)
    {
        CustomizationItem item = CustomizationItem.GetInstance(pos, RainBow_Run);

        // 召唤敌人的任务
        {
            int timeLeft = 0;
            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate
            {
                item.animatorController.Play("Appear");
            });
            task.AddTaskFunc(delegate
            {
                if (item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    timeLeft = wait;
                    item.animatorController.Play("Idle");
                    return true;
                }
                return false;
            });
            task.AddTaskFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    MouseUnit m = GameController.Instance.CreateMouseUnit(0, 0, new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
                    m.transform.position = pos;
                    item.animatorController.Play("Disappear");
                    return true;
                }
                return false;
            });
            task.AddTaskFunc(delegate
            {
                return item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
            });
            task.AddOnExitAction(delegate
            {
                item.MDestory();
            });
            item.AddTask(task);
            GameController.Instance.AddItem(item);
        }
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col1"));
        float start_summon_xIndex = 9 - Mathf.FloorToInt(GetParamValue("summon_right_col1_0"));
        float end_summon_xIndex = 9 - Mathf.FloorToInt(GetParamValue("summon_right_col1_1"));
        int num_y = Mathf.FloorToInt(GetParamValue("num1_0"));
        int num_x = Mathf.FloorToInt(GetParamValue("num1_1"));

        int type = Mathf.FloorToInt(GetParamValue("type1"));
        int shape = Mathf.FloorToInt(GetParamValue("shape1"));
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);

        // 变量
        int rowIndex = 0;
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                // 先瞬移
                task.AddTaskFunc(delegate {
                    animatorController.Play("PreMove0");
                    return true;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        rowIndex = FindR1();
                        transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                        animatorController.Play("PostMove0");
                        transform.localScale = new Vector2(1, 1);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("PreStand");
                        RemoveP(); // 失去减伤
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        timeLeft = wait;
                        // 召唤术
                        foreach (var r in GetS1SpawnRowIndexList(num_y, rowIndex))
                            for (int i = 0; i < num_x; i++)
                                CreateS1EnemySpawner(type, shape, wait, MapManager.GetGridLocalPosition(start_summon_xIndex + (end_summon_xIndex - start_summon_xIndex)*((float)i/Mathf.Max(1, num_x-1)), r));
                        animatorController.Play("Stand", true);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        animatorController.Play("PostStand");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        AddP(); // 重获减伤
                        return true;
                    }
                    return false;
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

    private void FindS2RowIndex(out int rowIndex)
    {
        rowIndex = 3;
        // 获取可作为目标的美食数最少的几行
        List<int> rowList = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
        // 从中随机取一行
        if (rowList.Count > 0)
            rowIndex = rowList[GetRandomNext(0, rowList.Count)];
        else
            GetRandomNext(0, 2);
    }

    private void FindS2RowIndex2(int except_rowIndex, out int rowIndex)
    {
        rowIndex = 3;
        // 获取可作为目标的美食数最多的几行
        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        list.Remove(except_rowIndex);
        List<int> rowList = FoodManager.GetRowListWhichHasMaxConditionAllyCount(list, (unit) =>
        {
            if (unit is FoodUnit)
            {
                FoodUnit f = unit as FoodUnit;
                return FoodManager.IsAttackableFoodType(f);
            }
            return false;
        });
        // 从中随机取一行
        if (rowList.Count > 0)
            rowIndex = rowList[GetRandomNext(0, rowList.Count)];
        else
            GetRandomNext(0, 2);
    }

    /// <summary>
    /// 回旋圆盾
    /// </summary>
    /// <param name="info"></param>
    private CustomizationSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // 常量
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col2"));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        int wait2 = Mathf.FloorToInt(GetParamValue("wait2_2") * 60);
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2") * 60);
        int max_add_time = Mathf.FloorToInt(GetParamValue("max_add_time2") * 60);
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);
        float dmg_rate = 1 - GetParamValue("defence2") / 100;


        // 变量
        int except_rowIndex = 0;
        int rowIndex = 0;
        int av_add_time_left = 0; // 还可以被用来延长的时间
        int timeLeft = 0;
        int totalTime = 0;
        FloatModifier DamageRateMod = new FloatModifier(dmg_rate);
        RingUI ru = null;
        CustomizationTask ruTask = null;
        // 被炸后加2秒的设定
        Action<CombatAction> bombAction = (combat) => {
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    int add = 0; // 初步确定要延长的时间
                    if (av_add_time_left <= add_time)
                    {
                        add = av_add_time_left;
                        av_add_time_left = 0;
                    }
                    else
                    {
                        add = add_time;
                        av_add_time_left -= add_time;
                    }

                    if (totalTime - timeLeft > add)
                        timeLeft += add;
                    else
                    {
                        add -= (totalTime - timeLeft);
                        av_add_time_left += add;
                        timeLeft = totalTime;
                    }
                }
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
                CustomizationTask task = new CustomizationTask();
                // 先瞬移
                task.AddTaskFunc(delegate {
                    animatorController.Play("PreMove0");
                    return true;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        FindS2RowIndex(out rowIndex);
                        except_rowIndex = rowIndex;
                        transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                        animatorController.Play("PostMove0");
                        transform.localScale = new Vector2(1, 1);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("PreDrop");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 加入读条UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // 添加被炸后加时设定
                        {
                            AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            // 获得减伤
                            NumericBox.DamageRate.AddModifier(DamageRateMod);
                        }
                        av_add_time_left = max_add_time;
                        timeLeft = wait0;
                        totalTime = wait0;
                        animatorController.Play("Drop", true);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    // 控制读条UI
                    {
                        ru.SetPercent(1 - (float)timeLeft/totalTime);
                    }

                    if (timeLeft <= 0)
                    {
                        // 移除读条UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // 移除被炸加时
                        RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        // 移除减伤
                        NumericBox.DamageRate.RemoveModifier(DamageRateMod);
                        animatorController.Play("PostDrop");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.55f)
                    {
                        //移除被动效果
                        RemoveP(); 
                        // 生成推进盾牌
                        CreateShield(transform.position + MapManager.gridWidth * Vector3.left, transform.position + 15 * MapManager.gridWidth * Vector3.left);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 加入读条UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // 添加被炸后加时设定
                        {
                            AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        }
                        av_add_time_left = max_add_time;
                        timeLeft = wait1;
                        totalTime = wait1;

                        animatorController.Play("Idle1", true);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    // 控制读条UI
                    {
                        ru.SetPercent(1 - (float)timeLeft / totalTime);
                    }
                    if (timeLeft <= 0)
                    {
                        // 移除读条UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // 移除被炸加时
                        RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        animatorController.Play("PreMove1");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        FindS2RowIndex2(except_rowIndex, out int rowIndex);
                        transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                        animatorController.Play("PostMove1");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 加入读条UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // 添加被炸后加时设定
                        {
                            AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        }

                        timeLeft = wait2;
                        totalTime = wait2;
                        animatorController.Play("Idle1", true);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    // 控制读条UI
                    {
                        ru.SetPercent(1 - (float)timeLeft / totalTime);
                    }

                    if (timeLeft <= 0)
                    {
                        // 移除读条UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // 移除被炸加时
                        RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        timeLeft = move_time;
                        // 生成回旋盾牌
                        CreateShield(transform.position + 15 * MapManager.gridWidth * Vector3.left, transform.position + MapManager.gridWidth * Vector3.left);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        animatorController.Play("Recieve");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        AddP(); // 添加被动效果
                        if(current_lost_hp_percent < 9999)
                            FinalSkillRingUI.Show(); // 重新显示UI
                        return true;
                    }
                    return false;
                });
                return task;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate{ };
        return c;
    }

    /// <summary>
    /// 扔出圆盾
    /// </summary>
    private void CreateShield(Vector2 start, Vector2 end)
    {
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2") * 60);

        BaseEffect e = BaseEffect.CreateInstance(ShieldBullet_Run, null, "Fly", null, true);
        e.SetSpriteRendererSorting("Effect", 10);
        e.transform.position = start;
        GameController.Instance.AddEffect(e);

        // 移动任务
        {
            // 死亡时自动销毁
            Action<BaseUnit> deathAction = delegate {
                e.ExecuteDeath();
            };
            

            CustomizationTask task;
            TaskManager.GetMoveToTask(e.transform, end, move_time, out task);
            task.AddOnEnterAction(delegate { AddBeforeDeathEvent(deathAction); });
            task.AddOnExitAction(delegate {
                RemoveBeforeDeathEvent(deathAction);
                e.ExecuteDeath();
            });
            e.taskController.AddTask(task);
        }

        // 格子判定检测
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(e.transform.position, new Vector2(0.1f * MapManager.gridWidth, 0.1f * MapManager.gridHeight), "CollideGrid");
            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction action = UnitManager.Execute(this, u);
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = e.transform.position;
                return !e.IsValid();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }

        // 老鼠判定检测
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(e.transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((u) => {
                if (u.IsBoss())
                    return;
                UnitManager.Execute(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = e.transform.position;
                return !e.IsValid();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
    }
    #endregion
}
