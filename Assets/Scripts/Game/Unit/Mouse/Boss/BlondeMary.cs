using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������
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
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.BlondeMary, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
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
        list.Add(SKill0Init(infoList[0]));
        list.Add(SKill2Init(infoList[2]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// �����ж�����
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
        // Ȼ�����ѡһ�а�
        if (rowList.Count > 0)
            R0 = rowList[GetRandomNext(0, rowList.Count)];
        else
            R0 = 3;
    }

    private RetangleAreaEffectExecution CreateCheckArea()
    {
        // ����һ�����ΰ�ը������
        //MouseUnit m = MouseManager.GetBombedToolMouse();
        //m.SetMaxHpAndCurrentHp(mMaxHp);
        //m.transform.position = transform.position;
        //{
        //    CustomizationTask t = new CustomizationTask();
        //    t.AddTaskFunc(delegate {
        //        if (this.IsAlive())
        //            m.transform.position = transform.position;
        //        else
        //            return true;
        //        return false;
        //    });
        //    t.AddOnExitAction(delegate {
        //        m.ExecuteDeath();
        //    });
        //    m.AddTask(t);
        //}

        // ����һ����̤�������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.1f, 0.5f, "EnemyAllyGrid");
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((u) => {
                if (u.IsBoss())
                    return;
                UnitManager.Execute(this, u);
            });

            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction action = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans0") / 100).ApplyAction();
                }, false);
            });
            GameController.Instance.AddAreaEffectExecution(r);
            return r;
        }
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
        // Ȼ�����ѡһ�а�
        if (rowList.Count > 0)
            R2 = rowList[GetRandomNext(0, rowList.Count)];
        else
            R2 = 3;
    }

    private BaseBullet CreateBullet()
    {
        int time = Mathf.FloorToInt(60 * GetParamValue("flyTime1", mHertIndex)); ; // һ�λ�����Ҫ��ʱ��
        float deltaAngle = 2 * Mathf.PI / time;
        float a1 = GetParamValue("a1", mHertIndex) * MapManager.gridWidth; // ����
        float b1 = GetParamValue("b1", mHertIndex) * MapManager.gridHeight; // ����
        float px = MapManager.GetColumnX(8);
        float py = MapManager.GetRowY(3);

        EnemyBullet b = EnemyBullet.GetInstance(Bullet_AnimatorController, this, 0);
        b.CloseCollision();

        // �����˶�
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


        // Я��һ���ж�����
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 0.1f, 0.1f, "EnemyAllyGrid");
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((u) => {
                if (u.IsBoss())
                    return;
                UnitManager.Execute(this, u);
            });

            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    DamageAction action = UnitManager.Execute(this, u);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans") / 100).ApplyAction();
                }, false);
                r.AddExcludeGrid(g); // ���ֻ��һ�Σ�
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
    /// ��̤
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int t0 = Mathf.FloorToInt(60*GetParamValue("t0", mHertIndex));
        int timeLeft = 0;
        float v = TransManager.TranToVelocity(6); // �ƶ��ٶ�

        bool isBombed = false;
        Action<CombatAction> act = (action) =>
        {
            if (action is DamageAction)
            {
                DamageAction dmgAction = action as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    isBombed = true;
                }
            };
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isBombed = false;
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ȥR0��C0��
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
                    // ͣһ���
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
                    // �����������һ�е�x����
                    float s = Mathf.Abs(transform.position.x - MapManager.GetColumnX(0));
                    timeLeft = Mathf.FloorToInt(s / v);
                    // ����
                    RetangleAreaEffectExecution r = CreateCheckArea();

                    CustomizationTask t = new CustomizationTask();
                    actionPointController.AddListener(ActionPointType.PostReceiveDamage, act);
                    t.AddTaskFunc(delegate {
                        r.transform.position = transform.position;
                        return isBombed;
                    });
                    t.AddOnExitAction(delegate {
                        actionPointController.RemoveListener(ActionPointType.PostReceiveDamage, act);
                        r.MDestory();
                    });
                    r.AddTask(t);

                    animatorController.Play("Move", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                transform.position += Vector3.left*v;
                if (timeLeft <= 0 || transform.position.x <= MapManager.GetColumnX(0) || isBombed)
                {
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
    /// ������
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        int t1 = Mathf.FloorToInt(60 * GetParamValue("t1", mHertIndex));
        int timeLeft = 0;
        BaseBullet bullet = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
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
                    // ͣһ���
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
                    // ��������
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
    /// ������
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int t2 = Mathf.FloorToInt(60 * GetParamValue("t2", mHertIndex));
        int stun2 = Mathf.FloorToInt(60 * GetParamValue("stun2", mHertIndex));
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
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
                    // ȥR0��C0��
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
                    // ͣһ���
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
                    // ͣһ���
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
