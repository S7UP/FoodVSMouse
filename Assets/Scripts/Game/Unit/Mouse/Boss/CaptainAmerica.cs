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
    private static RuntimeAnimatorController ShieldBulletAnimaotrController;
    private static RuntimeAnimatorController StrikeEffect_Run;
    private static RuntimeAnimatorController RainBow_Run;
    private static Sprite Wait_Icon_Sprite;
    private static Sprite Shield_Spr;

    private List<int>[] rowListArray;
    private List<int> avaliableIndexList = new List<int>();

    private Action<CombatAction> recordDamageAction;
    private float dmgRecord; // �ܵ����˺��ܺ�
    private bool canRecordDamage;

    private FloatModifier p_dmgRateMod = new FloatModifier(1.0f);
    private RetangleAreaEffectExecution ShieldArea;

    private RingUI FinalSkillRingUI;

    public override void Awake()
    {
        if (ShieldBulletAnimaotrController == null)
        {
            ShieldBulletAnimaotrController = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/ShieldBullet");
            StrikeEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/Strike");
            RainBow_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/16/RainBow");
            Shield_Spr = GameManager.Instance.GetSprite("Boss/16/Shield");
            Wait_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Wait");
        }
            
        base.Awake();
    }

    public override void MInit()
    {
        ShieldArea = null;
        rowListArray = null;
        avaliableIndexList.Clear();
        p_dmgRateMod = new FloatModifier(1.0f);
        dmgRecord = 0;
        base.MInit();
        // ����UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this));
            FinalSkillRingUI.SetPercent(1);
            FinalSkillRingUI.Hide();
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }
        // �ܻ��¼�
        {
            canRecordDamage = true;
            recordDamageAction = (combatAction) =>
            {
                if (!canRecordDamage)
                    return;
                if (combatAction is DamageAction)
                {
                    float triggerFinalSkillDamage = mMaxHp * GetParamValue("p_lost_hp_percent") / 100;
                    DamageAction dmgAction = combatAction as DamageAction;
                    dmgRecord += dmgAction.RealCauseValue;
                    FinalSkillRingUI.SetPercent(1-dmgRecord/ triggerFinalSkillDamage);
                    if (dmgRecord >= triggerFinalSkillDamage)
                    {
                        FinalSkillRingUI.Hide();
                        dmgRecord -= triggerFinalSkillDamage;
                        CustomizationSkillAbility s = SKill2Init(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[2]);
                        mSkillQueueAbilityManager.SetNextSkill(s);
                        canRecordDamage = false; // �ݲ���¼�˺�
                    }
                }
            };
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, recordDamageAction);
        }
        // ��ӱ���
        AddP();

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
        // ��ȡRset
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

        p_dmgRateMod = new FloatModifier(1 - GetParamValue("p_defence")/100);
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
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
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
                    e.SetSpriteRendererSorting(name, order);
                GameController.Instance.AddEffect(e);
                m.mEffectController.AddEffectToDict("MeiDui_Shield", e, Vector2.zero);
            }

        });
        r.SetOnEnemyExitAction((m) => {
            m.NumericBox.DamageRate.RemoveModifier(dmgRateMod);
            m.actionPointController.RemoveListener(ActionPointType.PreReceiveDamage, action);
            // �Ƴ��ӻ���Ч
            m.mEffectController.RemoveEffectFromDict("MeiDui_Shield");
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
    private void CreateS0Area()
    {
        float dmg_trans = GetParamValue("dmg_trans0") / 100;
        float dist = GetParamValue("dist0")*MapManager.gridWidth;
        float lost_hp_percent = GetParamValue("lost_hp_percent0")/100;
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time0") * 60);
        float shield_vale = GetParamValue("shield0");

        // ��������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*MapManager.gridWidth*Vector3.left, 1f, 0.5f, "CollideGrid");
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction action = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * dmg_trans).ApplyAction();
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ��������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + MapManager.gridWidth * Vector3.left, 2f, 0.5f, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.AddEnemyEnterConditionFunc((m) => {
                return !m.IsBoss() && MouseManager.IsGeneralMouse(m);
            });
            r.SetOnEnemyEnterAction((u) => 
            {
                u.taskController.AddTask(TaskManager.GetAccDecMoveTask(u.transform, dist*Vector2.left, 30));
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, lost_hp_percent * u.mCurrentHp).ApplyAction();
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false));
                // ��װ����
                new ShieldAction(CombatAction.ActionType.GiveShield, this, u, shield_vale).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ����Ҳ��λ�ƣ�
        {
            taskController.AddTask(TaskManager.GetAccDecMoveTask(transform, dist * Vector2.left, 30));
        }

    }

    /// <summary>
    /// Բ���Ļ����ܶ���
    /// </summary>
    private CustomizationSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col0", mHertIndex));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60);

        // ����
        List<int> rowIndexList = null;
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        // һЩ������ʼ��
        c.BeforeSpellFunc = delegate 
        {
            if (avaliableIndexList.Count <= 0)
                for (int i = 0; i < rowListArray.Length; i++)
                    avaliableIndexList.Add(i);

            // int ranIndex = GetRandomNext(0, avaliableIndexList.Count);
            int ranIndex = 0;
            int index = avaliableIndexList[ranIndex];
            avaliableIndexList.RemoveAt(ranIndex);
            rowIndexList = rowListArray[index];
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < rowIndexList.Count; _i++)
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
                            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndexList[i]);
                            animatorController.Play("PostMove0");
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
                                GameController.Instance.AddEffect(e);
                            }
                            // ��ǰ����������˺��Լ���������ƽ�Ч��
                            CreateS0Area();
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
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
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
    private List<int> GetS1SpawnRowIndexList(int rowIndex)
    {
        int num = Mathf.FloorToInt(GetParamValue("num1"));

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
    private void CreateS1EnemySpawner(Vector2 pos)
    {
        int type = Mathf.FloorToInt(GetParamValue("type1"));
        int shape = Mathf.FloorToInt(GetParamValue("shape1"));
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);

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
        int wait = Mathf.FloorToInt(GetParamValue("wait1")*60);

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
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("PreStand");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        timeLeft = wait;
                        // �ٻ���
                        foreach (var r in GetS1SpawnRowIndexList(rowIndex))
                            CreateS1EnemySpawner(MapManager.GetGridLocalPosition(colIndex, r));
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
                        return true;
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
        // ��ȡ����ΪĿ�����ʳ�����ļ���
        List<int> rowList = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
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
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);

        // ����
        int except_rowIndex = 0;
        int rowIndex = 0;
        int timeLeft = 0;
        int totalTime = 0;
        RingUI ru = null;
        CustomizationTask ruTask = null;
        // ��ը���2����趨
        Action<CombatAction> bombAction = (combat) => {
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    timeLeft = Mathf.Min(totalTime, timeLeft + add_time);
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
                        }

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
                        canRecordDamage = true; // ���¼�¼�˺�
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

        BaseEffect e = BaseEffect.CreateInstance(ShieldBulletAnimaotrController, null, "Fly", null, true);
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
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(e.transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "CollideGrid");
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
