using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ����Ͷ��
/// </summary>
public class EggPitcher : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private float mainDamageRate; // ��ҪĿ���˺�����
    private float aoeDamageRate; // ��Χ�˺�����
    private Vector2 targetPosition;

    public override void MInit()
    {
        base.MInit();
        if (BulletRuntimeAnimatorController == null)
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/37/" + mShape + "/Bullet");
        // ����תְ�����ȷ����ҪĿ���뷶Χ�˺����˺�����
        switch (mShape)
        {
            case 1:
                mainDamageRate = 6f;
                aoeDamageRate = 1.2f;
                break;
            case 2:
                mainDamageRate = 6f;
                aoeDamageRate = 1.2f;
                break;
            default:
                mainDamageRate = 4.5f;
                aoeDamageRate = 0.9f;
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
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
        if (targetUnit != null)
        {
            targetPosition = targetUnit.transform.position;
        }
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
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());

        CreateBullet(transform.position, mCurrentAttack, target);
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorController, this, mainDamageRate * ori_dmg);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        b.SetHitSoundEffect("Eggimpact"+GameManager.Instance.rand.Next(0, 2));

        // ������к��Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u != null)
                {
                    // ����AOE
                    CreateDamageArea(u.transform.position, ori_dmg);
                }
                else
                {
                    CreateDamageArea(b.transform.position, ori_dmg);
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
            t.AddOnExitAction(delegate
            {
                pudding = null; // ���Ŀ�겼��������ȡ��������
            });
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
                        CreateBullet(b.transform.position, ori_dmg, next_target);
                    }
                    else
                    {
                        // ԭ�����ѣ���ͬ���ᴥ����ΧЧ��
                        hitEnemyAction(b, null);
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


    /// <summary>
    /// AOE�˺�
    /// </summary>
    private void CreateDamageArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((u) => {
            // u.FlashWhenHited();
            // �������
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, u, aoeDamageRate * ori_dmg);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
