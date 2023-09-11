using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �׵糤�����
/// </summary>
public class RaidenBaguette : FoodUnit
{
    // �������͵�λ�ı�������
    private static List<RaidenBaguette> thisUnitList = new List<RaidenBaguette>();
    private static List<BaseUnit> hitedUnitList = new List<BaseUnit>();
    private static Sprite Lightning_Sprite;
    private static int currentShape; // ��ǰΪ��ת
    private static int nextAttackTimeLeft; // �´ι���ʣ��ʱ��

    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private FloatModifier decMaxHpPercentMod = new FloatModifier(0); // �����������ֵ���޵ı�ǩ

    public override void Awake()
    {
        if(Lightning_Sprite == null)
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        base.Awake();
    }

    public override void MInit()
    {
        // ����һ���׵类����ʱ������ȫ�ּ�ʱ��
        if(GetThisUnitList().Count == 0)
            GameController.Instance.mTaskController.AddUniqueTask("RaidenBaguette_Attack_Task", GetGlobalAttackTimerTask());

        if (!thisUnitList.Contains(this))
            thisUnitList.Add(this);
        decMaxHpPercentMod.Value = 0;
        base.MInit();
        NumericBox.Hp.AddFinalPctAddModifier(decMaxHpPercentMod);
        currentShape = mShape;
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void MDestory()
    {
        base.MDestory();
        GetThisUnitList().Remove(this);
        if(GetThisUnitList().Count == 0)
            GameController.Instance.mTaskController.RemoveUniqueTask("RaidenBaguette_Attack_Task");
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void OnCastStateEnter()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Lighting");
        animatorController.Play("Attack");
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    /// <summary>
    /// ����һ���������ֵ����
    /// </summary>
    private void DecMaxHp()
    {
        NumericBox.Hp.RemoveFinalPctAddModifier(decMaxHpPercentMod);
        if (mShape < 1)
            decMaxHpPercentMod.Value -= 20;
        else
            decMaxHpPercentMod.Value -= 15;
        NumericBox.Hp.AddFinalPctAddModifier(decMaxHpPercentMod);

        if (decMaxHpPercentMod.Value <= -100)
            ExecuteDeath();
        else if (mCurrentHp > mMaxHp)
            mCurrentHp = mMaxHp;
    }

    #region ������ͨ����
    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {

    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    public override bool IsMeetEndGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {

    }

    public override bool IsDamageJudgment()
    {
        return false;
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ����
    /// </summary>
    public override void LoadSkillAbility()
    {
        //foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        //{
        //    if (item.skillType == SkillAbility.Type.GeneralAttack)
        //    {
        //        generalAttackSkillAbility = new GeneralAttackSkillAbility(this, item);
        //        skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        //    }
        //}
    }

    protected override bool IsHasTarget()
    {
        return false;
    }

    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }
    #endregion

    #region ��̬˽�з���
    /// <summary>
    /// ��ȡ�ڳ���ȫ�����׵糤���������
    /// </summary>
    /// <returns></returns>
    private static List<RaidenBaguette> GetThisUnitList()
    {
        // �ȼ��һ��
        List<RaidenBaguette> delList = new List<RaidenBaguette>();
        foreach (var u in thisUnitList)
            if (!u.IsValid())
                delList.Add(u);
        foreach (var u in delList)
            thisUnitList.Remove(u);
        return thisUnitList;
    }

    /// <summary>
    /// ִ��һ�ε��
    /// </summary>
    private static void ExecuteAttack(List<RaidenBaguette> list)
    {
        hitedUnitList.Clear();
        // ʹ�öԽǾ������������ϣ���֤��������
        for (int i = 0; i < list.Count; i++)
        {
            RaidenBaguette u1 = list[i];
            u1.SetActionState(new CastState(u1));
            for (int j = i + 1; j < list.Count; j++)
            {
                RaidenBaguette u2 = list[j];
                // ��������뷽������
                float dist = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).magnitude;
                Vector2 rot = ((Vector2)u2.transform.position - (Vector2)u1.transform.position).normalized;
                // �����˺��ж���
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(Vector2.Lerp(u1.transform.position, u2.transform.position, 0.5f), dist / MapManager.gridWidth, 1, "ItemCollideEnemy");
                    r.transform.right = rot;
                    r.isAffectMouse = true;
                    r.SetInstantaneous();
                    r.SetOnEnemyEnterAction(EnemyEnterAction);
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
                    t.AddOnExitAction(delegate
                    {
                        l.ExecuteRecycle();
                    });
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
                r.SetOnEnemyEnterAction(EnemyEnterAction);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
        // ��������ٽ����Ѫ
        for (int i = 0; i < list.Count; i++)
        {
            list[i].DecMaxHp();
        }
    }

    /// <summary>
    /// ���з���λ�������ж�����ʱ
    /// </summary>
    /// <param name="u"></param>
    private static void EnemyEnterAction(BaseUnit u)
    {
        if (hitedUnitList.Contains(u))
            return;
        // ������ǰ�˺�
        float dmg = u.mMaxHp * u.mBurnRate * u.mAoeRate; 
        // �������ڻ���
        dmg = u.NumericBox.DamageShield(dmg);
        // ʣ���˺�תΪ�ҽ��˺�
        if (dmg > 0)
            new DamageAction(CombatAction.ActionType.BurnDamage, null, u, dmg).ApplyAction();
        //  ����Ƕ�ת����ʩ��2����ѣЧ��
        if(currentShape >= 2)
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, 120, false));
        // ��¼�ѱ������һ��
        hitedUnitList.Add(u);
    }
    
    /// <summary>
    /// ��ȡȫ�ּ���������
    /// </summary>
    /// <returns></returns>
    private static CustomizationTask GetGlobalAttackTimerTask()
    {
        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            nextAttackTimeLeft = 600;
        });
        task.AddTaskFunc(delegate {
            List<RaidenBaguette> l1 = GetThisUnitList();
            if (l1.Count <= 0)
                return true;
            nextAttackTimeLeft--;
            if(nextAttackTimeLeft <= 0)
            {
                // ����ѡ������δ��������׵糤�����
                List<RaidenBaguette> l2 = new List<RaidenBaguette>();
                foreach (var u in l1)
                {
                    if (u.GetNoCountUniqueStatus(StringManager.Stun) == null)
                        l2.Add(u);
                }
                // ���δ��������׵糤�������������2����ִ��һ�ε��
                if (l2.Count >= 2)
                {
                    ExecuteAttack(l2);
                    nextAttackTimeLeft = 600;
                }
            }
            return false;
        });
        return task;
    }
    #endregion
}
