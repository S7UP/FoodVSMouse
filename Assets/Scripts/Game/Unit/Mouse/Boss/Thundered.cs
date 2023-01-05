using System.Collections.Generic;
using System;
using UnityEngine;
using static BombAreaEffectExecution;
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
    public override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // �߿�ѹ��
        AddParamArray("t0_0", new float[] { 1, 0.5f,0 }); // ����ʱ�Ĺ۲�ʱ��
        AddParamArray("t0_1", new float[] { 4, 3, 2 }); // �ƶ�ʱ��
        AddParamArray("t0_2", new float[] { 1, 0.5f, 0 }); // �½�ǰͣ��ʱ��
        AddParamArray("dmg0_0", new float[] { 900, 900, 900 }); // ��ѹ�˺�
        AddParamArray("t0_3", new float[] { 3, 1.5f, 0.5f }); // ��ѹ���ԭ��ͣ��ʱ��
        // ��������
        AddParamArray("t1_0", new float[] { 1, 0.5f, 0 }); // ����ʱ�Ĺ۲�ʱ��
        AddParamArray("t1_1", new float[] { 4, 3, 2 }); // �ƶ�ʱ��
        AddParamArray("dmg1_0", new float[] { 900, 900, 900 }); // �����˺�
        AddParamArray("t1_2", new float[] { 3, 1.5f, 0.5f }); // ������������ͣ��ʱ��
        // ���𼤹�
        AddParamArray("t2_0", new float[] { 1, 0.5f, 0 }); // ����ʱ�Ĺ۲�ʱ��
        AddParamArray("t2_1", new float[] { 4, 3, 2 }); // �ƶ�ʱ��
        AddParamArray("t2_2", new float[] { 9, 6, 3 }); // ��������ʱ��
        AddParamArray("dmg2_0", new float[] { 900, 900, 900 }); // �����˺�
        AddParamArray("t2_3", new float[] { 2, 2, 2 }); // �����ԭ��ͣ��ʱ��
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
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex); // ��ѹ�˺�
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
                    SetNextGridIndexByRandom(0, 5, 1, 6);
                    Vector2 v2 = GetNextGridIndex();
                    targetPos = MapManager.GetGridLocalPosition(v2.x, v2.y);
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
                    DamageAreaEffectExecution e = DamageAreaEffectExecution.GetInstance();
                    e.Init(this, CombatAction.ActionType.CauseDamage, dmg0_0 * (mCurrentAttack/10), GetRowIndex(), 2, 2, 0.5f, 0.5f, true, true);
                    e.transform.position = transform.position;
                    e.AddExcludeMouseUnit(this); // ����Ҫ���ų����⣬��Ȼ�˵а˰�����һǧ
                    GameController.Instance.AddAreaEffectExecution(e);
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
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex); // �����˺�
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
                        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x);
                        Vector3 targetpos = Vector3.zero;
                        if (targetUnit == null)
                            targetpos = new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex()));
                        else
                            targetpos = targetUnit.transform.position;
                        EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/4/Missile"), this, dmg1_0 * (mCurrentAttack / 10));
                        TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(32f), 1.5f, transform.position, targetpos, true);
                        // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
                        b.GetTargetFunc = (unit) => {
                            BaseGrid g = unit.GetGrid();
                            if (g != null)
                            {
                                return g.GetThrowHighestAttackPriorityUnitInclude(this);
                            }
                            return unit;
                        };
                        GameController.Instance.AddBullet(b);
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
                            if (i == 0)
                            {
                                // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
                                BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x);
                                Vector3 targetpos = Vector3.zero;
                                if (targetUnit == null)
                                    targetPos = new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex()));
                                else
                                    targetPos = targetUnit.transform.position;
                            }

                            EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/4/Missile"), this, dmg1_0 * (mCurrentAttack / 10));
                            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(32f), 1.5f, transform.position, targetPos, true);
                            // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
                            b.GetTargetFunc = (unit) => {
                                BaseGrid g = unit.GetGrid();
                                if (g != null)
                                {
                                    return g.GetThrowHighestAttackPriorityUnitInclude(this);
                                }
                                return unit;
                            };
                            GameController.Instance.AddBullet(b);
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
                                BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x);
                                Vector3 targetpos = Vector3.zero;
                                if (targetUnit == null)
                                    targetPos = new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex()));
                                else
                                    targetPos = targetUnit.transform.position;
                            }
                            EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/4/Missile"), this, dmg1_0 * (mCurrentAttack / 10));
                            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(32f), 1.5f, transform.position, targetPos + Vector2.up*MapManager.gridHeight*(i-1), true);
                            // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
                            b.GetTargetFunc = (unit) => {
                                BaseGrid g = unit.GetGrid();
                                if (g != null)
                                {
                                    return g.GetThrowHighestAttackPriorityUnitInclude(this);
                                }
                                return unit;
                            };
                            GameController.Instance.AddBullet(b);
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
    /// ���𼤹�
    /// </summary>
    private CustomizationSkillAbility LasterInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_Laser = c;
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex)*60); // ����ʱ�Ĺ۲�ʱ��
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex)*60); // �ƶ�ʱ��
        int t2_2 = Mathf.FloorToInt(GetParamValue("t2_2", mHertIndex)*60); // ��������ʱ��
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex); // �����˺�
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
                        // ���ѷ���λ
                        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance(this, dmg2_0 * mCurrentAttack / 10, transform.position, 9, 0.5f, GridDamageAllyRange.Attackable, GridDamageAllyType.AscendingOrder);
                        bombEffect.transform.position = new Vector2(MapManager.GetColumnX(4.0f), MapManager.GetRowY(GetRowIndex()));
                        GameController.Instance.AddAreaEffectExecution(bombEffect);
                    }

                    {
                        // �Եз���λ
                        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance(this, dmg2_0 * mCurrentAttack / 10, transform.position, 9, 1);
                        bombEffect.isAffectMouse = true;
                        bombEffect.transform.position = new Vector2(MapManager.GetColumnX(4.0f), MapManager.GetRowY(GetRowIndex()));
                        bombEffect.AddExcludeMouseUnit(this);
                        GameController.Instance.AddAreaEffectExecution(bombEffect);
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
}
