using GameNormalPanel_UI;

using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;

public class Hulk : BossUnit
{
    private static RuntimeAnimatorController Crack_Run;
    private static RuntimeAnimatorController HitGround_Run;
    private static Sprite Angry_Icon_Sprite;
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
    private static Func<BaseUnit, BaseUnit, bool> noSelcetedFunc = delegate { return false; };

    public override void Awake()
    {
        if (Crack_Run == null)
        {
            Crack_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/20/Crack");
            HitGround_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/20/HitGround");
            Angry_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Angry");
        }
        base.Awake();
    }

    private FloatModifier p_defence_mod = new FloatModifier(0);
    private Action<CombatAction> decRealDamageAction; // �ǻҽ���ʵ�˺�����Ч�� 

    private RingUI FinalSkillRingUI; // ŭ��ͼ��
    private RingUI WaitRUI; // ÿ�ι�����ȴ�ͼ��
    private bool canStrengthen; // �ܷ�ǿ����һ������

    public override void MInit()
    {
        WaitRUI = null;
        p_defence_mod.Value = 0;
        canStrengthen = false;
        decRealDamageAction = (act) => 
        {
            if (!(act is DamageAction))
                return;
            DamageAction d = act as DamageAction;
            if(d.mActionType.Equals(CombatAction.ActionType.RealDamage) && !d.IsDamageType(DamageAction.DamageType.BombBurn))
            {
                d.DamageValue = d.DamageValue*p_defence_mod.Value;
            }
        };
        base.MInit();
        // �ȴ�UI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            // ��Ӱ�����
            taskController.AddTask(TaskManager.GetWaitRingUITask(rUI, this, 0.25f * MapManager.gridHeight * Vector3.up));
            rUI.Hide();
            rUI.SetPercent(0);
            AddOnDestoryAction(delegate { if (rUI.IsValid()) rUI.MDestory(); });
            WaitRUI = rUI;
        }
        // ����UI
        {
            RingUI rUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(rUI);
            // ��Ӱ�����
            {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    rUI.Show();
                    rUI.SetIcon(Angry_Icon_Sprite);
                    rUI.SetPercent(0);
                    rUI.SetColor(new Color(1, 1, 1, 0.75f));
                });
                task.AddTaskFunc(delegate {
                    rUI.transform.position = transform.position + 0.25f * MapManager.gridHeight * Vector3.down;
                    float per = rUI.GetPercent();
                    if (per < 1)
                        rUI.SetColor(new Color(1, 1 - per, 1 - per, 0.75f));
                    else
                    {
                        float rate = (GameController.Instance.GetCurrentStageFrame()/8)%2;
                        rUI.SetColor(new Color(1, 0.5f*rate, 0.5f*rate, 1));
                    }
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
            FinalSkillRingUI = rUI;
        }
        // ��ӳ��ֵļ���
        {
            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 0;
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = Vector2.zero;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                SetAlpha(0);
                animatorController.Play("Move0", true);
                AddCanHitFunc(noHitFunc);
                AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
            };
            c.AddSpellingFunc(delegate {
                float rowIndex = GetParamValue("p_row") - 1;
                float colIndex = 9 - GetParamValue("p_right_col");
                // ��һ֡ǿ�ưѺ�����ͬ������-3��
                transform.position = MapManager.GetGridLocalPosition(12, rowIndex);
                startPos = transform.position;
                endPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                SetAlpha(1);
                timeLeft = 120;
                return true;
            });
            c.AddSpellingFunc(delegate {
                timeLeft--;
                float rate = 1 - (float)timeLeft / 120;
                transform.position = Vector2.Lerp(startPos, endPos, rate);
                if (timeLeft <= 0)
                {
                    animatorController.Play("Idle0", true);
                    RemoveCanHitFunc(noHitFunc);
                    RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                    return true;
                }
                return false;
            });
            // ��ʼ�ȴ�
            c.AddCreateTaskFunc(delegate { 
                return GetStartWaitTask();
            });
            c.AddSpellingFunc(delegate {
                animatorController.Play("PreCast");
                // ��ü���
                NumericBox.DamageRate.AddModifier(p_defence_mod);
                AddActionPointListener(ActionPointType.PreReceiveDamage, decRealDamageAction);
                return true;
            });
            c.AddSpellingFunc(delegate {
                if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Casting");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    taskController.AddTask(GetAngryTask()); // ���ŭ������
                    return true;
                }
                return false;
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
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.Hulk, 0))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // ���������ʼ��
        // ��������
        {
            Action<float[]> action = delegate
            {
                p_defence_mod.Value = 1 - GetParamValue("p_defence") / 100;
            };
            AddParamChangeAction("p_defence", action);
            action(null);
        }
        
    }
    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
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
        list.Add(SKill2Init(infoList[2]));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    #region һ����
    private Vector2 GetS0AttackPos()
    {
        float x_center = MapManager.GetColumnX(GetParamValue("left_col0") - 1);
        float x_left, x_right;
        if(canStrengthen)
        {
            x_left = x_center - (GetParamValue("r0_1")+0.5f) * MapManager.gridWidth;
            x_right = x_center + (GetParamValue("r0_1")+0.5f) * MapManager.gridWidth;
        }
        else
        {
            x_left = x_center - (GetParamValue("r0_0")+0.5f) * MapManager.gridWidth;
            x_right = x_center + (GetParamValue("r0_0")+0.5f) * MapManager.gridWidth;
        }
        return new Vector2(x_center + 2*MapManager.gridWidth, GetAttackPos(x_left, x_right));
    }

    private void CreateHitArea(Vector2 pos)
    {
        // ��Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(HitGround_Run, null, "Appear", null, false);
            e.SetSpriteRendererSorting("Effect", 1);
            e.transform.position = pos;
            GameController.Instance.AddEffect(e);
        }

        // �ж�
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "BothCollide");
            r.name = "HitGroundArea";
            r.SetInstantaneous();
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.AddFoodEnterConditionFunc((u) => {
                return u.IsAlive() && FoodManager.IsAttackableFoodType(u);
            });
            r.SetOnFoodEnterAction(KnockFlyingFood);
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && !UnitManager.IsFlying(u) && MouseManager.IsGeneralMouse(u) && u.GetHeight()>-1;
            });
            r.SetOnEnemyEnterAction((u) => {
                // ���һ����������� (��Զ���͵����ſڣ��߼����ͽ����ˣ�����һ�й�Ƥ�����赲������
                float dist = UnityEngine.Random.Range(0.1f, 0.6f);
                CustomizationTask t = TaskManager.GetParabolaTask(u, dist / 120, UnityEngine.Random.Range(3, 5)*MapManager.gridHeight, u.transform.position, u.transform.position + new Vector3(Mathf.Sign(UnityEngine.Random.Range(-1.0f, 1.0f))*dist, 0), false, true);
                // �ҽ�ֹ�ƶ�
                u.DisableMove(true);
                t.AddOnExitAction(delegate {
                    u.DisableMove(false);
                    UnitManager.Execute(null, u); // ˤ����
                });
                u.AddTask(t);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private void CreateHitGroundTask(Vector2 pos)
    {
        int count;
        if(canStrengthen)
            count = Mathf.FloorToInt(GetParamValue("r0_1"));
        else
            count = Mathf.FloorToInt(GetParamValue("r0_0"));

        CustomizationTask task = new CustomizationTask();
        // ��һ�ε���
        task.AddTimeTaskFunc(20, delegate {
            CreateHitArea(pos);
        }, null, null);
        // �󼸴ε���
        for (int _i = 1; _i <= count; _i++)
        {
            int i = _i;
            task.AddTimeTaskFunc(20, delegate {
                for (int j = 1; j <= i; j++)
                {
                    int x = j;
                    int y = i - j;
                    CreateHitArea(pos + new Vector2(MapManager.gridWidth * x, MapManager.gridHeight * y));
                    CreateHitArea(pos + new Vector2(MapManager.gridWidth * -y, MapManager.gridHeight * x));
                    CreateHitArea(pos + new Vector2(MapManager.gridWidth * -x, MapManager.gridHeight * -y));
                    CreateHitArea(pos + new Vector2(MapManager.gridWidth * y, MapManager.gridHeight * -x));
                }
            }, null, null);
        }
        taskController.AddTask(task);
    }

    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int wait = Mathf.FloorToInt(GetParamValue("wait0")*60);

        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col0"));
        int jump_time = 65;
        int stun = Mathf.FloorToInt(GetParamValue("p_enemy_stun") * 60);
        int str_stun = Mathf.FloorToInt(GetParamValue("p_strength_enemy_stun") * 60);

        // ����
        int timeLeft = 0;
        bool isNoWait = false; // �Ƿ񲻵ȴ���һ��ǿ�������󲻵ȴ�����ֱ���ε���
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        FloatModifier yMod = new FloatModifier(0);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate{
            isNoWait = false;
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Jump");
                    timeLeft = jump_time;
                    startPos = transform.position;
                    endPos = GetS0AttackPos();
                    AddCanHitFunc(noHitFunc);
                    AddCanBeSelectedAsTargetFunc(noSelcetedFunc);
                    
                    yMod.Value = 0;
                    AddSpriteOffsetY(yMod);
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45-timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI*rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        animatorController.Play("Clap");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.31f)
                    {
                        // ��������
                        CreateHitGroundTask(transform.position + 2*MapManager.gridWidth*Vector3.left);
                        Action<BaseUnit> action = null;
                        if (canStrengthen)
                        {
                            isNoWait = true;
                            canStrengthen = false;
                            CustomizationSkillAbility s = StunSKillInit(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[3]);
                            mSkillQueueAbilityManager.SetNextSkill(s);
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, str_stun, false)); };
                        }
                        else
                        {
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun, false)); };
                        }
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13 * MapManager.gridWidth, 9 * MapManager.gridHeight), "ItemCollideEnemy");
                        r.SetInstantaneous();
                        r.isAffectMouse = true;
                        r.SetOnEnemyEnterAction(action);
                        GameController.Instance.AddAreaEffectExecution(r);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // ׼������ȥ
                        animatorController.Play("Jump");
                        timeLeft = jump_time;
                        startPos = transform.position;
                        endPos = new Vector2(MapManager.GetColumnX(colIndex), transform.position.y);
                        AddCanHitFunc(noHitFunc);
                        AddCanBeSelectedAsTargetFunc(noSelcetedFunc);

                        yMod.Value = 0;
                        AddSpriteOffsetY(yMod);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45 - timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI * rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        if (isNoWait)
                        {
                            timeLeft = 0;
                        }
                        else
                        {
                            timeLeft = wait;
                            WaitRUI.Show();
                            WaitRUI.SetPercent(1);
                        }
                        animatorController.Play("Idle");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    WaitRUI.SetPercent((float)timeLeft/wait);
                    if (timeLeft <= 0)
                    {
                        WaitRUI.Hide();
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
    private Vector2 GetS1AttackPos()
    {
        float x_center;
        float x_left, x_right;
        if (canStrengthen)
        {
            x_center = MapManager.GetColumnX(9 - GetParamValue("right_col1_2"));
            x_right = x_center - (1 - 0.5f) * MapManager.gridWidth;
            x_left = x_center - (GetParamValue("num1_1") + 0.5f) * MapManager.gridWidth;
        }
        else
        {
            x_center = MapManager.GetColumnX(9 - GetParamValue("right_col1_0"));
            x_right = x_center - (1 - 0.5f) * MapManager.gridWidth;
            x_left = x_center - (GetParamValue("num1_0") + 0.5f) * MapManager.gridWidth;
        }
        return new Vector2(x_center, GetAttackPos(x_left, x_right));
    }

    private void CreateS1CrackArea(Vector2 pos)
    {
        float lost_hp_percent = GetParamValue("lost_hp_percent1") / 100;
        int stun_time = Mathf.FloorToInt(GetParamValue("stun1")*60);
        float fly_dist;
        if (canStrengthen)
            fly_dist = GetParamValue("fly1_1") * MapManager.gridWidth;
        else
            fly_dist = GetParamValue("fly1_0") * MapManager.gridWidth;
        float min_x = MapManager.GetColumnX(GetParamValue("left_col1") - 1);

        // ��Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(Crack_Run, null, "Appear", null, false);
            e.SetSpriteRendererSorting("Effect", 1);
            e.transform.position = pos;
            GameController.Instance.AddEffect(e);
        }

        // �ж�
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "BothCollide");
            r.name = "CrackArea";
            r.SetAffectHeight(0);
            r.SetInstantaneous();
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.AddFoodEnterConditionFunc((u) => {
                return u.IsAlive() && FoodManager.IsAttackableFoodType(u);
            });
            r.SetOnFoodEnterAction(KnockFlyingFood);
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && !UnitManager.IsFlying(u) && MouseManager.IsGeneralMouse(u) && u.transform.position.x > min_x && u.GetHeight()==0;
            });
            r.SetOnEnemyEnterAction((u) => {
                // ���һ����������� (��Զ���͵����ſڣ��߼����ͽ����ˣ�����һ�й�Ƥ�����赲������
                float dist = Mathf.Min(fly_dist, u.transform.position.x - min_x);
                CustomizationTask t = TaskManager.GetParabolaTask(u, dist / 60, dist/2, u.transform.position, u.transform.position + dist*Vector3.left, false, true);
                // �ҽ�ֹ�ƶ�
                u.DisableMove(true);
                t.AddOnExitAction(delegate {
                    u.DisableMove(false);
                    new DamageAction(CombatAction.ActionType.CauseDamage, null, u, lost_hp_percent * u.mCurrentHp).ApplyAction(); // ����˺�
                    u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun_time, false)); // ��ѣ
                });
                u.AddTask(t);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    private void CreateS1CrackTask(Vector2 pos)
    {
        int count;
        if (canStrengthen)
            count = Mathf.FloorToInt(GetParamValue("num1_1"));
        else
            count = Mathf.FloorToInt(GetParamValue("num1_0"));

        CustomizationTask task = new CustomizationTask();
        for (int _i = 1; _i <= count; _i++)
        {
            int i = _i;
            task.AddTimeTaskFunc(10, delegate {
                CreateS1CrackArea(pos + MapManager.gridWidth * i * Vector2.left);
            }, null, null);
        }
        taskController.AddTask(task);
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int wait = Mathf.FloorToInt(GetParamValue("wait1") * 60);
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col1_1"));
        int jump_time = 65;
        int stun = Mathf.FloorToInt(GetParamValue("p_enemy_stun") * 60);
        int str_stun = Mathf.FloorToInt(GetParamValue("p_strength_enemy_stun") * 60);

        // ����
        int timeLeft = 0;
        bool isNoWait = false; // �Ƿ񲻵ȴ���һ��ǿ�������󲻵ȴ�����ֱ���ε���
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        FloatModifier yMod = new FloatModifier(0);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isNoWait = false;
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Jump");
                    timeLeft = jump_time;
                    startPos = transform.position;
                    endPos = GetS1AttackPos();
                    AddCanHitFunc(noHitFunc);
                    AddCanBeSelectedAsTargetFunc(noSelcetedFunc);

                    yMod.Value = 0;
                    AddSpriteOffsetY(yMod);
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45 - timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI * rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        animatorController.Play("Lift");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.7f)
                    {
                        // �����ѵ�
                        CreateS1CrackTask(transform.position);
                        Action<BaseUnit> action = null;
                        if (canStrengthen)
                        {
                            isNoWait = true;
                            canStrengthen = false;
                            CustomizationSkillAbility s = StunSKillInit(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[3]);
                            mSkillQueueAbilityManager.SetNextSkill(s);
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, str_stun, false)); };
                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13 * MapManager.gridWidth, 9 * MapManager.gridHeight), "BothCollide");
                            r.SetInstantaneous();
                            r.isAffectMouse = true;
                            r.isAffectFood = true;
                            r.SetOnEnemyEnterAction(action);
                            r.SetOnFoodEnterAction(action);
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                        else
                        {
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun, false)); };
                            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13 * MapManager.gridWidth, 9 * MapManager.gridHeight), "ItemCollideEnemy");
                            r.SetInstantaneous();
                            r.isAffectMouse = true;
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
                        // ׼������ȥ
                        animatorController.Play("Jump");
                        timeLeft = jump_time;
                        startPos = transform.position;
                        endPos = new Vector2(MapManager.GetColumnX(colIndex), transform.position.y);
                        AddCanHitFunc(noHitFunc);
                        AddCanBeSelectedAsTargetFunc(noSelcetedFunc);

                        yMod.Value = 0;
                        AddSpriteOffsetY(yMod);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45 - timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI * rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        if (isNoWait)
                        {
                            timeLeft = 0;
                        }
                        else
                        {
                            timeLeft = wait;
                            WaitRUI.Show();
                            WaitRUI.SetPercent(1);
                        }
                        animatorController.Play("Idle");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    WaitRUI.SetPercent((float)timeLeft / wait);
                    if (timeLeft <= 0)
                    {
                        WaitRUI.Hide();
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
    private Vector2 GetS2AttackPos()
    {
        float x_center;
        float x_left, x_right;
        x_center = MapManager.GetColumnX(GetParamValue("left_col2_0") - 1) + 0.5f*MapManager.gridWidth;
        x_right = x_center;
        x_left = MapManager.GetColumnX(GetParamValue("left_col2_1") - 1) - 0.5f*MapManager.gridWidth;
        return new Vector2(x_center, GetAttackPos(x_left, x_right));
    }

    private void CreateS2RollingTask(Vector2 pos, int totalTime, int interval)
    {
        float food_trans = GetParamValue("food_trans2") / 100;
        float mouse_trans = GetParamValue("mouse_trans2") / 100;
        float lowest_hp = mMaxHp * GetParamValue("no_rebound_hp_percent2")/100;

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "BothCollide");
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.AddFoodEnterConditionFunc((u) => {
            return u.IsAlive() && FoodManager.IsAttackableFoodType(u);
        });
        r.SetOnFoodEnterAction((u) => 
        {
            DamageAction d = UnitManager.Execute(this, u);
            if(mCurrentHp > lowest_hp)
            {
                float dmg = Mathf.Min((mCurrentHp - lowest_hp) / GetFinalDamageRate(), d.RealCauseValue * food_trans);
                new DamageAction(CombatAction.ActionType.CauseDamage, null, this, dmg).ApplyAction();
            }
        });
        r.AddEnemyEnterConditionFunc((u) => {
            return !u.IsBoss() && !UnitManager.IsFlying(u) && MouseManager.IsGeneralMouse(u) && u.GetHeight() == 0;
        });
        r.SetOnEnemyEnterAction((u) =>
        {
            DamageAction d = UnitManager.Execute(this, u);
            if (mCurrentHp > lowest_hp)
            {
                float dmg = Mathf.Min((mCurrentHp - lowest_hp) / GetFinalDamageRate(), d.RealCauseValue * mouse_trans);
                new DamageAction(CombatAction.ActionType.CauseDamage, null, this, dmg).ApplyAction();
            }
        });
        GameController.Instance.AddAreaEffectExecution(r);

        int timeLeft = totalTime + 1;
        int strength_attack_timeLeft = (canStrengthen?0:9999999);
        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position;
            timeLeft--;
            strength_attack_timeLeft--;
            if(strength_attack_timeLeft<=0)
            {
                strength_attack_timeLeft += interval;
                // �����౬��
                CreateHitArea(transform.position + Vector3.up * MapManager.gridHeight);
                CreateHitArea(transform.position + Vector3.down * MapManager.gridHeight);
            }
            return timeLeft <= 0 || !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int wait = Mathf.FloorToInt(GetParamValue("wait2") * 60);
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col2"));
        int jump_time = 65;
        int roll_grid_time = 10; // ��һ���ʱ��
        int roll_total_time = Mathf.FloorToInt(roll_grid_time * (GetParamValue("left_col2_0") - GetParamValue("left_col2_1")));
        int stun = Mathf.FloorToInt(GetParamValue("p_enemy_stun") * 60);
        int str_stun = Mathf.FloorToInt(GetParamValue("p_strength_enemy_stun") * 60);

        // ����
        int timeLeft = 0;
        bool isNoWait = false; // �Ƿ񲻵ȴ���һ��ǿ�������󲻵ȴ�����ֱ���ε���
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        FloatModifier yMod = new FloatModifier(0);

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            isNoWait = false;
        };
        {
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Jump");
                    timeLeft = jump_time;
                    startPos = transform.position;
                    endPos = GetS2AttackPos();
                    AddCanHitFunc(noHitFunc);
                    AddCanBeSelectedAsTargetFunc(noSelcetedFunc);

                    yMod.Value = 0;
                    AddSpriteOffsetY(yMod);
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45 - timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI * rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        animatorController.Play("PreRoll");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        timeLeft = roll_total_time;
                        animatorController.Play("Rolling", true);
                        startPos = transform.position;
                        endPos = new Vector2(MapManager.GetColumnX(GetParamValue("left_col2_1")-1), transform.position.y);
                        CreateS2RollingTask(transform.position, roll_total_time, roll_grid_time);
                        Action<BaseUnit> action = null;
                        if (canStrengthen)
                        {
                            isNoWait = true;
                            canStrengthen = false;
                            CustomizationSkillAbility s = StunSKillInit(AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape)[3]);
                            mSkillQueueAbilityManager.SetNextSkill(s);
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, str_stun, false)); };
                        }
                        else
                        {
                            action = (u) => { u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun, false)); };
                        }
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(MapManager.GetGridLocalPosition(4, 3), new Vector2(13 * MapManager.gridWidth, 9 * MapManager.gridHeight), "ItemCollideEnemy");
                        r.SetInstantaneous();
                        r.isAffectMouse = true;
                        r.SetOnEnemyEnterAction(action);
                        GameController.Instance.AddAreaEffectExecution(r);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate = 1 - (float)timeLeft / roll_total_time;
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        // ׼������ȥ
                        animatorController.Play("Jump");
                        timeLeft = jump_time;
                        startPos = transform.position;
                        endPos = new Vector2(MapManager.GetColumnX(colIndex), transform.position.y);
                        AddCanHitFunc(noHitFunc);
                        AddCanBeSelectedAsTargetFunc(noSelcetedFunc);

                        yMod.Value = 0;
                        AddSpriteOffsetY(yMod);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    float rate;
                    if (timeLeft >= 45)
                        rate = 0;
                    else if (timeLeft <= 20)
                        rate = 1;
                    else
                        rate = (float)(45 - timeLeft) / 25;
                    RemoveSpriteOffsetY(yMod);
                    yMod.Value = 0.5f * Mathf.Sin(Mathf.PI * rate);
                    AddSpriteOffsetY(yMod);
                    transform.position = Vector2.Lerp(startPos, endPos, rate);
                    if (timeLeft <= 0)
                    {
                        RemoveSpriteOffsetY(yMod);
                        if(isNoWait)
                        {
                            timeLeft = 0;
                        }
                        else
                        {
                            timeLeft = wait;
                            WaitRUI.Show();
                            WaitRUI.SetPercent(1);
                        }
                        animatorController.Play("Idle");
                        RemoveCanHitFunc(noHitFunc);
                        RemoveCanBeSelectedAsTargetFunc(noSelcetedFunc);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate {
                    timeLeft--;
                    WaitRUI.SetPercent((float)timeLeft / wait);
                    if (timeLeft <= 0)
                    {
                        WaitRUI.Hide();
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

    #region �ļ��ܣ�ǿ���������������ѣ��û�뵽�ɣ���Ҳ�Ǽ��ܣ���
    private CompoundSkillAbility StunSKillInit(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int stun_time = Mathf.FloorToInt(GetParamValue("p_stun_time") * 60);
        float lost_hp_val = GetParamValue("p_lost_hp_percent2") / 100 * mMaxHp;
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


        // ����
        int timeLeft = 0;
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate{};
        {
            if(stun_time > 0)
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("PreStun");
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            timeLeft = stun_time;
                            animatorController.Play("Stun", true);
                            hpLeft = lost_hp_val;
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
                            // ʧȥ��������
                            NumericBox.DamageRate.RemoveModifier(p_defence_mod);
                            RemoveActionPointListener(ActionPointType.PreReceiveDamage, decRealDamageAction);
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
                            // �Ƴ�������ڼ���ܻ��˺�
                            actionPointController.RemoveListener(ActionPointType.PostReceiveDamage, hitAction);
                            animatorController.Play("PreCast2");
                            // ���»�ñ�������
                            NumericBox.DamageRate.AddModifier(p_defence_mod);
                            AddActionPointListener(ActionPointType.PreReceiveDamage, decRealDamageAction);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Casting");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Idle", true);
                            taskController.AddTask(GetAngryTask()); // ���ŭ������
                            return true;
                        }
                        return false;
                    });
                    return task;
                });
            }
            else
            {
                c.AddCreateTaskFunc(delegate {
                    CustomizationTask task = new CustomizationTask();
                    task.AddOnEnterAction(delegate {
                        animatorController.Play("Idle", true);
                        taskController.AddTask(GetAngryTask()); // ���ŭ������
                    });
                    return task;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
    #endregion

    #region ����˽������
    /// <summary>
    /// ��ӳ�ʼ�ȴ�����ʱ�������˺�����
    /// </summary>
    private CustomizationTask GetStartWaitTask()
    {
        int totalTimeLeft = Mathf.FloorToInt(GetParamValue("p_wait") *60);
        float totalDamageLeft = mMaxHp*GetParamValue("p_lost_hp_percent")/100;

        RingUI waitUI = RingUI.GetInstance(0.3f*Vector2.one);
        GameNormalPanel.Instance.AddUI(waitUI);
        waitUI.mTaskController.AddTask(TaskManager.GetWaitRingUITask(waitUI, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.right));


        RingUI dmgUI = RingUI.GetInstance(0.3f * Vector2.one);
        GameNormalPanel.Instance.AddUI(dmgUI);
        dmgUI.mTaskController.AddTask(TaskManager.GetFinalSkillRingUITask(dmgUI, this, 0.25f * MapManager.gridHeight * Vector3.down + 0.3f * MapManager.gridWidth * Vector3.left));

        // ���˫���������
        int timeLeft = totalTimeLeft;
        float dmgLeft = totalDamageLeft;
        Action<CombatAction> hitAction = (combatAction) =>
        {
            if (combatAction is DamageAction)
            {
                DamageAction dmgAction = combatAction as DamageAction;
                dmgLeft -= dmgAction.RealCauseValue;
            }
        };
        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            AddActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
        });
        task.AddTaskFunc(delegate {
            timeLeft--;
            waitUI.SetPercent((float)timeLeft/totalTimeLeft);
            dmgUI.SetPercent(1-(float)dmgLeft/totalDamageLeft);
            if (timeLeft <= 0 || dmgLeft <= 0)
                return true;
            return false;
        });
        task.AddOnExitAction(delegate {
            RemoveActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
            waitUI.MDestory();
            dmgUI.MDestory();
        });
        return task;
    }

    /// <summary>
    /// ���ŭ������
    /// </summary>
    /// <returns></returns>
    private CustomizationTask GetAngryTask()
    {
        // ����
        float totalAngry = 100;
        float deltaAngry_idle = GetParamValue("p_idle_angry")/60; // ���������ÿ֡������
        float deltaAngry_hit = GetParamValue("p_hit_angry") / 60; // �����ÿ֡������
        int add_time = Mathf.FloorToInt(GetParamValue("p_add_time") * 60); // ���������ŭ����֡��
        float deltaAngry_burn = GetParamValue("p_burn_angry"); // ��ըһ�κ��������ӵ�ŭ����
        RingUI rUI = FinalSkillRingUI;

        // ����
        int hit_angry_timeLeft = 0; // ʣ�త����ŭʱ��
        float currentAngry = 0; // ��ǰŭ��
        Action<CombatAction> hitAction = (combatAction) =>
        {
            if (combatAction is DamageAction)
            {
                DamageAction dmgAction = combatAction as DamageAction;
                hit_angry_timeLeft = add_time;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    currentAngry += deltaAngry_burn;
            }
        };

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            rUI.Show(); // ��ʾŭ��ͼ��
            rUI.SetPercent(0);
            AddActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
        });
        task.AddTaskFunc(delegate {
            if (hit_angry_timeLeft <= 0)
                currentAngry += deltaAngry_idle;
            else
            {
                currentAngry += deltaAngry_hit;
                hit_angry_timeLeft--;
            }
            rUI.SetPercent(currentAngry/totalAngry);
            if(currentAngry >= totalAngry)
            {
                canStrengthen = true;
                RemoveActionPointListener(ActionPointType.PostReceiveDamage, hitAction);
                return true;
            }
            return false;
        });
        task.AddTaskFunc(delegate {
            return !canStrengthen;
        });
        task.AddTaskFunc(delegate {
            currentAngry-=2;
            rUI.SetPercent(currentAngry / totalAngry);
            if (currentAngry <= 0)
                return true;
            return false;
        });
        task.AddOnExitAction(delegate {
            rUI.Hide();
            rUI.SetPercent(0);
        });
        return task;
    }

    /// <summary>
    /// ��ȡ��x_left~x_right������ʳ��λ�����е�������
    /// </summary>
    /// <param name="x_left"></param>
    /// <param name="x_right"></param>
    /// <returns></returns>
    private float GetAttackPos(float x_left, float x_right)
    {
        int[] valArray = new int[7];
        foreach (var u in GameController.Instance.GetEachAlly())
        {
            if (u is FoodUnit)
            {
                FoodUnit f = u as FoodUnit;
                if (FoodManager.IsAttackableFoodType(f) && UnitManager.CanBeSelectedAsTarget(this, f))
                {
                    for (int i = 0; i < 7; i++)
                    {
                        int rowIndex = i;
                        if (Mathf.Abs(MapManager.GetYIndex(f.transform.position.y) - rowIndex) < 0.5f && f.transform.position.x > x_left && f.transform.position.x < x_right)
                            valArray[i]++;
                    }
                }
            }
        }

        int max = int.MinValue;
        List<int> rowList = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            if (valArray[i] > max)
            {
                max = valArray[i];
                rowList.Clear();
                rowList.Add(i);
            }
            else if (valArray[i] == max)
            {
                rowList.Add(i);
            }
        }
        return MapManager.GetRowY(rowList[GetRandomNext(0, rowList.Count)]);
    }

    /// <summary>
    /// ������ʳ��λ
    /// </summary>
    /// <param name="u"></param>
    private void KnockFlyingFood(FoodUnit u)
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
        Vector2 endPos = startPos + new Vector2(UnityEngine.Random.Range(-3, 3)*MapManager.gridWidth, UnityEngine.Random.Range(10, 15)*MapManager.gridHeight);

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
            e.transform.position = Vector2.Lerp(startPos, endPos, Mathf.Sin(rate*Mathf.PI/2));
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
}
