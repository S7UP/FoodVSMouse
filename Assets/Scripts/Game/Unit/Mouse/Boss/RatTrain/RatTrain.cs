using System;
using System.Collections.Generic;

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

    private int attackRound; // ��ǰ����������һ�μ���ѭ����Ϊһ�֣�

    public override void Awake()
    {
        if(SummonSoldier_RuntimeAnimatorController == null)
        {
            SummonSoldier_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/SummonSoldier");
            MissileAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/MissileAttacker");
            Missile_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Missile");
        }
        base.Awake();
    }

    public override void MInit()
    {
        attackRound = 0;
        retinueList.Clear();
        base.MInit();
        // ���ú�������
        hscale = 1.087f;
        // ����10�ڳ���
        CreateHeadAndBody(10);
        // ���ó�ͷ��������β���˺�����
        GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
        // Ĭ���ٶ�Ϊ2.4�ݸ�/ÿ��
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridHeight/60);
        SetAllComponentNoBeSelectedAsTargetAndHited();
        SetActionState(new MoveState(this));
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
    public override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.667f, 0.333f });
        // �ƶ��ٶ�
        AddParamArray("speed_rate", new float[] { 1.0f, 1.25f, 1.5f });
        // ��ͷ��������β�����˱���
        AddParamArray("head_normal", new float[] { 1.0f });
        AddParamArray("head_burn", new float[] { 1.0f });
        AddParamArray("body_normal", new float[] { 1.0f });
        AddParamArray("body_burn", new float[] { 1.0f });
        AddParamArray("tail_normal", new float[] { 1.0f });
        AddParamArray("tail_burn", new float[] { 10.0f }); // ��β10�������˺�
        // ���ҵ���ͣ��ʱ��
        AddParamArray("wait0", new float[] { 7, 5.6f, 4.6f});
        // �������м�ͣ��ʱ��
        AddParamArray("wait1", new float[] { 7, 5.6f, 4.6f});

        // ָ��-�ڻ�
        AddParamArray("t0_0", new float[] { 3, 1.5f, 0 });
        AddParamArray("dmg0_0", new float[] { 900 });
        // ָ��-��Ϯ
        AddParamArray("t1_0", new float[] { 2, 1, 0 });
        AddParamArray("soldier_type", new float[] { 0, 0, 20 });
        AddParamArray("soldier_shape", new float[] { 7, 8, 0 });
        AddParamArray("stun1_0", new float[] { 1, 2, 3 });
    }

    /// <summary>
    /// �л��׶Ρ����ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        // ���ú�������
        hscale = 1.087f;

        // ���û�ȡ������ķ���
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(GetNextAttackList);

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

    private void LoadP1SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);


        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                return actionList;
            }
            ));
        list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP2SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);


        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
         list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex);
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60);


        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement2(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement3(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        list.Add(Movement4(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement5(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
                actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
                return actionList;
            }
            ));
        list.Add(Movement6(
           delegate
           {
               List<Action<int, int>> actionList = new List<Action<int, int>>();
               actionList.Add(CreateMissileAttackAction(dmg0_0, t0_0)); // ָ��-�ڻ�
               actionList.Add(CreateEnemySpawnerAction(t1_0, soldier_type, soldier_shape, stun1_0)); // ָ��-��Ϯ
               return actionList;
           }
           ));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// ��ȡ��һ�鹥������˳��
    /// </summary>
    /// <returns></returns>
    private List<int> GetNextAttackList()
    {
        List<int> list = new List<int>();
        // �����û���ñ༭���Զ��幥��˳��
        if(!IsParamValueInValidOrOutOfBound("attack_order", attackRound))
        {
            int val = Mathf.FloorToInt(GetParamValue("attack_order", attackRound));
            while(val > 0)
            {
                int i = val % 10;
                i = Mathf.Max(0, i - 1);
                list.Insert(0, i);
                val /= 10;
            }
        }
        else
        {
            // ���û�еĻ���BOSS�ĵ�ǰ������������
            List<int> l = new List<int>() { 0,1,2,3,4,5,6 };
            while (l.Count > 0)
            {
                int i = GetRandomNext(0, l.Count);
                list.Add(l[i]);
                l.Remove(l[i]);
            }
        }
        attackRound++;
        return list;
    }

    public override void BeforeDeath()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteDeath();
        }
        retinueList.Clear();
        base.BeforeDeath();
    }

    public override void BeforeBurn()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteBurn();
        }
        retinueList.Clear();
        base.BeforeBurn();
    }

    public override void BeforeDrop()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteDrop();
        }
        retinueList.Clear();
        base.BeforeDrop();
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
        task.OnEnterFunc = delegate
        {
            m.transform.position = startV2; // �Գ�ʼ������н�һ������
            m.AddCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(noBlockFunc); // ���ɱ��赲
            m.AddCanHitFunc(noHitFunc); // ���ɱ��ӵ�����
            m.SetAlpha(0); // 0͸����
        };
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
        task.OnExitFunc = delegate
        {
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
            // ������ѣ
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        };
        m.AddTask(task);
    }

    /// <summary>
    /// ����һ������������
    /// </summary>
    private void CreateEnemySwpaner(RatTrainComponent master, bool isLeft, int waitTime, int type, int shape, int stun_time)
    {
        MouseModel m = MouseModel.GetInstance(SummonSoldier_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(900, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate * master.moveRotate.y;
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, (isLeft?1:-1)*0.49f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 1.49f * MapManager.gridHeight);
            m.isBoss = true;
            // ����Ϊ�������
            retinueList.Add(m);

            // ����
            {
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate
                {
                    if(isLeft)
                        m.animatorController.Play("PreSpawn1");
                    else
                        m.animatorController.Play("PreSpawn0");
                };
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
                t.OnExitFunc = delegate {
                    m.ExecuteRecycle(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                };
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.UpdateRenderLayer(master.spriteRenderer.sortingOrder - 2); // ͼ��-2
        }
    }


    /// <summary>
    /// ����һö�ڵ�
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateMissile(BaseUnit master, Vector2 pos, Vector2 rotate, float dmg)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Missile_RuntimeAnimatorController, master, 0);
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(24);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.AddHitAction((b, u)=> {
            BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(master, dmg, u.transform.position, 1, 1);
            r.isAffectFood = true;
            r.isAffectCharacter = false;
            r.isAffectMouse = true;
            GameController.Instance.AddAreaEffectExecution(r);
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����һ���ڵ�������
    /// </summary>
    private void CreateMissileAttacker(RatTrainComponent master, bool isAttackLeft, int waitTime, float dmg)
    {
        MouseModel m = MouseModel.GetInstance(MissileAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(900, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            if(isAttackLeft)
                m.transform.right = master.moveRotate*Mathf.Sign(master.moveRotate.y);
            else
                m.transform.right = -master.moveRotate * Mathf.Sign(master.moveRotate.y);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, 0.49f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 1.49f * MapManager.gridHeight);
            m.isBoss = true;
            // ����Ϊ�������
            retinueList.Add(m);

            // ����
            {
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate
                {
                    m.animatorController.Play("PreAttack");
                };
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
                            CreateMissile(master, pos + 0.5f * rot * MapManager.gridWidth, rot, dmg);
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
                t.OnExitFunc = delegate {
                    m.ExecuteRecycle(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                };
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.UpdateRenderLayer(master.spriteRenderer.sortingOrder - 1); // ͼ��-1
        }
    }

    /// <summary>
    /// ָ��-�ڻ�
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateMissileAttackAction(float dmg, int wait_time)
    {
        int index = 0;
        int j = 0;
        int interval = 20;
        Action<int, int> action = (timeLeft, totalTime) => {
            if ((totalTime - timeLeft) == interval * (j + 1))
            {
                bool flag = false;
                RatTrainBody body = GetBody(index);
                while (!flag)
                {
                    flag = (body!=null && !body.IsHide() && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f));
                    if (flag)
                    {
                        if (body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateMissileAttacker(body, true, wait_time, dmg);
                        }
                        else if (body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateMissileAttacker(body, false, wait_time, dmg);
                        }
                        else
                        {
                            CreateMissileAttacker(body, true, wait_time, dmg);
                            CreateMissileAttacker(body, false, wait_time, dmg);
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
    private Action<int, int> CreateEnemySpawnerAction(int waitTime, int type, int shape, int stun_time)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 1)
            {
                foreach (var body in GetBodyList())
                {
                    if (body != null && !body.IsHide() && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f))
                    {
                        if(body.transform.position.x > MapManager.GetColumnX(4.5f))
                        {
                            CreateEnemySwpaner(body, true, waitTime, type, shape, stun_time);
                        }else if(body.transform.position.x < MapManager.GetColumnX(3.5f))
                        {
                            CreateEnemySwpaner(body, false, waitTime, type, shape, stun_time);
                        }
                        else
                        {
                            CreateEnemySwpaner(body, true, waitTime, type, shape, stun_time);
                            CreateEnemySwpaner(body, false, waitTime, type, shape, stun_time);
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
        float dist = 0;
        int timeLeft = 0;
        bool flag = false;
        List<Action<int, int>> WaitTaskActionList = null;
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            isMoveToDestination = false;
            timeLeft = 0;
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
            {
                Vector2 start = p[0];
                Vector2 end = p[1];
                list.Add(new Vector2[] { new Vector2(start.x, start.y), new Vector2(end.x + 0.01f, end.y) });
            }
            dist = waitDist;
            WaitTaskActionList = createActionListFunc();
            flag = false;
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // �Ƿ�ִ�ͣ����
            c.AddSpellingFunc(delegate {
                // if (GetHead().mCurrentActionState is MoveState || (GetHead().mCurrentActionState is TransitionState && GetHead().IsDisappear()))
                if(GetRouteQueue().Count <= 0)
                {
                    if(flag)
                        dist -= GetHead().DeltaPosition.magnitude;
                    else
                        flag = true;
                }
                    
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
                    {
                        action(timeLeft, wait); // ִ��ָ��
                    }
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
                if (isMoveToDestination)
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
        return CreateMovementFunc(
            new List<Vector2[]>() { new Vector2[2]{ new Vector2(4f, -3f), new Vector2(4f, 27f) } }, // ��ʼ�����յ�
            MapManager.GetRowY(-3) - MapManager.GetRowY(6) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ���ڵ���������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
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
        float y;
        if(mHertIndex == 0)
        {
            y = -2;
        }else if(mHertIndex == 1)
        {
            y = -1.5f;
        }
        else
        {
            y = -1.25f;
        }
        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(6f, 8f), new Vector2(6f, y) },
                new Vector2[2]{ new Vector2(1f, -3f), new Vector2(1f, 27f) }
            }, // ��ʼ�����յ�
            MapManager.GetRowY(-3) - MapManager.GetRowY(6) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ���ڵ���������Ҫ�ߵ�·��
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
        float y;
        if (mHertIndex == 0)
        {
            y = -2;
        }
        else if (mHertIndex == 1)
        {
            y = -1.5f;
        }
        else
        {
            y = -1.25f;
        }
        return CreateMovementFunc(
            new List<Vector2[]>() {
                new Vector2[2]{ new Vector2(2f, 8f), new Vector2(2f, y) },
                new Vector2[2]{ new Vector2(7f, -3f), new Vector2(7f, 27f) }
            }, // ��ʼ�����յ�
            MapManager.GetRowY(-3) - MapManager.GetRowY(6) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ���ڵ���������Ҫ�ߵ�·��
            Mathf.FloorToInt(GetParamValue("wait1", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            );
    }
}
