using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class CaptainAmerica : BossUnit
{
    private static Sprite Wait_Icon_Sprite;
    private static Sprite AttackLeft_Icon_Sprite;

    private RuntimeAnimatorController ShieldBullet_Run;
    private RuntimeAnimatorController StrikeEffect_Run;
    private RuntimeAnimatorController RainBow_Run;
    private Sprite Shield_Spr;

    private float dmgRecord; // �ܵ����˺��ܺ�
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
        // Ĭ��Ƥ��
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

        // ����UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this, 0.25f * MapManager.gridHeight * Vector3.down));
            FinalSkillRingUI.SetPercent(0);
            FinalSkillRingUI.Hide();
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // ����������ʾUI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            // ��Ӱ�����
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
        // �ܻ��¼�
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

        // ��ӳ��ֵļ���
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
                    // ���Ƴ�һ�α������ˣ����մ�ʩ��
                    RemoveP();
                    // ��ӱ�������
                    AddP();
                    return true;
                }
            });
            // ǿ�����Ƶ�ǰ����Ϊ���
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.CaptainAmerica, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // ���������ʼ��
        // ��ȡ������ʧ����ֵ����
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
        // ��������
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
        // ���ݴ����skin��Ƥ��0ΪԭƤ��1Ϊ���죩
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
    /// ���ؼ���
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
    /// �����ж�����
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

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////


    /// <summary>
    /// ��ӱ�������
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
            // ��ӱӻ���Ч
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
            // �Ƴ��ӻ���Ч
            // m.mEffectController.RemoveEffectFromGroup("MeiDui_Shield" + skin, e);
            m.mEffectController.RemoveEffectFromDict("MeiDui_Shield" + skin);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        // ������
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
    /// �Ƴ���������
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

    #region һ����
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
        // ��������
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

        // ��������
        RetangleAreaEffectExecution push_area = CreateS0PushArea(isLeft, stun_time, min_x, max_x);

        // ����Ҳ��λ�ƣ�
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
    /// Բ���Ļ����ܶ���
    /// </summary>
    private CustomizationSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
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

        // ����
        List<int> rowIndexList = new List<int>();
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        // һЩ������ʼ��
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
                    // ��˲��
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
                            // ��Ч
                            {
                                BaseEffect e = BaseEffect.CreateInstance(StrikeEffect_Run, null, "StrikeEffect", null, false);
                                e.transform.position = transform.position;
                                if (!isLeft)
                                    e.transform.localScale = new Vector2(-1, 1);
                                GameController.Instance.AddEffect(e);
                            }
                            // ��ǰ����������˺��Լ���������ƽ�Ч��
                            CreateS0Area(dmg_trans, dmg, dist, isLeft, stun_time, min_x, max_x);
                            // ����UI+1
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

    #region ������
    private int FindR1()
    {
        int rowIndex = 3; // Ĭ��ֵ
        List<int> rowList = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        List<int> list = new List<int>();
        int min = int.MaxValue;
        foreach (var index in rowList)
        {
            int count = 0;
            foreach (var unit in GameController.Instance.GetSpecificRowEnemyList(index))
            {
                // ���Ŀ���ڴ���������ж���Ϊ�棬�����+1
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
    /// ��ȡ�ٻ����б�
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
    /// ����һ������������
    /// </summary>
    private void CreateS1EnemySpawner(int type, int shape, int wait, Vector2 pos)
    {
        CustomizationItem item = CustomizationItem.GetInstance(pos, RainBow_Run);

        // �ٻ����˵�����
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
    /// �ٻ�ʿ��
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col1"));
        float start_summon_xIndex = 9 - Mathf.FloorToInt(GetParamValue("summon_right_col1_0"));
        float end_summon_xIndex = 9 - Mathf.FloorToInt(GetParamValue("summon_right_col1_1"));
        int num_y = Mathf.FloorToInt(GetParamValue("num1_0"));
        int num_x = Mathf.FloorToInt(GetParamValue("num1_1"));

        int type = Mathf.FloorToInt(GetParamValue("type1"));
        int shape = Mathf.FloorToInt(GetParamValue("shape1"));
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);

        // ����
        int rowIndex = 0;
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                // ��˲��
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
                        RemoveP(); // ʧȥ����
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        timeLeft = wait;
                        // �ٻ���
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
                        AddP(); // �ػ����
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

    #region ������
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
        // ��ȡ����ΪĿ�����ʳ�����ٵļ���
        List<int> rowList = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
        // �������ȡһ��
        if (rowList.Count > 0)
            rowIndex = rowList[GetRandomNext(0, rowList.Count)];
        else
            GetRandomNext(0, 2);
    }

    private void FindS2RowIndex2(int except_rowIndex, out int rowIndex)
    {
        rowIndex = 3;
        // ��ȡ����ΪĿ�����ʳ�����ļ���
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
        // �������ȡһ��
        if (rowList.Count > 0)
            rowIndex = rowList[GetRandomNext(0, rowList.Count)];
        else
            GetRandomNext(0, 2);
    }

    /// <summary>
    /// ����Բ��
    /// </summary>
    /// <param name="info"></param>
    private CustomizationSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col2"));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        int wait2 = Mathf.FloorToInt(GetParamValue("wait2_2") * 60);
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2") * 60);
        int max_add_time = Mathf.FloorToInt(GetParamValue("max_add_time2") * 60);
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);
        float dmg_rate = 1 - GetParamValue("defence2") / 100;


        // ����
        int except_rowIndex = 0;
        int rowIndex = 0;
        int av_add_time_left = 0; // �����Ա������ӳ���ʱ��
        int timeLeft = 0;
        int totalTime = 0;
        FloatModifier DamageRateMod = new FloatModifier(dmg_rate);
        RingUI ru = null;
        CustomizationTask ruTask = null;
        // ��ը���2����趨
        Action<CombatAction> bombAction = (combat) => {
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    int add = 0; // ����ȷ��Ҫ�ӳ���ʱ��
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
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                // ��˲��
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
                        // �������UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // ��ӱ�ը���ʱ�趨
                        {
                            AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            // ��ü���
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
                    // ���ƶ���UI
                    {
                        ru.SetPercent(1 - (float)timeLeft/totalTime);
                    }

                    if (timeLeft <= 0)
                    {
                        // �Ƴ�����UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // �Ƴ���ը��ʱ
                        RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        // �Ƴ�����
                        NumericBox.DamageRate.RemoveModifier(DamageRateMod);
                        animatorController.Play("PostDrop");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.55f)
                    {
                        //�Ƴ�����Ч��
                        RemoveP(); 
                        // �����ƽ�����
                        CreateShield(transform.position + MapManager.gridWidth * Vector3.left, transform.position + 15 * MapManager.gridWidth * Vector3.left);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // �������UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // ��ӱ�ը���ʱ�趨
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
                    // ���ƶ���UI
                    {
                        ru.SetPercent(1 - (float)timeLeft / totalTime);
                    }
                    if (timeLeft <= 0)
                    {
                        // �Ƴ�����UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // �Ƴ���ը��ʱ
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
                        // �������UI
                        {
                            ru = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(ru);
                            ruTask = GetS2WaitRingUITask(ru);
                            ru.mTaskController.AddTask(ruTask);
                        }
                        // ��ӱ�ը���ʱ�趨
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
                    // ���ƶ���UI
                    {
                        ru.SetPercent(1 - (float)timeLeft / totalTime);
                    }

                    if (timeLeft <= 0)
                    {
                        // �Ƴ�����UI
                        {
                            ru.mTaskController.RemoveTask(ruTask);
                            ru = null;
                        }
                        // �Ƴ���ը��ʱ
                        RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                        timeLeft = move_time;
                        // ���ɻ�������
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
                        AddP(); // ��ӱ���Ч��
                        if(current_lost_hp_percent < 9999)
                            FinalSkillRingUI.Show(); // ������ʾUI
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
    /// �ӳ�Բ��
    /// </summary>
    private void CreateShield(Vector2 start, Vector2 end)
    {
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2") * 60);

        BaseEffect e = BaseEffect.CreateInstance(ShieldBullet_Run, null, "Fly", null, true);
        e.SetSpriteRendererSorting("Effect", 10);
        e.transform.position = start;
        GameController.Instance.AddEffect(e);

        // �ƶ�����
        {
            // ����ʱ�Զ�����
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

        // �����ж����
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

        // �����ж����
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
