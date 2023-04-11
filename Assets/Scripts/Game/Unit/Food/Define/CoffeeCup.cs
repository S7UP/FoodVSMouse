using UnityEngine;

public class CoffeeCup : FoodUnit
{
    private static RuntimeAnimatorController Bullet_Run;

    public override void Awake()
    {
        if (Bullet_Run == null)
            Bullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Food/25/BUllet");
        base.Awake();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        BaseCardBuilder builder = GetCardBuilder();
        if (builder != null)
            SmallStove.CreateAddFireEffect(transform.position, (float)builder.attr.GetCost(mLevel));
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
            mAttackFlag = false;
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
        return mAttackFlag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        float dmg = mCurrentAttack;
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.NoStrengthenNormal, Bullet_Run, this, 0);
        if (mShape >= 2)
            b.isnKillSelf = true;
        b.transform.position = transform.position;
        b.SetStandardVelocity(24);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Puff");
        b.AddHitAction((bullet, u) => {
            if(u is MouseUnit)
                (u as MouseUnit).FlashWhenHited();
            u.AddRecordDamage(dmg);
            if(mShape >= 1)
                StatusManager.AddDamageRateBuff(u, 1.05f, 60);
        });
        GameController.Instance.AddBullet(b);

        float s = 0;
        float maxDist = MapManager.gridWidth * 3;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            s += b.GetVelocity();
            if (s >= maxDist)
                return true;
            return false;
        });
        t.AddOnExitAction(delegate {
            b.KillThis();
        });
        b.AddTask(t);
    }
}
