using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ����
/// </summary>
public class IceSlag : BossUnit
{
    // ר��Ч��
    private static string IceMagicKey = "�����ı�ħ��";
    private static string FireMagicKey = "�����Ļ�ħ��";
    private Sprite Sprite_IceMagic;
    private Sprite Sprite_FireMagic;
    private RuntimeAnimatorController Run_IceBullet;
    private RuntimeAnimatorController Run_FireBullet;
    private RuntimeAnimatorController Run_LightBullet;
    private static BoolModifier IceMagic = new BoolModifier(true); // ��ħ��DEBUFF
    private static BoolModifier FireMagic = new BoolModifier(true); // ��ħ��DEBUFF

    // ����
    private bool isFireMode; // �Ƿ�Ϊ��ħ��ʦ��̬���Ǻڼ��ף����ǻ���Ǳ���
    private List<BaseUnit> affectedUnitList = new List<BaseUnit>(); // ��ħ��Ӱ��ĵ�λ��

    // ������
    private CustomizationSkillAbility Skill_CastBall; // ʩ��Ԫ����
    private CustomizationSkillAbility Skill_LightHit; // ���

    public override void Awake()
    {
        base.Awake();
        Sprite_IceMagic = GameManager.Instance.GetSprite("Boss/3/IceIcon");
        Sprite_FireMagic = GameManager.Instance.GetSprite("Boss/3/FireIcon");
        Run_IceBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/IceBullet");
        Run_FireBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/FireBullet");
        Run_LightBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/LightBullet");
    }

    public override void MInit()
    {
        affectedUnitList.Clear();
        isFireMode = true;
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // ��ӳ��ֵļ���
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            if(isFireMode)
                animatorController.Play("Appear1");
            else
                animatorController.Play("Appear0");
            // ���ɱ��ӵ�����
            AddCanHitFunc(noHitFunc);
        };
        c.AddSpellingFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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

        AddParamArray("dmg_boom", new float[] { 900, 900, 900 });
        // ʩ��Ԫ����
        AddParamArray("t0_0", new float[] { 5, 3, 1.334f }); // ����ʱ��
        AddParamArray("t0_1", new float[] { 2f, 1.75f, 1.5f }); // ��������ԭ��ͣ��ʱ��
        AddParamArray("t0_2", new float[] { 3, 4.5f, 6 }); // ����ѹ�ͱ�ը��Կ�Ƭ�����ѣʱ��
        AddParamArray("v0_0", new float[] { 3, 3, 3 }); // Ԫ������ٶ�
        AddParamArray("v0_1", new float[] { 6, 9, 12 }); // Ԫ����ĩ�ٶ�
        // ���
        AddParamArray("t1_0", new float[] { 3, 1.5f, 0 }); // ������ǰ������ʱ��
        AddParamArray("t1_1", new float[] { 2.25f, 2.25f, 1.5f }); // �ⵯ�Ĺ������
        AddParamArray("t1_2", new float[] { 1.5f, 0.75f, 0 }); // �ⵯ������ͣ��ʱ��
        AddParamArray("num1_0", new float[] { 4, 3, 5 }); // �ⵯ��
        AddParamArray("num1_1", new float[] { 0, 1, 1 }); // Ԫ�ص���
        AddParamArray("dmg1_0", new float[] { 900, 900, 900 }); // �ⵯ�˺�
        AddParamArray("stun1_0", new float[] { 9, 9, 9 }); // �ⵯ���������ѣʱ��
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        list.Add(CastBallInit(infoList[0])); // ����
        list.Add(LightHitInit(infoList[1])); // ���

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


    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// Ϊĳ��Ŀ��ʩ�ӱ�ħ��debuff
    /// </summary>
    private void AddIceMagic(BaseUnit unit)
    {
        // ����Լ��������򲻻�ʩ��ħ��Ч��
        if (!IsAlive())
            return;

        float dmg_boom = GetParamValue("dmg_boom", mHertIndex);

        // ���Լ���BOSS��λ���ܸ�BUFFӰ��
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() && m != this)
                return;
        }

        // ���Ŀ���ѱ�ʩ�ӻ�ħ��������ɱ�ըЧ�����Ƴ���ħ��
        if (unit.NumericBox.GetBoolNumericValue(FireMagicKey))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg_boom * (mCurrentAttack / 10)).ApplyAction();
            RemoveFireMagic(unit);
        }
        else if (!unit.NumericBox.GetBoolNumericValue(IceMagicKey))
        {
            // ����ʩ�ӱ�ħ��
            unit.NumericBox.AddDecideModifierToBoolDict(IceMagicKey, IceMagic);
            affectedUnitList.Add(unit);
            // �����Ч
            BaseEffect e = BaseEffect.CreateInstance(Sprite_IceMagic);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(e);
            unit.AddEffectToDict(IceMagicKey, e, 0f*MapManager.gridHeight*Vector2.up);
        }
    }

    /// <summary>
    /// �Ƴ���ħ��
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveIceMagic(BaseUnit unit)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(IceMagicKey, IceMagic);
        affectedUnitList.Remove(unit);
        // �Ƴ���Ч
        unit.RemoveEffectFromDict(IceMagicKey);
    }

    /// <summary>
    /// Ϊĳ��Ŀ��ʩ�ӻ�ħ��debuff
    /// </summary>
    private void AddFireMagic(BaseUnit unit)
    {
        // ����Լ��������򲻻�ʩ��ħ��Ч��
        if (!IsAlive())
            return;

        float dmg_boom = GetParamValue("dmg_boom", mHertIndex);

        // ���Լ���BOSS��λ���ܸ�BUFFӰ��
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() && m != this)
                return;
        }

        // ���Ŀ���ѱ�ʩ�ӱ�ħ��������ɱ�ըЧ�����Ƴ���ħ��
        if (unit.NumericBox.GetBoolNumericValue(IceMagicKey))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg_boom * (mCurrentAttack / 10)).ApplyAction();
            RemoveIceMagic(unit);
        }
        else if (!unit.NumericBox.GetBoolNumericValue(FireMagicKey))
        {
            // ����ʩ�ӻ�ħ��
            unit.NumericBox.AddDecideModifierToBoolDict(FireMagicKey, FireMagic);
            affectedUnitList.Add(unit);
            // �����Ч
            BaseEffect e = BaseEffect.CreateInstance(Sprite_FireMagic);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(e);
            unit.AddEffectToDict(FireMagicKey, e, 0f * MapManager.gridHeight * Vector2.up);
        }
    }

    /// <summary>
    /// �Ƴ���ħ��
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveFireMagic(BaseUnit unit)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(FireMagicKey, FireMagic);
        affectedUnitList.Remove(unit);
        // �Ƴ���Ч
        unit.RemoveEffectFromDict(FireMagicKey);
    }

    public override void OnDieStateEnter()
    {
        if(isFireMode)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    /// <summary>
    /// ����ʱ�Ƴ�Ŀ��ı���ħ��
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        List<BaseUnit> l = new List<BaseUnit>();
        foreach (var unit in affectedUnitList)
        {
            l.Add(unit);
        }
        foreach (var unit in l)
        {
            RemoveIceMagic(unit);
            RemoveFireMagic(unit);
        }
    }

    /// <summary>
    /// Ŀ���Ԫ���Ƿ��������෴
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool IsOpposite(BaseUnit unit)
    {
        if ((isFireMode && unit.NumericBox.GetBoolNumericValue(IceMagicKey)) || (!isFireMode && unit.NumericBox.GetBoolNumericValue(FireMagicKey)))
            return true;
        return false;
    }

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////

    /// <summary>
    /// ������
    /// </summary>
    private CustomizationSkillAbility CastBallInit(SkillAbility.SkillAbilityInfo info)
    {
        // �жϷ�����Ŀ����������Ԫ������Ŀɹ����ѷ���λ
        Func<BaseUnit, bool> IsOppositeAndCanTargetedAlly = (unit) =>
        {
            if (unit is FoodUnit)
            {
                FoodUnit f = unit as FoodUnit;
                FoodInGridType t = f.GetFoodInGridType();
                if (IsOpposite(unit) && (t.Equals(FoodInGridType.Default) || t.Equals(FoodInGridType.Shield)))
                    return true;
            }
            return false;
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_CastBall = c;
        bool isNoHit = false;
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60); // ����ʱ��
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex) * 60); // ��������ԭ��ͣ��ʱ��
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // ��ը��Կ�Ƭ�����ѣʱ��

        int timeLeft = 0;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            isNoHit = false;
        };
        {
            // ��ʧ����
            c.AddSpellingFunc(delegate {
                if(isFireMode)
                    animatorController.Play("Disappear1");
                else
                    animatorController.Play("Disappear0");
                isNoHit = true;
                CloseCollision();
                return true;
            });
            // ����ȷ��˲��λ��Ȼ�����
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isFireMode = !isFireMode; // �л�ģʽ
                    // �������� ��/�� ħ��
                    if (isFireMode)
                    {
                        RemoveIceMagic(this);
                        AddFireMagic(this);
                    }
                    else
                    {
                        RemoveFireMagic(this);
                        AddIceMagic(this);
                    }

                    List<int> list = FoodManager.GetRowListWhichHasMaxConditionAllyCount(IsOppositeAndCanTargetedAlly);
                    if (list.Count < GameController.Instance.mAllyList.Length)
                    {
                        // ����������7�У���ô�����������ȡһ����ΪĿ����
                        int selectRowIndex = list[GameController.Instance.GetRandomInt(0, list.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }
                    else
                    {
                        // ������ֽ����7��һ�£���ô���ܵ����Ӧ����ȫ����û�п�Ƭ����ħ��Ч��������һ��ʩ�Ÿü���ʱ�����߾��Ǹոպ�ÿ������һ����
                        // ����������£�����ѡȡ����Ϊ����Ŀ��Ŀ�Ƭ������������ΪĿ����
                        List<int> list2 = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                        int selectRowIndex = list2[GameController.Instance.GetRandomInt(0, list2.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }

                    if (isFireMode)
                        animatorController.Play("Appear1");
                    else
                        animatorController.Play("Appear0");
                    return true;
                }
                return false;
            });
            // ׼��������
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isNoHit)
                {
                    isNoHit = false;
                    OpenCollision();
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (isFireMode)
                        animatorController.Play("Charge1", true);
                    else
                        animatorController.Play("Charge0", true);
                    timeLeft = t0_0;
                    return true;
                }
                return false;
            });
            // ��t0_0ʱ������ų�����Ȼ��ͣ��
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    CreateBall();
                    if (isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    timeLeft = t0_1;
                    return true;
                }
                return false;
            });
            // ͣ�����˳�
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
    /// ���
    /// </summary>
    private CustomizationSkillAbility LightHitInit(SkillAbility.SkillAbilityInfo info)
    {
        // �жϷ�����ȡ��ǰ�����һ��
        Func<BaseUnit, BaseUnit, bool> RowCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
                return true;
            else if (u2 == null)
                return false;
            return u2.transform.position.x < u1.transform.position.x;
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
            else if(u2 == null)
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

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_LightHit = c;
        bool isNoHit = false;

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60); // ������ǰ������ʱ��
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex) * 60); // �ⵯ�Ĺ������
        int t1_2 = Mathf.FloorToInt(GetParamValue("t1_2", mHertIndex) * 60); // �ⵯ������ͣ��ʱ��
        int num1_0 = Mathf.FloorToInt(GetParamValue("num1_0", mHertIndex)); // �ⵯ��
        int num1_1 = Mathf.FloorToInt(GetParamValue("num1_1", mHertIndex)); // Ԫ�ص���
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex); // �ⵯ�˺�

        //int attack_time = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Attack0").aniTime); // �ⵯ�Ķ���ʱ��
        //int ExcuteDamage_time = Mathf.FloorToInt(0.5f*animatorController.GetAnimatorStateRecorder("Attack0").aniTime); // �ⵯ�ķ��䴥��ʱ��
        int attack_time = 85; // �ⵯ�Ķ���ʱ��
        int ExcuteDamage_time = Mathf.FloorToInt(0.5f * attack_time); // �ⵯ�ķ��䴥��ʱ��

        int n = 0;
        int timeLeft = 0;
        float minLeftX = 0; // �ⵯ��Զ���Դ򵽵�λ��

        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isNoHit = true;
            CloseCollision();
            n = 0;
            timeLeft = 0;
            minLeftX = MapManager.GetColumnX(0);
        };
        {
            // ��ʧ����
            c.AddSpellingFunc(delegate {
                if (isFireMode)
                    animatorController.Play("Disappear1");
                else
                    animatorController.Play("Disappear0");
                isNoHit = true;
                CloseCollision();
                return true;
            });
            // ����ȷ��˲��λ��Ȼ�����
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    List<int> list = FoodManager.GetRowListBySpecificConditions(RowCompareFunc, LastCompareFunc);
                    int selectRowIndex;
                    if (list.Count == 0)
                    {
                        // ���û�н���������ȡһ�а�
                        selectRowIndex = GameController.Instance.GetRandomInt(0, GameController.Instance.mAllyList.Length);
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }
                    else
                    {
                        // �����ڽ���������ȡһ��
                        selectRowIndex = list[GameController.Instance.GetRandomInt(0, list.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }   

                    if (isFireMode)
                        animatorController.Play("Appear1");
                    else
                        animatorController.Play("Appear0");
                    return true;
                }
                return false;
            });
            // ����
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isNoHit)
                {
                    isNoHit = false;
                    OpenCollision();
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // 
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
            // ÿ��t1_1�ӹⵯ
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    if(timeLeft == t1_1 - attack_time)
                    {
                        if (isFireMode)
                            animatorController.Play("Idle1", true);
                        else
                            animatorController.Play("Idle0", true);
                    }
                    else if(timeLeft == t1_1 - attack_time + ExcuteDamage_time)
                    {
                        // ���䵯��
                        CreateLightBullet(minLeftX);
                    }
                }else if(n < num1_0)
                {
                    n++;
                    timeLeft = t1_1;
                    if (isFireMode)
                        animatorController.Play("Attack1");
                    else
                        animatorController.Play("Attack0");
                }
                else if(n == num1_0)
                {
                    n = 0;
                    // timeLeft = t1_1;
                    //if (isFireMode)
                    //    animatorController.Play("Attack1");
                    //else
                    //    animatorController.Play("Attack0");
                    return true;
                }
                return false;
            });

            // ��Ԫ�عⵯ��Ȼ��ͣ��
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    if (timeLeft == t1_1 - attack_time)
                    {
                        if (isFireMode)
                            animatorController.Play("Idle1", true);
                        else
                            animatorController.Play("Idle0", true);
                    }
                    else if (timeLeft == t1_1 - attack_time + ExcuteDamage_time)
                    {
                        // ���䵯��
                        CreateElementalBullet(minLeftX);
                    }
                }
                else if (n < num1_1)
                {
                    n++;
                    timeLeft = t1_1;
                    if (isFireMode)
                        animatorController.Play("Attack1");
                    else
                        animatorController.Play("Attack0");
                }else
                {
                    timeLeft = t1_2;
                    if(isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    return true;
                }
                return false;
            });
            // ͣ�����˳�
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
    /// ����Ԫ����
    /// </summary>
    private void CreateBall()
    {
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // ��ը��Կ�Ƭ�����ѣʱ��
        int v0_0 = Mathf.FloorToInt(GetParamValue("v0_0", mHertIndex));
        int v0_1 = Mathf.FloorToInt(GetParamValue("v0_1", mHertIndex));

        // ����һ�����ΰ�ը������
        MouseUnit m = MouseManager.GetBombedToolMouse();

        string ball_res;
        bool isFireBall = isFireMode;
        // �Ե�λʩ��Ԫ��Ч�� �Լ� ����Ч��
        Action<BaseUnit> unitAction = (unit) =>
        {
            if (isFireBall)
            {
                AddFireMagic(unit);
                if(unit != m)
                    unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, t0_2, false));
            }
            else
            {
                AddIceMagic(unit);
                if (unit != m)
                    unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, t0_2, false));
            }
        };

        if (isFireMode)
            ball_res = "FireBall";
        else
            ball_res = "IceBall";



        {
            if (!isFireBall)
            {
                m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
                m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߱���
            }
            m.NumericBox.Defense.SetBase(100); // 100%����

            // ���һ�����֡����ٵ�����
            CustomizationTask t = new CustomizationTask();
            float v0 = TransManager.TranToVelocity(v0_0);
            float v1 = TransManager.TranToVelocity(v0_1);
            int accTime = 180;
            float acc = (v1 - v0) / accTime;
            int timeLeft = accTime;
            t.AddOnEnterAction(delegate
            {
                m.transform.position = transform.position;
                m.SetMoveRoate(moveRotate);
            });
            // ����
            t.AddTaskFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    v0 += acc;
                    m.SetPosition(m.GetPosition() + v0 * (Vector3)m.moveRotate);
                }
                m.SetPosition(m.GetPosition() + v0 * (Vector3)m.moveRotate);
                return false;
            });
            m.AddTask(t);

            // �����һ����ת�͵��������б�ը��Ч��
            CustomizationTask t2 = new CustomizationTask();
            t2.AddTaskFunc(delegate {
                if (m.transform.position.x < MapManager.GetColumnX(3))
                    return true;
                // ���Ŀ�걻���������ǻ�����ֱ���Ա�
                if (isFireBall)
                {
                    if (m.GetNoCountUniqueStatus(StringManager.Frozen) != null)
                    {
                        return true;
                    }
                    else if (m.GetNoCountUniqueStatus(StringManager.Stun) != null)
                    {
                        // �����ѣЧ��
                        m.GetNoCountUniqueStatus(StringManager.Stun).ClearLeftTime();
                    }
                }
                return false;
            });
            t2.AddOnExitAction(delegate
            {
                m.ExecuteDeath();
            });
            m.AddTask(t2);
        }

        // ����Ϊ��ĵз��ӵ�
        EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/3/"+ ball_res), this, 0);
        {
            b.CloseCollision(); // �����Ǹ����ж����ӵ����ж�д�ھ��η�ΧЧ����
            // ���һ�����֡����ٵ�����
            CustomizationTask t = new CustomizationTask();
            int totalTime = 30;
            int timeLeft = totalTime;
            t.AddOnEnterAction(delegate
            {
                b.transform.localScale = Vector2.zero;
                b.SetRotate(moveRotate);
            });
            // ���
            t.AddTaskFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    b.transform.localScale = Vector2.one * (1 - (float)timeLeft/totalTime);
                }
                else
                {
                    return true;
                }
                return false;
            });
            b.AddTask(t);

            // �����һ�����桢��ת�͵��������б�ը��Ч��
            CustomizationTask t2 = new CustomizationTask();
            float w0 = 0;
            float dw = -Mathf.Sign(moveRotate.x)*3.5f/180*Mathf.PI;
            t2.AddTaskFunc(delegate {
                if (!m.IsAlive())
                    return true;
                w0 += dw;
                b.transform.right = new Vector2(Mathf.Cos(w0), Mathf.Sin(w0));
                b.transform.position = m.transform.position;
                return false;
            });
            t2.AddOnExitAction(delegate
            {
                // ��������
                b.transform.right = Vector2.right;
                // �Ա�
                b.KillThis();
                // ������ըЧ��
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 3, 3, "BothCollide");
                r.isAffectFood = true;
                r.isAffectMouse = true;
                r.SetOnFoodEnterAction(unitAction);
                r.SetOnEnemyEnterAction(unitAction);
                r.SetInstantaneous();
                GameController.Instance.AddAreaEffectExecution(r);
            });
            b.AddTask(t2);

            GameController.Instance.AddBullet(b);
        }
        
        // �þ��η�ΧЧ���ܹ������ѷ��ӵ���������ѹ�ĵ�λʩ�Ӷ�Ӧ��Ԫ��Ч�������һ���������ƶ�
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "CollideTriple");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectBullet = true;
            r.SetOnFoodEnterAction(unitAction);
            r.SetOnEnemyEnterAction(unitAction);

            RetangleAreaEffectExecution r2 = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "Enemy");
            r2.isAffectBullet = true;
            r2.SetOnBulletEnterAction((bullet) => {
                // �ݻٽӴ����ӵ�
                bullet.TakeDamage(null);
                bullet.KillThis();
            });

            GameController.Instance.AddTasker(
            //Action InitAction,
                null,
            //Action UpdateAction, 
                delegate
                {
                // ÿ֡����ͬ�����������
                    r.transform.position = b.transform.position;
                    r2.transform.position = b.transform.position;
                },
            //Func<bool> EndCondition, 
                delegate
                {
                // ����󶨵��ӵ����ˣ���ô�Լ�Ҳ�ᱻ�ݻ�
                    return !b.IsAlive();
                },
            //Action EndEvent
                delegate
                {
                    r.MDestory();
                    r2.MDestory();
                }
            );
            GameController.Instance.AddAreaEffectExecution(r);
            GameController.Instance.AddAreaEffectExecution(r2);
        }
    }

    /// <summary>
    /// �����ⵯ
    /// </summary>
    private void CreateLightBullet(float minLeftX)
    {
        // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
        Vector3 targetPos;
        if (targetUnit == null)
            targetPos = new Vector2(minLeftX, MapManager.GetRowY(GetRowIndex()));
        else
            targetPos = new Vector2(Mathf.Max(minLeftX, targetUnit.transform.position.x), MapManager.GetRowY(GetRowIndex()));

        // �����ǲ�������
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60); // ���������ѣʱ��
        RuntimeAnimatorController run = Run_LightBullet;
        float dmg = dmg1_0 * (mCurrentAttack / 10); // ���մ������ϵ��˺�
        EnemyBullet b = EnemyBullet.GetInstance(run, this, 0);
        // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
        b.GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetThrowHighestAttackPriorityUnitInclude(this);
            }
            return unit;
        };
        b.AddHitAction((b, u)=> {
            if(u is CharacterUnit)
            {
                // ������������Ϊ����ʩ����ѣЧ��
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun1_0, false));
            }
            else
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
            }
        });
        TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetPos, true);
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// ����Ԫ�ص�
    /// </summary>
    private void CreateElementalBullet(float minLeftX)
    {
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // ��ը��Կ�Ƭ�����ѣʱ��
        bool isFireBullet = isFireMode;

        // �Ե�λʩ��Ԫ��Ч�� �Լ� ����Ч��
        Action<BaseUnit> unitAction = (unit) =>
        {
            if (isFireBullet)
            {
                AddFireMagic(unit);
                unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, t0_2, false));
            }
            else
            {
                AddIceMagic(unit);
                unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, t0_2, false));
            }
        };

        // ��ȥ��ȡ��ǰ�����Ŀɹ�����ʳ��λ
        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
        Vector3 targetPos;
        if (targetUnit == null)
            targetPos = new Vector2(minLeftX, MapManager.GetRowY(GetRowIndex()));
        else
            targetPos = new Vector2(Mathf.Max(minLeftX, targetUnit.transform.position.x), MapManager.GetRowY(GetRowIndex()));

        // �����ǲ�������
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        RuntimeAnimatorController run;
        if (isFireMode)
            run = Run_FireBullet;
        else
            run = Run_IceBullet;
        EnemyBullet b = EnemyBullet.GetInstance(run, this, 0);
        TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetPos, true);
        GameController.Instance.AddBullet(b);
        // ��ӻ���ʱЧ��
        b.AddHitAction((bullet, unit) => {
            // ����3*3����Ԫ��Ч��
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(b.GetColumnIndex()), MapManager.GetRowY(b.GetRowIndex())), 3, 3, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodEnterAction(unitAction);
            r.SetOnEnemyEnterAction(unitAction);
            r.SetInstantaneous();
            GameController.Instance.AddAreaEffectExecution(r);
        });
    }
}
