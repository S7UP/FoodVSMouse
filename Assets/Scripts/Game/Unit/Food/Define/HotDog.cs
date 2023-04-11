using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �ȹ�����
/// </summary>
public class HotDog : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private float airUnitDamageRate; // �Կ���Ŀ���˺�����
    private float airAoeDamageRate; // �Կ��з�Χ�˺�����
    private float groundUnitDamageRate; // �Ե���Ŀ���˺�����
    private int airSlowTime; // �Կռ���ʱ��
    private int groundSlowTime; // �Եؼ���ʱ��
    private Vector2 targetPosition;
    private BaseUnit airTarget; // ��ǰ��ѡΪĿ��Ŀ��е�λ
    private BaseUnit groundTarget; // ��ǰ��ѡΪĿ��ĵ��浥λ
    private int attackCount; // ������������
    private int attackLeft; // ʣ�๥������
    private static List<float> attackPercentList = new List<float>() { 0.5f, 0.75f };

    public override void Awake()
    {
        if (BulletRuntimeAnimatorController == null)
        {
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/41/Bullet");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ�����ȷ���ڵ��˺�����
        switch (mShape)
        {
            case 1:
                airUnitDamageRate = 2.66f;
                attackCount = 1;
                break;
            case 2:
                airUnitDamageRate = 2.66f;
                attackCount = 2;
                break;
            default:
                airUnitDamageRate = 2.0f;
                attackCount = 1;
                break;
        }
        attackLeft = attackCount;
        targetPosition = Vector2.zero;
        groundUnitDamageRate = 1.0f;
        airAoeDamageRate = 0.2f;
        airSlowTime = 180;
        groundSlowTime = 30;
        airTarget = null;
        groundTarget = null;
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        FindTarget();
        return GetAirTarget()!=null || GetGroundTarget()!=null;
    }

    /// <summary>
    /// ���з�ʽ
    /// </summary>
    private void FindTarget()
    {
        // ��յ�ǰ����
        airTarget = null;
        groundTarget = null;

        // �Ȼ�ȡ��ǰ�е����ез���λ,Ȼ��ѡ�������Ҳ�ĵз���λ
        List<BaseUnit> unitList = new List<BaseUnit>();
        foreach (var unit in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if(unit.transform.position.x > transform.position.x && UnitManager.CanBeSelectedAsTarget(this, unit))
            {
                unitList.Add(unit);
            }
        }
        
        // ȡ����A��B�е�Ŀ��
        foreach (var m in unitList)
        {
            if(m.GetHeight() == 1 && (GetAirTarget() == null || m.transform.position.x < GetAirTarget().transform.position.x))
            {
                airTarget = m;
            }
            else if (m.GetHeight() == 0 && (GetGroundTarget() == null || m.transform.position.x < GetGroundTarget().transform.position.x))
            {
                groundTarget = m;
            }
        }
        if (airTarget != null)
            targetPosition = airTarget.transform.position;
        else if (groundTarget != null)
            targetPosition = groundTarget.transform.position;
    }

    /// <summary>
    /// ��ȡ��ǰ�����Ŀ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetAirTarget()
    {
        if (airTarget != null && !airTarget.IsAlive())
            airTarget = null;
        return airTarget;
    }

    /// <summary>
    /// ��ȡ��ǰ�����ĵ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetGroundTarget()
    {
        if (groundTarget != null && !groundTarget.IsAlive())
            groundTarget = null;
        return groundTarget;
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
            ExecuteDamage();
            attackLeft--;
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
        attackLeft = attackCount;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (attackLeft > 0 && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[attackCount - attackLeft]);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ��������һ�����и���
        FindTarget();
        int c = attackCount - attackLeft; // ���㵱ǰ�ǵڼ��ι���(0,1)
        if(c == 0)
        {
            // ���ȶԿ�����
            CreateBullet(transform.position, mCurrentAttack, (GetAirTarget() != null ? GetAirTarget() : GetGroundTarget()));
        }
        else if(c == 1)
        {
            // ���ȶԵ�����
            CreateBullet(transform.position, mCurrentAttack, (GetGroundTarget() != null ? GetGroundTarget() : GetAirTarget()));
        }
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        bool isAirTarget = (target != null && target.GetHeight() == 1); // �Ƿ�Ϊ���е�λ

        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorController, this, (isAirTarget ? airUnitDamageRate:groundUnitDamageRate) * ori_dmg);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.SetHeight(isAirTarget ? 1:0);
        b.isnDelOutOfBound = true; // ��������ɾ

        // ������к��Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u != null)
                {
                    int timeLeft = (isAirTarget ? airSlowTime : groundSlowTime);
                    // ΪĿ��ʩ�Ӽ���
                    u.AddStatusAbility(new FrozenSlowStatusAbility(-50, u, timeLeft));
                    // ��Ŀ��Ϊ���е��ˣ�������Կ�AOE
                    if(isAirTarget)
                        CreateDamageArea(u.transform.position, ori_dmg);
                }
            };
        }

        // Ŀ���ǵз���ֱ����Ӽ���
        b.AddHitAction(hitEnemyAction);

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);

        GameController.Instance.AddBullet(b);
        return b;
    }


    /// <summary>
    /// �Կ�AOE�˺�
    /// </summary>
    private void CreateDamageArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(1);
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((u) => {
            new DamageAction(CombatAction.ActionType.CauseDamage, this, u, Mathf.Min(airAoeDamageRate * ori_dmg, u.mMaxHp * 0.1f)).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
