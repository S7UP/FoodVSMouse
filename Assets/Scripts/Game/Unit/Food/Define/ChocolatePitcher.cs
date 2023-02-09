using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �ɿ���Ͷ��
/// </summary>
public class ChocolatePitcher : FoodUnit
{
    private static RuntimeAnimatorController BigBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController SmallBulletRuntimeAnimatorController;

    private Vector2 targetPosition;
    private int BigBulletAttackCount; // Ͷ���ɿ���������Ҫ�Ĺ�������
    private int bigLeft; // Ͷ���ɿ�����ǰ����Ҫ�Ĺ�������
    private float BigDamageRate; // �ɿ�������ɵ��˺�����
    private int StunTime; // �ɿ�������ɵ���ѣЧ��

    public override void Awake()
    {
        base.Awake();
        if (BigBulletRuntimeAnimatorController == null)
        {
            BigBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/BigBullet");
            SmallBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/SmallBullet");
        }
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ�����ȷ������
        switch (mShape)
        {
            case 1:
                BigBulletAttackCount = 2;
                BigDamageRate = 1f;
                StunTime = 360;
                break;
            case 2:
                BigBulletAttackCount = 1;
                BigDamageRate = 1f;
                StunTime = 360;
                break;
            default:
                BigBulletAttackCount = 2;
                BigDamageRate = 1f;
                StunTime = 240;
                break;
        }
        bigLeft = BigBulletAttackCount;
        targetPosition = Vector2.zero;
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    public override void OnAttackStateEnter()
    {
        if(bigLeft > 0)
            animatorController.Play("Attack0");
        else
            animatorController.Play("Attack1");
    }

    protected override void UpdateAttackAnimationSpeed()
    {
        UpdateAnimationSpeedByAttackSpeed("Attack0");
        UpdateAnimationSpeedByAttackSpeed("Attack1");
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());
        if (targetUnit != null)
            targetPosition = targetUnit.transform.position;
        return targetUnit != null;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����Ŀ�꼴��
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
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
        mAttackFlag = true;
        SetActionState(new IdleState(this));
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
        int rowIndex = GetRowIndex();
        if(bigLeft > 0)
        {
            bigLeft--;
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, rowIndex);
            CreateSmallBullet(transform.position, target);
        }
        else
        {
            bigLeft = BigBulletAttackCount;
            // ������
            int startIndex = Mathf.Max(0, rowIndex - 1);
            int endIndex = Mathf.Min(6, rowIndex + 1);
            for (int i = startIndex; i <= endIndex; i++)
            {
                // ����i��û�б����������λ
                List<BaseUnit> unitList = new List<BaseUnit>();
                foreach (var u in GameController.Instance.GetSpecificRowEnemyList(i))
                {
                    unitList.Add(u);
                }
                // Ѱ�ұ��е��ѷ�������λ��Ҳһ������
                foreach (var u in GameController.Instance.GetSpecificRowAllyList(i))
                {
                    if (u.mType == (int)FoodNameTypeMap.CherryPudding)
                        unitList.Add(u);
                }
                List<BaseUnit> list = UnitManager.GetList(unitList, (u) => { return u.GetNoCountUniqueStatus(StringManager.Stun) == null; });
                BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, list, null);
                if(target == null)
                    target = PitcherManager.FindTargetByPitcher(this, transform.position.x, i);
                if(target != null || i == rowIndex)
                    CreateBigBullet(transform.position, target);
            }
        }
        
    }

    /// <summary>
    /// Ͷ���ɿ�����
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateSmallBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, SmallBulletRuntimeAnimatorController, this, mCurrentAttack);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // �����ж�Ŀ�����ѷ����ǵз�������û�У�������Щ������ƶ��߼�
        if (target != null && target is FoodUnit && target.mType == (int)FoodNameTypeMap.CherryPudding)
        {
            // Ŀ�����ѷ�����
            CherryPuddingFoodUnit pudding = target.GetComponent<CherryPuddingFoodUnit>();

            // ���Ŀ�겼��������
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                return !pudding.IsAlive();
            });
            t.OnExitFunc = delegate
            {
                pudding = null; // ���Ŀ�겼��������ȡ��������
            };
            b.AddTask(t);

            // ����������ض�������
            b.AddHitAction((b, u) => {
                // �ض���
                if (pudding != null)
                {
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // ��ǰ�ӵ������û��ж�����ֱ����ʧ
                        CreateSmallBullet(b.transform.position, next_target);
                    }
                }
            });
        }

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);

        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// Ͷ���ɿ�����
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private BaseBullet CreateBigBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BigBulletRuntimeAnimatorController, this, BigDamageRate*mCurrentAttack);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);


        // �����ж�Ŀ�����ѷ����ǵз�������û�У�������Щ������ƶ��߼�
        if (target != null && target is FoodUnit && target.mType == (int)FoodNameTypeMap.CherryPudding)
        {
            // Ŀ�����ѷ�����
            CherryPuddingFoodUnit pudding = target.GetComponent<CherryPuddingFoodUnit>();

            // ���Ŀ�겼��������
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                return !pudding.IsAlive();
            });
            t.OnExitFunc = delegate
            {
                pudding = null; // ���Ŀ�겼��������ȡ��������
            };
            b.AddTask(t);

            // ����������ض�������
            b.AddHitAction((b, u) => {
                // �ض���
                if (pudding != null)
                {
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // ��ǰ�ӵ������û��ж�����ֱ����ʧ
                        CreateBigBullet(b.transform.position, next_target);
                    }
                }
            });
        }
        else
        {
            // ��ӻ��к���¼�
            b.AddHitAction((b, u) => {
                if (u == null || !u.IsAlive())
                    return;

                // ���Ŀ���Ѵ��ڶ���״̬��Ч�����ӳ�������಻����15��
                StatusAbility s = u.GetNoCountUniqueStatus(StringManager.Stun);
                if (s != null)
                    s.leftTime += Mathf.Min(60, Mathf.Max(0, 540 - s.leftTime));
                else
                    u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, StunTime, false));
                //if (u.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun))
                //{
                //    // ��Ŀ��������ѣ������ɶ����˺�
                //    new DamageAction(CombatAction.ActionType.CauseDamage, this, u, BigDamageRate * mCurrentAttack).ApplyAction();
                //}
            });
        }

        GameController.Instance.AddBullet(b);
        return b;
    }
}
