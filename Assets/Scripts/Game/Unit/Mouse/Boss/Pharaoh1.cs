using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// BOSS����
/// </summary>
public class Pharaoh1 : BossUnit
{
    private const string CurseKey = "����"; // ��������
    private const string IgnoreCurseKey = "��������"; // ������������
    private const string BandageKey = "������λ"; // ������λ����
    private FloatModifier CurseAddDamageModifier; // �����˺�����Ч��
    private static FloatModifier DecDmgModifier = new FloatModifier(0); // ת��ʱ100%����Ч��
    private static FloatModifier DecDmgWhenClosedModifier = new FloatModifier(0); // �ײĺ���ʱ50%����Ч��
    private Func<BaseUnit, BaseBullet, bool> noHitFunc = (u, b) => { return false; }; // ���ɱ�����

    private static RuntimeAnimatorController[] selfAnimatorController = new RuntimeAnimatorController[3]; // ������������
    private static RuntimeAnimatorController coffinAnimatorController; // �ײĵĶ���������
    private static RuntimeAnimatorController[] mummyAnimatorController = new RuntimeAnimatorController[2]; // ľ�����Ķ���������
    private static RuntimeAnimatorController bandageAnimatorController; // �����Ķ���������
    private static RuntimeAnimatorController bugAnimatorController; // ʥ�׳�Ķ���������
    private static RuntimeAnimatorController curseEffectAnimatorController; // ������Ч����������

    private CustomizationSkillAbility Skill_PharaohCurse; // ����֮��

    private List<BaseUnit> cursedUnit = new List<BaseUnit>(); // ������ĵ�λ
    private List<BaseUnit> bandageList = new List<BaseUnit>(); // ������λ

    private bool isDisappear; // �Ƿ�����ʧ��������ԭ�Ϳ��ã����ڼ���ƶ�ʱ�Ƿ�Ҫ����ض����ģ�
    private bool isReal; // �Ƿ�Ϊ������̬

    public override void Awake()
    {
        if(coffinAnimatorController == null)
        {
            for (int i = 0; i < 3; i++)
            {
                selfAnimatorController[i] = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/0/"+i);
            }
            coffinAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Coffin");
            for (int i = 0; i < 2; i++)
            {
                mummyAnimatorController[i] = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Mummy/" + i);
            }
            bandageAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Bandage");
            bugAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Bug");
            curseEffectAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Curse");
        }
        base.Awake();
    }

    public override void MInit()
    {
        cursedUnit.Clear();
        bandageList.Clear();
        isDisappear = true;
        isReal = false;
        base.MInit();
        animator.runtimeAnimatorController = selfAnimatorController[0];
        CurseAddDamageModifier = new FloatModifier(GetParamValue("AddDamageRate", 0));
        SetAlpha(0);
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var unit in cursedUnit)
        {
            if (!unit.IsAlive())
                delList.Add(unit);
        }
        foreach (var unit in delList)
        {
            cursedUnit.Remove(unit);
        }

        delList = new List<BaseUnit>();
        foreach (var unit in bandageList)
        {
            if (!unit.IsAlive())
                delList.Add(unit);
        }
        foreach (var unit in delList)
        {
            bandageList.Remove(unit);
        }

        base.MUpdate();
    }

    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    public override void InitBossParam()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.8f, 0.65f, 0.5f, 0.2f });
        // ͨ�û���
        AddParamArray("AddDamageRate", new float[] { 1 }); // �������˱���
        AddParamArray("KilledHp", new float[] { 50 }); // ���������նɱѪ��
        AddParamArray("DropDamage", new float[] { 900 }); // �ײĵ����˺�
        AddParamArray("StunTime", new float[] { 4 }); // �ײĵ�����ѣʱ��

        AddParamArray("LidHp", new float[] { 900 }); // �и�����ֵ
        AddParamArray("BoxHp", new float[] { 300 }); // ��������ֵ
        AddParamArray("LidOpenTime", new float[] { 3 }); // �иǽҿ��ȴ�ʱ��

        AddParamArray("MummyHp", new float[] { 100 }); // ľ������
        AddParamArray("MummyAliveTime", new float[] { 18 }); // ľ�������н���ʱ��

        AddParamArray("BugHp", new float[] { 100 }); // ʥ�׳������ֵ

        // ����֮��
        AddParamArray("r0_0", new float[] { 5, 6, 7 }); // ����֮����������У�������������
        AddParamArray("t0_0", new float[] { 3, 1.5f, 0, float.NaN, float.NaN });
        // ���ؼ���
        AddParamArray("count1_0", new float[] { 0, 0, 0, 1, 1 }); // �ظ�����
        AddParamArray("t1_0", new float[] { 2, 1, 0, 2, 1 }); // ���ؼ���ͣ��ʱ��
        AddParamArray("t1_1", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // ������̬�����ؼ����ƶ�ʱ��
        // ������ʽ
        AddParamArray("t2_0", new float[] { 6, 4, 2, 2, 0 }); // ������ʽͣ��ʱ��
        AddParamArray("t2_1", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // ������̬�»�����ʽ�ƶ�ʱ��
        // ������֮��
        AddParamArray("t3_0", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // ������֮��ص��г����ƶ�ʱ��
        AddParamArray("t3_1", new float[] { float.NaN, float.NaN, float.NaN, 2, 0 }); // ������֮��ͣ��ʱ��
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        // �����л�Ϊ����Ĺ���Ч��
        if(mHertIndex == 3)
        {
            // ���100%����
            NumericBox.DamageRate.AddModifier(DecDmgModifier);
            ChangeToRealMode(); // ����л�Ϊ���������
            mSkillQueueAbilityManager.SetNextSkillIndex(-1); // �����±�Ҫ��-1
        }

        if(mHertIndex < 3)
        {
            isReal = false;
            // ԭ�ͼ�����
            list.Add(PharaohCurseInit(infoList[0])); // ����֮��
            list.Add(MysticalSacrificesInit(infoList[1])); // ���ؼ���
            list.Add(AwakeningCeremonyInit(infoList[2])); // ������ʽ
            list.Add(MysticalSacrificesInit(infoList[1])); // ���ؼ���
        }
        else
        {
            isReal = true;
            // ��������
            list.Add(MysticalSacrificesInit(infoList[1])); // ���ؼ���
            list.Add(PharaohKingCurseInit(infoList[3])); // ������֮��
            list.Add(MysticalSacrificesInit(infoList[1])); // ���ؼ���
            list.Add(AwakeningCeremonyInit(infoList[2])); // ������ʽ
        }
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// �л�������
    /// </summary>
    private void ChangeToRealMode()
    {
        // ��ӳ��ֵļ���
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animator.runtimeAnimatorController = selfAnimatorController[1];
            animatorController.Play("Appear");
        };
        c.AddSpellingFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        c.AfterSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
            // �Ƴ�����
            NumericBox.DamageRate.RemoveModifier(DecDmgModifier);
        };
        // ǿ��������һ������Ϊ�����ת����
        mSkillQueueAbilityManager.SetNextSkill(c);
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void OnDieStateEnter()
    {
        if (!IsRealStage())
            animatorController.Play("Disappear");
        else
            animatorController.Play("Die");
    }

    /// <summary>
    /// �Ƿ�Ϊ��������׶�
    /// </summary>
    /// <returns></returns>
    private bool IsRealStage()
    {
        return isReal;
    }

    /// <summary>
    /// �ٻ�ľ������
    /// </summary>
    private void SummonMummy(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(mummyAnimatorController, new float[] { 0.5f });
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // ��������
        m.SetBaseAttribute(GetParamValue("MummyHp", 0), 10, 0.5f, 0.75f, 0, 0.9f, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));

        CustomizationTask t = new CustomizationTask();
        int aliveTimeLeft = Mathf.FloorToInt(GetParamValue("MummyAliveTime", 0) * 60);
        t.AddTaskFunc(delegate{
            if (aliveTimeLeft > 0)
                aliveTimeLeft--;
            else
            {
                // ʱ��һ�����Ի�
                m.ExecuteDeath();
                return true;
            }
            if (m.IsBlock())
                AddCurse(m.GetCurrentTarget());
            return false;
        });
        m.AddTask(t);
        GameController.Instance.AddMouseUnit(m);
    }

    /// <summary>
    /// �ٻ�һ���ײ�
    /// </summary>
    private void SummonCoffin(Vector2 pos)
    {
        float LidHp = GetParamValue("LidHp", 0);
        float BoxHp = GetParamValue("BoxHp", 0);

        MouseModel m = MouseModel.GetInstance(coffinAnimatorController);
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // ��������
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.BeFrightened, new BoolModifier(true)); // ����èǹ����
        m.SetBaseAttribute(LidHp + BoxHp, 0, 1, 0, 0, 0, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.CanBlockFuncList.Add(delegate { return false; }); // �ײĲ����赲����Ҳ���ṥ��
        GameController.Instance.AddMouseUnit(m);

        CustomizationTask t = new CustomizationTask();
        bool dropFlag = true;
        bool isSummon = false; // �Ƿ��Ѿ�����ľ������
        int timeLeft = Mathf.FloorToInt(GetParamValue("LidOpenTime", 0)*60);
        Func<BaseUnit, BaseBullet, bool> noHitFunc = (unit, bullet) => { return false; };

        int stunTime = Mathf.FloorToInt(GetParamValue("StunTime", 0) * 60);
        // ��ѣ��λ
        Action<BaseUnit> stunUnitAction = (unit) =>
        {
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, stunTime, false));
        };

        t.OnEnterFunc = delegate
        {
            m.SetActionState(new IdleState(m));
            // �����ϵ�����
            m.animatorController.Play("Drop");
            m.CanHitFuncList.Add(noHitFunc); // ������ɻ���
        };
        t.AddTaskFunc(delegate {
            if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                m.animatorController.Play("Idle", true);
                return true;
            }
            else if(m.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.33f && dropFlag)
            {
                m.CanHitFuncList.Remove(noHitFunc); // �ɻ���
                // ������Ϊ����0.5��ĵ�λ��ɷ�Χ�˺� ������
                DamageAreaEffectExecution e = DamageAreaEffectExecution.GetInstance();
                e.Init(m, CombatAction.ActionType.CauseDamage, GetParamValue("DropDamage", 0), GetRowIndex(), 0.5f, 0.5f, 0, 0, true, true);
                e.transform.position = m.transform.position;
                e.SetOnFoodEnterAction(stunUnitAction);
                e.SetOnEnemyEnterAction(stunUnitAction);
                e.AddExcludeMouseUnit(m); // ����Ҫ���ų����⣬��Ȼ�˵а˰�����һǧ
                GameController.Instance.AddAreaEffectExecution(e);
                dropFlag = false;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (timeLeft > 0 || m.GetCurrentHp()<=BoxHp)
                timeLeft--;
            else
            {
                if (m.GetCurrentHp() > BoxHp)
                    m.SetMaxHpAndCurrentHp(BoxHp);
                else
                    m.NumericBox.Hp.SetBase(BoxHp);
                m.animatorController.Play("Open");
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                // �˴���������ľ������
                isSummon = true;
                SummonMummy(m.transform.position);
                m.animatorController.Play("Idle1", true);
                return true;
            }
            return false;
        });
        m.AddTask(t);
        // �����û������ľ�����������¹ײľͱ��ˣ���ô�ڱ�ըʱ����ľ������
        m.AddBeforeDeathEvent(delegate {
            if (!isSummon)
            {
                SummonMummy(m.transform.position);
            }
        });
    }

    /// <summary>
    /// ��������ĵ�λ�ܵ��˺�����ж�
    /// </summary>
    /// <param name="unit"></param>
    private void AfterDamageWhenCursed(CombatAction action)
    {
        BaseUnit unit = action.Target;
        if(unit!=null && unit.IsAlive() && unit.GetCurrentHp() < GetParamValue("KilledHp", 0))
        {
            unit.ExecuteDeath();
        }
    }

    private void BeforeDeathWhenCursed(BaseUnit unit)
    {
        if(unit.transform.position.x > MapManager.GetColumnX(3))
            SummonCoffin(unit.transform.position); // ԭ������һ���ײ�
    }

    /// <summary>
    /// Ϊ��λʩ������Ч��
    /// </summary>
    /// <param name="unit"></param>
    private void AddCurse(BaseUnit unit)
    {
        // ��������
        if (!IsAlive() || unit is CharacterUnit || unit.NumericBox.GetBoolNumericValue(IgnoreCurseKey) || cursedUnit.Contains(unit))
            return;
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return;
        }

        // ����Ϊʩ�ӷ���
        unit.NumericBox.DamageRate.AddModifier(CurseAddDamageModifier); // �ܵ��˺�����
        //unit.AddActionPointListener(ActionPointType.PostReceiveDamage, AfterDamageWhenCursed);
        //unit.AddActionPointListener(ActionPointType.PostReceiveReboundDamage, AfterDamageWhenCursed);
        unit.AddBeforeDeathEvent(BeforeDeathWhenCursed);
        cursedUnit.Add(unit);
        // ΪĿ��ʩ��������Ч
        BaseEffect e = BaseEffect.CreateInstance(curseEffectAnimatorController, "Appear", "Idle", "Disappear", true);
        unit.AddEffectToDict(CurseKey, e, 0f * MapManager.gridHeight * Vector2.up);
        GameController.Instance.AddEffect(e);
    }

    /// <summary>
    /// �Ƴ���λ������Ч��
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveCurse(BaseUnit unit)
    {
        unit.NumericBox.DamageRate.RemoveModifier(CurseAddDamageModifier);
        //unit.RemoveActionPointListener(ActionPointType.PostReceiveDamage, AfterDamageWhenCursed);
        //unit.RemoveActionPointListener(ActionPointType.PostReceiveReboundDamage, AfterDamageWhenCursed);
        unit.RemoveBeforeDeathEvent(BeforeDeathWhenCursed);
        cursedUnit.Remove(unit);
        // ΪĿ���Ƴ�������Ч
        unit.RemoveEffectFromDict(CurseKey);
    }

    /// <summary>
    /// �ٻ�ʥ�׳�
    /// </summary>
    private void SummonBug(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(bugAnimatorController);
        m.typeAndShapeValue = -1;
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // ��������
        m.SetBaseAttribute(GetParamValue("BugHp", 0), 900, 1.0f, 0f, 0, 0.5f, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);


        CustomizationTask t = new CustomizationTask();
        int accTimeLeft = 240; // ����ʱ��
        float acc = TransManager.TranToVelocity(6)/accTimeLeft;
        t.OnEnterFunc = delegate
        {
            m.SetActionState(new MoveState(m));
            // ������ѣ2��
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
        };
        t.AddTaskFunc(delegate {
            // ���������ڱ���ѣ�����ʱ��ͣ
            if (m.isFrozenState)
                return false;

            if(accTimeLeft > 0)
            {
                // ����
                m.NumericBox.MoveSpeed.SetBase(m.mBaseMoveSpeed + acc);
                accTimeLeft--;
            }
            // ������赲�ˣ�����赲�����900���˺�Ȼ���Լ�����(��������Ч��
            if (m.IsBlock())
            {
                BaseUnit target = m.GetCurrentTarget();
                if(target !=null && !(target is CharacterUnit)){
                    m.TakeDamage(target);
                    m.ExecuteDeath();
                }
            }
            return false;
        });
        m.AddTask(t);

        int decDmgTimeLeft = 240;
        FloatModifier decDmgModifier = new FloatModifier(0.01f);
        GameController.Instance.AddTasker(
            //Action InitAction, 
            delegate {
                m.NumericBox.DamageRate.AddModifier(decDmgModifier); // ��ü���
            },
            //Action UpdateAction, 
            delegate {
                decDmgTimeLeft--;
            },
            //Func<bool> EndCondition, 
            delegate { return decDmgTimeLeft <= 0 || !m.IsAlive(); },
            //Action EndEvent
            delegate {
                m.NumericBox.DamageRate.RemoveModifier(decDmgModifier); // �Ƴ�����
            }
            );

        GameController.Instance.AddMouseUnit(m);
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void CreateBandageArea(BaseGrid masterGrid)
    {
        Vector2 pos = masterGrid.transform.position;
        // ���ɱ�����
        MouseModel m = MouseModel.GetInstance(bandageAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // ��������
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // ���߶���
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // ������ѣ
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // ���߱�������
            m.SetBaseAttribute(900, 1, 1f, 0, 0, 0, 0);
            m.transform.position = pos;
            m.currentYIndex = MapManager.GetYIndex(pos.y);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // ������Ϊѡȡ��Ŀ��
            m.AddCanBlockFunc(delegate { return false; }); // ���ɱ��赲
            m.AddCanHitFunc(delegate { return false; }); // ���ɱ��ӵ�����
            m.DieClipName = "Disappear";
            // ��������������� �� ��ը��ʱ ����ֳ���
            Action<BaseUnit> action = delegate {
                // λ��������������ƫ�������ɳ���
                if(m.transform.position.x > MapManager.GetColumnX(3))
                    SummonBug(m.transform.position);
            };

            m.AddBeforeDeathEvent(action);
            m.AddBeforeBurnEvent(action);

            // ���ֶ���
            {
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate
                {
                    m.animatorController.Play("Appear");
                };
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        m.animatorController.Play("Idle", true);
                        return true;
                    }
                    return false;
                });
                m.AddTask(t);
            }

            // ��������
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate
                {
                    if (masterGrid.isActiveAndEnabled)
                    {
                        m.transform.position = masterGrid.transform.position;
                        return false;
                    }
                    else
                    {
                        m.ExecuteDeath();
                        return true;
                    }
                });
                m.AddTask(t);
            }

            GameController.Instance.AddMouseUnit(m);
            bandageList.Add(m);
            
            // ���ԭ�������б�������ô���Ƚ������ı���
            BaseUnit origin_bandage = masterGrid.GetUnitFromDict(BandageKey);
            if (origin_bandage != null)
            {
                origin_bandage.ExecuteDeath();
                bandageList.Remove(origin_bandage);
                masterGrid.RemoveUnitFromDict(BandageKey);
            }
            // ֮���ٰ��������ü��뱾��
            masterGrid.AddUnitToDict(BandageKey, m);
        }
        // �����������򣬲��������λ�ð�
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 1, 1, "BothCollide");
        {
            // ÿ֡������
            Action<BaseUnit> unitAction = (unit) => {
                if (unit is MouseUnit)
                {
                    MouseUnit m = unit as MouseUnit;
                    // Ŀ����BOSS����Ŀ��߶Ȳ�Ϊ0�������ж�
                    if (m.IsBoss() || m.GetHeight()!=0)
                        return;
                }

                AddCurse(unit);
                // ����ʩ����ѣЧ��
                StatusAbility s = unit.GetNoCountUniqueStatus(StringManager.Stun);
                if (s == null)
                {
                    s = new StunStatusAbility(unit, 1, false);
                    unit.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
                }

                if (s.leftTime < 2)
                {
                    s.leftTime = 2;
                }
            };

            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodStayAction(unitAction);
            r.SetOnEnemyStayAction(unitAction);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (m!=null && m.IsAlive())
                {
                    r.transform.position = m.transform.position;
                    return false;
                }
                r.MDestory();
                return true;
            });
            r.AddTask(t);

            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    /// <summary>
    /// ������б���
    /// </summary>
    public void ReleaseAllBandage()
    {
        foreach (var item in bandageList)
        {
            item.ExecuteDeath();
        }
        bandageList.Clear();
    }

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////

    /// <summary>
    /// ����ԭ���ƶ���ĳ��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move0(Vector2 pos)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // ʵ��
        bool isNoHit = false;
        bool isClose = false;
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            if(!isDisappear)
                animatorController.Play("Disappear");
            else
            {
                isNoHit = true;
                AddCanHitFunc(noHitFunc);
                transform.position = pos;
                animatorController.Play("Appear");
                SetAlpha(1);
                isDisappear = false;
            }
        };
        {
            if(!isDisappear)
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        transform.position = pos;
                        animatorController.Play("Appear");
                        return true;
                    }else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.3f && !isClose)
                    {
                        isClose = true;
                        NumericBox.DamageRate.AddModifier(DecDmgWhenClosedModifier);
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.65f && !isNoHit)
                    {
                        isNoHit = true;
                        AddCanHitFunc(noHitFunc);
                    }
                    return false;
                });


            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.3f && isNoHit)
                {
                    isNoHit = false;
                    RemoveCanHitFunc(noHitFunc);
                }
                else if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.65f && isClose)
                {
                    isClose = false;
                    NumericBox.DamageRate.RemoveModifier(DecDmgWhenClosedModifier);
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// ���������ƶ���ĳ��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move1(Vector2 endPos, int moveTime)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        Vector2 startPos = transform.position;
        Vector2 lastPos = transform.position;
        int timeLeft = moveTime;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            startPos = transform.position;
            lastPos = transform.position;
            timeLeft = moveTime;
            animatorController.Play("Idle");
        };
        {
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    float r = (float)timeLeft / moveTime;
                    Vector2 pos = Vector2.Lerp(startPos, endPos, 1-r);
                    SetPosition((Vector2)GetPosition() + (pos - lastPos));
                    lastPos = pos;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// ����֮��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility PharaohCurseInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_PharaohCurse = c;
        int u = -1;
        int startRowRight = 3; // ����֮����ʼ�У�������������
        int r0_0 = Mathf.FloorToInt(GetParamValue("r0_0", mHertIndex)); // ����֮����������У�������������
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60); // ͣ��ʱ��

        int timeLeft = 0;
        bool flag = true;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
            // �����г�
            c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(8, 3)));
        };
        {
            // ��һ���ĵذ�
            c.AddSpellingFunc(delegate {
                if (u < 0)
                    animatorController.Play("AttackLeft");
                else
                    animatorController.Play("AttackRight");
                u = -u;
                return true;
            });
            for (int i = startRowRight; i <= r0_0; i++)
            {
                int j = i;
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        if(j < r0_0)
                        {
                            if (u < 0)
                                animatorController.Play("AttackLeft");
                            else
                                animatorController.Play("AttackRight");
                            u = -u;
                        }
                        else
                        {
                            // ���һ�ν����л���ͣ��״̬
                            animatorController.Play("Idle", true);
                            timeLeft = t0_0;
                        }
                        flag = true;
                        return true;
                    }
                    else if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5f && flag)
                    {
                        flag = false;
                        int xIndex = (9 - j);
                        // ȥ�ҵ�ǰ��������ֵ��ߵĿ�Ƭ���ڸ���
                        List<BaseGrid> l = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(), MapManager.GetColumnX(xIndex - 0.5f), MapManager.GetColumnX(xIndex + 0.5f), MapManager.GetRowY(-0.5f), MapManager.GetRowY(6.5f));
                        List<BaseGrid> l2 = GridManager.GetGridListWhichHasMaxCondition(l, (g) => {
                            List<FoodUnit> fList = g.GetAttackableFoodUnitList();
                            float max = float.MinValue;
                            foreach (var item in fList)
                            {
                                if (item.GetCurrentHp() > max)
                                    max = item.GetCurrentHp();
                            }
                            return max;
                        });
                        List<BaseGrid> l3 = GridManager.GetRandomUnitList(l2, 1);
                        // ������ɸѡ�����ĸ����а��ϱ���
                        foreach (var g in l3)
                        {
                            CreateBandageArea(g);
                        }
                    }
                    return false;
                });
            }
            // �ȴ�ʱ�䵽�˺��˳�
            c.AddSpellingFunc(delegate {
                if(timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// ���ؼ���
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility MysticalSacrificesInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        int count1_0 = Mathf.FloorToInt(GetParamValue("count1_0", mHertIndex)); // �ظ�����
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // ���ؼ���ͣ��ʱ��
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex)*60); // ������̬�����ؼ����ƶ�ʱ��
        int timeLeft = 0;
        bool flag = true;
        bool isRealStage = IsRealStage();
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
        };
        for(int k = 0; k < count1_0 + 1; k++){
            c.AddSpellingFunc(delegate
            {
                flag = true;
                SetNextGridIndexByRandom(3, 6, 1, 5);
                Vector2 v = GetNextGridIndex();
                if(isRealStage)
                    c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(v.x, v.y), t1_1));
                else
                    c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(v.x, v.y)));
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                if(isRealStage)
                    animatorController.Play("Attack");
                else
                    animatorController.Play("AttackLeft");
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    timeLeft = t1_0;
                    return true;
                }else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5f && flag)
                {
                    flag = false;
                    int xIndex = GetColumnIndex();
                    int yIndex = GetRowIndex();
                    for (int i = -1; i <= 1; i++)
                    {
                        BaseGrid g = GameController.Instance.mMapController.GetGrid(xIndex - 1, yIndex + i);
                        if(g!=null)
                            CreateBandageArea(g);
                    }
                        
                }
                return false;
            });
            // �ȴ����˳�
            c.AddSpellingFunc(delegate
            {
                if(timeLeft > 0)
                    timeLeft--; 
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
    /// ������ʽ
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility AwakeningCeremonyInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex)*60); // ������ʽͣ��ʱ��
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex)*60); // ������̬�»�����ʽ�ƶ�ʱ��
        bool flag = true;
        bool isRealStage = IsRealStage();
        int timeLeft = 0;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            flag = true;
            timeLeft = 0;
            // �����г�
            if(isRealStage)
                c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(8, 3), t2_1));
            else
                c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(8, 3)));
        };
        {
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("Cast");
                return true;
            });

            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    timeLeft = t2_0;
                    return true;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.35f && flag)
                {
                    flag = false;
                    ReleaseAllBandage();
                }
                return false;
            });
            // �ȴ��˳�
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                    timeLeft--;
                else
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// ������֮��
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility PharaohKingCurseInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);

        int t3_0 = Mathf.FloorToInt(GetParamValue("t3_0", mHertIndex)*60); // ������֮��ص��г����ƶ�ʱ��
        int t3_1 = Mathf.FloorToInt(GetParamValue("t3_1", mHertIndex)*60); // ������֮��ͣ��ʱ��

        int timeLeft = 0;
        bool flag = true;
        float rate = 2.0f / 7;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
            // �����г�
            c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(8, 3), t3_0));
        };
        {
            // ������
            c.AddSpellingFunc(delegate {
                animatorController.Play("Charge", true);
                return true;
            });
            // ����
            for (int i = 0; i < 7; i++)
            {
                int j = i;
                // ������
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= rate * (j+1))
                    {
                        flag = true;
                        return true;
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > rate*j && flag)
                    {
                        BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(j, float.MinValue);
                        if(unit != null && unit.GetGrid() != null)
                        {
                            CreateBandageArea(unit.GetGrid());
                        }
                        flag = false;
                    }
                    return false;
                });
            }
            // ������һ���ȴ�ʱ��
            c.AddSpellingFunc(delegate {
                timeLeft = t3_1;
                return true;
            });
            // �ȴ�ʱ�䵽�˺��˳�
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
