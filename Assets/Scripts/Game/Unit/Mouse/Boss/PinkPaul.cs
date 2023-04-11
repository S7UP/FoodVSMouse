using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �ۺ챣��
/// </summary>
public class PinkPaul : BossUnit
{
    private static RuntimeAnimatorController Bullet_AnimatorController; 
    private static RuntimeAnimatorController Tentacle_AnimatorController;
    private const string PinkPaul_Bullet = "PinkPaul_Bullet";

    private BaseUnit[] TentacleList = new BaseUnit[7];

    public override void Awake()
    {
        if (Bullet_AnimatorController == null)
        {
            Bullet_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/5/Bullet");
            Tentacle_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/5/Tentacle");
        }
        base.Awake();
    }

    public override void MInit()
    {
        for (int i = 0; i < TentacleList.Length; i++)
        {
            TentacleList[i] = null;
        }
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
                // ��һ֡ǿ�ưѺ�����ͬ�����Ҷ���
                transform.position = new Vector2(MapManager.GetColumnX(7), transform.position.y);
                return true;
            });
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

    public override void MUpdate()
    {
        for (int i = 0; i < TentacleList.Length; i++)
        {
            if (TentacleList[i] != null && !TentacleList[i].IsAlive())
                TentacleList[i] = null;
        }
        base.MUpdate();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        for (int i = 0; i < TentacleList.Length; i++)
        {
            if (TentacleList[i] != null && TentacleList[i].IsAlive())
                TentacleList[i].ExecuteDeath();
        }
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    public override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.PinkPaul, 0))
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

        if(mHertIndex < 2)
        {
            list.Add(SKill0Init(infoList[0]));
            list.Add(SKill1Init(infoList[1]));
            list.Add(SKill2Init(infoList[2]));
        }
        else
        {
            mSkillQueueAbilityManager.SetNextSkillIndex(mSkillQueueAbilityManager.GetCurrentSkillIndex()+1);
            list.Add(SKill1Init(infoList[1]));
            list.Add(SKill0Init(infoList[0]));
            list.Add(SKill1Init(infoList[1]));
            list.Add(SKill2Init(infoList[2]));
        }
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

    private int FindR0Row()
    {
        // ������ȼ�����ˮ·��Ҫ��ûˮ·�Ż���½����
        List<BaseGrid> gList1 = new List<BaseGrid>();
        bool hasWater = false;
        for (int row = 0; row < 7; row++)
        {
            BaseGrid g = GameController.Instance.mMapController.GetGrid(7, row);
            if (g != null)
            {
                if (!hasWater)
                {
                    if (g.IsContainGridType(GridType.Water))
                    {
                        gList1.Clear();
                        hasWater = true;
                        gList1.Add(g);
                    }
                    else
                        gList1.Add(g);
                }
                else if (g.IsContainGridType(GridType.Water))
                    gList1.Add(g);
            }
        }
        // �ҳ����Ҳ࿨Ƭ������
        List<BaseGrid> gList2 = new List<BaseGrid>();
        float min = float.MaxValue;
        foreach (var g in gList1)
        {
            // �����max�ǻ�ȡg���������Ҳ࿨Ƭ��x����
            float max = float.MinValue;
            foreach (var unit in GameController.Instance.GetSpecificRowAllyList(g.GetRowIndex()))
            {
                // ֻ�ܴ������������Ϳ�Ƭ��ѡ
                if (unit is FoodUnit)
                {
                    FoodUnit f = unit as FoodUnit;
                    if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, unit) && unit.transform.position.x > max)
                        max = unit.transform.position.x;
                }
            }

            if(max < min)
            {
                min = max;
                gList2.Clear();
                gList2.Add(g);
            }else if(max == min)
                gList2.Add(g);
        }
        // ����Щ���������һ�а�
        if(gList2.Count > 0)
            return gList2[GetRandomNext(0, gList2.Count)].GetRowIndex();
        else
        {
            // �����Ǳ��մ�����û�к��ʵĽ���Ļ��Ǿ͹̶���·���ְ�
            GetRandomNext(0, 1);
            return 3;
        }
    }

    private BaseUnit FindS0Target(int row)
    {
        row = Mathf.Max(0, Mathf.Min(row, 6));
        float max = float.MinValue;
        BaseUnit target = null;
        foreach (var unit in GameController.Instance.GetSpecificRowAllyList(row))
        {
            if(unit is FoodUnit)
            {
                FoodUnit f = unit as FoodUnit;
                if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, unit) && unit.transform.position.x > max)
                {
                    max = unit.transform.position.x;
                    target = unit;
                }
            }
        }
        return target;
    }

    private void FindR2C2(out int r2, out int c2)
    {
        // ������ȼ�����ˮ·��Ҫ��ûˮ·�Ż���½����
        List<BaseGrid> gList1 = new List<BaseGrid>();
        bool hasWater = false;
        for (int row = 0; row < 7; row++)
        {
            BaseGrid g = GameController.Instance.mMapController.GetGrid(7, row);
            if (g != null)
            {
                if (!hasWater)
                {
                    if (g.IsContainGridType(GridType.Water))
                    {
                        gList1.Clear();
                        hasWater = true;
                        gList1.Add(g);
                    }
                    else
                        gList1.Add(g);
                }
                else if (g.IsContainGridType(GridType.Water))
                    gList1.Add(g);
            }
        }
        // Ȼ���������ұ߿�����
        List<BaseGrid> gList2 = new List<BaseGrid>();
        float max = float.MinValue;
        foreach (var g in gList1)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(g.GetRowIndex(), float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
            {
                if (unit.transform.position.x > max)
                {
                    max = unit.transform.position.x;
                    gList2.Clear();
                    gList2.Add(g);
                }
                else if(unit.transform.position.x == max)
                {
                    gList2.Add(g);
                }
            }
        }
        // ����Щ���������һ�а�
        if (gList2.Count > 0)
        {
            BaseGrid g = gList2[GetRandomNext(0, gList2.Count)];
            r2 = g.GetRowIndex();
            c2 = Mathf.Max(1, Mathf.Min(5, FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(r2, float.MinValue, MapManager.GetColumnX(6.5f), false).GetColumnIndex()-1));
        }
        else
        {
            // �����Ǳ��մ�����û�к��ʵĽ���Ļ��Ǿ͹̶���·���ְ�
            GetRandomNext(0, 1);
            r2 = 3;
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(r2, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
                c2 = Mathf.Max(1, Mathf.Min(5, unit.GetColumnIndex() - 1));
            else
                c2 = 5;
        }
    }

    /// <summary>
    /// ����һ�����ǵ���
    /// </summary>
    private void CreateBullet(Vector2 pos, Vector2 rot)
    {
        bool isAttach = false;
        BaseUnit target = null; // ��������Ŀ��

        MouseModel m = MouseModel.GetInstance(Bullet_AnimatorController);
        m.transform.position = pos;
        m.SetBaseAttribute(1, 10, 1.0f, 18.0f, 100, 0.5f, 1);
        m.SetMoveRoate(rot);
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        m.IdleClipName = "Fly";
        m.MoveClipName = "Fly";
        m.AttackClipName = "Fly";
        m.DieClipName = "Hit";
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // �������ˮʴ
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));
        GameController.Instance.AddMouseUnit(m);

        // ����һ�������ж���������������ź����ƶ������ڽӴ���һ����ʳ��λʹ����������ȥ��Ȼ��������ʧ
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "ItemCollideAlly");
            r.isAffectFood = true;
            Action<BaseUnit> action = (u) =>
            {
                if (isAttach || !UnitManager.CanBeSelectedAsTarget(m, u))
                    return;
                // ��Ŀ���ѱ���ѣ��ֱ���ܵ��൱���䵱ǰ����ֵ����ʵ�˺�
                if (u.IsContainNoCountUniqueStatus(StringManager.Stun))
                {
                    new DamageAction(CombatAction.ActionType.RealDamage, this, u, u.GetCurrentHp()).ApplyAction();
                    m.ExecuteDeath();
                }
                if (!u.IsContainUnit(PinkPaul_Bullet))
                {
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, u, GetParamValue("dmg0", mHertIndex)).ApplyAction();
                    u.AddUnitToDict(PinkPaul_Bullet, m);
                    isAttach = true;
                    target = u;
                }
            };
            r.SetOnFoodEnterAction(action);
            r.SetOnFoodStayAction(action);
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (isAttach || !m.IsAlive())
                    return true;
                else
                {
                    r.transform.position = m.transform.position;
                    return false;
                }
            });
            t.AddOnExitAction(delegate
            {
                r.MDestory();
            });
            r.AddTask(t);
        }

        // �Ժ��Ǳ��������
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (isAttach)
                {
                    m.animatorController.Play("Attach", true);
                    m.NumericBox.MoveSpeed.SetBase(0);
                    m.transform.position = target.transform.position;
                    m.SetSpriteLocalPosition(target.GetSpriteLocalPosition());
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate
            {
                if (target.IsAlive())
                {
                    m.transform.position = target.transform.position;
                    m.SetSpriteLocalPosition(target.GetSpriteLocalPosition());
                    // ����ʩ����ѣЧ��
                    StatusAbility s = target.GetNoCountUniqueStatus(StringManager.Stun);
                    if (s == null)
                    {
                        s = new StunStatusAbility(target, 1, false);
                        target.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
                    }
                    if (s.leftTime < 2)
                        s.leftTime = 2;
                    return false;
                }
                return true;
            });
            t.AddOnExitAction(delegate
            {
                m.ExecuteDeath();
            });
            m.AddTask(t);
        }
    }

    /// <summary>
    /// ����һ������
    /// </summary>
    private BaseUnit CreateTentacle(Vector2 pos)
    {
        bool hasTarget = false;
        BaseUnit target = null;
        int interval = 180;
        int dmgTimeLeft = interval;
        float dmg = 10;
        bool isMoveLeft = true; // �Ƿ�������
        int moveTimeLeft = Mathf.FloorToInt(60*GetParamValue("t1_0", mHertIndex));

        MouseModel m = MouseModel.GetInstance(Tentacle_AnimatorController);
        m.transform.position = pos;
        m.SetBaseAttribute(1, 10, 1.0f, GetParamValue("v1", mHertIndex), 100, 0.5f, 0);
        m.SetMoveRoate(Vector2.left);
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.canDrivenAway = false;
        m.isIgnoreRecordDamage = true;
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.MoveClipName = "Idle";
        m.DieClipName = "Disappear";
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // �������ˮʴ
        m.SetActionState(new MoveState(m));
        Action<CombatAction> hitAction = (com) => {
            if(com is DamageAction)
            {
                DamageAction d = com as DamageAction;
                new DamageAction(CombatAction.ActionType.CauseDamage, d.Creator, this, GetParamValue("tran_rate1", mHertIndex) * 0.01f * d.DamageValue).ApplyAction();
            }
        };
        m.AddActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
        m.AddActionPointListener(ActionPointType.PostReceiveReboundDamage, hitAction);
        GameController.Instance.AddMouseUnit(m);

        // ����һ�������ж����򣬼�⵽������ĵ�һ����ʳ�Ὣ������
        Action createCheckArea = delegate
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "ItemCollideAlly");
            r.isAffectFood = true;
            Action<BaseUnit> stayAction = (u) =>
            {
                if (!hasTarget && u.IsAlive() && UnitManager.CanBeSelectedAsTarget(m, u))
                {
                    m.animatorController.Play("Attack", true);
                    m.DisableMove(true);
                    hasTarget = true;
                    target = u;
                    dmgTimeLeft = interval;
                }
            };
            r.SetOnFoodEnterAction(stayAction);
            r.SetOnFoodStayAction(stayAction);
            r.SetOnFoodExitAction((u) =>
            {
                if (hasTarget && u.Equals(target))
                {
                    m.animatorController.Play("Idle", true);
                    m.DisableMove(false);
                    hasTarget = false;
                    target = null;
                    isMoveLeft = false; // �Ƿ�������
                    m.SetMoveRoate(Vector2.right);
                    moveTimeLeft = Mathf.FloorToInt(60 * GetParamValue("t1_1", mHertIndex));
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (m.IsAlive())
                {
                    r.transform.position = m.transform.position;
                    return false;
                }
                return true;
            });
            t.AddOnExitAction(delegate
            {
                r.MDestory();
            });
            r.AddTask(t);
        };

        // �����������Ϊ����
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.animatorController.Play("Appear");
            });
            t.AddTaskFunc(delegate {
                if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.animatorController.Play("Idle", true);
                    createCheckArea();
                    return true;
                }
                return false;
            });
            t.AddTaskFunc(delegate {
                if (hasTarget)
                {
                    if (!target.IsAlive())
                    {
                        m.animatorController.Play("Idle", true);
                        m.DisableMove(false);
                        hasTarget = false;
                        target = null;
                        isMoveLeft = false; // �Ƿ�������
                        m.SetMoveRoate(Vector2.right);
                        moveTimeLeft = Mathf.FloorToInt(60 * GetParamValue("t1_1", mHertIndex));
                        return false;
                    }
                    // ��Ŀ�����ʩ����ѣЧ����ÿ3��10�˺�
                    dmgTimeLeft--;
                    if(dmgTimeLeft <= 0)
                    {
                        new DamageAction(CombatAction.ActionType.CauseDamage, m, target, dmg).ApplyAction();
                        dmgTimeLeft += interval;
                    }
                    // ����ʩ����ѣЧ��
                    StatusAbility s = target.GetNoCountUniqueStatus(StringManager.Stun);
                    if (s == null)
                    {
                        s = new StunStatusAbility(target, 1, false);
                        target.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
                    }
                    if (s.leftTime < 2)
                        s.leftTime = 2;
                    // ���Ŀ���������޺��ǣ��еĻ�ֱ�Ӵ���Ŀ��
                    if (target.IsContainUnit(PinkPaul_Bullet))
                        new DamageAction(CombatAction.ActionType.RealDamage, m, target, target.GetCurrentHp()).ApplyAction();
                }
                else
                {
                    // û��Ŀ��ʱ�ε�
                    moveTimeLeft--;
                    if(moveTimeLeft <= 0)
                    {
                        isMoveLeft = !isMoveLeft;
                        if (isMoveLeft)
                        {
                            m.SetMoveRoate(Vector2.left);
                            moveTimeLeft += Mathf.FloorToInt(60*GetParamValue("t1_0", mHertIndex));
                        }
                        else
                        {
                            m.SetMoveRoate(Vector2.right);
                            moveTimeLeft += Mathf.FloorToInt(60*GetParamValue("t1_1", mHertIndex));
                        }
                    }
                }
                return false;
            });
            m.AddTask(t);
        }

        return m;
    }

    /// <summary>
    /// ���ǹ���
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        int t0 = Mathf.FloorToInt(60*GetParamValue("t0", mHertIndex));
        int num0 = Mathf.FloorToInt(GetParamValue("num0", mHertIndex));

        int timeLeft = t0;

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
                    // ȥR0���Ҷ���
                    transform.position = MapManager.GetGridLocalPosition(7, FindR0Row());
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
                    return true;
                }
                return false;
            });
            // ����num0�ι���
            for (int i = 0; i < num0; i++)
            {
                int index = i;
                c.AddSpellingFunc(delegate
                {
                    animatorController.Play("Attack");
                    return true;
                });
                c.AddSpellingFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        int row = Mathf.FloorToInt(7 * (float)(index + 1) / num0) - 1;
                        BaseUnit target = FindS0Target(row);
                        if (target != null)
                            CreateBullet(transform.position, (target.transform.position - transform.position).normalized);
                        else
                            CreateBullet(transform.position, (MapManager.GetGridLocalPosition(0, row) - transform.position).normalized);
                        return true;
                    }
                    return false;
                });
            }
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
        int interval = 20;
        int timeLeft = 20;

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
                    CloseCollision();
                    SetAlpha(0);
                    return true;
                }
                return false;
            });
            // ÿ��·����һֻ����
            for (int i = 0; i < 7; i++)
            {
                int index = i;
                c.AddSpellingFunc(delegate
                {
                    timeLeft += interval;
                    return true;
                });
                c.AddSpellingFunc(delegate
                {
                    timeLeft--;
                    if(timeLeft <= 0)
                    {
                        BaseUnit unit = TentacleList[index];
                        if(unit == null || !unit.IsAlive())
                        {
                            TentacleList[index] = CreateTentacle(MapManager.GetGridLocalPosition(7, index));
                        }
                        return true;
                    }
                    return false;
                });
            }
            c.AddSpellingFunc(delegate
            {
                OpenCollision();
                SetAlpha(1);
                animatorController.Play("Appear");
                return true;
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
        c.AfterSpellFunc = delegate {

        };
        return c;
    }

    /// <summary>
    /// ͻϮ
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        int t2 = Mathf.FloorToInt(60 * GetParamValue("t2", mHertIndex));
        int num2 = Mathf.FloorToInt(GetParamValue("num2", mHertIndex));
        float dmg2 = GetParamValue("dmg2", mHertIndex);
        int stun2 = Mathf.FloorToInt(60 * GetParamValue("stun2", mHertIndex));

        int timeLeft = t2;

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
                    // ȥR2��C2��
                    int R2 = 4;
                    int C2 = 5;
                    FindR2C2(out R2, out C2);
                    transform.position = MapManager.GetGridLocalPosition(C2, R2);
                    // �������Ŀ�Ƭ
                    BaseGrid g = GameController.Instance.mMapController.GetGrid(C2, R2);
                    if (g != null)
                    {
                        FoodInGridType[] arr = new FoodInGridType[3] { FoodInGridType.WaterVehicle, FoodInGridType.Default, FoodInGridType.Shield };
                        foreach (var item in arr)
                        {
                            BaseUnit u = g.GetFoodByTag(item);
                            if (u != null)
                                u.ExecuteDeath();
                        }
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
                    return true;
                }
                return false;
            });
            // ����num2�ι���
            for (int i = 0; i < num2; i++)
            {
                int index = i;
                c.AddSpellingFunc(delegate
                {
                    animatorController.Play("Attack2");
                    return true;
                });
                c.AddSpellingFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // ��ʮ�ֵĸ������һ���˺�
                        {
                            Vector2[] v2Array = new Vector2[5] {
                            new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0)
                            };
                            int row = GetRowIndex();
                            int col = GetColumnIndex();
                            for (int j = 0; j < v2Array.Length; j++)
                            {
                                BaseGrid g = GameController.Instance.mMapController.GetGrid(col + Mathf.FloorToInt(v2Array[j].x), row + Mathf.FloorToInt(v2Array[j].y));
                                if (g != null)
                                {
                                    // Debug.Log("x = " + g.GetColumnIndex() + ", y = " + g.GetRowIndex());
                                    g.TakeDamage(this, dmg2, false);
                                }
                            }
                        }

                        // ���������һ���˺�
                        {
                            Vector2[] v2Array = new Vector2[2] {
                                new Vector2(1, 3), new Vector2(3, 1)
                            };
                            for (int j = 0; j < v2Array.Length; j++)
                            {
                                DamageAreaEffectExecution d = DamageAreaEffectExecution.GetInstance(this, transform.position, v2Array[j].x, v2Array[j].y, CombatAction.ActionType.CauseDamage, dmg2);
                                d.isAffectMouse = true;
                                d.isAffectFood = false;
                                d.isAffectCharacter = false;
                                d.SetInstantaneous();
                                d.AddExcludeMouseUnit(this); // ���ܴ��Լ�
                                GameController.Instance.AddAreaEffectExecution(d);
                            }
                        }

                        // ��������һ�ι����������3*3��ѣЧ��
                        if(index+1 == num2)
                        {
                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "BothCollide");
                            Action<BaseUnit> action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun2, false)); };
                            r.isAffectFood = true;
                            r.isAffectCharacter = true;
                            r.isAffectMouse = true;
                            r.SetOnFoodEnterAction(action);
                            r.SetOnEnemyEnterAction(action);
                            r.SetOnCharacterEnterAction(action);
                            r.SetInstantaneous();
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                        return true;
                    }
                    return false;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
