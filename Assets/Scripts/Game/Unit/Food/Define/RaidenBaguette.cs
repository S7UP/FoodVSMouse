using System.Collections.Generic;

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
    private static int currentLevel;
    private static int currentShape; // ��ǰΪ��ת
    private static int nextAttackTimeLeft; // �´ι���ʣ��ʱ��

    private GeneralAttackSkillAbility generalAttackSkillAbility;

    public override void Awake()
    {
        if(Lightning_Sprite == null)
            Lightning_Sprite = GameManager.Instance.GetSprite("Food/40/lightning");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        currentShape = mShape;
        

        // ����һ���׵类����ʱ������ȫ�ּ�ʱ��
        if (GetThisUnitList().Count == 0)
            GameController.Instance.mTaskController.AddUniqueTask("RaidenBaguette_Attack_Task", GetGlobalAttackTimerTask());

        if (!thisUnitList.Contains(this))
            thisUnitList.Add(this);
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
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
        NumericBox.Attack.SetBase(FoodManager.GetAttack(FoodNameTypeMap.RaidenBaguette, mLevel, mShape));
        currentLevel = mLevel;
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
    }

    /// <summary>
    /// ���з���λ�������ж�����ʱ
    /// </summary>
    /// <param name="u"></param>
    private static void EnemyEnterAction(BaseUnit u)
    {
        if (hitedUnitList.Contains(u))
            return;
        new DamageAction(CombatAction.ActionType.BurnDamage, null, u, Mathf.Max(0.2f*u.mMaxHp*u.mBurnRate, FoodManager.GetAttack(FoodNameTypeMap.RaidenBaguette, currentLevel, currentShape)*u.NumericBox.AoeRate.TotalValue)).ApplyAction();
        // ��¼�ѱ������һ��
        hitedUnitList.Add(u);
    }
    
    /// <summary>
    /// ��ȡȫ�ּ���������
    /// </summary>
    /// <returns></returns>
    private static CustomizationTask GetGlobalAttackTimerTask()
    {
        int[] attackTimeArray;
        int index = 0;
        if (currentShape == 0)
            attackTimeArray = new int[] { 180 };
        else if (currentShape == 1)
            attackTimeArray = new int[] { 120 };
        else
            attackTimeArray = new int[] { 90, 30 };

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            nextAttackTimeLeft = attackTimeArray[index];
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
                    index = (index + 1) % attackTimeArray.Length;
                    nextAttackTimeLeft = attackTimeArray[index];
                }
            }
            return false;
        });
        return task;
    }
    #endregion
}
