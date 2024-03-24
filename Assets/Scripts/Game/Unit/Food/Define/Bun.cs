using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// С����
/// </summary>
public class Bun : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private static int[] attackCountArray = new int[] { 1, 2, 4 };
    private static float[] speed_rate = new float[] { 1, 1, 2f };

    private int maxAttackCount;
    private int currentAttackCount; // ��ǰ����������
    private List<float> attackPercentList;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/21/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ���������ǰ������
        maxAttackCount = attackCountArray[mShape];

        attackPercentList = new List<float>();
        if (mShape == 0)
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
        return GameController.Instance.CheckRowCanAttack(this, GetRowIndex());
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
        return currentAttackCount<attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, this, mCurrentAttack);
        b.transform.position = transform.position;
        b.SetSpriteLocalPosition(GetSpriteLocalPosition() + Vector2.up * 0.2f);
        b.SetStandardVelocity(24 * speed_rate[mShape]);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        GameController.Instance.AddBullet(b);
    }
}
