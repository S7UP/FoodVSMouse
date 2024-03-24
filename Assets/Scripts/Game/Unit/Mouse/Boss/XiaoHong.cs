using System;
using System.Collections.Generic;

using Environment;

using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ����С��
/// </summary>
public class XiaoHong : BossUnit
{
    private static RuntimeAnimatorController Diamond_Run;
    private static RuntimeAnimatorController Summon_Run;
    private static RuntimeAnimatorController StarHitEffect_Run;
    private static RuntimeAnimatorController IceBreak_Run;
    private static Sprite Diamond_Bullet_Sprite;
    private static Sprite Star_Bullet_Sprite;
    private static Sprite Wait_Icon_Sprite;

    private List<int>[] rowListArray;
    private List<int> avaliableIndexList = new List<int>();
    private float dmgRecord; // �ܵ����˺��ܺ�
    private Queue<float> lost_hp_percent_queue = new Queue<float>();
    private float current_lost_hp_percent;

    private RingUI FinalSkillRingUI;

    public override void Awake()
    {
        base.Awake();
        if(Diamond_Run == null)
        {
            Diamond_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/15/Diamond");
            Summon_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/15/Summon");
            StarHitEffect_Run = GameManager.Instance.GetRuntimeAnimatorController("Boss/15/StarHitEffect");
            Diamond_Bullet_Sprite = GameManager.Instance.GetSprite("Boss/15/Diamond/Bullet");
            Star_Bullet_Sprite = GameManager.Instance.GetSprite("Boss/15/Star/Bullet");
            IceBreak_Run = GameManager.Instance.GetRuntimeAnimatorController("Effect/IceBreak");
            Wait_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Wait");
        }
    }

    public override void MInit()
    {
        dmgRecord = 0;
        rowListArray = null;
        avaliableIndexList.Clear();
        lost_hp_percent_queue.Clear();
        current_lost_hp_percent = 9999;
        base.MInit();
        // ����UI
        {
            FinalSkillRingUI = RingUI.GetInstance(0.3f * Vector2.one);
            GameNormalPanel.Instance.AddUI(FinalSkillRingUI);
            taskController.AddTask(TaskManager.GetFinalSkillRingUITask(FinalSkillRingUI, this, 0.25f * MapManager.gridHeight * Vector3.down));
            FinalSkillRingUI.Hide();
            FinalSkillRingUI.SetPercent(0);
            AddOnDestoryAction(delegate { if (FinalSkillRingUI.IsValid()) FinalSkillRingUI.MDestory(); });
        }

        // �ܻ��¼�
        CreateHitAction();

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
                    if (current_lost_hp_percent != 9999)
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
        GetSpriteRenderer().transform.localScale = new Vector2(1, 1);
        // ��������BOSS
        foreach (var u in GameController.Instance.GetEachEnemy())
        {
            if(u is XiaoMing)
            {
                // ������һP
                BossUnit boss = u as XiaoMing;
                boss.SkipStage();
            }
        }
        base.AfterDeath();
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.66f, 0.33f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.XiaoHong, 0))
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
            AddParamChangeAction("RSet1", action);
            action(GetParamArray("RSet1"));
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
        mBoxCollider2D.size = new Vector2(0.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    private void CreateHitAction()
    {
        actionPointController.AddListener(ActionPointType.PostReceiveDamage, (combatAction) =>
        {
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

    #region һ����
    private List<Vector2> GetS0PosList(int count)
    {
        float colIndex = 9 - GetParamValue("right_col0");

        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        // ���ݿ���������������
        list.Sort((x, y) => {
            int xRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[x])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    xRowAllyCount++;
            }
            int yRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[y])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    yRowAllyCount++;
            }
            return xRowAllyCount.CompareTo(yRowAllyCount);
        });

        while(list.Count > count)
            list.RemoveAt(list.Count - 1);

        // �ٸ���������������
        list.Sort((x, y) => {
            return x.CompareTo(y);
        });

        List<Vector2> posList = new List<Vector2>();
        foreach (var rowIndex in list)
        {
            posList.Add(new Vector2(MapManager.GetColumnX(colIndex), MapManager.GetRowY(rowIndex)));
        }
        return posList;
    }

    private void CreateS0Summon(BaseGrid g, Vector2 pos, int type, int shape, int stun_time)
    {
        // �ٻ���Ч
        BaseEffect e = BaseEffect.CreateInstance(Summon_Run, null, "Cast", null, false);
        e.SetSpriteRendererSorting("Effect", 2);
        e.transform.position = pos;
        // ���������������У�
        {
            if (g != null)
            {
                CustomizationTask task = new CustomizationTask();
                task.AddTaskFunc(delegate {
                    e.transform.position = g.transform.position;
                    return false;
                });
                e.taskController.AddTask(task);
            }
        }
        // ʵ���ж�����
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return e.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.44f;
            });
            task.AddOnExitAction(delegate {
                int count = 1;
                if (mHertIndex >= 3)
                    count = 2;

                // ��������һֻ����
                List<MouseUnit> baodiList = new List<MouseUnit>();
                for (int i = 0; i < count; i++)
                {
                    MouseUnit new_mouse = GameController.Instance.mMouseFactory.GetMouse(type, shape);
                    new_mouse.transform.position = e.transform.position;
                    GameController.Instance.AddMouseUnit(new_mouse);

                    CustomizationTask stun_task = new CustomizationTask();
                    stun_task.AddTimeTaskFunc(4);
                    stun_task.AddOnExitAction(delegate { new_mouse.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(new_mouse, stun_time, false)); });
                    new_mouse.taskController.AddTask(stun_task);
                    baodiList.Add(new_mouse);
                }
                // ����������λ
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(e.transform.position, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    foreach (var m in baodiList)
                        r.AddExcludeMouseUnit(m);
                    r.AddEnemyEnterConditionFunc((m)=>{
                        return !m.IsBoss() && MouseManager.IsGeneralMouse(m);
                    });
                    r.AddBeforeDestoryAction(delegate {
                        foreach (var m in r.mouseUnitList.ToArray())
                        {
                            for (int i = 0; i < count; i++)
                            {
                                MouseUnit new_mouse = GameController.Instance.mMouseFactory.GetMouse(type, shape);
                                new_mouse.transform.position = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                                GameController.Instance.AddMouseUnit(new_mouse);

                                CustomizationTask stun_task = new CustomizationTask();
                                stun_task.AddTimeTaskFunc(4);
                                stun_task.AddOnExitAction(delegate { new_mouse.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(new_mouse, stun_time, false)); });
                                new_mouse.taskController.AddTask(stun_task);
                            }
                            m.MDestory();
                        }
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
            });
            e.taskController.AddTask(task);
        }
        GameController.Instance.AddEffect(e);
    }

    private CompoundSkillAbility SKill0Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int count = Mathf.FloorToInt(GetParamValue("row_count0"));
        int move_time = Mathf.FloorToInt(GetParamValue("move_time0") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait0_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait0_1") * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type0"));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape0"));
        int stun_time = Mathf.FloorToInt(GetParamValue("stun_time0")*60);

        // ����
        List<Vector2> posList = null;
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        // һЩ������ʼ��
        c.BeforeSpellFunc = delegate
        {
            posList = null;
        };
        {
            // ��һ��˲��+����
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task = new CustomizationTask();
                task.AddTaskFunc(delegate
                {
                    animatorController.Play("Disappear");
                    return true;
                });
                task.AddTaskFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        posList = GetS0PosList(count);
                        transform.position = posList[0];
                        animatorController.Play("Appear");
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        timeLeft = wait0;
                        animatorController.Play("Idle", true);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate
                {
                    timeLeft--;
                    if (timeLeft <= 0)
                    {
                        animatorController.Play("Summon");
                        GetSpriteRenderer().transform.localScale = new Vector2(-1, 1);
                        CreateS0Summon(GameController.Instance.mMapController.GetGrid(GetColumnIndex()+1, GetRowIndex()), transform.position + new Vector3(MapManager.gridWidth, 0), soldier_type, soldier_shape, stun_time);
                        return true;
                    }
                    return false;
                });
                task.AddTaskFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        GetSpriteRenderer().transform.localScale = new Vector2(1, 1);
                        return true;
                    } 
                    return false;
                });
                return task;
            });

            // �ƶ�+����
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                // ֮���ƶ�
                for (int _i = 1; _i < posList.Count; _i++)
                {
                    int i = _i;
                    task.AddTaskFunc(delegate
                    {
                        timeLeft = move_time;
                        startPos = transform.position;
                        endPos = posList[i];
                        if (endPos.x - startPos.x < 0)
                            animatorController.Play("MoveLeft", true);
                        else
                            animatorController.Play("MoveRight", true);
                        return true;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        transform.position = Vector2.Lerp(startPos, endPos, 1 - (float)timeLeft / move_time);
                        if (timeLeft <= 0)
                        {
                            timeLeft = wait0;
                            animatorController.Play("Idle", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Summon");
                            GetSpriteRenderer().transform.localScale = new Vector2(-1, 1);
                            CreateS0Summon(GameController.Instance.mMapController.GetGrid(GetColumnIndex() + 1, GetRowIndex()), transform.position + new Vector3(MapManager.gridWidth, 0), soldier_type, soldier_shape, stun_time);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            GetSpriteRenderer().transform.localScale = new Vector2(1, 1);
                            return true;
                        }
                        return false;
                    });
                }
                return task;
            });
            // ĩβ�ȴ�
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
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
    /// <summary>
    /// �������ǵ�����ص��˺��ж�
    /// </summary>
    private void CreateS1StarDamageArea(Vector2 pos)
    {
        // ������Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(StarHitEffect_Run, null, "Disappear", null, false);
            e.transform.position = pos;
            GameController.Instance.AddEffect(e);
        }

        // �Ը���
        {
            float dmg = GetParamValue("dmg1");
            float ice_dmg_trans = GetParamValue("ice_dmg_trans1");

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f*MapManager.gridWidth, 0.5f*MapManager.gridHeight), "CollideGrid");
            r.transform.position = pos;
            r.SetInstantaneous();
            r.isAffectGrid = true;
            r.SetOnGridEnterAction((g) => {
                g.TakeAction(this, (u) => {
                    float total_dmg = dmg;
                    ITask t = EnvironmentFacade.GetIceDebuff(u);
                    if (t != null)
                        total_dmg += (t as IceTask).GetValue() * ice_dmg_trans;
                    new DamageAction(CombatAction.ActionType.RealDamage, this, u, total_dmg).ApplyAction();
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
            r.SetOnEnemyEnterAction((u) => {
                if(!u.IsBoss())
                    UnitManager.Execute(this, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    /// <summary>
    /// ����һ�����ǵ���
    /// </summary>
    private void CreateS1StarBullet(Vector2 pos, BaseUnit target1, BaseUnit target2)
    {
        Vector2 pos1 = (target1 == null ? new Vector2(MapManager.GetColumnX(0), pos.y) : (Vector2)target1.transform.position);
        Vector2 pos2 = (target2 == null ? new Vector2(MapManager.GetColumnX(0), pos.y) : (Vector2)target2.transform.position);

        EnemyBullet b = EnemyBullet.GetInstance(Star_Bullet_Sprite, this);
        b.transform.position = pos;
        b.isnUseHitEffect = true;
        b.isnKillSelf = true;
        b.AddCanHitFunc(delegate { return false; });
        // ��ת����
        {
            float rad = 0;
            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                b.AddHitAction(delegate {
                    b.spriteRenderer.transform.right = Vector2.right;
                });
            });
            task.AddTaskFunc(delegate {
                rad -= Mathf.PI/45;
                b.spriteRenderer.transform.right = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                return false;
            });
            b.taskController.AddTask(task);
        }
        // ���Ŀ���Ƿ�������
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                if (target1 != null && !target1.IsAlive())
                    target1 = null;
                if (target2 != null && !target2.IsAlive())
                    target2 = null;
                return target1 == null && target2 == null;
            });
            b.taskController.AddTask(task);
        }
        // ������Ծ����
        {
            float v = 7 * MapManager.gridWidth / 90;
            if(target1 == null && target2 == null)
            {
                // �����ȫûĿ�꣬��ֱ�����������ȥ��������ʧ
                float dist = (pos2 - (Vector2)b.transform.position).magnitude;
                CustomizationTask task1 = TaskManager.GetParabolaTask(b, Mathf.FloorToInt(dist/v), dist / 2, b.transform.position, pos2, false, true);
                task1.AddOnExitAction(delegate {
                    CreateS1StarDamageArea(b.transform.position);
                    b.ExecuteRecycle();
                });
                b.taskController.AddTask(task1);
            }
            else if(target1 == target2)
            {
                // ���ֻ��һ��Ŀ�꣬��ֻ�����Ŀ��
                float dist = (target1.transform.position - b.transform.position).magnitude;
                CustomizationTask task1 = TaskManager.GetParabolaTask(b, Mathf.FloorToInt(dist / v), dist / 2, b.transform.position, target1, false, true);
                task1.AddOnExitAction(delegate {
                    CreateS1StarDamageArea(b.transform.position);
                    b.ExecuteRecycle();
                });
                b.taskController.AddTask(task1);
            }
            else
            {
                // ���������Ŀ�꣬����������Ŀ��
                float dist = (target1.transform.position - b.transform.position).magnitude;
                CustomizationTask task1 = TaskManager.GetParabolaTask(b, Mathf.FloorToInt(dist / v), dist / 2, b.transform.position, target1, false, true);
                task1.AddOnExitAction(delegate {
                    // ������һ��Ŀ�����ڸ���
                    CreateS1StarDamageArea(b.transform.position);

                    // �����ڶ���Ŀ�굯��������
                    CustomizationTask task2;
                    if (target2 == null || !target2.IsAlive())
                    {
                        dist = (pos2 - (Vector2)b.transform.position).magnitude;
                        task2 = TaskManager.GetParabolaTask(b, Mathf.FloorToInt(dist / v), dist / 2, b.transform.position, pos2, false, true);
                    }
                    else
                    {
                        dist = (target2.transform.position - b.transform.position).magnitude;
                        task2 = TaskManager.GetParabolaTask(b, Mathf.FloorToInt(dist / v), dist / 2, b.transform.position, target2, false, true);
                    }
                    task2.AddOnExitAction(delegate {
                        CreateS1StarDamageArea(b.transform.position);
                        b.ExecuteRecycle();
                    });
                    b.taskController.AddTask(task2);
                });
                b.taskController.AddTask(task1);
            }



        }
        GameController.Instance.AddBullet(b);
    }

    private CompoundSkillAbility SKill1Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        float colIndex = 9 - GetParamValue("right_col1");
        int move_time = Mathf.FloorToInt(GetParamValue("move_time1") * 60);
        int wait0 = Mathf.FloorToInt(GetParamValue("wait1_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait1_1") * 60);

        // ����
        List<int> rowIndexList = null;
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

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
            // �ƶ�+������
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                // ��һ��˲��
                {
                    task.AddTaskFunc(delegate
                    {
                        animatorController.Play("Disappear");
                        return true;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndexList[0]);
                            animatorController.Play("Appear");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            timeLeft = wait0;
                            animatorController.Play("Idle", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Attack");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.37f)
                        {
                            // Ѱ������Ŀ�� �� ��������
                            BaseUnit target1 = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(GetRowIndex(), float.MinValue, transform.position.x, false);
                            BaseUnit target2 = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, false);
                            CreateS1StarBullet(transform.position, target1, target2);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                            return true;
                        return false;
                    });
                }
                // ֮���ƶ�
                for (int _i = 1; _i < rowIndexList.Count; _i++)
                {
                    int i = _i;
                    task.AddTaskFunc(delegate
                    {
                        startPos = transform.position;
                        endPos = MapManager.GetGridLocalPosition(colIndex, rowIndexList[i]);
                        timeLeft = move_time;
                        animatorController.Play("Idle", true);
                        return true;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        transform.position = Vector2.Lerp(startPos, endPos, 1 - (float)timeLeft / move_time);
                        if (timeLeft <= 0)
                        {
                            timeLeft = wait0;
                            animatorController.Play("Idle", true);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        timeLeft--;
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("Attack");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.37f)
                        {
                            // Ѱ������Ŀ�� �� ��������
                            BaseUnit target1 = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(GetRowIndex(), float.MinValue, transform.position.x, false);
                            BaseUnit target2 = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, false);
                            CreateS1StarBullet(transform.position, target1, target2);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                            return true;
                        return false;
                    });
                }
                return task;
            });
            // ĩβ�ȴ�
            c.AddCreateTaskFunc(delegate {
                CustomizationTask task;
                CompoundSkillAbilityManager.GetWaitTimeTask(wait1, out task);
                task.AddOnEnterAction(delegate {
                    animatorController.Play("Idle", true);
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
    /// <summary>
    /// ��ȡ������P3�׶�ʩ����UI����
    /// </summary>
    /// <param name="ru"></param>
    /// <returns></returns>
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

    private List<Vector2> GetS2PosList(int count)
    {
        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        // ���ݿ���������������
        list.Sort((x, y) => {
            int xRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[x])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    xRowAllyCount++;
            }
            int yRowAllyCount = 0;
            foreach (var unit in GameController.Instance.mAllyList[y])
            {
                if (unit is FoodUnit && FoodManager.IsAttackableFoodType((FoodUnit)unit))
                    yRowAllyCount++;
            }
            return -xRowAllyCount.CompareTo(yRowAllyCount);
        });

        while (list.Count > count)
            list.RemoveAt(list.Count - 1);

        // �ٸ���������������
        list.Sort((x, y) => {
            return x.CompareTo(y);
        });

        List<Vector2> posList = new List<Vector2>();
        foreach (var rowIndex in list)
        {
            BaseUnit leftUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(rowIndex, float.MaxValue, false);
            BaseUnit rightUnit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, false);
            int colIndex;
            if (leftUnit == null)
                colIndex = 4;
            else
                colIndex = MapManager.GetXIndex((leftUnit.transform.position.x + rightUnit.transform.position.x) / 2);
            posList.Add(new Vector2(MapManager.GetColumnX(colIndex), MapManager.GetRowY(rowIndex)));
        }
        return posList;
    }

    private void CreateS2DiamondDrop(Vector2 pos, float ice_val0, float ice_val1, float ice_val2, int interval, float burn_rate, int alive_time)
    {
        float dist = 10*MapManager.gridHeight;
        Vector2 startPos = new Vector2(pos.x + dist*Mathf.Cos(70f/180*Mathf.PI), pos.y + dist * Mathf.Sin(70f / 180 * Mathf.PI));

        BaseEffect e = BaseEffect.CreateInstance(Diamond_Bullet_Sprite);
        e.SetSpriteRendererSorting("Effect", 10);
        e.transform.position = startPos;
        // ����
        {
            int totalTime = 90;
            int timeLeft = totalTime;
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                timeLeft--;
                float rate = 1 - (float)timeLeft / totalTime;
                e.transform.position = Vector2.Lerp(startPos, pos, rate);
                if (!IsAlive())
                    e.ExecuteDeath();
                return timeLeft <= 0;
            });
            task.AddOnExitAction(delegate {
                // ������ʯ
                MouseModel diamond = CreateS2Diamond(pos, ice_val1, ice_val2, interval, burn_rate, alive_time);
                // ������Ч
                for (int i = 0; i < 3; i++)
                {
                    BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
                    eff.SetSpriteRendererSorting("Effect", 2);
                    eff.transform.position = e.transform.position;
                    float a = UnityEngine.Random.Range(0, 2 * Mathf.PI);
                    eff.SetSpriteRight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                    GameController.Instance.AddEffect(eff);
                }
                // ��Χ�Ը��Ӵ���Ч��
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "CollideGrid");
                    r.transform.position = pos;
                    r.SetInstantaneous();
                    r.isAffectGrid = true;
                    //r.SetOnGridEnterAction((g) => {
                    //    g.TakeAction(this, (u) => { UnitManager.Execute(this, u); }, false);
                    //});
                    r.AddBeforeDestoryAction(delegate {
                        BaseGrid targetGrid = null;
                        float minDist = float.MaxValue;
                        foreach (var g in r.gridList.ToArray())
                        {
                            float dist = (r.transform.position - g.transform.position).magnitude;
                            if(dist < minDist)
                            {
                                targetGrid = g;
                                minDist = dist;
                            }
                        }
                        if(targetGrid != null)
                        {
                            targetGrid.TakeAction(this, (u) => { UnitManager.Execute(this, u); }, false);
                        }
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                // ��Χ�����󴦾�Ч��
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, MapManager.gridHeight), "ItemCollideEnemy");
                    r.AddExcludeMouseUnit(diamond);
                    r.transform.position = pos;
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    r.SetOnEnemyEnterAction((u) => {
                        if(!u.IsBoss())
                            UnitManager.Execute(this, u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                // ��Χ����Ч��
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(e.transform.position, new Vector2(3 * MapManager.gridWidth, 3 * MapManager.gridHeight), "BothCollide");
                    r.SetInstantaneous();
                    r.isAffectFood = true;
                    r.isAffectMouse = true;
                    r.SetOnFoodEnterAction((u) => {
                        if (FoodManager.IsAttackableFoodType(u))
                            Environment.EnvironmentFacade.AddIceDebuff(u, ice_val0);
                    });
                    r.SetOnEnemyEnterAction((u) => {
                        if (!u.IsBoss())
                            Environment.EnvironmentFacade.AddIceDebuff(u, ice_val0);
                    });
                    r.SetOnCharacterEnterAction((u) => {
                        Environment.EnvironmentFacade.AddIceDebuff(u, ice_val0);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                // ������ʧ
                e.ExecuteDeath();
            });
            e.taskController.AddTask(task);
        }
        GameController.Instance.AddEffect(e);
    }

    private MouseModel CreateS2Diamond(Vector2 pos, float ice_val1, float ice_val2, int interval, float burn_rate, int alive_time)
    {
        MouseModel m = MouseModel.GetInstance(Diamond_Run);
        m.transform.position = pos;
        m.DisableMove(true);
        m.SetBaseAttribute(10000, 10, 1.0f, 0.0f, 100, 0.5f, 0);
        m.NumericBox.BurnRate.AddModifier(new FloatModifier(burn_rate));
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        m.IdleClipName = "Idle";
        m.MoveClipName = "Idle";
        m.AttackClipName = "Idle";
        m.DieClipName = "Disappear";
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));
        // ����
        {
            int totalTimeLeft = alive_time;
            int timeLeft = interval;
            CustomizationTask task = new CustomizationTask();
            task.AddOnEnterAction(delegate {
                m.animatorController.Play("Appear");
                totalTimeLeft = alive_time;
                timeLeft = interval;
            });
            task.AddTaskFunc(delegate {
                totalTimeLeft--;
                timeLeft--;
                if(timeLeft <= 0)
                {
                    m.animatorController.Play("Appear");
                    timeLeft += interval;
                    // ������Ч
                    {
                        BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
                        eff.SetSpriteRendererSorting("Effect", 2);
                        eff.transform.position = m.transform.position;
                        float a = UnityEngine.Random.Range(0, 2 * Mathf.PI);
                        eff.SetSpriteRight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                        GameController.Instance.AddEffect(eff);
                    }

                    // ��ΧЧ��
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, new Vector2(3 * MapManager.gridWidth, 3 * MapManager.gridHeight), "BothCollide");
                        r.SetInstantaneous();
                        r.isAffectFood = true;
                        r.isAffectMouse = true;
                        r.SetOnFoodEnterAction((u)=>{
                            if (FoodManager.IsAttackableFoodType(u))
                                Environment.EnvironmentFacade.AddIceDebuff(u, ice_val1);
                        });
                        r.SetOnEnemyEnterAction((u) => {
                            if (!u.IsBoss())
                                Environment.EnvironmentFacade.AddIceDebuff(u, ice_val1);
                        });
                        r.SetOnCharacterEnterAction((u) => {
                            Environment.EnvironmentFacade.AddIceDebuff(u, ice_val1);
                        });
                        GameController.Instance.AddAreaEffectExecution(r);
                    }
                }
                return totalTimeLeft <= 0 || !IsAlive();
            });
            task.AddOnExitAction(delegate {
                m.MDestory();
            });
            m.taskController.AddTask(task);
        }
        // ����
        {
            m.AddOnDestoryAction(delegate {
                // ������Ч
                for (int i = 0; i < 3; i++)
                {
                    BaseEffect eff = BaseEffect.CreateInstance(IceBreak_Run, null, "Appear", null, false);
                    eff.SetSpriteRendererSorting("Effect", 2);
                    eff.transform.position = m.transform.position;
                    float a = UnityEngine.Random.Range(0, 2 * Mathf.PI);
                    eff.SetSpriteRight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                    GameController.Instance.AddEffect(eff);
                }

                // ��ΧЧ��
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, new Vector2(5 * MapManager.gridWidth, 5 * MapManager.gridHeight), "BothCollide");
                    r.SetInstantaneous();
                    r.isAffectFood = true;
                    r.isAffectMouse = true;
                    r.SetOnFoodEnterAction((u) => {
                        if (FoodManager.IsAttackableFoodType(u))
                            Environment.EnvironmentFacade.AddIceDebuff(u, ice_val2);
                    });
                    r.SetOnEnemyEnterAction((u) => {
                        if (!u.IsBoss())
                            Environment.EnvironmentFacade.AddIceDebuff(u, ice_val2);
                    });
                    r.SetOnCharacterEnterAction((u) => {
                        Environment.EnvironmentFacade.AddIceDebuff(u, ice_val2);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
            });
        }
        GameController.Instance.AddMouseUnit(m);
        return m;
    }

    private CompoundSkillAbility SKill2Init(SkillAbility.SkillAbilityInfo info)
    {
        // ����
        int rowIndex = Mathf.FloorToInt(GetParamValue("row2")) - 1;
        int colIndex = 9 - Mathf.FloorToInt(GetParamValue("right_col2"));
        int count = Mathf.FloorToInt(GetParamValue("row_count2"));
        int wait0 = Mathf.FloorToInt(GetParamValue("wait2_0") * 60);
        int wait1 = Mathf.FloorToInt(GetParamValue("wait2_1") * 60);
        float ice_val0 = GetParamValue("ice_val2_0");
        float ice_val1 = GetParamValue("ice_val2_1");
        float ice_val2 = GetParamValue("ice_val2_2");
        float burn_rate = 1 - GetParamValue("burn_defence") / 100;
        int interval = Mathf.FloorToInt(GetParamValue("interval2")*60);
        int alive_time = Mathf.FloorToInt(GetParamValue("alive_time2") * 60);
        float dmg_rate = 1 - GetParamValue("defence2") / 100;
        int max_add_time = Mathf.FloorToInt(GetParamValue("max_add_time2") * 60);
        int add_time = Mathf.FloorToInt(GetParamValue("add_time2") * 60);

        // ����
        RingUI rUI = null;
        CustomizationTask waitTask = null;
        List<Vector2> posList = null;
        FloatModifier DamageRateMod = new FloatModifier(dmg_rate);
        // ��ը���2����趨
        int av_add_time_left = 0; // �����Ա������ӳ���ʱ��
        int timeLeft = 0;
        Action<CombatAction> bombAction = (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    int add = 0; // ����ȷ��Ҫ�ӳ���ʱ��
                    if(av_add_time_left <= add_time)
                    {
                        add = av_add_time_left;
                        av_add_time_left = 0;
                    }
                    else
                    {
                        add = add_time;
                        av_add_time_left -= add_time;
                    }

                    if (wait0 - timeLeft > add)
                        timeLeft += add;
                    else
                    {
                        add -= (wait0 - timeLeft);
                        av_add_time_left += add;
                        timeLeft = wait0;
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
            c.AddCreateTaskFunc(delegate
            {
                CustomizationTask task = new CustomizationTask();
                // ˲�� + �ȴ�
                {
                    task.AddTaskFunc(delegate
                    {
                        animatorController.Play("Disappear");
                        return true;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            transform.position = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                            animatorController.Play("Appear");
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("PreCast");
                            // ��ü���
                            NumericBox.DamageRate.AddModifier(DamageRateMod);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            animatorController.Play("Casting", true);
                            rUI = RingUI.GetInstance(0.3f * Vector2.one);
                            GameNormalPanel.Instance.AddUI(rUI);
                            waitTask = GetS2WaitRingUITask(rUI);
                            rUI.mTaskController.AddTask(waitTask);
                            timeLeft = wait0;
                            av_add_time_left = max_add_time;
                            // ��ӱ�ը���ʱ�趨
                            AddActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate {
                        timeLeft--;
                        rUI.SetPercent(1-(float)timeLeft/wait0);
                        if (timeLeft <= 0)
                        {
                            animatorController.Play("PostCast");
                            // �Ƴ�����
                            NumericBox.DamageRate.RemoveModifier(DamageRateMod);
                            // �Ƴ���ը��ʱ
                            RemoveActionPointListener(ActionPointType.PreReceiveDamage, bombAction);
                            rUI.mTaskController.RemoveTask(waitTask);
                            rUI = null;
                            posList = GetS2PosList(count); // ��ȡ������λ
                            // ����ȥ
                            foreach (var v2 in posList)
                                CreateS2DiamondDrop(v2, ice_val0, ice_val1, ice_val2, interval, burn_rate, alive_time);
                            return true;
                        }
                        return false;
                    });
                    task.AddTaskFunc(delegate
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                        {
                            if(current_lost_hp_percent < 9999)
                                FinalSkillRingUI.Show();
                            animatorController.Play("Idle", true);
                            timeLeft = wait1;
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
}
