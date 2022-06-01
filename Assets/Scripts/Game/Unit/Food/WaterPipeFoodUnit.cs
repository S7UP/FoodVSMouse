using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ˫��ˮ�ܾ���ʵ��
/// </summary>
public class WaterPipeFoodUnit : FoodUnit
{
    private List<int> maxFrontAttackCountList = new List<int>{ 1, 2, 3};
    private List<int> maxBackAttackCountList = new List<int> { 2, 2, 3};

    private int maxFrontAttackCount;
    private int maxBackAttackCount;
    private int maxAttackCount;
    private int currentAttackCount; // ��ǰ����������
    private float endAttackPercent; // �������һ���ӵ�ʱ�Ķ������Űٷֱ�
    private List<float> attackPercentList;

    public override void MInit()
    {
        base.MInit();
        SetLevel(12);
        // ����תְ���������ǰ������
        maxFrontAttackCount = maxFrontAttackCountList[mShape];
        maxBackAttackCount = maxBackAttackCountList[mShape];
        maxAttackCount = Mathf.Max(maxFrontAttackCount, maxBackAttackCount);
        endAttackPercent = 0.90f;
        attackPercentList = new List<float>();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent)*i/(maxAttackCount-1));
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
        List<BaseUnit>[] list = GameController.Instance.GetEnemyList();
        if (list[GetRowIndex()].Count > 0)
           return true;
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
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
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
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (currentAttackCount < maxAttackCount && info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercentList[currentAttackCount]);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ǰ����
        if (currentAttackCount < maxFrontAttackCount)
        {
            GameController.Instance.CreateBullet(this, transform.position + Vector3.right * 0.25f + Vector3.up*0.1f, Vector2.right, BulletStyle.Water).SetDamage(mCurrentAttack);
        }
        // �󹥻�
        if(currentAttackCount < maxBackAttackCount)
        {
            GameController.Instance.CreateBullet(this, transform.position + Vector3.left * 0.25f, Vector2.left, BulletStyle.Water).SetDamage(mCurrentAttack);
        }
    }
}
