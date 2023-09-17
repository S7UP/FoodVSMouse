using System;
using System.Collections.Generic;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �г�һ��
/// </summary>
public class RatTrain : BaseRatTrain
{
    private static RuntimeAnimatorController SummonSoldier_RuntimeAnimatorController;
    private static RuntimeAnimatorController MissileAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController Missile_RuntimeAnimatorController;
    private FloatModifier moveSpeedModifier = new FloatModifier(0); // ����������ǩ
    private List<BaseUnit> retinueList = new List<BaseUnit>(); // ������ӱ�������ʱͬʱ����������ӣ�

    private List<int> attack_orderList = new List<int>(); // ����˳���

    private float dmgRecord; // �ܵ����˺��ܺ�
    private int stun_timeLeft; // ��ѣʱ��
    private Action<CombatAction> burnAction; // ��ը�¼�

    private RingUI FinalSkillRingUI;

    public override void Awake()
    {
        if(SummonSoldier_RuntimeAnimatorController == null)
        {
            SummonSoldier_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/SummonSoldier");
            MissileAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/MissileAttacker");
            Missile_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Missile");
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
        attack_orderList.Clear();
        dmgRecord = 0;
        stun_timeLeft = 0;
        retinueList.Clear();
        base.MInit();
        // ���ú�������
        hscale = 1.11f;
        // ����10�ڳ���
        CreateHeadAndBody(10);
        // ���ó�ͷ��������β���˺�����
        GetHead().SetDmgRate(GetParamValue("head_normal"), GetParamValue("head_burn"));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal"), GetParamValue("body_burn"));
        }
        // ��β����
        {
            GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal"), GetParamValue("tail_burn"));
        }
        
        // Ĭ���ٶ�Ϊ2.4�ݸ�/ÿ��
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridHeight/60);
        SetAllComponentNoBeSelectedAsTargetAndHited();
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
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.RatTrain, 0))
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
        // ���ú�������
        hscale = 1.087f;

        // ���û�ȡ������ķ���
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(delegate { return attack_orderList; });

        if(mHertIndex == 0)
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
    private CustomizationSkillAbility GetUltimateSkill()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type1"));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape1"));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // ��̨����
        float hp0 = GetParamValue("hp0");
        float extra_burn_defence0 = GetParamValue("extra_burn_defence0")/100;
        // ���ʹ�����
        float hp1 = GetParamValue("hp1");
        float extra_burn_defence1 = GetParamValue("extra_burn_defence1")/100;

        return Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, extra_burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, extra_burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            );
    }

    private void LoadP1SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // ��̨����
        float hp0 = GetParamValue("hp0");
        float burn_defence0 = GetParamValue("burn_defence0")/100;
        // ���ʹ�����
        float hp1 = GetParamValue("hp1");
        float burn_defence1 = GetParamValue("burn_defence1")/100;

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(GetUltimateSkill());
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP2SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // ��̨����
        float hp0 = GetParamValue("hp0");
        float burn_defence0 = GetParamValue("burn_defence0");
        float extra_burn_defence0 = GetParamValue("extra_burn_defence0")/100;
        // ���ʹ�����
        float hp1 = GetParamValue("hp1");
        float burn_defence1 = GetParamValue("burn_defence1");
        float extra_burn_defence1 = GetParamValue("extra_burn_defence1")/100;

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(GetUltimateSkill());
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
         list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);
        // ��̨����
        float hp0 = GetParamValue("hp0");
        float burn_defence0 = GetParamValue("burn_defence0");
        float extra_burn_defence0 = GetParamValue("extra_burn_defence0")/100;
        // ���ʹ�����
        float hp1 = GetParamValue("hp1");
        float burn_defence1 = GetParamValue("burn_defence1");
        float extra_burn_defence1 = GetParamValue("extra_burn_defence1")/100;

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(GetUltimateSkill());
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateMissileAttackAction(hp0, burn_defence0, t0_0)); // ָ��-�ڻ�
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0, hp1, burn_defence1)); // ָ��-��Ϯ
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
    /// �ڽ����ƶ�״̬ʱ������һ��Ч���������ը���ֹͣһ��ʱ��
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if(GetBodyList().Count>0)
            GetBody(GetBodyList().Count - 1).AddActionPointListener(ActionPointType.PostReceiveDamage, burnAction);
        base.OnMoveStateEnter();
    }

    public override void OnMoveState()
    {
        if(stun_timeLeft > 0)
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
    /// ����һ������������
    /// </summary>
    private void CreateEnemySwpaner(RatTrainComponent master, bool isLeft, int waitTime, int type, int shape, int stun_time, float hp, float burn_defence)
    {
        MouseModel m = MouseModel.GetInstance(SummonSoldier_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(1 - burn_defence));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate * master.moveRotate.y;
            m.transform.localScale = new Vector2(hscale, 1);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, (isLeft?-1:1)*0.98f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            m.canTriggerCat = false;
            // ����Ϊ�������
            retinueList.Add(m);

            // ����
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
                        // �ٻ�һֻС��
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
                    m.MDestory(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 2; // ͼ��-2
            // m.UpdateRenderLayer(master.spriteRenderer.sortingOrder - 2); // ͼ��-2
        }
    }


    /// <summary>
    /// ����һö�ڵ�
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateMissile(BaseUnit master, Vector2 pos, Vector2 rotate)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Missile_RuntimeAnimatorController, master, 0);
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(24);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.AddHitAction((b, u)=> {
            // �˺�����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 0.5f, 0.5f, "CollideGrid");
                r.SetInstantaneous();
                r.isAffectGrid = true;
                r.SetOnGridEnterAction((g) => {
                    g.TakeAction(this, (u) => {
                        BurnManager.BurnDamage(master, u);
                    }, false);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // �˺�����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1f, 1f, "ItemCollideEnemy");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    if(!u.IsBoss())
                        BurnManager.BurnDamage(master, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����һ���ڵ�������
    /// </summary>
    private void CreateMissileAttacker(RatTrainComponent master, bool isAttackLeft, int waitTime, float hp, float burn_defence)
    {
        MouseModel m = MouseModel.GetInstance(MissileAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(hp, 1, 1f, 0, 100, 0, 0);
            m.NumericBox.BurnRate.AddModifier(new FloatModifier(1-burn_defence));
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            if(isAttackLeft)
                m.transform.right = master.moveRotate * Mathf.Sign(master.moveRotate.y);
            else
                m.transform.right = -master.moveRotate * Mathf.Sign(master.moveRotate.y);
            m.transform.localScale = new Vector2(hscale, 1);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, 0.98f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            m.canTriggerCat = false;
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
                        // ����һö�ڵ�
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
                    m.MDestory(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.spriteRenderer.sortingOrder = master.spriteRenderer.sortingOrder - 1; // ͼ��-1
        }
    }

    /// <summary>
    /// ָ��-�ڻ�
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateMissileAttackAction(float hp, float burn_defence, int wait_time)
    {
        int index = 0;
        int j = 0;
        int interval = 10;
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
    /// ָ��-��Ϯ
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
    /// ����һ���ƶ���صļ��ܣ�������ķ����ã�
    /// </summary>
    /// <param name="pointList">��ʼ��,������ ��</param>
    /// <param name="waitDist">ͣ��ǰ��Ҫ�ƶ��ľ���</param>
    /// <param name="wait">ͣ��ʱ��</param>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility CreateMovementFunc(List<Vector2[]> pointList, float waitDist, int wait, Func<List<Action<int, int>>> createActionListFunc)
    {
        // ����
        int timeLeft = 0;
        float distLeft = 0;
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
                // Debug.Log("distLeft="+distLeft);
                if (distLeft <= 0)
                {
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
    /// �Ҳ�ͣ����������
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2]{ new Vector2(8f, 8f), new Vector2(8f, -22f) } }, // ��ʼ�����յ�
            MapManager.GetRowY(0) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            ); 
    }

    /// <summary>
    /// �Ҳ�ͣ����ż����
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2]{ new Vector2(8f, 8f), new Vector2(8f, -22f) } }, // ��ʼ�����յ�
            MapManager.GetRowY(1) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// �г�ͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(Func<List<Action<int, int>>> createActionListFunc)
    {
        int wait = Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60);
        List<Vector2[]> pointList = new List<Vector2[]>() { new Vector2[2] { new Vector2(4f, -3f), new Vector2(4f, 27f) } };

        // ������ʼ��
        float dist = 0;
        int timeLeft = 0;
        List<Action<int, int>> WaitTaskActionList = null;
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            FinalSkillRingUI.Show();
            timeLeft = 0;
            SetHeadMoveDestination();
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
                list.Add(new Vector2[] { p[0], p[1] });
            dist = 9*MapManager.gridHeight + GetHeadToBodyLength(); // �������һ�γ����һ��ͣ�ڵ�7������Ҫ��·��
            WaitTaskActionList = createActionListFunc();
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // �Ƿ�ִ�ͣ����
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
                    // ����һ�Σ����Ҫ���㳵β���ڶ��еľ���
                    dist += 9 * GetBodyLength() - 4.5f*MapManager.gridHeight;
                    WaitTaskActionList = createActionListFunc(); // ���õȴ�����
                    return true;
                }
            });
            // �Ƿ�ִ�ͣ����
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





        //return CreateMovementFunc(
        //    new List<Vector2[]>() { new Vector2[2]{ new Vector2(4f, -3f), new Vector2(4f, 27f) } }, // ��ʼ�����յ�
        //    MapManager.GetRowY(-3) - MapManager.GetRowY(6) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ���ڵ���������Ҫ�ߵ�·��
        //    Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
        //    createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
        //    );
    }

    /// <summary>
    /// ����ͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement3(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(6f, 9f), new Vector2(6f, -3.5f) },
                new Vector2[2]{ new Vector2(1f, -3.5f), new Vector2(1f, 27f) }
            }, // ��ʼ�����յ�
            22*MapManager.gridHeight + GetHeadToBodyLength(),
            Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// ���ͣ����������
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement4(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2] { new Vector2(0f, 8f), new Vector2(0f, -22f) } }, // ��ʼ�����յ�
            MapManager.GetRowY(0) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// ���ͣ����ż����
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement5(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2] { new Vector2(0f, 8f), new Vector2(0f, -22f) } }, // ��ʼ�����յ�
            MapManager.GetRowY(1) - MapManager.GetRowY(8) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }

    /// <summary>
    /// ����ͣ��������
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement6(Func<List<Action<int, int>>> createActionListFunc)
    {
        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(2f, 9f), new Vector2(2f, -3.5f) },
                new Vector2[2]{ new Vector2(7f, -3.5f), new Vector2(7f, 27f) }
            }, // ��ʼ�����յ�
            22 * MapManager.gridHeight + GetHeadToBodyLength(),
            Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }
}
