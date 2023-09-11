using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// ��¡¡
/// </summary>
public class Thundered : BossUnit
{
    // ������
    private CustomizationSkillAbility Skill_PushDown; // �߿�ѹ��
    private CustomizationSkillAbility Skill_Missile; // ��������
    private CustomizationSkillAbility Skill_Laser; // ���𼤹�

    public override void MInit()
    {
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // ��ӳ��ֵļ���
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Appear");
            // ���ɱ��ӵ�����
            AddCanHitFunc(noHitFunc);
        };
        c.AddSpellingFunc(delegate {
            if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                return true;
            return false;
        });
        c.AfterSpellFunc = delegate
        {
            RemoveCanHitFunc(noHitFunc);
        };
        // ǿ�����Ƶ�ǰ����Ϊ���
        mSkillQueueAbilityManager.SetCurrentSkill(c);
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.Thundered, 0))
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
        
        list.Add(PushDownInit(infoList[0])); // �߿�ѹ��
        list.Add(MissileInit(infoList[1])); // ��������
        list.Add(PushDownInit(infoList[0])); // �߿�ѹ��
        list.Add(LasterInit(infoList[2])); // ���𼤹�

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


    public override void MUpdate()
    {
        base.MUpdate();
    }

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////
    
    /// <summary>
    /// �߿�ѹ��
    /// </summary>
    private CustomizationSkillAbility PushDownInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_PushDown = c;
        bool isFly = false; // �Ƿ��ڷ���״̬
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // ����ʱ�Ĺ۲�ʱ��
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex)*60); // �ƶ�ʱ��
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex)*60); // �½�ǰͣ��ʱ��
        int t0_3 = Mathf.FloorToInt(GetParamValue("t0_3", mHertIndex)*60); // ��ѹ���ԭ��ͣ��ʱ��
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero; // ��ʼ��
        Vector2 targetPos = Vector2.zero; // Ŀ��ѹ�Ƶ�
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isFly = false;
            timeLeft = 0;
            startPos = Vector2.zero;
            targetPos = Vector2.zero;
        };
        {
            // ����
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // ������һ���ʱ���߶���Ϊ1�����������󶯻���Ϊ���д���
            c.AddSpellingFunc(delegate {
                if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>=0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t0_0;
                    return true;
                }
                return false;
            });
            // �ڿ��й۲�t0_0ʱ���ȷ��Ŀ���
            c.AddSpellingFunc(delegate
            {
                if(timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // ����ȡ����㷨����֤2*2ѹ�Ƶ�����λ����һ�е�������֮��
                    //SetNextGridIndexByRandom(0, 5, 1, 6);
                    //Vector2 v2 = GetNextGridIndex();
                    int rowIndex;
                    int colIndex;
                    FindR0C0(out rowIndex, out colIndex);
                    targetPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    startPos = transform.position;
                    if((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t0_1;
                    return true;
                }
                return false;
            });
            // �ڿ����ƶ�t0_1ʱ��󵽴�Ŀ���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t0_1;
                }
                else
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t0_2;
                    return true;
                }
                return false;
            });
            // �ڿ���ͣ��t0_2ʱ���׼����ѹ
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    animatorController.Play("PushDown");
                    return true;
                }
                return false;
            });
            // ��ѹ��һ���ʱ���߶���Ϊ0�������Ϊδ���գ�ͬʱ�����˺��ж�
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;

                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*new Vector3(MapManager.gridWidth, MapManager.gridHeight), 1.75f, 1.75f, "EnemyAllyGrid");
                    r.SetInstantaneous();
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
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle0", true);
                    timeLeft = t0_3;
                    return true;
                }
                return false;
            });
            // ��ͣ��t0_3ʱ�������ü���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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
    /// ��������
    /// </summary>
    private CustomizationSkillAbility MissileInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_Missile = c;
        bool isFly = false; // �Ƿ��ڷ���״̬
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // ����ʱ�Ĺ۲�ʱ��
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex)*60); // �ƶ�ʱ��
        
        int t1_2 = Mathf.FloorToInt(GetParamValue("t1_2", mHertIndex)*60); // ������������ͣ��ʱ��

        int timeLeft = 0;
        int missileCounter = 0; // ����������

        Vector2 startPos = Vector2.zero; // ��ʼ��
        Vector2 targetPos = Vector2.zero; // Ŀ���
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            timeLeft = 0;
            missileCounter = 0;
            isFly = false;
        };
        {
            // ����
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // ������һ���ʱ���߶���Ϊ1�����������󶯻���Ϊ���д���
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // �ڿ��й۲�t1_0ʱ���ȷ��Ŀ���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // ��ȡ����ΪĿ�����ʳ�����ٵļ���
                    List<int> rowList = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
                    // �������ȡһ�У���������BOSS�����ȫ�޹�Ҳ��Ӱ�쵱ǰBOSS��������ӣ�
                    int selectRowIndex = rowList[GameController.Instance.GetRandomInt(0, rowList.Count)];
                    targetPos = new Vector2(MapManager.GetColumnX(8), MapManager.GetRowY(selectRowIndex));
                    startPos = transform.position;
                    if ((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t1_1;
                    return true;
                }
                return false;
            });
            // �ڿ����ƶ�t1_1ʱ��󵽴�Ŀ���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t1_1;
                }
                else
                {
                    animatorController.Play("Down");
                    return true;
                }
                return false;
            });
            // �½���һ���ʱ���߶���Ϊ0�������Ϊδ���գ�Ȼ�󷢵���
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Missile");
                    return true;
                }
                return false;
            });

            // ������Ҫ�ֽ׶�
            if(mHertIndex == 0)
            {
                // ��һ������
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3 && missileCounter == 0)
                    {
                        // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
                        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                        if (targetUnit == null)
                            CreateMissile(new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex())));
                        else
                            CreateMissile(targetUnit.transform.position);
                        missileCounter++;
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }else if(mHertIndex == 1)
            {
                // ������������ͬһλ��
                c.AddSpellingFunc(delegate {
                    for (int i = 0; i < 3; i++)
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3*(i+1) && missileCounter == i)
                        {
                            // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
                            BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                            if (targetUnit == null)
                                CreateMissile(new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex())));
                            else
                                CreateMissile(targetUnit.transform.position);
                            missileCounter++;
                        }
                    }

                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }
            else
            {
                // ����������������
                c.AddSpellingFunc(delegate {
                    for (int i = 0; i < 3; i++)
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3 * (i + 1) && missileCounter == i)
                        {
                            if (i == 0)
                            {
                                // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
                                BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                                if (targetUnit == null)
                                    targetPos = new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex()));
                                else
                                    targetPos = targetUnit.transform.position;
                            }
                            CreateMissile(targetPos + Vector2.up * MapManager.gridHeight * (i - 1));
                            missileCounter++;
                        }
                    }

                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }




            // ��ͣ��t1_2ʱ�������ü���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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
    /// ����һ������
    /// </summary>
    private void CreateMissile(Vector2 targetpos)
    {
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60); // ���������ѣʱ��
        EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/4/Missile"), this, 0);
        // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
        b.GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetThrowHighestAttackPriorityUnitInclude(this);
            }
            return unit;
        };
        b.AddHitAction((b, u) => {
            if (u is CharacterUnit)
            {
                // ������������Ϊ����ʩ����ѣЧ��
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun1_0, false));
            }
            else
            {
                if (u != null && u.IsAlive())
                    BurnManager.BurnDamage(this, u);
            }
        });
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetpos, true));
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// ���𼤹�
    /// </summary>
    private CustomizationSkillAbility LasterInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_Laser = c;
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex)*60); // ����ʱ�Ĺ۲�ʱ��
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex)*60); // �ƶ�ʱ��
        int t2_2 = Mathf.FloorToInt(GetParamValue("t2_2", mHertIndex)*60); // ��������ʱ��
        int t2_3 = Mathf.FloorToInt(GetParamValue("t2_3", mHertIndex)*60); // �����ԭ��ͣ��ʱ��

        bool isFly = false; // �Ƿ��ڷ���״̬
        int timeLeft = 0;

        Vector2 startPos = Vector2.zero; // ��ʼ��
        Vector2 targetPos = Vector2.zero; // Ŀ���
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            isFly = false;
            timeLeft = 0;
        };
        {
            // ����
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // ������һ���ʱ���߶���Ϊ1�����������󶯻���Ϊ���д���
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t2_0;
                    return true;
                }
                return false;
            });
            // �ڿ��й۲�t2_0ʱ���ȷ��Ŀ���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // ��ȡ����ΪĿ�����ʳ�����ļ���
                    List<int> rowList = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                    // �������ȡһ�У���������BOSS�����ȫ�޹�Ҳ��Ӱ�쵱ǰBOSS��������ӣ�
                    int selectRowIndex = rowList[GameController.Instance.GetRandomInt(0, rowList.Count)];
                    targetPos = new Vector2(MapManager.GetColumnX(8), MapManager.GetRowY(selectRowIndex));
                    startPos = transform.position;
                    if ((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t2_1;
                    return true;
                }
                return false;
            });
            // �ڿ����ƶ�t2_1ʱ��󵽴�Ŀ���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t2_1;
                }
                else
                {
                    animatorController.Play("Down");
                    return true;
                }
                return false;
            });
            // �½���һ���ʱ���߶���Ϊ0�������Ϊδ���գ�Ȼ�󷢼���
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Laser", true);
                    timeLeft = t2_2;
                    return true;
                }
                return false;
            });
            // ����t2_2s���伤�⣬Ȼ��ͣ��
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // ��һ��������Ч
                    BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("BOSS/4/Laser"), null, "Laser", null, false);
                    e.transform.position = transform.position;
                    GameController.Instance.AddEffect(e);
                    // һ�лҽ�Ч��
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(4.0f), MapManager.GetRowY(GetRowIndex())), 9, 0.5f, "EnemyAllyGrid");
                        r.SetInstantaneous();
                        r.isAffectFood = false;
                        r.isAffectCharacter = false;
                        r.isAffectMouse = true;
                        r.isAffectGrid = true;
                        r.SetOnGridEnterAction((g) => {
                            BaseUnit u = g.GetHighestAttackPriorityUnit(this);
                            if (u != null && !(u is CharacterUnit))
                                BurnManager.BurnDamage(this, u);
                        });
                        r.SetOnEnemyEnterAction((u) => {
                            BurnManager.BurnDamage(this, u);
                        });
                        r.AddExcludeMouseUnit(this);
                        GameController.Instance.AddAreaEffectExecution(r);
                    }

                    animatorController.Play("Idle0", true);
                    timeLeft = t2_3;
                    return true;
                }
                return false;
            });
            // ��ͣ��t2_3ʱ�������ü���
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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


    private void FindR0C0(out int rowIndex, out int colIndex)
    {
        // Ȼ���������ұ߿�����
        List<int> list = new List<int>();
        float max = float.MinValue;
        for (int row = 0; row < 7; row++)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(row, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
            {
                if (unit.transform.position.x > max)
                {
                    max = unit.transform.position.x;
                    list.Clear();
                    list.Add(row);
                }
                else if (unit.transform.position.x == max)
                {
                    list.Add(row);
                }
            }
        }
        // ����Щ���������һ�а�
        if (list.Count > 0)
        {
            rowIndex = Mathf.Max(1, Mathf.Min(6, list[GetRandomNext(0, list.Count)]));
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit !=null)
                colIndex = Mathf.Max(0, Mathf.Min(5, unit.GetColumnIndex() - 1));
            else
                colIndex = 5;
        }
        else
        {
            // �����Ǳ��մ�����û�к��ʵĽ���Ļ��Ǿ͹̶���·���ְ�
            GetRandomNext(0, 1);
            rowIndex = 3;
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
                colIndex = Mathf.Max(1, Mathf.Min(5, unit.GetColumnIndex() - 1));
            else
                colIndex = 5;
        }
    }
}
