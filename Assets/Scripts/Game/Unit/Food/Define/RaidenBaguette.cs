using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �׵糤�����
/// </summary>
public class RaidenBaguette : FoodUnit
{
    private const string TaskName = "RaidenBaguetteDebuff";

    // �������͵�λ�ı�������
    private static List<RaidenBaguette> thisUnitList = new List<RaidenBaguette>();
    private static List<float> AttackSpeedList = new List<float>() { 
        0.117f, 0.125f, 0.133f, 0.143f, 0.154f, 0.167f, 0.182f, 0.2f, 
        0.22f, 0.25f, 0.286f, 0.308f, 0.33f, 0.364f, 0.4f, 0.44f, 0.5f
    };
    private static Sprite Lightning_Sprite;

    private GeneralAttackSkillAbility generalAttackSkillAbility;

    public override void Awake()
    {
        if(Lightning_Sprite == null)
        {
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        }

        base.Awake();
    }

    public override void MInit()
    {
        if (!thisUnitList.Contains(this))
            thisUnitList.Add(this);
        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
        NumericBox.AttackSpeed.SetBase(AttackSpeedList[mLevel]);
        if (generalAttackSkillAbility != null)
            generalAttackSkillAbility.UpdateNeedEnergyByAttackSpeed();
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ����
    /// </summary>
    public override void LoadSkillAbility()
    {
        foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        {
            if (item.skillType == SkillAbility.Type.GeneralAttack)
            {
                generalAttackSkillAbility = new GeneralAttackSkillAbility(this, item);
                skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
            }
        }
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // ����Ҫ
        return true;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �������ٴ��������׵�
        return GetThisUnitList().Count >= 2;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // ȡ�������������л�Ϊ������ͼ
        foreach (var u in GetThisUnitList())
        {
            u.SetActionState(new AttackState(u));
            u.generalAttackSkillAbility.ClearCurrentEnergy();
        }
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �л�Ϊ��ֹ��ͼ
        foreach (var u in GetThisUnitList())
        {
            u.SetActionState(new IdleState(u));
        }
        mAttackFlag = true;
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        ExecuteAttack();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        GetThisUnitList().Remove(this);
    }

    public override void ExecuteRecycle()
    {
        base.ExecuteRecycle();
        GetThisUnitList().Remove(this);
    }

    //////////////////////////////////////////////////// ����Ϊ��̬����
    /// <summary>
    /// ��ȡ�ڳ���ȫ�����׵糤���������
    /// </summary>
    /// <returns></returns>
    public static List<RaidenBaguette> GetThisUnitList()
    {
        // �ȼ��һ��
        List<RaidenBaguette> delList = new List<RaidenBaguette>();
        foreach (var u in thisUnitList)
        {
            if (!u.IsValid())
            {
                delList.Add(u);
            }
        }
        foreach (var u in delList)
        {
            thisUnitList.Remove(u);
        }
        return thisUnitList;
    }

    /// <summary>
    /// ִ��һ�ε��
    /// </summary>
    public static void ExecuteAttack()
    {
        List<BaseUnit> hitedUnitList = new List<BaseUnit>(); // �ѱ�����ĵ�λ��Ŀ���Ƿ�ֹ���ε��

        List<RaidenBaguette> list = GetThisUnitList();
        // ʹ�öԽǾ������������ϣ���֤��������
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                RaidenBaguette u1 = list[i];
                RaidenBaguette u2 = list[j];
                // ��������뷽������
                float dist = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).magnitude;
                Vector2 rot = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).normalized;
                // �����˺��ж���
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(Vector2.Lerp(u1.transform.position, u2.transform.position, 0.5f), dist / MapManager.gridWidth, 0.5f, "ItemCollideEnemy");
                    r.transform.right = rot;
                    r.isAffectMouse = true;
                    r.SetInstantaneous();
                    r.SetOnEnemyEnterAction((u)=> {
                        if (hitedUnitList.Contains(u))
                            return;
                        // ���һ���˺�Ϊ900*(���ߵ�ǰ������֮��/20)�ı��ƻҽ�Ч��
                        CombatActionManager.BombBurnDamageUnit(null, u, 900*(u1.mCurrentAttack+u2.mCurrentAttack)/20);
                        // ����Ƕ�ת�������Ը���Ψһ��1��80%����Ч����3���20%����Ч��
                        if (u1.mShape >= 2)
                        {
                            LightningTask t;
                            if (u.GetTask(TaskName) == null)
                            {
                                t = new LightningTask(u);
                                u.AddUniqueTask(TaskName, t);
                            }
                            else
                            {
                                t = u.GetTask(TaskName) as LightningTask;
                                t.Refresh(); // ˢ��
                            }
                        }
                        // ��¼�ѱ������һ��
                        hitedUnitList.Add(u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                // ����������Ч
                {
                    BaseLaser l = BaseLaser.GetAllyInstance(null, 0, u1.transform.position, rot, null, Lightning_Sprite, null, null, null, null, null, null);
                    l.mMaxLength = dist;
                    l.mCurrentLength = dist;
                    l.mVelocity = TransManager.TranToVelocity(36);
                    l.isCollide = false;
                    l.laserRenderer.SetSortingLayerName("Effect");

                    int timeLeft = 20;
                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate
                    {
                        if (timeLeft > 0)
                            timeLeft--;
                        else
                        {
                            // �۵���ʧ
                            l.laserRenderer.SetVerticalOpenTime(-20);
                            timeLeft = 20;
                            return true;
                        }
                        return false;
                    });
                    t.AddTaskFunc(delegate
                    {
                        if (timeLeft > 0)
                            timeLeft--;
                        else
                            return true;
                        return false;
                    });
                    t.OnExitFunc = delegate
                    {
                        l.ExecuteRecycle();
                    };
                    l.AddTask(t);

                    GameController.Instance.AddLaser(l);
                }
            }
            // ����������Ϊ����1*1�ĵ糡
            {
                RaidenBaguette currentUnit = list[i];
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(currentUnit.transform.position, 1, 1, "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetInstantaneous();
                r.SetOnEnemyEnterAction((u) => {
                    if (hitedUnitList.Contains(u))
                        return;
                    // ���һ���˺�Ϊ900*(��ǰ������/20)�ı��ƻҽ�Ч��
                    CombatActionManager.BombBurnDamageUnit(null, u, 900 * currentUnit.mCurrentAttack / 10);
                    // ����Ƕ�ת�������Ը���Ψһ��1��80%����Ч����3���20%����Ч��
                    if (currentUnit.mShape >= 2)
                    {
                        LightningTask t;
                        if (u.GetTask(TaskName) == null)
                        {
                            t = new LightningTask(u);
                            u.AddUniqueTask(TaskName, t);
                        }
                        else
                        {
                            t = u.GetTask(TaskName) as LightningTask;
                            t.Refresh(); // ˢ��
                        }
                    }
                    // ��¼�ѱ������һ��
                    hitedUnitList.Add(u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }


    /// <summary>
    /// ��ת�ĵ������
    /// </summary>
    private class LightningTask : ITask
    {
        private FloatModifier AddDamageModifier = new FloatModifier(1.2f); // ����
        private FloatModifier moveSpeedModifier = new FloatModifier(-80); // ����

        private const int SlowDownTime = 60;
        private const int AddDamageTime = 180;

        private int slowDownTimeLeft;
        private int addDamageTimeLeft;

        private BaseUnit unit;

        public LightningTask(BaseUnit unit)
        {
            this.unit = unit;
            
        }

        public void OnEnter()
        {
            Refresh();
        }

        public void OnUpdate()
        {
            if (slowDownTimeLeft > 0)
                slowDownTimeLeft--;
            else
                unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier);

            if (addDamageTimeLeft > 0)
                addDamageTimeLeft--;
            else
                unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);
        }

        public bool IsMeetingExitCondition()
        {
            return slowDownTimeLeft <= 0 && addDamageTimeLeft <= 0;
        }

        public void OnExit()
        {
            // �Ƴ�����
            unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier);
            // �Ƴ�����
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);
        }

        /// <summary>
        /// ˢ��ʱ��
        /// </summary>
        public void Refresh()
        {
            // �Ƴ�����
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageModifier);

            slowDownTimeLeft = SlowDownTime;
            addDamageTimeLeft = AddDamageTime;

            // ����
            unit.AddStatusAbility(new SlowStatusAbility(-80, unit, SlowDownTime));
            unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedModifier);
            // ����
            unit.NumericBox.DamageRate.AddModifier(AddDamageModifier);
        }
    }
} 
