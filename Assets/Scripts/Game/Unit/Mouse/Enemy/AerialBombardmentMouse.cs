using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��еͶ����
/// </summary>
public class AerialBombardmentMouse : MouseUnit, IFlyUnit
{
    private static RuntimeAnimatorController Bomb_Run;
    private static Vector3 offset = new Vector3(-0.2f, 0.25f);

    private GeneralAttackSkillAbility generalAttackSkillAbility;

    private bool isThrow;
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private BoolModifier boolMod = new BoolModifier(true);
    private FloatModifier moveSpeedMod = new FloatModifier(100);
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

    private void Awake()
    {
        base.Awake();
        if (Bomb_Run == null)
            Bomb_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/9/Bomb");
    }

    public override void MInit()
    {
        dropColumn = 0; // ������Ĭ��Ϊ0������һ��
        base.MInit();
        mHeight = 1;
        isThrow = false;
        isDrop = false;
        Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            OnShootDown();
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // Ͷը������
        if (infoList.Count > 1)
        {
            RetangleAreaEffectExecution r = null;
            CustomizationSkillAbility s = new CustomizationSkillAbility(this, infoList[1]);
            // ����
            {
                s.IsMeetSkillConditionFunc = delegate {
                    return !isDrop && !isThrow;
                };
                s.BeforeSpellFunc = delegate 
                {
                    // �����
                    r = RetangleAreaEffectExecution.GetInstance(transform.position + new Vector3(offset.x, 0), new Vector2(0.1f * MapManager.gridWidth, 0.1f * MapManager.gridHeight), "ItemCollideAlly");
                    r.SetAffectHeight(0);
                    r.AddFoodEnterConditionFunc((u) => {
                        return FoodManager.IsAttackableFoodType(u) && UnitManager.CanBeSelectedAsTarget(this, u);
                    });
                    r.isAffectFood = true;
                    GameController.Instance.AddAreaEffectExecution(r);

                    CustomizationTask task = new CustomizationTask();
                    task.AddTaskFunc(delegate {
                        r.transform.position = transform.position + new Vector3(offset.x, 0);
                        return isDrop;
                    });
                    task.AddOnExitAction(delegate {
                        r.MDestory();
                    });
                    r.taskController.AddTask(task);
                };
                s.IsMeetCloseSpellingConditionFunc = delegate {
                    return r.foodUnitList.Count > 0 || isDrop;
                };
                s.AfterSpellFunc = delegate {
                    if (!isDrop)
                    {
                        isThrow = true;
                        animatorController.Play("Idle1");
                        CreateBomb();
                        r.MDestory();
                        // Ͷ��ͼ���
                        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedMod);
                    }
                };
            }
            skillAbilityManager.AddSkillAbility(s);
            // �뼼�����ʹҹ�
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1)*100);
                s.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    s.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    s.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }
        }
    }

    /// <summary>
    /// ���һ��ը��
    /// </summary>
    private void CreateBomb()
    {
        Vector3 start = transform.position + offset;
        Vector3 end = transform.position + new Vector3(offset.x, 0);

        BaseEffect e = BaseEffect.CreateInstance(Bomb_Run, null, "Fly", "Hit", true);
        e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
        e.transform.position = start;
        GameController.Instance.AddEffect(e);

        CustomizationTask task = new CustomizationTask();
        task.AddTimeTaskFunc(45, null, (leftTime, totalTime) => 
        {
            float rate = 1 - (float)leftTime / totalTime;
            e.transform.position = Vector3.Lerp(start, end, rate*rate);
        }, null);
        task.AddOnExitAction(delegate {
            // ����ʳ
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
                r.SetAffectHeight(0);
                r.isAffectFood = true;
                r.isAffectMouse = true;
                r.SetTotoalTimeAndTimeLeft(2);
                r.AddExcludeMouseUnit(this);
                r.AddBeforeDestoryAction(delegate
                {
                    // ������ʳ��λ��̯�˺�
                    int count = r.foodUnitList.Count;
                    foreach (var u in r.foodUnitList.ToArray())
                        new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(50, u.mMaxHp / 2) / count).ApplyAction();
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            // ������
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(1f * MapManager.gridWidth, 1f * MapManager.gridHeight), "ItemCollideEnemy");
                r.SetAffectHeight(0);
                r.isAffectMouse = true;
                r.SetTotoalTimeAndTimeLeft(2);
                r.AddExcludeMouseUnit(this);
                r.AddBeforeDestoryAction(delegate
                {
                    // ��������λ�ܵ�һ�λҽ�Ч��
                    foreach (var u in r.mouseUnitList.ToArray())
                        BurnManager.BurnDamage(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            e.ExecuteDeath();
        });
        e.taskController.AddTask(task);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void OnShootDown()
    {
        if (!isDrop)
        {
            isDrop = true;
            // ����ǰ����ֵ���ڷ���״̬�ٽ�㣬����Ҫǿ��ͬ������ֵ���ٽ��
            // if(mCurrentHp> mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0]*mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ǿ�Ƹ���һ����ͼ
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedMod); // �Ƴ����٣����ܣ�
            // ��Ϊת��״̬����״̬�µľ���ʵ�����¼�������
            SetActionState(new TransitionState(this));
        }
    }

    /// <summary>
    /// ����ͼ����ʱҪ������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // ����һ���л����ǵ�0�׶ε���ͼʱ�����˳�����״̬��������0�׶ε�Ѫ���ٷֱ���Ϊ����1.0������֮����Զ�ﲻ������Ȼ�󲥷�׹�䶯��
        // ��ǰȡֵ��ΧΪ1~3ʱ����
        // 0 ����
        // 1 �������->�����ƶ�
        // 2 �����ƶ�
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
        {
            OnShootDown();
        }
    }

    /// <summary>
    /// ����ת��״̬ʱҪ�����£�������ָ����ձ�����ʱҪ����
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        // �������ڼ��Ƴ����ж�����Ч�������߶�����Ч��
        StatusManager.RemoveAllSettleDownDebuff(this);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        animatorController.Play("Drop");
        AddCanBeSelectedAsTargetFunc(noBeSelectFunc);
        AddCanHitFunc(noHitFunc);
    }

    public override void OnTransitionState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        if (!animatorController.GetCurrentAnimatorStateRecorder().aniName.Equals("Drop") || animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
            AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(this, 60, false));
        }
    }

    public override void OnTransitionStateExit()
    {
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        mHeight = 0; // �߶Ƚ���Ϊ����߶�
        RemoveCanBeSelectedAsTargetFunc(noBeSelectFunc);
        RemoveCanHitFunc(noHitFunc);
    }

    public override void OnMoveStateEnter()
    {
        if(!isDrop && isThrow)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move", true);
        //if (isDrop)
        //{
        //    animatorController.Play("Move", true);
        //}
        //else
        //{
        //    if (isThrow)
        //    {
        //        animatorController.Play("Move1", true);
        //    }else
        //        animatorController.Play("Move", true);
        //}
    }
}
