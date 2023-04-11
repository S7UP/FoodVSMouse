
using UnityEngine;

public class BunGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

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
        base.MInit();
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
        base.AfterGeneralAttack();
        mAttackFlag = true;
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
            mAttackFlag = false;
        }
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
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.SetStandardVelocity(36);
        b.SetRotate(Vector2.right);
        GameController.Instance.AddBullet(b);
    }
}
