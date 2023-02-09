using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ����
/// </summary>
public class HeaterFoodUnit : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

    /// <summary>
    /// ����ͨ��������ӵ����ͱ�
    /// </summary>
    public static List<BulletStyle> canThroughtBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Normal
    };


    private float mulRate; //�˺���������

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/8/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        CreateCheckArea();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        mulRate = (float)attr.valueList[mLevel];
    }

    /// <summary>
    /// ��⴫���ӵ��Ƿ��ܴ���
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet b)
    {
        // ����������
        if (isFrozenState)
            return false;
        // �ӵ������ѷ��ӵ�
        if (!(b is AllyBullet))
            return false;
        // �ж��ӵ��Ƿ��ѱ���ǿ
        if (b.GetTagCount(StringManager.BulletDamgeIncreasement) > 0)
            return false;
        // �ж��ӵ������ܷ����
        foreach (var item in canThroughtBulletStyleList)
        {
            if (item == b.style)
                return true;
        }
        return false;
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <returns></returns>
    public float GetDamageRate()
    {
        return 1 + Mathf.Min(1, mCurrentAttackSpeed) * Mathf.Min(1, mCurrentAttack / 10) * (mulRate - 1);
    }

    /// <summary>
    /// ����������Ȧ
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1.0f, 1.0f, "ItemCollideBullet");
        r.isAffectBullet = true;
        r.SetOnBulletEnterAction(OnCollision);
        r.SetOnBulletStayAction(OnCollision);
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (this.IsAlive())
                {
                    r.transform.position = transform.position;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            t.OnExitFunc = delegate {
                r.MDestory();
            };
            r.AddTask(t);
        }
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public void OnCollision(BaseBullet b)
    {
        if (CanThrought(b)) // ����ӵ��ܷ񴩹�
        {
            // ǿ�ư��ӵ���ͼ��Ϊ�𵯣����ǲ��ı�ԭʼstyleֵ��
            b.animator.runtimeAnimatorController = Bullet_RuntimeAnimatorController;
            b.SetDamage(b.GetDamage() * GetDamageRate()); // �����˺�
            b.SetVelocity(b.GetVelocity() * 1.5f); // ����
            b.AddTag(StringManager.BulletDamgeIncreasement); // Ϊ�ӵ������������ı�ǣ���ֹ��ι���
        }
    }

    /////////////////////////////////���¹��ܾ�ʧЧ������Ҫ���·���/////////////////////////////////////

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // �����Ϳ�Ƭ����Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ����Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �����Ϳ�Ƭ�޹���״̬
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ��
        return true;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // �����Ϳ�Ƭ��
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // �����Ϳ�Ƭ��
    }
}
