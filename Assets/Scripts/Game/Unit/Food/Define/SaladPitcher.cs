using UnityEngine;
using System;
/// <summary>
/// ɫ��Ͷ��
/// </summary>
public class SaladPitcher : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private int bounceCount; // �ӵ���������
    private float bounceDamageRate; // ���������˺�����
    private Vector2 targetPosition;

    public override void Awake()
    {
        base.Awake();
        if(BulletRuntimeAnimatorController == null)
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/34/Bullet");
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ�����ȷ��������������������˺�����
        switch (mShape)
        {
            case 1:
                bounceCount = 1;
                bounceDamageRate = 1;
                break;
            case 2:
                bounceCount = 2;
                bounceDamageRate = 1;
                break;
            default:
                bounceCount = 1;
                bounceDamageRate = 0.5f;
                break;
        }
        targetPosition = Vector2.zero;
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
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());
        if (targetUnit != null)
        {
            targetPosition = targetUnit.transform.position;
        }
        return targetUnit!=null;
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
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());

        CreateBullet(transform.position, mCurrentAttack, target, bounceCount);
    }

    /// <summary>
    /// ����һ�������ӵ���ע�����ǵݹ鷽����
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    /// <param name="bounceLeft"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target, int bounceLeft)
    {
        // ���㵱ǰ�ӵ����˺�
        float real_dmg = ori_dmg;
        if(mShape == 2)
        {
            if (bounceLeft == 2)
                real_dmg = ori_dmg;
            else
                real_dmg = ori_dmg * bounceDamageRate;
        }
        else if(mShape < 2)
        {
            if (bounceLeft == 1)
                real_dmg = ori_dmg;
            else
                real_dmg = ori_dmg * bounceDamageRate;
        }
        AllyBullet b = AllyBullet.GetInstance(BulletRuntimeAnimatorController, this, real_dmg);
        b.AddSpriteOffsetY(new FloatModifier(0.5f*MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // ������к�ĵ���Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u == null)
                {
                    // ���û�ҵ������ǲ��ᵯ����
                    return;
                }

                BaseUnit target = null;
                // ���ݲ�ͬ�����ѡ��ͬ�������㷨
                if (bounceLeft == 1)
                {
                    if (mShape < 2)
                    {
                        // �Ƕ�ת��ȥ�����ҵĵ�λ
                        target = MouseManager.FindTheMostRightTarget(this, u.transform.position.x, float.MaxValue, b.GetRowIndex());
                    }
                    else
                    {
                        // ��ת��ȥ������ĵ�λ
                        target = MouseManager.FindTheMostLeftTarget(this, float.MinValue, u.transform.position.x, b.GetRowIndex());
                    }
                }
                else if (bounceLeft == 2)
                {
                    // ����ֻ�ж�ת����ֵ��������Ч��һת���һ������
                    target = MouseManager.FindTheMostRightTarget(this, u.transform.position.x, float.MaxValue, b.GetRowIndex());
                }

                if (target != null)
                {
                    // ������Ŀ��ͽ��е��䣨�����Ǵ���һ���µ��ӵ����������Ի٣�ɶҲ���ɣ�
                    CreateBullet(u.transform.position, ori_dmg, target, bounceLeft - 1);
                }
            };
        }

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
                    // pudding.RedirectThrowingObject();
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if(next_target != null)
                    {
                        b.isnUseHitEffect = true; // ��ǰ�ӵ������û��ж�����ֱ����ʧ
                        CreateBullet(b.transform.position, ori_dmg, next_target, bounceCount);
                    }   
                }
            });

            // ����������˶�
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        }
        else
        {
            // Ŀ���ǵз���ֱ����Ӽ���
            b.AddHitAction(hitEnemyAction);

            // ȷ���ò���������������˶�
            if (target != null)
                PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
            else
                PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);
        }
        

        GameController.Instance.AddBullet(b);

        return b;
    }
}
