using System.Collections.Generic;
using UnityEngine;
using System;
using S7P.Numeric;
public class DongJun : BossUnit
{
    private static RuntimeAnimatorController Cave_Road_AnimatorController;
    private static RuntimeAnimatorController Cave_Water_AnimatorController;
    private static RuntimeAnimatorController Pipeline_Road_AnimatorController;
    private static RuntimeAnimatorController Pipeline_Water_AnimatorController;

    private const string PipelineKey = "�ܵ�";

    public override void Awake()
    {
        if (Cave_Road_AnimatorController == null)
        {
            Cave_Road_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Cave_Road");
            Cave_Water_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Cave_Water");
            Pipeline_Road_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Pipeline_Road");
            Pipeline_Water_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Pipeline_Water");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // ��ӳ��ֵļ���
        {
            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 120;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Appear");
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stun", true);
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
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.DongJun, 0))
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

        list.Add(BuildPipelinesInit(infoList[0])); // �����ܵ�
        list.Add(StampedeAccidentsInit(infoList[1])); // ��̤�¹�

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
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
    /// ����һ���޵׶�
    /// </summary>
    /// <param name="g">�������ĸ���</param>
    /// <param name="timeLeft">���Ĵ���ʱ��</param>
    private void CreateCave(BaseGrid g, int timeLeft)
    {
        // �������ĸ�����Ч ���� �����Ѱ����߿յؿ� �򲻻ᷢ���κ���
        if (g == null || g.IsContainGridType(GridType.Sky))
            return;

        // ��Ӹ߿յؿ�
        BaseGridType gt = BaseGridType.GetInstance(GridType.Sky, 0);
        g.AddGridType(GridType.Sky, gt);
        // ��ֹ�ſ�
        Func<BaseGrid, FoodInGridType, bool> noBuildFunc = delegate { return false; };
        g.AddCanBuildFuncListener(noBuildFunc);
        // ��������ǿ���Ƴ�������ؾ�
        g.KillFoodUnit(FoodInGridType.WaterVehicle);
        g.KillFoodUnit(FoodInGridType.LavaVehicle);

        // �������ʱ����Ƴ�������
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            // ������
            if(g.IsContainGridType(GridType.Water))
                gt.animator.runtimeAnimatorController = Cave_Water_AnimatorController;
            else
                gt.animator.runtimeAnimatorController = Cave_Road_AnimatorController;
            gt.animatorController.Play("Appear");
        });
        t.AddTaskFunc(delegate
        {
            timeLeft--;
            if (gt.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                gt.animatorController.Play("Idle", true);
                return true;
            }
            return false;
        });
        
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
                timeLeft--;
            else
            {
                gt.animatorController.Play("Disappear");
                return true;
            }
            return false;
        });

        t.AddTaskFunc(delegate
        {
            if (gt.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                g.RemoveCanBuildFuncListener(noBuildFunc);
                g.RemoveGridType(GridType.Sky);
                return true;
            }
            return false;
        });
        g.AddTask(t);
    }

    /// <summary>
    /// ����һ���ܵ�
    /// </summary>
    /// <param name="g"></param>
    private MouseModel CreatePipeline(BaseGrid g, float hp)
    {
        if (g == null)
            return null;

        if (g.GetUnitFromDict(PipelineKey) != null)
        {
            g.GetUnitFromDict(PipelineKey).ExecuteDeath();
            g.RemoveUnitFromDict(PipelineKey);
        }

        MouseModel m;
        if (g.IsContainGridType(GridType.Water))
            m = MouseModel.GetInstance(Pipeline_Water_AnimatorController);
        else
            m = MouseModel.GetInstance(Pipeline_Road_AnimatorController);
        BoolModifier boolModifier = new BoolModifier(true);
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier); // ���߶���
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier); // ������ѣ
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, boolModifier); // ���߱�������
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); // ����׹��
        m.NumericBox.AddDecideModifierToBoolDict(PipelineKey, boolModifier); // ����Լ��Ƕ������ܱ���Ķ�����
        WaterGridType.AddNoAffectByWater(m, boolModifier); // �������ˮʴ
        m.SetBaseAttribute(hp, 0, 1, 0, 100, 0, 0);
        m.transform.position = g.transform.position;
        m.currentYIndex = MapManager.GetYIndex(g.transform.position.y);
        m.CanBlockFuncList.Add(delegate { return false; }); // �ܵ������赲����Ҳ���ṥ��
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        m.CanHitFuncList.Add(noHitFunc); // �ܵ�Ҳ���ᱻ�ӵ����У�ת���ӵ����÷�ΧЧ����������
        m.CanBeSelectedAsTargetFuncList.Add(delegate { return false; }); // ������ΪĿ�걻ѡȡ
        // ��ֹ�ſ�
        Func<BaseGrid, FoodInGridType, bool> noBuildFunc = delegate { return false; };
        g.AddCanBuildFuncListener(noBuildFunc);
        GameController.Instance.AddMouseUnit(m);
        g.AddUnitToDict(PipelineKey, m); // ���ڸ�����

        // ��Ӷ���
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.SetActionState(new IdleState(m));
                m.animatorController.Play("Appear");
            });
            t.AddTaskFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.RemoveCanHitFunc(noHitFunc);
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
        }
        // ���ܵ����ݻ�ʱ�������޵׶����Ƴ���Ϊ�ܵ���ɵĲ��ɷ���
        m.AddBeforeDeathEvent(delegate {
            g.RemoveCanBuildFuncListener(noBuildFunc);
            CreateCave(g, Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex) * 60));
        });
        return m;
    }

    /// <summary>
    /// ���������ܵ�
    /// </summary>
    /// <param name="p1">����Ĺܵ�</param>
    /// <param name="p2">���ҵĹܵ�</param>
    private void LinkDoublePipeline(MouseModel p1, MouseModel p2)
    {
        if(p1 == null || p2 == null)
        {
            if (p1 != null)
                p1.ExecuteDeath();
            if (p2 != null)
                p2.ExecuteDeath();
            return;
        }

        RetangleAreaEffectExecution r1 = null;
        RetangleAreaEffectExecution r1_bullet = null;
        RetangleAreaEffectExecution r2 = null;
        RetangleAreaEffectExecution r2_bullet = null;

        // Ϊ����ܵ����ת����������� ���������� �ܵ���
        {
            Vector3 offset = 0*Vector3.left * 0.375f * MapManager.gridWidth; // ����ƫ����
            r1 = RetangleAreaEffectExecution.GetInstance(p1.transform.position + offset, 0.25f, 1, "ItemCollideEnemy");
            r1.isAffectMouse = true;
            // ת�Ƶй�
            r1.SetOnEnemyEnterAction((m) => {
                // BOSS��ܵ�������ͨ��
                if (m.IsBoss() || m.NumericBox.GetBoolNumericValue(PipelineKey))
                    return;

                if (m.moveRotate.x > 0)
                {
                    m.transform.position = r2.transform.position;
                }
            });
            r1.AddExcludeMouseUnit(p1); // ���ܰ��Լ�����ѽ
            r1.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r1);

            // ��Ӱ�����
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate{
                if (p1.IsAlive())
                {
                    r1.transform.position = p1.transform.position + offset;
                    return false;
                }
                else
                {
                    r1.MDestory();
                    return true;
                }
            });
            r1.AddTask(t);
        }
        // Ϊ����ܵ����ת���ӵ������� ���������� �ܵ���
        {
            Vector3 offset = 0 * Vector3.left * 0.375f * MapManager.gridWidth; // ����ƫ����
            r1_bullet = RetangleAreaEffectExecution.GetInstance(p1.transform.position + offset, 0.25f, 1, "Enemy");
            r1_bullet.isAffectBullet = true;
            // ת���ӵ�
            r1_bullet.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x > 0)
                {
                    b.transform.position = p2.transform.position;
                    r1_bullet.AddExcludeBullet(b); // ���ٶ��δ�������ӵ�
                }
            });
            r1_bullet.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r1_bullet);

            // ��Ӱ�����
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p1.IsAlive())
                {
                    r1_bullet.transform.position = p1.transform.position + offset;
                    return false;
                }
                else
                {
                    r1_bullet.MDestory();
                    return true;
                }
            });
            r1_bullet.AddTask(t);
        }



        // Ϊ����ܵ����ת����������� ���������� �ܵ���
        {
            Vector3 offset = 0*Vector3.right * 0.375f * MapManager.gridWidth; // ����ƫ����
            r2 = RetangleAreaEffectExecution.GetInstance(p2.transform.position + offset, 0.25f, 1, "ItemCollideEnemy");
            r2.isAffectMouse = true;
            r2.isAffectBullet = true;
            // ת���ӵ�
            r2.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x < 0)
                {
                    b.transform.position = r1.transform.position;
                }
            });
            // ת�Ƶй�
            r2.SetOnEnemyEnterAction((m) => {
                // BOSS��ܵ�������ͨ��
                if (m.IsBoss() || m.NumericBox.GetBoolNumericValue(PipelineKey))
                    return;

                if (m.moveRotate.x < 0)
                {
                    m.transform.position = r1.transform.position;
                }
            });
            r2.AddExcludeMouseUnit(p2); // ���ܰ��Լ�����ѽ
            r2.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r2);

            // ��Ӱ�����
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p2.IsAlive())
                {
                    r2.transform.position = p2.transform.position + offset;
                    return false;
                }
                else
                {
                    r2.MDestory();
                    return true;
                }
            });
            r2.AddTask(t);
        }
        // Ϊ����ܵ����ת���ӵ������� ���������� �ܵ���
        {
            Vector3 offset = 0 * Vector3.right * 0.375f * MapManager.gridWidth; // ����ƫ����
            r2_bullet = RetangleAreaEffectExecution.GetInstance(p2.transform.position + offset, 0.25f, 1, "Enemy");
            r2_bullet.isAffectBullet = true;
            // ת���ӵ�
            r2_bullet.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x < 0)
                {
                    b.transform.position = p1.transform.position;
                    r2_bullet.AddExcludeBullet(b); // ���ٶ��δ�������ӵ�
                }
            });
            r2_bullet.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r2_bullet);

            // ��Ӱ�����
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p2.IsAlive())
                {
                    r2_bullet.transform.position = p2.transform.position + offset;
                    return false;
                }
                else
                {
                    r2_bullet.MDestory();
                    return true;
                }
            });
            r2_bullet.AddTask(t);
        }

        // ������ʩ�������󶨣�һ�����˻�������һ��Ҳ����
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if(!p1.IsAlive())
                {
                    p2.ExecuteDeath();
                    return true;
                }else if (!p2.IsAlive())
                {
                    p1.ExecuteDeath();
                    return true;
                }
                return false;
            });
            p1.AddTask(t);
            p2.AddTask(t);
        }
    }

    ////////////////////////////////////////////////////////////////����ΪBOSS�ļ��ܶ���////////////////////////////////////////////////////////////

    /// <summary>
    /// �ڵ��ƶ���ĳ��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility DigMove(Vector2 pos, int timeLeft, int stunTime)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            // ԭ��������ʧ
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ԭ������һ����
                    CreateCave(GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex()), timeLeft);
                    transform.position = pos;
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stun", true);
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (stunTime > 0)
                {
                    stunTime--;
                    return false;
                }
                else
                    return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// �����ܵ�
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility BuildPipelinesInit(SkillAbility.SkillAbilityInfo info)
    {
        int CaveAliveTime = Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex)*60); // ���Ĵ���ʱ��
        float Pipeline_Hp = GetParamValue("Pipeline_Hp", mHertIndex); // �ܵ�������ֵ

        // �����ܵ�
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // �������������ѣʱ��
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex)*60); // 2�������������ѣʱ��
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // ��Ծ����

        // ����
        BaseGrid selectedGrid1 = null; // ��ѡ�д򶴵ĸ���
        BaseGrid selectedGrid2 = null;

        MouseModel pipeline_right = null;
        MouseModel pipeline_left = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            // ��ʼ��
            selectedGrid1 = null;
            selectedGrid2 = null;
            pipeline_right = null;
            pipeline_left = null;

            // ���Ҷ�����ȡ��������Ч���ӣ�Ȼ�������һ����Ч���Ӻ����
            List<BaseGrid> gridList = new List<BaseGrid>();
            for (int row = 0; row < 7; row++)
            {
                BaseGrid g = GameController.Instance.mMapController.GetGrid(7, row);
                if(g!=null)
                    gridList.Add(g);
            }
            Vector2 pos;
            // ֮��ʼȡBOSS�������
            if(gridList.Count > 0)
            {
                int selectedIndex = GetRandomNext(0, gridList.Count);
                selectedGrid1 = gridList[selectedIndex];
                pos = new Vector2(selectedGrid1.transform.position.x + 1f*MapManager.gridWidth, selectedGrid1.transform.position.y);
            }
            else
            {
                // ��Ҫ��û�еĻ������������ȡһ���
                int selectedRow = GetRandomNext(0, 7);
                pos = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectedRow));
            }
            // ԭ��������ʧ
            c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t0_0));
        };
        {
            // ������ѣ�����¡����򶴣�
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("Dig", false, 0);
                return true;
            });

            // ����������һ�κ�������Ȼ���ٲ�һ��
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ���ϴα�ѡȡ�ĸ��������ɶ�
                    CreateCave(selectedGrid1, CaveAliveTime);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    SmallJump(MapManager.gridWidth);
                    return true;
                }
                return false;
            });

            // ���궴�����ȥ
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    int col = 6;
                    // ���������п�Ƭ�ĸ���
                    List<BaseGrid> gridList = new List<BaseGrid>();
                    for (int row = 0; row < 7; row++)
                    {
                        BaseGrid g = GameController.Instance.mMapController.GetGrid(col, row);
                        if (g != null && g.GetAttackableFoodUnitList().Count > 0)
                        {
                            gridList.Add(g);
                        }   
                    }
                    // ����Ҳ������ָ��ӣ��Ǿ�������Ч�ĸ���
                    if(gridList.Count <= 0)
                    {
                        for (int row = 0; row < 7; row++)
                        {
                            BaseGrid g = GameController.Instance.mMapController.GetGrid(col, row);
                            if (g != null)
                            {
                                gridList.Add(g);
                            }
                        }
                    }

                    Vector2 pos;
                    // ���ȡһ������
                    if (gridList.Count > 0)
                    {
                        int selectedIndex = GetRandomNext(0, gridList.Count);
                        selectedGrid2 = gridList[selectedIndex];
                        pos = selectedGrid2.transform.position + MapManager.gridWidth*Vector3.left;
                    }
                    else
                    {
                        // ��Ҫ��û�еĻ������������ȡһ���
                        int selectedRow = GetRandomNext(0, 7);
                        pos = new Vector2(MapManager.GetColumnX(5f), MapManager.GetRowY(selectedRow));
                    }
                    // ԭ��������ʧ
                    c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t0_1));
                    return true;
                }
                return false;
            });
            // ���ֺ�������Ȼ���ٲ��Ŵ򶴶���
            c.AddSpellingFunc(delegate
            {
                transform.localScale = new Vector2(-1, 1);
                animatorController.Play("Dig", false, 0);
                return true;
            });
            // ��һ�β�����������ѡ���ĸ������ɹܵ�A���ҳ���
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    pipeline_right = CreatePipeline(selectedGrid1, Pipeline_Hp);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            // �ڶ��β������һ��������
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ������һ����
                    CreateCave(selectedGrid2, CaveAliveTime);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            // �����β��������ɹܵ�B������Ȼ��ʹ�����ܵ�������
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    pipeline_left = CreatePipeline(selectedGrid2, Pipeline_Hp);
                    if (pipeline_left != null)
                        pipeline_left.transform.localScale = new Vector2(-1, 1);
                    LinkDoublePipeline(pipeline_left, pipeline_right);
                    transform.localScale = new Vector2(1, 1); // ����ָ�����
                    return true;
                }
                return false;
            });

            // ׼������
            if(num0_0 > 0)
            {
                c.AddSpellingFunc(delegate
                {
                    SmallJump(2 * MapManager.gridWidth);
                    return true;
                });
            }
            // ����С���ж�
            for (int i = 0; i < num0_0 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        SmallJump(2 * MapManager.gridWidth);
                        // ���һ�ε���Ĳ�̤�˺�
                        DamageCurrentPosition();
                        return true;
                    }
                    return false;
                });
            }

            // ���һ��С������
            if (num0_0 > 0)
            {
                c.AddSpellingFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // ���һ�ε���Ĳ�̤�˺�
                        DamageCurrentPosition();
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
    /// С��
    /// </summary>
    private void SmallJump(float Dist)
    {
        int JumpClipTime = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Jump").aniTime) - 10;
        animatorController.Play("Jump", false, 0);
        AddTask(TaskManager.GetParabolaTask(this, Dist / JumpClipTime, 0.5f, transform.position, transform.position + Dist * Vector3.left, false));
    }

    /// <summary>
    /// ����
    /// </summary>
    private void BigJump(float Dist)
    {
        int JumpClipTime = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Jump").aniTime) - 10;
        animatorController.Play("Jump", false, 0);
        AddTask(TaskManager.GetParabolaTask(this, Dist / JumpClipTime, 3f, transform.position, transform.position + Dist * Vector3.left, false));
    }

    /// <summary>
    /// �Ե�ǰλ�����һ���̤�˺�
    /// </summary>
    private void DamageCurrentPosition()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "EnemyAllyGrid");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u)=>{
            if (u.IsBoss())
                return;
            UnitManager.Execute(this, u);
        });

        r.isAffectGrid = true;
        r.SetOnGridEnterAction((g) => {
            g.TakeAction(this, (u) => { 
                DamageAction action = UnitManager.Execute(this, u);
                new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans")/100).ApplyAction();
            }, false);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// ���ε�ǰ��Χ�ڵ�λ
    /// </summary>
    /// <param name="stunTime"></param>
    private void StunCurrentpostion(int stunTime)
    {
        Action<BaseUnit> stunAction = (u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stunTime, false));
        };

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction(stunAction);
        r.SetOnEnemyEnterAction(stunAction);
        r.AddExcludeMouseUnit(this); // �����ų�����
        GameController.Instance.AddAreaEffectExecution(r);
    }



    /// <summary>
    /// ��̤�¹�
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility StampedeAccidentsInit(SkillAbility.SkillAbilityInfo info)
    {
        int CaveAliveTime = Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex)*60); // ���Ĵ���ʱ��
        float Pipeline_Hp = GetParamValue("Pipeline_Hp", mHertIndex); // �ܵ�������ֵ

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60); // �ڶ���������ѣʱ��
        int num1_0 = Mathf.FloorToInt(GetParamValue("num1_0", mHertIndex)); // ��Ծ����
        int num1_1 = Mathf.FloorToInt(GetParamValue("num1_1", mHertIndex)); // ��������
        int StunTime1_0 = Mathf.FloorToInt(GetParamValue("StunTime1_0", mHertIndex) * 60); // �����Կ�Ƭ����ѣʱ��

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

            // ����һ����ȡ��������Ч���ӣ�Ȼ�������һ����Ч���Ӻ����
            List<BaseGrid> gridList = new List<BaseGrid>();
            for (int row = 0; row < 7; row++)
            {
                BaseGrid g = GameController.Instance.mMapController.GetGrid(8, row);
                if (g != null)
                    gridList.Add(g);
            }
            Vector2 pos;
            // ֮��ʼȡBOSS�������
            if (gridList.Count > 0)
            {
                int selectedIndex = GetRandomNext(0, gridList.Count);
                BaseGrid g = gridList[selectedIndex];
                pos = new Vector2(g.transform.position.x, g.transform.position.y);
            }
            else
            {
                // ��Ҫ��û�еĻ������������ȡһ���
                int selectedRow = GetRandomNext(0, 7);
                pos = new Vector2(MapManager.GetColumnX(8f), MapManager.GetRowY(selectedRow));
            }
            // ԭ��������ʧ
            c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t1_0));
        };
        {
            // ׼������
            c.AddSpellingFunc(delegate
            {
                SmallJump(2 * MapManager.gridWidth);
                return true;
            });
            // ����С���ж�
            for (int i = 0; i < num1_0 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        SmallJump(2 * MapManager.gridWidth);
                        // ���һ�ε���Ĳ�̤�˺�
                        DamageCurrentPosition();
                        return true;
                    }
                    return false;
                });
            }

            // ���һ��С������
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ���һ�ε���Ĳ�̤�˺�
                    DamageCurrentPosition();
                    // ����д����Ļ�����׼�����д���
                    if(num1_1 > 0)
                    {
                        BigJump(2 * MapManager.gridWidth);
                    }
                    
                    return true;
                }
                return false;
            });
            // ���δ����ж�
            for (int i = 0; i < num1_1 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        BigJump(2 * MapManager.gridWidth);
                        // ���һ�ε���Ĳ�̤�˺�
                        DamageCurrentPosition();
                        // ���һ��3*3����ѣЧ��
                        StunCurrentpostion(StunTime1_0);
                        return true;
                    }
                    return false;
                });
            }
            // ���һ��������
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ���һ�δ�������������ǣ�
                    if (num1_1 > 0)
                    {
                        // ���һ�ε���Ĳ�̤�˺�
                        DamageCurrentPosition();
                        // ���һ��3*3����ѣЧ��
                        StunCurrentpostion(StunTime1_0);
                    }
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