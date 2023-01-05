using System;
using System.Collections.Generic;

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
    private Dictionary<int, List<BaseUnit>> laserAttackerPairDict = new Dictionary<int, List<BaseUnit>>(); // ���ⷢ�������ֵ�
    

    private int attackRound; // ��ǰ����������һ�μ���ѭ����Ϊһ�֣�

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
        base.Awake();
    }

    public override void MInit()
    {
        attackRound = 0;
        retinueList.Clear();
        laserAttackerPairDict.Clear();
        base.MInit();
        // ����13�ڳ���
        CreateHeadAndBody(13);
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
        // ��·��������ʽͣ��ʱ��
        AddParamArray("wait0", new float[] { 9, 7, 6});
        // ��������Ϯ��ʽͣ��ʱ��
        AddParamArray("wait1", new float[] { 9, 7, 6});

        // ָ��-��Ϯ
        AddParamArray("t0_0", new float[] { 3, 3, 2 });
        AddParamArray("fog0_0", new float[] { 3, 4, 5 });
        AddParamArray("soldier_type", new float[] { 0, 0, 20 });
        AddParamArray("soldier_shape", new float[] { 7, 8, 0 });
        AddParamArray("stun0_0", new float[] { 1, 2, 3 });
        AddParamArray("fog_hp0_0", new float[] { 900 });

        // ָ��-��������
        AddParamArray("t1_0", new float[] { 5, 3, 2 });
        AddParamArray("dmg1_0", new float[] { 900 });
        AddParamArray("laser_hp", new float[] { 2700 });

        // ָ��-����֧Ԯ
        AddParamArray("t2_0", new float[] { 1.5f, 1, 0 });
        AddParamArray("num2_0", new float[] { 1, 2, 2 });
        AddParamArray("dmg2_0", new float[] { 900 });
        AddParamArray("dmg2_1", new float[] { 900 });
        AddParamArray("t2_1", new float[] { 0, 0, 0 });
        AddParamArray("soldier_type2_0", new float[] { 19, 19, 19 });
        AddParamArray("soldier_shape2_0", new float[] { 1, 1, 1 });
        AddParamArray("stun2_0", new float[] { 5, 5, 5 });
        AddParamArray("fog_hp2_0", new float[] { 3600 });
        AddParamArray("fog2_0", new float[] { 5, 5, 5 });
    }

    /// <summary>
    /// �л��׶Ρ����ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
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
    }

    private void LoadP1SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);
        
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP2SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);

        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);

        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
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
            List<int> l = new List<int>() { 0,1,2 };
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
    /// ����һ�����ⷢ����
    /// </summary>
    private BaseUnit CreateLaserAttacker(RatTrainComponent master, bool isLeft, float laser_hp)
    {
        MouseModel m = MouseModel.GetInstance(LaserAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true)); // ���߻ҽ���ɱ
            m.SetBaseAttribute(laser_hp, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate;
            m.transform.localScale = new Vector2(1, (isLeft ? -1 : 1) * Mathf.Sign(master.moveRotate.x));
            // m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            // m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.mBoxCollider2D.offset = new Vector2(0, 1.0f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // �ܵ�һ�λҽ��˺���̶���ʧ50%�������ֵ��ʵ�˺�
            Action<CombatAction> OnBombDamage = (action) => {
                if (action is DamageAction)
                {
                    var damageAction = action as DamageAction;
                    if(damageAction is BombDamageAction)
                    {
                        damageAction.DamageValue = Mathf.Max(damageAction.DamageValue, 0.5f * m.mMaxHp); // �˺�ֵתΪ50%�������ֵ�˺���ԭ�˺���С��
                    }
                }
            };
            m.AddActionPointListener(ActionPointType.PreReceiveDamage, OnBombDamage);
            m.AddActionPointListener(ActionPointType.PreReceiveReboundDamage, OnBombDamage);
            // ����Ϊ�������
            retinueList.Add(m);
        }
        GameController.Instance.AddMouseUnit(m);
        m.UpdateRenderLayer(master.spriteRenderer.sortingOrder -1); // ͼ��-1
        return m;
    }

    /// <summary>
    /// ���ɼ��ⷢ����������ִ�������߼�������
    /// </summary>
    /// <param name="waitTime"></param>
    /// <param name="dmg"></param>
    private void LinkLaserAttackers(int waitTime, float dmg, float laser_hp)
    {
        laserAttackerPairDict.Clear();
        // ���ɼ��ⷢ�������Ҵ���ͬһ�еļ��ⷢ����
        foreach (var body in GetBodyList())
        {
            if (body != null && !body.IsHide() && body.transform.position.x >= MapManager.GetColumnX(-1) && body.transform.position.x <= MapManager.GetColumnX(9))
            {
                int columnIndex = int.MinValue;
                BaseUnit u = null;
                if (body.transform.position.y > MapManager.GetRowY(3f))
                {
                    u = CreateLaserAttacker(body, false, laser_hp);
                    columnIndex = body.GetColumnIndex();
                }
                else
                {
                    u = CreateLaserAttacker(body, false, laser_hp);
                    columnIndex = body.GetColumnIndex();
                }

                if(columnIndex != int.MinValue)
                {
                    if (!laserAttackerPairDict.ContainsKey(columnIndex))
                    {
                        laserAttackerPairDict.Add(columnIndex, new List<BaseUnit>());
                    }
                    if(u!=null)
                        laserAttackerPairDict[columnIndex].Add(u);
                }
            }
        }
        // Ϊ�������һ������ÿ֡��⼤�ⷢ�����Ĵ��������޳������ļ��ⷢ�������ڵȴ�ʱ��������ͷż���
        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate
        {
            foreach (var u in GetAllLaserAttacker())
            {
                u.animatorController.Play("PreAttack");
            }
        };
        t.AddTaskFunc(delegate
        {
            UpdateLaserAttackerPairDict();
            List<BaseUnit> l = GetAllLaserAttacker();
            if (l.Count <= 0)
                return true;
            if (l[0].animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                foreach (var u in l)
                {
                    u.animatorController.Play("Attack", true);
                }
                // ���ɼ�����Ч
                foreach (var keyValuePair in laserAttackerPairDict)
                {
                    List<BaseUnit> list = keyValuePair.Value;
                    if (list.Count >= 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            BaseUnit u = list[i];
                            Vector2 rot = u.transform.position.y > MapManager.GetRowY(3f) ? Vector2.down : Vector2.up;
                            BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PreAttack", "Disappear", true);
                            if (rot.Equals(Vector2.up))
                                e.transform.localScale = new Vector2(1, -1);
                            GameController.Instance.AddEffect(e);
                            u.AddEffectToDict(LaserEffectKey, e, 1f*Vector2.up*MapManager.gridHeight);
                        }
                    }
                }
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (waitTime > 0)
            {
                waitTime--;
                UpdateLaserAttackerPairDict();
                return false;
            }
            {
                List<BaseUnit> laserList = GetAllLaserAttacker();
                foreach (var u in laserList)
                {
                    u.animatorController.Play("PostAttack");
                    u.RemoveEffectFromDict(LaserEffectKey);
                    u.CloseCollision();
                }
                // ���伤��
                foreach (var keyValuePair in laserAttackerPairDict)
                {
                    List<BaseUnit> list = keyValuePair.Value;
                    if(list.Count >= 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            BaseUnit u = list[i];
                            Vector2 rot = u.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                            BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PostAttack", null, false);
                            if (rot.Equals(Vector2.up))
                                e.transform.localScale = new Vector2(1, -1);
                            GameController.Instance.AddEffect(e);
                            u.AddEffectToDict(LaserEffectKey, e, 1.5f*Vector2.up * MapManager.gridHeight);
                            // ���������ļ����ж�
                            BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(this, dmg, new Vector2(u.transform.position.x, MapManager.GetRowY(3)), 0.5f, 5);
                            r.isAffectFood = true;
                            r.isAffectMouse = true;
                            r.isAffectCharacter = false;
                            foreach (var laserUnit in retinueList)
                            {
                                if(laserUnit is MouseUnit)
                                {
                                    MouseUnit m = laserUnit as MouseUnit;
                                    r.AddExcludeMouseUnit(m); // ���ܻٵ���������
                                }
                            }
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                    }
                }
                return true;
            }
        });
        t.AddTaskFunc(delegate
        {
            UpdateLaserAttackerPairDict();
            List<BaseUnit> l = GetAllLaserAttacker();
            if (l.Count <= 0)
                return true;
            if (l[0].animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        t.OnExitFunc = delegate {
            foreach (var u in GetAllLaserAttacker())
            {
                u.ExecuteRecycle(); // ֱ�ӻ��գ���������������Ҳ�������������У�
            }
        };
        AddTask(t);
    }

    /// <summary>
    /// ���¼��ⷢ�����ֵ���Ķ������޳�����Ҫ������
    /// </summary>
    private void UpdateLaserAttackerPairDict()
    {
        foreach (var keyValuePair in laserAttackerPairDict)
        {
            List<BaseUnit> delList = new List<BaseUnit>();
            foreach (var u in keyValuePair.Value)
            {
                if (!u.IsAlive())
                    delList.Add(u);
            }
            foreach (var u in delList)
            {
                keyValuePair.Value.Remove(u);
            }
        }
    }

    /// <summary>
    /// ��ȡ���м��ⷢ����
    /// </summary>
    /// <returns></returns>
    private List<BaseUnit> GetAllLaserAttacker()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        foreach (var keyValuePair in laserAttackerPairDict)
        {
            foreach (var u in keyValuePair.Value)
            {
                list.Add(u);
            }
        }
        return list;
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
    /// ����һ��������
    /// </summary>
    private BaseUnit CreateFogCreator(BaseUnit master, float hp, bool isLeft, int waitTime, int type, int shape, int stun_time, int fog_time)
    {
        MouseModel m = MouseModel.GetInstance(FogCreator_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true)); // ���߻ҽ���ɱ
            m.SetBaseAttribute(hp, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate;
            m.transform.localScale = new Vector2(1, (isLeft?-1:1)*Mathf.Sign(master.moveRotate.x));
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
                            t.OnExitFunc = delegate {
                                e.SetDisappear();
                            };
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
                t.OnExitFunc = delegate {
                    m.ExecuteRecycle(); // ֱ�ӻ��գ���������������Ҳ�������������У�
                };
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.UpdateRenderLayer(master.GetSpriteRenderer().sortingOrder - 2); // ͼ��-2
        }
        return m;
    }


    /// <summary>
    /// ����һö����
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateFireBullet(BaseUnit master, Vector2 pos, Vector2 rotate, float dmg)
    {
        EnemyBullet b = EnemyBullet.GetInstance(FireBullet_RuntimeAnimatorController, master, 0);
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(18);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.mCircleCollider2D.radius = 0.25f*MapManager.gridWidth;
        b.AddHitAction((b, u)=> {
            BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(master, dmg, u.transform.position, 0.75f, 0.75f);
            r.isAffectFood = true;
            r.isAffectCharacter = false;
            r.isAffectMouse = true;
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
        MouseModel m = MouseModel.GetInstance(runtimeAnimatorController);
        {
            BossUnit.AddBossIgnoreDebuffEffect(m);
            m.SetBaseAttribute(mCurrentHp, 1, 1f, 0, 0, 0, 0);
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.mBoxCollider2D.offset = new Vector2(0, 0);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            // �˺�ת�Ƹ�����ķ���
            Action<CombatAction> transToMaster = (action) => {
                if(action is DamageAction)
                {
                    var damageAction = action as DamageAction;
                    float dmg = damageAction.DamageValue*((isFireAttacker && damageAction is BombDamageAction) ? GetParamValue("tail_burn", mHertIndex) : GetParamValue("tail_normal", mHertIndex));
                    new DamageAction(action.mActionType, action.Creator, this, dmg).ApplyAction();
                }
            };
            m.AddActionPointListener(ActionPointType.PreReceiveDamage, transToMaster);
            m.AddActionPointListener(ActionPointType.PreReceiveReboundDamage, transToMaster);
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
            m.UpdateRenderLayer(GetBody(0).spriteRenderer.sortingOrder - 1); // ͼ��-1
        }
        return m;
    }

    /// <summary>
    /// ���ɴ���̨���г�
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTrain(int t2_0, int totalAttackCount, float dmg, float boom_dmg)
    {
        List<BaseUnit> compList = new List<BaseUnit>();
        // ���ɳ�ͷͶӰ
        BaseUnit head = CreateProjection(Head_RuntimeAnimatorController, false);
        compList.Add(head);
        // ���ɳ�������̨ͶӰ
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));
        BaseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);
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
        int timeLeft = t2_0;
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
                        u.ExecuteRecycle();
                }
                return true;
            }
            return false;
        });
        // ����ǵ����׶Σ���׷��������
        if(mHertIndex >= 2)
        {
            t.AddTaskFunc(delegate {
                CreateFogAreaToFireBulletAttacker(fireBulletAttacker, fireBulletAttacker.GetSpriteRenderer().sortingOrder - 1);
                return true;
            });
        }

        // ����̨
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
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
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f*MapManager.gridWidth*rot, rot, dmg);
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
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
                timeLeft = t2_0;
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
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
        // ǰ���׶�������Ȼ���󣬵����׶ο�ʼԭ���Ա�
        if(mHertIndex < 2)
        {
            // ������,Ȼ������
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    fireBulletAttacker.animatorController.Play("Disappear");
                    fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                    fireBulletAttacker.AddCanHitFunc(noHitFunc);
                    return true;
                }
                return false;
            });
            // ����������������Լ�
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    fireBulletAttacker.ExecuteRecycle();
                    return true;
                }
                else
                {
                    fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
                }
                return false;
            });
        }
        else
        {
            // �Ա���
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "BoomDie", null, false);
                    e.transform.position = fireBulletAttacker.transform.position;
                    GameController.Instance.AddEffect(e);
                    fireBulletAttacker.ExecuteRecycle();
                    // ����3*3����Ч��
                    BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(this, boom_dmg, fireBulletAttacker.transform.position, 3, 3);
                    r.isAffectFood = true;
                    r.isAffectMouse = true;
                    GameController.Instance.AddAreaEffectExecution(r);
                    return true;
                }
                return false;
            });
        }

        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// ���������P3����̨
    /// </summary>
    private void CreateFogAreaToFireBulletAttacker(BaseUnit master, int sortingOrder)
    {
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex) * 60);
        int fog2_0 = Mathf.FloorToInt(GetParamValue("fog2_0", mHertIndex) * 60);
        int soldier_type2_0 = Mathf.FloorToInt(GetParamValue("soldier_type2_0", mHertIndex));
        int soldier_shape2_0 = Mathf.FloorToInt(GetParamValue("soldier_shape2_0", mHertIndex));
        int stun2_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp2_0 = GetParamValue("fog_hp2_0", mHertIndex);
        BaseUnit[] arr = new BaseUnit[2];
        arr[0] = CreateFogCreator(master, fog_hp2_0, true, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0);
        arr[0].GetSpriteRenderer().sortingOrder = sortingOrder;
        arr[1] = CreateFogCreator(master, fog_hp2_0, false, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0);
        arr[1].GetSpriteRenderer().sortingOrder = sortingOrder;
        int timeLeft = 67 + t2_1;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
                timeLeft--;
            else
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
                            FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + new Vector2(v2.x, v2.y*r));
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
                            t.OnExitFunc = delegate {
                                e.SetDisappear();
                            };
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
        AddTask(t);
    }

    /// <summary>
    /// ֻ����һ������̨�ĳ�β������P3��������
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTail(Vector2 start, int t2_0, int totalAttackCount, float dmg)
    {
        BaseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // ������Ϊѡȡ��Ŀ��
        fireBulletAttacker.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
        fireBulletAttacker.AddCanHitFunc(noHitFunc); // ���ɱ��ӵ�����
        fireBulletAttacker.moveRotate = Vector2.left;
        fireBulletAttacker.transform.position = start;
        float maxDist = GetBodyLength();
        float distLeft = GetBodyLength();
        int timeLeft = t2_0;
        int currentAttackCount = 0;
        Vector2 stop = MapManager.GetGridLocalPosition(5, 3);
        
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            distLeft -= mCurrentMoveSpeed;
            fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            if (distLeft <= 0)
            {
                fireBulletAttacker.animatorController.Play("Move");
                return true;
            }
            else
            {
                fireBulletAttacker.animatorController.Play("Appear", false, 0.75f * (1 - distLeft / maxDist));
                return false;
            }
        });
        t.AddTaskFunc(delegate {
            // ����ǰ��
            fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            if (fireBulletAttacker.transform.position.x < stop.x)
            {
                // ��̨���𣬱�ÿɱ�����
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                return true;
            }
            return false;
        });
        // ����̨
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
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
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
                timeLeft = t2_0;
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
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
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
        // ������,Ȼ������
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.animatorController.Play("Disappear");
                fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.AddCanHitFunc(noHitFunc);
                return true;
            }
            return false;
        });
        // ����������������Լ�
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.ExecuteRecycle();
                return true;
            }
            else
            {
                fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            }
            return false;
        });
        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// ָ��-��������
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateLaserAttackerAction(float dmg, int wait_time, float laser_hp)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                LinkLaserAttackers(wait_time, dmg, laser_hp);
            }
        };
        return action;
    }

    /// <summary>
    /// ָ��-��Ϯ
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateFogCreatorAction(int waitTime, int type, int shape, int stun_time, int fog0_0, float fog_hp0_0)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                foreach (var body in GetBodyList())
                {
                    if (body != null && !body.IsHide() && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f))
                    {
                        if(body.transform.position.y > MapManager.GetRowY(2.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
                        }else if(body.transform.position.y < MapManager.GetRowY(3.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
                        }
                        else
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, type, shape, stun_time, fog0_0);
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
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
            // ���ٸ���
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
            moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
            NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);

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
    /// ��·��������ʽͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x;
        if (mHertIndex == 0)
            x = 0.1f;
        else if (mHertIndex == 1)
            x = 0.9f;
        else
            x = 1.5f;
        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(8f, 0f), new Vector2(x, 0f) }, 
                new Vector2[2] { new Vector2(0f, 6f), new Vector2(9f+29f, 6f) }
            }, // ��ʼ�����յ�
            MapManager.GetColumnX(7) - MapManager.GetColumnX(0) + GetHeadToBodyLength(), // ����ӵ����г��ֵ���һ�ڳ���ͣ��
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
            createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
            ); 
    }

    /// <summary>
    /// ��������Ϯ��ʽͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc, int wait_time, int totalAttackCount, float dmg)
    {
        float x;
        if (mHertIndex == 0)
            x = 0;
        else if (mHertIndex == 1)
            x = 0.55f;
        else
            x = 0.9f;

        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            c.ActivateChildAbility(CreateMovementFunc(
                new List<Vector2[]>() {
                    new Vector2[2]{ new Vector2(9-x, 0f), new Vector2(x, 0f) },
                    new Vector2[2] { new Vector2(x, 6f), new Vector2(7.875f-x, 6f) },
                    new Vector2[2] { new Vector2(8f, 3f), new Vector2(0, 3f) },
                }, // ��ʼ�����յ�
                MapManager.GetColumnX(8f) - MapManager.GetColumnX(3) + GetHeadToBodyLength(), // ����ʹ��һ�ڳ���ͣ��������������Ҫ�ߵ�·��
                Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // ��ȡͣ��ʱ��
                createActionListFunc // ������ͣ��ʱִ�е�ָ��ϣ��ķ���
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
            // ���׶�֮����ڳ�β�ִ�������һ�����һ���������ʱ������һ����̨���Ӿ��Ͼ��Ƕ���һ�ڳ��ᣩ
            if(mHertIndex >= 2)
            {
                BaseUnit u = null;
                c.AddSpellingFunc(delegate
                {
                    BaseUnit tail = GetLastBody();
                    if (MapManager.GetYIndex(tail.transform.position.y) == 3 && tail.transform.position.x <= MapManager.GetColumnX(8) - GetBodyLength())
                    {
                        u = CreateFireBulletAttackerTail((Vector2)tail.transform.position - moveRotate* GetBodyLength(), wait_time, totalAttackCount, dmg);
                        return true;
                    }
                    return false;
                });
                // �ж���̨�Ƿ���ʧ����ʧ��Ϊ�ü��ܽ����ı�־
                c.AddSpellingFunc(delegate {
                    return u == null || !u.IsAlive();
                });
            }
            c.AddSpellingFunc(delegate
            {
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
    /// �����ȫϢͶӰͣ��
    /// </summary>
    /// <param name="createActionListFunc">������ͣ���ڼ�Ҫִ�е�ָ��ϣ��ķ���������ÿ��ͣ��Ҫִ�е�ָ���У���һ��intΪͣ��ʣ��ʱ�䣬�ڶ���intΪͣ����ʱ��</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(int wait_time, int totalAttackCount, float dmg, float boom_dmg)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        BaseUnit u = null;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate {
            // ���ٸ���
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
            moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
            NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
            return true;
        };
        c.BeforeSpellFunc = delegate{
            u = CreateFireBulletAttackerTrain(wait_time, totalAttackCount, dmg, boom_dmg);
        };
        {
            // �ж���̨�Ƿ���ʧ����ʧ��Ϊ�ü��ܽ����ı�־
            c.AddSpellingFunc(delegate {
                return u==null || !u.IsAlive();
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
