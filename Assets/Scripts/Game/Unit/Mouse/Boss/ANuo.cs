using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��ŵ
/// </summary>
public class ANuo : BossUnit
{
    private static RuntimeAnimatorController Canister_AnimatorController; // ����

    public override void Awake()
    {
        if (Canister_AnimatorController == null)
        {
            Canister_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/1/Canister");
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
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Enter");
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
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
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // ��ȡ����
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.ANuo, 0))
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

        list.Add(CanisterInit(infoList[0])); // ����ǹ
        list.Add(DragRacingInit(infoList[1])); // 쭳�

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// �����䵯
    /// </summary>
    private void CreateBullet(float dmg, float areaDmg, float childDmg)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Canister_AnimatorController, this, dmg);
        b.transform.position = transform.position + 0.5f * MapManager.gridWidth * Vector3.left;
        b.isAffectCharacter = false;
        b.isAffectFood = true;
        b.SetStandardVelocity(18);
        b.SetRotate(Vector2.left);
        // ���к����3*3��ը
        b.AddHitAction(delegate {
            DamageAreaEffectExecution e = DamageAreaEffectExecution.GetInstance(this, b.transform.position, 3, 3, CombatAction.ActionType.CauseDamage, areaDmg);
            GameController.Instance.AddAreaEffectExecution(e);
        });
        GameController.Instance.AddBullet(b);

        // ���һ�����񣬸������¼�䵯��·�̣�·�̳���һ��ͻ��Ա���ɢ��
        {
            float s = 0.75f*MapManager.gridWidth; // ·��
            Vector3 lastPos = b.transform.position; // ��һ֡����λ��

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate { 
                if(s > 0)
                {
                    s -= (b.transform.position - lastPos).magnitude;
                    lastPos = b.transform.position;
                    return false;
                }
                else
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        CreateSmallBullet(b.transform.position, childDmg, i);
                    }
                    b.ExecuteRecycle();
                    return true;
                }
            });
            b.AddTask(t);
        }
    }

    /// <summary>
    /// ����С�䵯
    /// </summary>
    /// <param name="dmg"></param>
    private void CreateSmallBullet(Vector3 pos, float dmg, int i)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Canister_AnimatorController, this, dmg);
        b.transform.position = pos;
        b.transform.localScale = Vector2.one * 0.75f;
        b.isAffectCharacter = false;
        b.isAffectFood = true;
        b.SetStandardVelocity(18);
        b.SetRotate(Vector2.left);
        // ���һ������λ�Ƶ�����
        b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 15, 0, Vector3.up * i, MapManager.gridHeight));
        // GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 15, 0, Vector3.up * i, MapManager.gridHeight));
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// ������ѹ�ѷ����˺��ж�����
    /// </summary>
    /// <param name="pos"></param>
    private void CreateDamageAllyArea(Vector3 pos, float dmgRate)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 2, 1, "ItemCollideAlly");
        r.SetAffectHeight(0);
        r.isAffectFood = true;
        r.isAffectMouse = false;
        r.isAffectCharacter = false;
        r.SetInstantaneous();
        // ��ӽ������50%�������ֵ�˺���Ч����ͬʱ�����൱��200%����˺����Լ�
        r.SetOnFoodEnterAction((u)=>{
            float dmg = 0.5f*u.mMaxHp;
            new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
            if(!u.NumericBox.GetBoolNumericValue(StringManager.Invincibility))
                new DamageAction(CombatAction.ActionType.ReboundDamage, u, this, dmg*dmgRate).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// ������ѹ�з����˺��ж�����
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dmgRate"></param>
    private RetangleAreaEffectExecution CreateDamageEnemyArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f * MapManager.gridWidth * Vector3.right, 2, 1, "ItemCollideEnemy");
        r.SetAffectHeight(0);
        r.isAffectFood = false;
        r.isAffectMouse = true;
        r.isAffectCharacter = false;
        r.AddExcludeMouseUnit(this);
        // ��ӽ�������˺�Ч��
        r.SetOnEnemyEnterAction((u) => {
            if (u.IsBoss())
                return;
            UnitManager.Execute(this, u);
        });
        GameController.Instance.AddAreaEffectExecution(r);
        // ��Ӹ�������
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                r.transform.position = transform.position + 0.5f * MapManager.gridWidth * Vector3.right;
                return false;
            });
            r.AddTask(t);
        }

        return r;
    }

    ///////////////////////////////////////////////////////////////������BOSS�ļ��ܶ���////////////////////////////////////////////////////

    /// <summary>
    /// ˲��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move(Vector3 pos)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
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
                    transform.position = pos;
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// Ѱ��һ�����ʵĿ�����ǹ��λ��
    /// �������е������С��ڶ��е���������Χ�ɵľ����� ���� �ɹ�����Ƭ�������ٵ����и��� �� ���ѡȡĳ�����һ����ΪĿ���
    /// </summary>
    /// <returns></returns>
    private Vector3 FindGunPosition()
    {
        // l1�����η�Χ�����и���
        List<BaseGrid> l1 = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(), MapManager.GetColumnX(4 - 0.5f), MapManager.GetColumnX(6 + 0.5f), MapManager.GetRowY(0.5f), MapManager.GetRowY(5.5f));
        // l2���ɹ�����Ƭ�������ٵ����и���
        List<BaseGrid> l2 = GridManager.GetGridListWhichHasMinCondition(l1, (g) => {
            return g.GetAttackableFoodUnitList().Count;
        });
        // ���ȡһ���õ��㷨��BOSS�����ȡ������㷨
        if(l2.Count > 0)
        {
            return l2[GetRandomNext(0, l2.Count)].transform.position + MapManager.gridWidth*Vector3.right;
        }
        else
        {
            // ��Ҫ��һ��û�еĻ��������������������������Ҹ�λ�ð��ֵ�ɣ����������������Ϊ�����ܷ������������ͼ�������������һ��û��
            int xIndex = GetRandomNext(4, 6);
            int yIndex = GetRandomNext(1, 5);
            return MapManager.GetGridLocalPosition(xIndex, yIndex);
        }
    }


    /// <summary>
    /// Ѱ��һ���ʺ�쭳���λ��
    /// ��������ѡȡ������Ҳ�Ŀɹ�����Ƭ��һ����ΪĿ����
    /// </summary>
    /// <returns></returns>
    private Vector3 FindDragRacingPosition()
    {
        // �жϷ�����ȡ��ǰ�����һ��
        Func<BaseUnit, BaseUnit, bool> RowCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
                return true;
            else if (u2 == null)
                return false;
            return u2.transform.position.x > u1.transform.position.x;
        };

        // �жϷ�����ȡ��ҵ�
        Func<BaseUnit, BaseUnit, int> LastCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
            {
                if (u2 == null)
                    return -1;
                else
                    return 1;
            }
            else if (u2 == null)
                return -1;

            if (u2.transform.position.x > u1.transform.position.x)
            {
                return 1;
            }
            else if (u2.transform.position.x == u1.transform.position.x)
            {
                return 0;
            }
            else
                return -1;
        };
        // ��ȡĿ����
        List<int> list = FoodManager.GetRowListBySpecificConditions(RowCompareFunc, LastCompareFunc);
        if (list.Count > 0)
        {
            int selectedRow = list[GetRandomNext(0, list.Count)];
            return MapManager.GetGridLocalPosition(8, selectedRow);
        }
        else
        {
            // �ҷ�����������ʲôʱ�������������
            // Ҳ�������Գ���û����ͼ�����п��ܻ���ְ�
            GetRandomNext(0, 2);
            return MapManager.GetGridLocalPosition(8, 3);
        }
    }

    /// <summary>
    /// ��һ��ǹ
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Shoot(float dmg, float areaDmg, float childDmg)
    {
        bool attackFlag = true;
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // �������
        int count = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Attack");
            num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // �������(��̬ˢ�£���
            count = 1;
            attackFlag = true;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (count >= num0_0)
                        return true;
                    else
                    {
                        animatorController.Play("Attack", false, 0);
                        count++;
                        attackFlag = true;
                        return false;
                    }
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.57f && attackFlag)
                {
                    // �����䵯
                    CreateBullet(dmg, areaDmg, childDmg);
                    attackFlag = false;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// ����ǹ
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility CanisterInit(SkillAbility.SkillAbilityInfo info)
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // ׼��ʱ��
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // �������
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex); // �����������˺�
        float dmg0_1 = GetParamValue("dmg0_1", mHertIndex); // �������Ʒ�Χ�˺�
        float dmg0_2 = GetParamValue("dmg0_2", mHertIndex); // ����С�����˺�
        int count0_0 = Mathf.FloorToInt(GetParamValue("count0_0", mHertIndex)); // �ظ�ʩ�Ŵ���

        // ��������
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate{ };
        for (int j = 0; j <= count0_0; j++)
        {
            c.AddSpellingFunc(delegate
            {
                // ��ʼ������
                timeLeft = 0;

                // ȥ��λ��
                Vector3 v3 = FindGunPosition();
                // Ȼ��˲�Ƶ��Ǹ�λ��
                c.ActivateChildAbility(Move(v3));
                return true;
            });

            c.AddSpellingFunc(delegate
            {
                // ͣ�ͣ���߳�ѽҡҡ��
                animatorController.Play("Idle", true);
                timeLeft = t0_0;
                return true;
            });
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
                    // Ȼ��ǹ��
                    return true;
                }
            });
            // ��ǹ
            c.AddSpellingFunc(delegate {
                c.ActivateChildAbility(Shoot(dmg0_0, dmg0_1, dmg0_2));
                return true;
            });
            // ռλ�����룩
            c.AddSpellingFunc(delegate {
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 쭳�
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility DragRacingInit(SkillAbility.SkillAbilityInfo info)
    {
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // 쭳�ǰҡ
        float v1_0 = TransManager.TranToVelocity(GetParamValue("v1_0", mHertIndex)); // �ٶ�
        float dmgRate1_0 = GetParamValue("dmgRate1_0", mHertIndex)/100; // ��������ת������
        int moveTime = 4 * 60 - 55;
        int dmgAllyInterval = 20; // ÿ1/3�����һ�η�Χ�˺��ж�

        // ��������
        int timeLeft = 0;
        int dmgTimeLeft = 0;

        // ǰ������
        Action moveUpdate = delegate {
            // ǰ�����£����赲ʱ����75%�ٶȣ�
            if (IsBlock())
                transform.position += 0.75f * Vector3.left * v1_0;
            else
                transform.position += Vector3.left * v1_0;

            // �˺��ж�
            if (dmgTimeLeft > 0)
                dmgTimeLeft--;
            else
            {
                // ������ѹ�ж���
                CreateDamageAllyArea(transform.position + 0.5f * MapManager.gridWidth * Vector3.right, dmgRate1_0);
                dmgTimeLeft += dmgAllyInterval;
            }
        };

        RetangleAreaEffectExecution dmgEnemyArea = null; // �˺����˵��ж���

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            dmgTimeLeft = 0;
            dmgEnemyArea = null;

            Vector3 v3 = FindDragRacingPosition(); // ȥ��һ�����ʵ�쭳�����
            c.ActivateChildAbility(Move(v3));
        };
        {
            // ������ǰҡ
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("PreDrag");
                return true;
            });
            // �����ӵȴ�ʱ��
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Drag", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // �����Ӻ�ҡ
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    animatorController.Play("PostDrag");
                    return true;
                }
                return false;
            });
            // ǰ��ǰҡ
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("PreMove");
                    timeLeft = moveTime;
                    // �����˺����˵��ж���
                    dmgEnemyArea = CreateDamageEnemyArea();
                    return true;
                }
                return false;
            });
            // ǰ������
            c.AddSpellingFunc(delegate {
                moveUpdate();
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Move", true);
                    return true;
                }
                return false;
            });
            // ǰ��ʱÿ֡����
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 쭳�����ʱɾ���Ե��˵��ж���
                    if (dmgEnemyArea != null)
                        dmgEnemyArea.MDestory();
                    animatorController.Play("PostMove", true);
                    return true;
                }
                moveUpdate();
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });

            //c.AddSpellingFunc(delegate {
            //    if (timeLeft > 0)
            //    {
            //        timeLeft--;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //    return false;
            //});

            //c.AddSpellingFunc(delegate {
            //    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            //    {
            //        return true;
            //    }
            //    return false;
            //});

        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

}
