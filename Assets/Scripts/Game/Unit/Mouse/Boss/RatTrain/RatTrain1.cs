using System;
using System.Collections.Generic;

using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �г�����
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

    private FloatModifier moveSpeedModifier = new FloatModifier(0); // ����������ǩ
    private List<BaseUnit> retinueList = new List<BaseUnit>(); // ������ӱ�������ʱͬʱ����������ӣ�

    private List<int> attack_orderList = new List<int>(); // ����˳���

    private float dmgRecord; // �ܵ����˺��ܺ�
    private int stun_timeLeft; // ��ѣʱ��
    private Action<CombatAction> burnAction; // ��ը�¼�

    private RingUI FinalSkillRingUI;


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
        // ��ը�¼�����
        burnAction = (combat) =>
        {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    stun_timeLeft += Mathf.FloorToInt(GetParamValue("p_stun_time") * 60);
            }
        };
        base.Awake();
    }

    public override void MInit()
    {
        retinueList.Clear();
        base.MInit();
        // ����13�ڳ���
        CreateHeadAndBody(13);
        // ���ó�ͷ��������β���˺�����
        GetHead().SetDmgRate(GetParamValue("head_normal"), GetParamValue("head_burn"));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal"), GetParamValue("body_burn"));
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal"), GetParamValue("tail_burn"));
        // Ĭ���ٶ�Ϊ2.4���/ÿ��
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridWidth/60);
        SetAllComponentNoBeSelectedAsTargetAndHited();

        // �ǳ�����������Ļ�������һ�£������һ�ηż��ܳ��ֶ��������������ţ�
        {
            // AddRouteListByGridIndex(new List<Vector2[]>() { new Vector2[] { MapManager.GetGridLocalPosition(40, 0), MapManager.GetGridLocalPosition(40, 1) }  });
        }
        SetActionState(new MoveState(this));

        // ����UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, GetHead()));
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(1);

            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(240);
            task.AddOnExitAction(delegate {
                FinalSkillRingUI.Show();
            });
            taskController.AddTask(task);
        }
        // �ܻ��¼�
        {
            actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) =>
            {
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
                        CustomizationSkillAbility s = GetUltimateSkill();
                        mSkillQueueAbilityManager.SetNextSkill(s);
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
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.667f, 0.333f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.RatTrain, 1))
            AddParamArray(keyValuePair.Key, keyValuePair.Value);

        // �����������
        {
            // ����˳��
            attack_orderList.Clear();
            float[] orderArray = GetParamArray("attack_order");
            // ���û���Զ��幥��˳����Ĭ�ϵĹ���˳����
            foreach (var item in orderArray)
            {
                int i = Mathf.Max(0, (int)item - 1);
                attack_orderList.Add(i);
            }
        }
    }

    /// <summary>
    /// �л��׶Ρ����ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        // ���û�ȡ������ķ���
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(delegate { return attack_orderList; });

        if (mHertIndex == 0)
        {
            LoadP1SkillAbility();
        }
        else if (mHertIndex == 1)
        {
            LoadP2SkillAbility();
        }
        else
        {
            LoadP3SkillAbility();
        }

        // ���ٱ仯
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
    }

    /// <summary>
    /// ��ȡ�ռ�����
    /// </summary>
    /// <returns></returns>
    //private CustomizationSkillAbility GetUltimateSkill()
    //{
    //    CompoundSkillAbility c = new CompoundSkillAbility(this);
    //    BaseUnit u = null;
    //    // ʵ��
    //    c.IsMeetSkillConditionFunc = delegate {
    //        // ���ٸ���
    //        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
    //        moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
    //        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
    //        return true;
    //    };
    //    c.BeforeSpellFunc = delegate {
    //        u = CreateFireBulletAttackerTrain();
    //    };
    //    {
    //        // �ж���̨�Ƿ���ʧ����ʧ��Ϊ�ü��ܽ����ı�־
    //        c.AddSpellingFunc(delegate {
    //            return u == null || !u.IsAlive();
    //        });
    //    }
    //    c.OnNoSpellingFunc = delegate { };
    //    c.AfterSpellFunc = delegate { };
    //    return c;
    //}

    private void LoadP1SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float hp = GetParamValue("hp0", mHertIndex);
        float burn_rate0 = 1 - GetParamValue("burn_defence0") / 100;
        
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);
        float burn_rate1 = 1 - GetParamValue("burn_defence1") / 100;

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
            List<Action<int, int>> actionList = new List<Action<int, int>>();
            actionList.Add(CreateLaserAttackerAction(burn_rate1));
            return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement3(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP2SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
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
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement3(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
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
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                return actionList;
            }
            ));
        list.Add(Movement3(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(burn_rate1));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
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
    /// ����һ�����ⷢ����
    /// </summary>
    private BaseUnit CreateLaserAttacker(RatTrainComponent master, bool isLeft, float burn_rate)
    {
        float hp = GetParamValue("hp1");
        int wait = Mathf.FloorToInt(GetParamValue("t1_0")*60);
        float dmg_rate = GetParamValue("dmg_rate1");
        float ice_val = GetParamValue("ice_val1");

        MouseModel m = MouseModel.GetInstance(LaserAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = master.transform.right;
            m.transform.localScale = new Vector2(hscale, (isLeft ? 1 : -1));
            m.AddCanHitFunc(delegate { return false; });
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.mBoxCollider2D.offset = new Vector2(0, 1.0f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // ����Ϊ�������
            retinueList.Add(m);
        }
        GameController.Instance.AddMouseUnit(m);
        m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 1; // ͼ��-1

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
                // �Ƴ�������Ч
                m.mEffectController.RemoveEffectFromDict(LaserEffectKey);
                Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                // ����һ������
                {
                    BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PostAttack", null, false);
                    e.SetSpriteRendererSorting("Effect", 10);
                    GameController.Instance.AddEffect(e);
                    m.mEffectController.AddEffectToDict(LaserEffectKey, e, 1f * Vector2.up * MapManager.gridHeight);
                    e.transform.right = rot;
                }
                // ���������ļ����ж�
                {
                    Action<BaseUnit> action = (u) => 
                    {
                        ITask t = EnvironmentFacade.GetIceDebuff(u);
                        if (t != null)
                            new DamageAction(CombatAction.ActionType.BurnDamage, this, u, (t as IceTask).GetValue() * dmg_rate).ApplyAction();
                        if (ice_val > 0)
                            EnvironmentFacade.AddIceDebuff(u, ice_val);
                    };

                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position + (Vector3)rot*(1+2/2)*MapManager.gridHeight,  0.4f, 2, "BothCollide");
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
                            r.AddExcludeMouseUnit(m); // ���ܻٵ���������
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
    /// �ٻ�ʿ��
    /// </summary>
    private void SpawnEnemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape, int stun_time)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.moveRotate = rot;
        if (m.moveRotate.x > 0)
            m.transform.localScale = new Vector2(-1, 1);
        // һЩ��ʼ���ֶ������ܱ����е�Ч��
        Func<BaseUnit, BaseUnit, bool> noSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        CustomizationTask task = new CustomizationTask();
        int totalTime = 90;
        int currentTime = 0;
        task.AddOnEnterAction(delegate
        {
            m.transform.position = startV2; // �Գ�ʼ������н�һ������
            m.AddCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(noBlockFunc); // ���ɱ��赲
            m.AddCanHitFunc(noHitFunc); // ���ɱ��ӵ�����
            m.SetAlpha(0); // 0͸����
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
            return true;
        });
        task.AddOnExitAction(delegate
        {
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
            // ������ѣ
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        });
        m.AddTask(task);
    }

    /// <summary>
    /// ����һ��������
    /// </summary>
    private BaseUnit CreateFogCreator(MouseUnit master, float hp, bool isLeft, int waitTime, int type, int shape, int stun_time, int fog_time, float burn_rate)
    {
        MouseModel m = MouseModel.GetInstance(FogCreator_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = master.transform.right;
            m.transform.localScale = new Vector2(hscale, (isLeft ? 1 : -1));
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, 1f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // ����Ϊ�������
            retinueList.Add(m);

            // ����
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
                        // �ٻ�һֻС�ֲ��ͷ�����
                        if (m.IsAlive())
                        {
                            Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                            Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                            FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + MapManager.gridHeight*rot);
                            e.SetOpen();
                            CustomizationTask t = new CustomizationTask();
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
                            SpawnEnemy(pos + 0.5f * rot * MapManager.gridWidth, pos + 1.0f * rot * MapManager.gridWidth, Vector2.left, type, shape, stun_time);
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
                    m.MDestory(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 2; // ͼ��-2
        }
        return m;
    }


    /// <summary>
    /// ����һö����
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateFireBullet(BaseUnit master, Vector2 pos, Vector2 rotate)
    {
        EnemyBullet b = EnemyBullet.GetInstance(FireBullet_RuntimeAnimatorController, master, 0);
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(36);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.mCircleCollider2D.radius = 0.25f*MapManager.gridWidth;
        b.AddHitAction((b, u)=> {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(u.transform.position, 0.5f, 0.5f, "BothCollide");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.isAffectFood = true;
            r.SetAffectHeight(0);
            r.SetOnFoodEnterAction((u) => {
                BurnManager.BurnDamage(master, u);
            });
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(master, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����һ��ͶӰ
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
            // �˺�ת�Ƹ�����ķ���
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
            // ����Ϊ�������
            retinueList.Add(m);
            // ������è������Ҳ������
            m.canTriggerCat = false;
            m.canTriggerLoseWhenEnterLoseLine = false;
            m.isBoss = true;

            // ����
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate
                {
                    m.mCurrentHp = mCurrentHp; // ����ֵ��BOSS����ͬ��
                    return false;
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            // m.UpdateRenderLayer(GetBody(0).spriteRenderer.sortingOrder - 1); // ͼ��-1
        }
        return m;
    }

    /// <summary>
    /// ���ɴ���̨���г�
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTrain()
    {
        int wait = Mathf.FloorToInt(GetParamValue("t2_0") * 60);
        int num = Mathf.FloorToInt(GetParamValue("num2_0"));

        CustomizationTask fogTask = null;

        List<BaseUnit> compList = new List<BaseUnit>();
        // ���ɳ�ͷͶӰ
        BaseUnit head = CreateProjection(Head_RuntimeAnimatorController, false);
        compList.Add(head);
        // ���ɳ�������̨ͶӰ
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));
        MouseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);
        compList.Add(fireBulletAttacker);
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        // ����Ϊ����Ϊ����
        foreach (var u in compList)
        {
            u.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // ������Ϊѡȡ��Ŀ��
            u.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            u.AddCanHitFunc(noHitFunc); // ���ɱ��ӵ�����
            u.CloseCollision();
            u.moveRotate = Vector2.left;
            u.transform.position = MapManager.GetGridLocalPosition(8, -3); // ֱ�Ӳ�����Ļ���
        }
        float maxDist = GetHeadToBodyLength();
        float distLeft = GetHeadToBodyLength();
        int count = 1; // ���ֵĳ����������ͷ����̨Ҳ��ģ�
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
            // ����ǰ��
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
        // ͣ��ʱ�������ۣ���ͷ�͵�һ�ڳ������ǰ������ʧ����β������ʧ
        t.AddTaskFunc(delegate {
            // ����ǰ��
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
                // ��̨���𣬱�ÿɱ�����
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                // �������������
                foreach (var u in compList)
                {
                    if (u != fireBulletAttacker)
                        u.MDestory();
                }
                return true;
            }
            return false;
        });
        // ����̨
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // ��һ���
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
        // ��������
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
                    // תһ��
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // ��ת��
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // �ٵ�һ���
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
        // ��������
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
                    // תһ��
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // ����������׷������������
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
        // �Ա���
        t.AddTaskFunc(delegate {
            if (fogTask.IsEnd())
            {
                // ��ը��Ч
                {
                    BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "BoomDie", null, false);
                    e.transform.position = fireBulletAttacker.transform.position;
                    GameController.Instance.AddEffect(e);
                }
                fireBulletAttacker.MDestory();
                // ����3*3����Ч��
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
    /// ���������P3����̨
    /// </summary>
    private CustomizationTask GetFogAreaToFireBulletAttackerTask(MouseUnit master, int sortingOrder)
    {
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex) * 60);
        int fog2_0 = Mathf.FloorToInt(GetParamValue("fog2_0", mHertIndex) * 60);
        int soldier_type2_0 = Mathf.FloorToInt(GetParamValue("soldier_type2_0", mHertIndex));
        int soldier_shape2_0 = Mathf.FloorToInt(GetParamValue("soldier_shape2_0", mHertIndex));
        int stun2_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp2_0 = GetParamValue("fog_hp2_0", mHertIndex);
        float burn_rate = 1 - GetParamValue("burn_defence2")/100;


        BaseUnit[] arr = new BaseUnit[2];
        arr[0] = CreateFogCreator(master, fog_hp2_0, true, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0, burn_rate);
        arr[0].NumericBox.BurnRate.AddModifier(new FloatModifier(0.25f));
        arr[0].GetSpriteRenderer().sortingOrder = sortingOrder;
        arr[1] = CreateFogCreator(master, fog_hp2_0, false, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0, burn_rate);
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
                // ��������ط����������
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

                            SpawnEnemy(pos + 0.5f * new Vector2(v2.x, v2.y * r), pos + 1.0f * new Vector2(v2.x, v2.y * r), Vector2.left, soldier_type2_0, soldier_shape2_0, stun2_0);
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
    /// ֻ����һ������̨�ĳ�β������P3��������
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTail(Vector2 start)
    {
        int wait = Mathf.FloorToInt(GetParamValue("t2_0") * 60);
        int num = Mathf.FloorToInt(GetParamValue("num2_0"));
        float colIndex = 9 - GetParamValue("right_col3");

        BaseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // ������Ϊѡȡ��Ŀ��
        fireBulletAttacker.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
        fireBulletAttacker.AddCanHitFunc(noHitFunc); // ���ɱ��ӵ�����
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
                // ��̨���𣬱�ÿɱ�����
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                return true;
            }
            else
            {
                // ����ǰ��
                fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            }
            return false;
        });
        // ����̨
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // ��һ���
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
        // ��������
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
                    // תһ��
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // ��ת��
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = wait;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // �ٵ�һ���
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
        // ��������
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
                    // תһ��
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // �Ա���
        t.AddTaskFunc(delegate {
            // ��ը��Ч
            {
                BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "BoomDie", null, false);
                e.SetSpriteRendererSorting("Effect", 10);
                e.transform.position = fireBulletAttacker.transform.position;
                GameController.Instance.AddEffect(e);
            }
            fireBulletAttacker.MDestory();
            // ����3*3����Ч��
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
    /// ָ��-��������
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateLaserAttackerAction(float burn_rate)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                // �������������ĳ������ɼ��ⷢ����
                foreach (var body in GetBodyList())
                {
                    if (body != null && body.transform.position.x >= MapManager.GetColumnX(-1) && body.transform.position.x <= MapManager.GetColumnX(9))
                    {
                        if (body.transform.position.y > MapManager.GetRowY(2.5f))
                        {
                            CreateLaserAttacker(body, false, burn_rate);
                        }
                        else if (body.transform.position.y < MapManager.GetRowY(3.5f))
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
    /// ָ��-��Ϯ
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateFogCreatorAction(int waitTime, int type, int shape, int stun_time, int fog0_0, float fog_hp0_0, float burn_rate)
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
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0, burn_rate);
                        }else if(body.transform.position.y < MapManager.GetRowY(5.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, type, shape, stun_time, fog0_0, burn_rate);
                        }
                        else
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, type, shape, stun_time, fog0_0, burn_rate);
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0, burn_rate);
                        }
                    }
                }
            }
        };
        return action;
    }

    /// <summary>
    /// ����һ���ƶ���صļ��ܣ�������ķ����ã�
    /// </summary>
    /// <param name="pointList">��ʼ��,������ ��</param>
    /// <param name="waitDist">ͣ��ǰ��Ҫ�ƶ��ľ���</param>
    /// <param name="wait">ͣ��ʱ��</param>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility CreateMovementFunc(List<Vector2[]> pointList, float waitDist, int wait, Func<List<Action<int, int>>> createActionListFunc)
    {
        // ������ʼ��
        float distLeft = 0;
        int timeLeft = 0;
        List<Action<int, int>> WaitTaskActionList = null;
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
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
            // �Ƿ�ִ�ͣ����
            c.AddSpellingFunc(delegate {
                distLeft -= GetMoveSpeed();
                Debug.Log("distLeft=" + distLeft);
                if (distLeft <= 0)
                {
                    AddHeadMoveDist(distLeft); // ���겹��һ��
                    CancelSetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new IdleState(this));
                    timeLeft = wait;
                    return true;
                }
                return false;
            });
            // �ȴ�
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    foreach (var action in WaitTaskActionList)
                        action(timeLeft, wait); // ִ��ָ��
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
            // �Ƿ�ִ��յ�
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
    /// ����ͣ����������
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 1, x3 = 1, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 6f), new Vector2(x2, 6f) },
                new Vector2[2] { new Vector2(x3, 0f), new Vector2(x4, 0f) }
            }, // ��ʼ�����յ�
            (x1 - x2 - x3 + 8) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // ����ͣ��ǰ���ƶ�����
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// ����ͣ����ż����
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0, x3 = 0, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(x1, 0f), new Vector2(x2, 0f) }, 
                new Vector2[2] { new Vector2(x3, 6f), new Vector2(x4, 6f) }
            }, // ��ʼ�����յ�
            (x1 - x2 - x3 + 7)*MapManager.gridWidth + GetHeadToBodyLength() + 5*GetBodyLength(), // ����ͣ��ǰ���ƶ�����
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            ); 
    }

    /// <summary>
    /// ����ͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0.5f, x3 = 0.5f, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 1f), new Vector2(x2, 1f) },
                new Vector2[2] { new Vector2(x3, 5f), new Vector2(x4, 5f) }
            }, // ��ʼ�����յ�
            (x1 - x2 - x3 + 7) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // ����ͣ��ǰ���ƶ�����
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// ����ͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement3(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x1 = 11, x2 = 0.5f, x3 = 0.5f, x4 = 38;

        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(x1, 5f), new Vector2(x2, 5f) },
                new Vector2[2] { new Vector2(x3, 1f), new Vector2(x4, 1f) }
            }, // ��ʼ�����յ�
            (x1 - x2 - x3 + 7) * MapManager.gridWidth + GetHeadToBodyLength() + 5 * GetBodyLength(), // ����ͣ��ǰ���ƶ�����
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// �������֥ʿͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility GetUltimateSkill()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float hp = GetParamValue("hp0", mHertIndex);
        float burn_rate0 = 1 - GetParamValue("burn_defence0") / 100;
        float burn_rate1_1 = 1 - GetParamValue("burn_defence1_1") / 100;

        float x1 = 9, x2 = 1, x3 = 1, x4 = 9, x5 = 10, x6 = -3;




        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            c.ActivateChildAbility(CreateMovementFunc(
                new List<Vector2[]>() {
                    new Vector2[2]{ new Vector2(x1, 0f), new Vector2(x2, 0f) },
                    new Vector2[2] { new Vector2(x3, 6f), new Vector2(x4, 6f) },
                    new Vector2[2] { new Vector2(x5, 3f), new Vector2(x6, 3f) },
                }, // ��ʼ�����յ�
                (x1 - x2 - x3 + x4 + x5 - 1) * MapManager.gridWidth + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
                Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
                delegate {
                    List<Action<int, int>> actionList = new List<Action<int, int>>();
                    actionList.Add(CreateLaserAttackerAction(burn_rate1_1));
                    actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, hp, burn_rate0));
                    return actionList;
                } // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
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
                // �ж���̨�Ƿ���ʧ����ʧ��Ϊ�ü��ܽ����ı�־
                c.AddSpellingFunc(delegate
                {
                    return u == null || !u.IsAlive();
                });
            }
            // �Ƿ�ִ��յ�
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
    /// �ڽ����ƶ�״̬ʱ������һ��Ч���������ը���ֹͣһ��ʱ��
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if (GetBodyList().Count > 0)
            GetBody(GetBodyList().Count - 1).AddActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        base.OnMoveStateEnter();
    }

    public override void OnMoveState()
    {
        if (stun_timeLeft > 0)
        {
            DisableMove(true);
            stun_timeLeft--;
        }
        else
        {
            DisableMove(false);
        }
        base.OnMoveState();
    }

    public override void OnMoveStateExit()
    {
        if (GetBodyList().Count > 0)
            GetBody(GetBodyList().Count - 1).RemoveActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        base.OnMoveStateExit();
    }
}
