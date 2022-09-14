using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �Ǻ�«�ڵ�
/// </summary>
public class SugarGourd : FoodUnit
{
    private static readonly int[] countArray = { 1, 1, 2 }; // ����תְ�����ȷ�����伸���ӵ�
    private static readonly float[] damageRateArray = { 1.0f, 1.0f, 0.65f}; // �˺�������תְ��ϵ
    private static readonly int[] stunTimeArray = { 6, 15, 15}; // ����ʱ�䣨֡����תְ��ϵ
    private int currentAttackCount;
    private int maxAttackCount;
    private List<float> attackPercentList;
    private float endAttackPercent; // �������һ���ӵ�ʱ�Ķ������Űٷֱ�

    public override void MInit()
    {
        base.MInit();
        currentAttackCount = 0;
        maxAttackCount = countArray[mShape];
        endAttackPercent = 0.7f;
        attackPercentList = new List<float>();
        if (maxAttackCount <= 1)
        {
            attackPercentList.Add(endAttackPercent);
        }
        else
        {
            for (int i = 0; i < maxAttackCount; i++)
            {
                attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
            }
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
        // ֻҪ�п��е�λ���ҿ��Ա�ѡȡ���ɹ���
        foreach (var item in GameController.Instance.GetEachEnemy())
        {
            if (item.GetHeight() == 1 && item.CanBeSelectedAsTarget() && item.IsAlive())
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
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
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
        return (currentAttackCount < maxAttackCount && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount]);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        TrackingBullets b = (TrackingBullets)GameController.Instance.CreateBullet(this, transform.position + Vector3.up * MapManager.gridHeight/2, Vector2.up, BulletStyle.SugarGourd);
        b.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/"+ ((int)BulletStyle.SugarGourd)+"/"+mShape);
        b.SetHeight(1); // ���ø߶�Ϊ�Կո߶�
        b.SetSearchEnemyEnable(true); // ��������ģʽ
        b.SetCompareFunc(BulletCompareFunc);
        b.SetVelocityChangeEvent(TransManager.TranToVelocity(12), TransManager.TranToVelocity(48), 90);
        b.SetDamage(mCurrentAttack* damageRateArray[mShape]);
        b.SetHitAction(BulletHitAction); // ���û��к���¼�
    }

    ////////////////////////////////////////////////////////////������˽�з���///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// �ӵ����бȽ��߼�
    /// </summary>
    /// <param name="currentTarget">��ǰĿ��</param>
    /// <param name="compareTarget">���Ƚϵ�Ŀ��</param>
    /// <returns>��Ϊtrue��compareTarget��ȡ��currentTarget��Ϊ��ǰtarget</returns>
    private bool BulletCompareFunc(BaseUnit currentTarget, BaseUnit compareTarget)
    {
        if (compareTarget == null || !compareTarget.IsAlive() || !compareTarget.CanBeSelectedAsTarget() || compareTarget.GetHeight()!=1)
            return false;
        if (currentTarget == null || !currentTarget.IsAlive() || !currentTarget.CanBeSelectedAsTarget() || currentTarget.GetHeight() != 1)
            return true;
        return (compareTarget.transform.position.x < currentTarget.transform.position.x);
    }

    /// <summary>
    /// �ӵ������¼�
    /// </summary>
    /// <param name="bullet">��ǰ�ӵ�</param>
    /// <param name="targetUnit">�����е�Ŀ��</param>
    private void BulletHitAction(BaseBullet bullet, BaseUnit targetUnit)
    {
        if (bullet == null || targetUnit == null)
            return;
        targetUnit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(targetUnit, stunTimeArray[mShape], false));
    }
}
