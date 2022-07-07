using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ����
/// </summary>
public class HeaterFoodUnit : FoodUnit
{
    /// <summary>
    /// ����ͨ��������ӵ����ͱ�
    /// </summary>
    public static List<BulletStyle> canThroughtBulletStyleList = new List<BulletStyle>() { 
        BulletStyle.Wine, BulletStyle.Water
    };


    private float mulRate; //�˺���������
    private Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = GetComponent<Rigidbody2D>();
    }

    public override void MInit()
    {
        base.MInit();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        mulRate = (float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]);
    }

    /// <summary>
    /// �õ�λװ��rigibody2d�������Ҫ��д�˷���
    /// </summary>
    /// <param name="V3"></param>
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    /// <summary>
    /// ��⴫���ӵ��Ƿ��ܴ���
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet baseBullet)
    {
        // �ж��ӵ��Ƿ��ѱ���ǿ
        if (baseBullet.GetTagCount(StringManager.BulletDamgeIncreasement) > 0)
            return false;
        // �ж��ӵ������ܷ����
        foreach (var item in canThroughtBulletStyleList)
        {
            if (item == baseBullet.style)
                return true;
        }
        return false;
    }

    public void OnCollision(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }


        if (collision.tag.Equals("Bullet"))
        {
            // ��⵽�ӵ���λ��ײ��
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (CanThrought(bullet)) // ����ӵ��ܷ񴩹�
            {
                // ǿ�ư��ӵ���ͼ��Ϊ�𵯣����ǲ��ı�ԭʼstyleֵ��
                bullet.ChangeAnimatorWithoutChangeStyle(BulletStyle.Fire);
                bullet.SetDamage(bullet.GetDamage() * mulRate); // �����˺�
                bullet.SetVelocity(bullet.GetVelocity() * 1.5f); // ����
                bullet.AddTag(StringManager.BulletDamgeIncreasement); // Ϊ�ӵ������������ı�ǣ���ֹ��ι���
            }
        }
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
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
