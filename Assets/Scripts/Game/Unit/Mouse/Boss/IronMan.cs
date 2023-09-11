using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using static UnityEngine.Rendering.DebugUI.Table;

public class IronMan : BossUnit
{
    private List<int>[] rowListArray;
    private List<int> avaliableIndexList = new List<int>();

    private Action<CombatAction> recordDamageAction;
    private float dmgRecord; // �ܵ����˺��ܺ�
    private bool canRecordDamage;

    private RingUI FinalSkillRingUI;

    private static RuntimeAnimatorController LaserHitEffect_Run;
    private static RuntimeAnimatorController Missile_Run;
    private static Sprite Laser_Spr;
    private static RuntimeAnimatorController HeavyArmor_Run;

    private FloatModifier p_dmgRateMod = new FloatModifier(1.0f);

    public override void Awake()
    {
        if(LaserHitEffect_Run == null)
        {
            LaserHitEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/LaserHitEffect");
            Missile_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/Missile");
            Laser_Spr = GameManager.Instance.GetSprite("Boss/17/Laser");
            HeavyArmor_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/17/0/HeavyArmor");
        }
        base.Awake();
    }

    public override void MInit()
    {
        dmgRecord = 0;
        rowListArray = null;
        avaliableIndexList.Clear();
        p_dmgRateMod.Value = 1.0f;
        base.MInit();
        // ����UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this));
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(1);
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
                    FinalSkillRingUI.SetPercent(1 - dmgRecord / triggerFinalSkillDamage);
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

        // ��ӳ��ֵļ���
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
                    // ��ӱ�������
                    NumericBox.DamageRate.AddModifier(p_dmgRateMod);

                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                    return true;
                }
            });
            // ǿ�����Ƶ�ǰ����Ϊ���
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
    }

    public override void MDestory()
    {
        base.MDestory();
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.IronMan, 0))
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

        // ��������
        {
            p_dmgRateMod.Value = 1 - GetParamValue("p_defence")/100;
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
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
    }

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////

    #region һ����
    /// <summary>
    /// ������ص��˺��ж�
    /// </summary>
    private void CreateS0LandDamageArea(Vector2 pos)
    {
        float dmg_trans = GetParamValue("dmg_trans0") / 100;
        // �Ը���
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "CollideGrid");
            r.transform.position = pos;
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.SetAffectHeight(0);
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction dmgAction = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, dmgAction.RealCauseValue * dmg_trans).ApplyAction();
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
            r.transform.position = pos;
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetOnEnemyEnterAction((u) => {
                if (!u.IsBoss())
                    UnitManager.Execute(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private void CreateS0Mouse(Vector2 pos)
    {
        int type = Mathf.FloorToInt(GetParamValue("type0"));
        int shape = Mathf.FloorToInt(GetParamValue("shape0"));
        int totalTime = 60;
        float fly_height = 3*MapManager.gridHeight;

        MouseUnit m = GameController.Instance.mMouseFactory.GetMouse(type, shape);
        m.transform.position = pos;
        GameController.Instance.AddMouseUnit(m);

        // ���ֶ���
        {
            Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            FloatModifier yOffsetMod = new FloatModifier(0);

            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                // ���ɱ����С����ɱ�ѡΪ����Ŀ�꣬�����赲
                m.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                m.AddCanHitFunc(noHitFunc);
                m.AddCanBlockFunc(noBeSelectedAsTargetFunc);
                m.SetAlpha(0);
                m.DisableMove(true);
            });
            task.AddTimeTaskFunc(totalTime, null,
            (leftTime, totalTime) =>
            {
                float rate = 1 - (float)leftTime / totalTime;
                m.SetAlpha(rate);
                m.RemoveSpriteOffsetY(yOffsetMod);
                yOffsetMod.Value = fly_height * (1 - Mathf.Sqrt(rate));
                // yOffsetMod.Value = fly_height * rate;
                m.AddSpriteOffsetY(yOffsetMod);
            },
            delegate
            {
            // ���ɱ����С����ɱ�ѡΪ����Ŀ�꣬�����赲
            m.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                m.RemoveCanHitFunc(noHitFunc);
                m.RemoveCanBlockFunc(noBeSelectedAsTargetFunc);
                m.SetAlpha(1);
                m.RemoveSpriteOffsetY(yOffsetMod);
                m.DisableMove(false);
            });
            m.taskController.AddTask(task);
        }
    }

    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        float min_colIndex = 9 - GetParamValue("right_col0");
        int wait = Mathf.FloorToInt(GetParamValue("wait0") * 60);

        int fly_time = 30;
        float fly_height = 10*MapManager.gridHeight;

        // ����
        List<int> rowIndexList = null;
        int timeLeft = 0;
        FloatModifier yOffsetMod = new FloatModifier(0);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            if (avaliableIndexList.Count <= 0)
                for (int i = 0; i < rowListArray.Length; i++)
                    avaliableIndexList.Add(i);

            int ranIndex = 0; // �̶���0
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
                    task.AddTaskFunc(delegate {
                        animatorController.Play("PreFly");
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Fly1", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTimeTaskFunc(fly_time, null, 
                        (leftTime, totalTime) => 
                        {
                            RemoveSpriteOffsetY(yOffsetMod);
                            float rate = 1 - (float)leftTime / totalTime;
                            yOffsetMod.Value = fly_height * rate;
                            AddSpriteOffsetY(yOffsetMod);
                        }, null);
                    task.AddTimeTaskFunc(fly_time, 
                        delegate {
                            int rowIndex = rowIndexList[i];
                            float colIndex = min_colIndex;
                            {
                                BaseUnit u = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, false);
                                if (u != null && u.transform.position.x > MapManager.GetColumnX(colIndex))
                                    colIndex = MapManager.GetXIndexF(u.transform.position.x);
                            }
                            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                            animatorController.Play("Down", true);
                            // �ٻ�����
                            CreateS0Mouse(transform.position);
                        }, 
                        (leftTime, totalTime) => 
                        {
                            RemoveSpriteOffsetY(yOffsetMod);
                            float rate = (float)leftTime / totalTime;
                            yOffsetMod.Value = fly_height * rate;
                            AddSpriteOffsetY(yOffsetMod);
                        }, 
                        delegate { 
                            RemoveSpriteOffsetY(yOffsetMod);
                            animatorController.Play("Land");
                            // ��غ���ɷ�Χ�˺�
                            CreateS0LandDamageArea(transform.position);
                        });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            timeLeft = wait;
                            animatorController.Play("Idle", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            return true;
                        }
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
    private MouseModel CreateS1HeavyArmor(Vector2 pos)
    {
        FloatModifier dmgRateMod = new FloatModifier(1 - GetParamValue("defence1")/100);

        MouseModel m = MouseModel.GetInstance(HeavyArmor_Run);
        m.transform.position = pos;
        m.SetBaseAttribute(mMaxHp, 10, 1.0f, 1.0f, 0, 0.5f, 0);
        m.NumericBox.DamageRate.AddModifier(dmgRateMod);
        // �ۺ��˺����ݸ�����
        m.actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combat) => 
        { 
            if(combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, this);
                d.DamageValue = dmgAction.RealCauseValue;
                if (d.IsDamageType(DamageAction.DamageType.BombBurn))
                    d.DamageValue /= m.mBurnRate;
                d.ApplyAction();
            }
        });
        m.SetGetCurrentHpFunc(delegate { return mCurrentHp; });
        m.SetGetBurnRateFunc(delegate { return mBurnRate; });
        m.isBoss = true;
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBlockFunc(delegate { return false; });
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        m.AttackClipName = "AttackLeftUp";

        m.SetActionState(new IdleState(this));
        GameController.Instance.AddMouseUnit(m);

        return m;
    }

    private Vector2 GetS1AttackPos()
    {
        float min_hp = GetParamValue("min_hp1");

        List<BaseUnit> l1 = GameController.Instance.GetEachAlly();
        if (l1.Count <= 0)
            return new Vector2(MapManager.GetColumnX(0), transform.position.y);

        float min_dist = float.MaxValue;
        BaseUnit target = null;

        foreach (var u in l1)
        {
            if (u.GetCurrentHp() >= min_hp && !(u is CharacterUnit))
            {
                float dist = (transform.position - u.transform.position).magnitude;
                if(dist < min_dist)
                {
                    target = u;
                    min_dist = dist;
                }
            }
        }

        if(target != null)
        {
            return target.transform.position;
        }

        foreach (var u in l1)
        {
            if (!(u is CharacterUnit))
            {
                float dist = (transform.position - u.transform.position).magnitude;
                if (dist < min_dist)
                {
                    target = u;
                    min_dist = dist;
                }
            }
        }
        return target.transform.position;
    }

    private void CreateS1LaserAndHit(Vector2 start, Vector2 end)
    {
        float dist = (end - start).magnitude;
        Vector2 rot = (end - start).normalized;
        // ����������Ч
        {
            BaseLaser l = BaseLaser.GetAllyInstance(null, 0, start, rot, null, Laser_Spr, null, null, null, null, null, null);
            l.mMaxLength = dist;
            l.mCurrentLength = dist;
            l.mVelocity = TransManager.TranToVelocity(108);
            l.isCollide = false;
            l.laserRenderer.SetSortingLayerName("Effect");

            int timeLeft = 20;
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate 
            {
                l.laserRenderer.SetVerticalOpenTime(-20);
                timeLeft = 20;
            });
            t.AddTaskFunc(delegate
            {
                timeLeft--;
                if (timeLeft <= 0)
                    return true;
                return false;
            });
            t.AddOnExitAction(delegate
            {
                l.ExecuteRecycle();
            });
            l.AddTask(t);

            GameController.Instance.AddLaser(l);
        }

        // ����������Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(LaserHitEffect_Run, null, "Disappear", null, false);
            e.SetSpriteRendererSorting("Effect", 10);
            e.transform.position = end;
            GameController.Instance.AddEffect(e);
        }

        // �˺��ж�
        {
            Action<BaseUnit> action = (u) => 
            {
                BurnManager.BurnDamage(this, u);
            };

            // ����ʳ��λ
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(end, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
                r.isAffectFood = true;
                r.SetOnFoodEnterAction(action);
                r.SetInstantaneous();
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // ������λ
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(end, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction(action);
                r.SetInstantaneous();
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        float colIndex = 9 - GetParamValue("right_col1");
        int move_time = Mathf.FloorToInt(GetParamValue("move_time1")*60);
        FloatModifier dmgRateMod = new FloatModifier(1-GetParamValue("defence1")/100);
        int num = Mathf.FloorToInt(GetParamValue("num1"));
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time1")*60);

        // ����
        int timeLeft = 0;
        MouseModel model = null;
        Vector3 laserStartPos = Vector2.zero;
        Vector3 laserEndPos = Vector2.zero;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

        };
        {
            // ׼�����
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("PreFly1");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Fly2");
                        return true;
                    }
                    return false;
                });
                return task;
            });
            // �ƶ����ض���
            c.AddCreateTaskFunc(delegate
            {
                Vector2 startPos = Vector2.zero;
                Vector2 endPos = Vector2.zero;

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    startPos = transform.position;
                    List<int> list = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                    int rowIndex = list[GetRandomNext(0, list.Count)];
                    endPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                });
                task.AddTimeTaskFunc(move_time, 
                    delegate { mHeight = 1; },
                    (leftTime, totalTime) => 
                    {
                        float rate = 1 - (float)leftTime / totalTime;
                        transform.position = Vector2.Lerp(startPos, endPos, rate);
                    },
                    delegate 
                    {
                        mHeight = 0;
                        transform.position = endPos;
                        animatorController.Play("Land");
                    }
                    );
                task.AddTaskFunc(delegate {
                    if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Breakup");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // ����������ȡ���ж���Ȼ������һ������װ�״�������
                        SetAlpha(0);
                        CloseCollision();
                        model = CreateS1HeavyArmor(transform.position);
                        model.animatorController.Play("Appear");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if(!model.IsAlive() || model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                return task;
            });
            // ���⹥��
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < num; _i++)
                {
                    int i = _i;
                    task.AddTaskFunc(delegate {
                        model.animatorController.Play("Idle");
                        timeLeft = wait;
                        return true;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        if(timeLeft <= 0)
                        {
                            // Ѱ�ҹ���Ŀ�겢����
                            Vector3 attackPos = GetS1AttackPos(); // ������
                            Vector3 delta = attackPos - transform.position;
                            if(attackPos.x <= 0 || attackPos.y >= 0)
                            {
                                laserStartPos = transform.position + new Vector3(-1.038f, 0.917f);
                                model.animatorController.Play("AttackLeftUp");
                            }else if(attackPos.x <= 0 || attackPos.y < 0)
                            {
                                laserStartPos = transform.position + new Vector3(-0.645f, 0.076f);
                                model.animatorController.Play("AttackLeftDown");
                            }else if(attackPos.x > 0 || attackPos.y < 0)
                            {
                                laserStartPos = transform.position + new Vector3(1.148f, 0.141f);
                                model.animatorController.Play("AttackRightDown");
                            }
                            else
                            {
                                laserStartPos = transform.position + new Vector3(1.804f, 0.984f);
                                model.animatorController.Play("AttackRightUp");
                            }
                            laserEndPos = attackPos;
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (model.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.5f)
                        {
                            CreateS1LaserAndHit(laserStartPos, laserEndPos);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            return true;
                        }
                        return false;
                    });
                }
                return task;
            });
            // �����Ա�
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    model.animatorController.Play("Disappear");
                });
                task.AddTaskFunc(delegate {
                    if (model.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        model.MDestory();
                        // ������ʾ�ҿ����ж�
                        SetAlpha(1);
                        OpenCollision();
                        animatorController.Play("Appear");
                        // ȫ����ѣ
                        {
                            Action<BaseUnit> action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false)); };

                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13*MapManager.gridWidth, 9*MapManager.gridHeight), "BothCollide");
                            r.SetInstantaneous();
                            r.isAffectFood = true;
                            r.isAffectMouse = true;
                            r.SetOnFoodEnterAction(action);
                            r.SetOnEnemyEnterAction(action);
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
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


    private Vector2 GetS2AttackPos(int colIndex)
    {
        int[] valArray = new int[5];
        Vector2[] pArray = new Vector2[5];
        for (int i = 1; i <= 5; i++)
            pArray[i-1] = MapManager.GetGridLocalPosition(colIndex, i);

        foreach (var u in GameController.Instance.GetEachAlly())
        {
            if(u is FoodUnit)
            {
                FoodUnit f = u as FoodUnit;
                if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f))
                {
                    for (int i = 0; i < pArray.Length; i++)
                    {
                        Vector2 p = pArray[i];
                        if(Mathf.Abs(f.transform.position.x - p.x)<1.15f*MapManager.gridWidth && Mathf.Abs(f.transform.position.y - p.y) < 1.15f * MapManager.gridHeight)
                            valArray[i]++;
                    }
                }
            }
        }

        int max = int.MinValue;
        List<Vector2> posList = new List<Vector2>();
        for (int i = 0; i < valArray.Length; i++)
        {
            if(valArray[i] > max)
            {
                max = valArray[i];
                posList.Clear();
                posList.Add(pArray[i]);
            }
            else if(valArray[i] == max)
            {
                posList.Add(pArray[i]);
            }
        }

        return posList[GetRandomNext(0, posList.Count)];
    }

    /// <summary>
    /// ����һ������
    /// </summary>
    private void CreateS2Missile(Vector2 endPos)
    {
        int t = 120;
        float deltaX = 0.238f, deltaY = 2.061f;

        EnemyBullet b = EnemyBullet.GetInstance(Missile_Run, this, 0);
        b.isnDelOutOfBound = true;
        b.AddHitAction((b, u) => {
            // ը����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 2.25f, 2.25f, "CollideGrid");
                r.isAffectGrid = true;
                r.SetInstantaneous();
                r.SetOnGridEnterAction((g) => {
                    g.TakeAction(this, (u) => {
                        BurnManager.BurnDamage(this, u);
                    }, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // ը����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 3, 3, "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetInstantaneous();
                r.SetOnEnemyEnterAction((u) => {
                    BurnManager.BurnDamage(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        });
        
        Vector2 startPos = transform.position + new Vector3(deltaX, deltaY);
        float dist = (startPos - endPos).magnitude;
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, dist/t, 5*MapManager.gridHeight, startPos, endPos, true));
        GameController.Instance.AddBullet(b);
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int num = Mathf.FloorToInt(GetParamValue("num2"));
        int move_time = Mathf.FloorToInt(GetParamValue("move_time2")*60);
        float stand_colIndex = 9 - GetParamValue("right_col2");
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        int stun_time = Mathf.FloorToInt(GetParamValue("stun2") * 60);
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);

        // ����
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        Vector2 attackPos = Vector2.zero;
        // ��ը���2����趨
        Action<CombatAction> bombAction = (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    timeLeft = Mathf.Min(wait0, timeLeft + add_time);
            }
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {};
        {
            // ��������ѭ��
            c.AddCreateTaskFunc(delegate {
                RingUI ru = null;
                CustomizationTask ruTask = null;

                CustomizationTask task = new CustomizationTask();
                for (int _i = 0; _i < num; _i++)
                {
                    int i = _i;
                    int colIndex = Mathf.FloorToInt(GetParamValue("target_left_col2", i))-1;
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("PreFly1");
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Fly2");
                            attackPos = GetS2AttackPos(colIndex);
                            startPos = transform.position;
                            endPos = new Vector2(MapManager.GetColumnX(stand_colIndex), attackPos.y);
                            return true;
                        }
                        return false;
                    });
                    task.AddTimeTaskFunc(move_time, 
                    delegate { mHeight = 1; },
                    (leftTime, totalTime) => 
                    {
                        float rate = 1 - (float)leftTime / totalTime;
                        transform.position = Vector2.Lerp(startPos, endPos, rate);
                    },
                    delegate 
                    {
                        mHeight = 0;
                        transform.position = endPos;
                        animatorController.Play("Land");
                    }
                    );
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Idle");
                            timeLeft = wait0;
                            // �������UI
                            {
                                ru = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru);
                                ruTask = TaskManager.GetWaitRingUITask(ru, this, 0.25f * MapManager.gridHeight * Vector3.down);
                                ru.mTaskController.AddTask(ruTask);
                            }
                            // ��ӱ�ը���ʱ�趨
                            {
                                AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            }

                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        // ���ƶ���UI
                        {
                            ru.SetPercent(1 - (float)timeLeft / wait0);
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
                            animatorController.Play("Attack");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>=0.65f)
                        {
                            // ��������
                            CreateS2Missile(attackPos);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Idle");
                            timeLeft = wait1;
                            // �������UI
                            {
                                ru = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru);
                                ruTask = TaskManager.GetWaitRingUITask(ru, this, 0.25f * MapManager.gridHeight * Vector3.down);
                                ru.mTaskController.AddTask(ruTask);
                            }
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        // ���ƶ���UI
                        {
                            ru.SetPercent(1 - (float)timeLeft / wait1);
                        }

                        if (timeLeft <= 0)
                        {
                            // �Ƴ�����UI
                            {
                                ru.mTaskController.RemoveTask(ruTask);
                                ru = null;
                            }
                            return true;
                        }
                        return false;
                    });
                }

                return task;
            });

            // ̱������ѣ
            c.AddCreateTaskFunc(delegate {
                float lost_hp_val = GetParamValue("lost_hp_percent2")/100 * mMaxHp;
                float hpLeft = lost_hp_val;

                RingUI ru_time = null;
                CustomizationTask ru_timeTask = null;

                RingUI ru_hp = null;
                CustomizationTask ru_hpTask = null;

                Action<CombatAction> hitAction = (combatAction) =>
                {
                    if (combatAction is DamageAction)
                    {
                        DamageAction dmgAction = combatAction as DamageAction;
                        hpLeft -= dmgAction.RealCauseValue;
                    }
                };

                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Die");
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // ������ѣ����
                        animatorController.Play("Stun", true);
                        // �Ƴ��ӷ�
                        NumericBox.DamageRate.RemoveModifier(p_dmgRateMod);
                        timeLeft = stun_time;
                        // �������UI
                        {
                            // ʱ����
                            {
                                ru_time = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru_time);
                                ru_timeTask = TaskManager.GetWaitRingUITask(ru_time, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.left);
                                ru_time.mTaskController.AddTask(ru_timeTask);
                            }
                            // ʣ��������
                            {
                                ru_hp = RingUI.GetInstance(0.3f * Vector2.one);
                                GameNormalPanel.Instance.AddUI(ru_hp);
                                ru_hpTask = TaskManager.GetStunRingUITask(ru_hp, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.right);
                                ru_hp.mTaskController.AddTask(ru_hpTask);
                            }
                        }
                        // ��ʼ������ڼ��ܻ��˺�
                        actionPointController.AddListener(ActionPointType.PostReceiveDamage, hitAction);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    // ���ƶ���UI
                    {
                        ru_time.SetPercent((float)timeLeft / stun_time);
                        ru_hp.SetPercent((float)hpLeft / lost_hp_val);
                    }
                    if (timeLeft <= 0 || hpLeft <= 0)
                    {
                        // �Ƴ�����UI
                        {
                            ru_time.mTaskController.RemoveTask(ru_timeTask);
                            ru_time = null;
                            ru_hp.mTaskController.RemoveTask(ru_hpTask);
                            ru_hp = null;
                        }
                        animatorController.Play("Appear");
                        // �Ƴ�������ڼ���ܻ��˺�
                        actionPointController.RemoveListener(ActionPointType.PostReceiveDamage, hitAction);
                        canRecordDamage = true; // ���¼�¼�˺�
                        FinalSkillRingUI.Show(); // ������ʾUI
                        NumericBox.DamageRate.AddModifier(p_dmgRateMod); // ���¼ӷ�
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle", true);
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
}
