using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// ������
/// </summary>
public class Takoyaki : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private float hpRate;
    private float addDamage;
    private int maxAttackCount;
    private int currentAttackCount; // ��ǰ����������
    private float endAttackPercent; // �������һ���ӵ�ʱ�Ķ������Űٷֱ�
    private List<float> attackPercentList;


    public override void Awake()
    {
        if(Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/13/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        hpRate = 0.75f; // �ܵ��˺�������ʱ��Ѫ��
        if(mShape == 1)
        {
            addDamage = 0.5f;
        }else if(mShape == 2)
        {
            addDamage = 1.5f;
        }
        else
        {
            addDamage = 0;
        }

        maxAttackCount = 2;
        endAttackPercent = 0.769f;
        attackPercentList = new List<float>();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
        }
        currentAttackCount = 0;
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
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            if (UnitManager.CanBeSelectedAsTarget(this, unit))
                return true;
        }
        return false;
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
            currentAttackCount++;
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
        currentAttackCount = 0;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return currentAttackCount < attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        CreateBullet();
    }

    /// <summary>
    /// ����һ�����ٻ�����
    /// </summary>
    private void CreateBullet()
    {
        float dmg = mCurrentAttack;
        AllyBullet b = AllyBullet.GetInstance(Bullet_RuntimeAnimatorController, this, dmg);
        b.isNavi = false;
        b.isIgnoreHeight = true; // ����ʱ���Ӹ߶�
        b.transform.position = transform.position;
        b.SetStandardVelocity(36);
        b.SetRotate(Vector2.right);
        b.AddHitAction((b, u) => {
            if(mShape > 0 && u.GetHeathPercent() <= hpRate)
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, addDamage*dmg).ApplyAction();
        });
        GameController.Instance.AddBullet(b);
        // ��������ı���
        Func<BaseBullet, BaseUnit, bool> canHitFunc = null; // ֻ�������Ŀ�������
        float angleDeltaRate = 0;
        int aliveTime = 3 * 60;
        // ���׷������
        TaskManager.AddTrackAbility(b,
                // Func < BaseBullet, BaseUnit > FindTargetFunc
                (bullet) =>
                {
                    // ������ֵ��͵Ŀɱ�ѡȡ�ĵ���
                    float minHp = float.MaxValue;
                    BaseUnit targetUnit = null;
                    foreach (var unit in GameController.Instance.GetEachEnemy())
                    {
                        if (UnitManager.CanBeSelectedAsTarget(this, unit) && UnitManager.CanBulletHit(unit, b))
                        {
                            if (unit.mCurrentHp < minHp)
                            {
                                minHp = unit.mCurrentHp;
                                targetUnit = unit;
                            }
                        }
                    }
                    // ���ֻ�ܻ��б�ѡȡĿ���Ч��
                    if (targetUnit != null)
                    {
                        canHitFunc = (b1, u) => { return targetUnit == u; };
                        bullet.AddCanHitFunc(canHitFunc);
                    }
                    return targetUnit;
                },
                // Func<BaseBullet, BaseUnit, bool> InValidFunc,
                (bullet, unit) =>
                {
                    bool isInValid = !unit.IsValid() || !UnitManager.CanBeSelectedAsTarget(this, unit) || !UnitManager.CanBulletHit(unit, b);
                    if (isInValid)
                    {
                        bullet.RemoveCanHitFunc(canHitFunc);
                    }
                    return isInValid;
                },
                // Action < BaseBullet, BaseUnit > TrackAction,
                (bullet, unit) =>
                {
                    angleDeltaRate = Mathf.Min(1, angleDeltaRate + 0.0025f);
                    Vector2 currentRotate = bullet.GetRotate();
                    Vector2 targetRotate = (unit.transform.position - bullet.transform.position).normalized; // ����ó�������Ҫ�ﵽ�ķ���
                    float dAngle = Vector2.Angle(currentRotate, targetRotate); // ������ߵļнǣ�һ�������ģ�ȡֵ��Χ��0~180��
                    float sign = Mathf.Sign(currentRotate.x * targetRotate.y - targetRotate.x * currentRotate.y); // ͨ�������ȷ���ǶȲ������Ļ��Ǹ���
                    dAngle = dAngle * sign;
                    bullet.SetRotate(bullet.mAngle + angleDeltaRate * dAngle);
                    aliveTime--;
                },
                // Action < BaseBullet > NoTargetAction,
                (bullet) =>
                {
                    angleDeltaRate = 0;
                },
                // Func<BaseBullet, BaseUnit, bool> ExitConditionFunc);
                (bullet, unit) =>
                {
                    // �Ի�
                    bool isDestory = (aliveTime <= 0);
                    if (isDestory) 
                    {
                        bullet.KillThis();
                    }
                    return isDestory;
                });
        // ���һ����������
        TaskManager.AddSpinAbility(b, 6);
    }
}
