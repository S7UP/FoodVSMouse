
using System.Collections.Generic;

using UnityEngine;

public class BunGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private static int[] attackCountArray = new int[] { 1, 2, 4 };
    private static float[] speed_rate = new float[] { 1, 1, 2f };

    private int maxAttackCount;
    private int currentAttackCount; // ��ǰ����������
    private List<float> attackPercentList;
    private float dmgValue;

    private int shape;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Weapons/5/Bullet");
        }
            
        base.Awake();
    }

    public override void MInit()
    {
        shape = 0;
        base.MInit();
        // ����Я��С������Ƭ��תְ��������㷢����
        int level = 0;
        BaseCardBuilder builder = GameController.Instance.mCardController.GetCardBuilderByType(FoodNameTypeMap.Bun);
        if (builder != null)
        {
            shape = builder.mShape;
            level = builder.mLevel;
        }
        // ����תְ���������ǰ������
        maxAttackCount = attackCountArray[shape];

        attackPercentList = new List<float>();
        if (shape == 0)
        {
            attackPercentList.Add(attackPercent);
        }
        else
        {
            float end = 1.0f;
            for (int i = 0; i < maxAttackCount; i++)
                attackPercentList.Add(attackPercent + (end - attackPercent) * i / (maxAttackCount - 1));
        }

        currentAttackCount = 0;
        // ��ȡ�����ӵ��˺�
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.Bun, level, shape);
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(master, GetRowIndex());
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
        base.AfterGeneralAttack();
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
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10 * dmgValue);
        b.transform.position = transform.position;
        b.SetStandardVelocity(24 * speed_rate[shape]);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddHitAction((b, u) => {
            if (u is MouseUnit)
            {
                MouseUnit m = u as MouseUnit;
                if (!m.IsBoss())
                {
                    new DamageAction(CombatAction.ActionType.CauseDamage, master, u, 0.02f * u.GetCurrentHp()).ApplyAction();
                }
            }
        });
        GameController.Instance.AddBullet(b);
    }
}
