using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ӣ�ҷ�������
/// </summary>
public class CherryPuddingFoodUnit : FoodUnit
{
    /// <summary>
    /// ���Ա��������ӵ����ͱ�
    /// </summary>
    public static List<BulletStyle> canReboundBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Wine, BulletStyle.Water, BulletStyle.Fire
    };


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
        if(mShape >= 1)
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel])*1.25f);
        else
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
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
    /// ��⴫���ӵ��Ƿ��ܷ���
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet baseBullet)
    {
        // �ж��ӵ��Ƿ��ѱ�����
        if (baseBullet.GetTagCount(StringManager.BulletRebound) > 0)
            return false;
        // �ж��ӵ������ܷ񷴵�
        foreach (var item in canReboundBulletStyleList)
        {
            if (item == baseBullet.style)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Ѱ�ұ��ض���Ͷ�����Ŀ���㷨��������������ģ�������������ģ�
    /// </summary>
    /// <returns></returns>
    public BaseUnit FindRedirectThrowingObjectTarget(BaseBullet b)
    {
        // ��Ѱ����Ч��λ
        BaseUnit target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, float.MinValue, b.transform.position.x, b.GetRowIndex());
        if (target != null)
        {
            return target;
        }
        else
        {
            // ���ûĿ���Ǿ�����һ�Σ���δ��Ҳ���
            target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, b.transform.position.x, float.MaxValue, b.GetRowIndex());
            return target;
        }
    }

    /// <summary>
    /// �ض���Ͷ����
    /// </summary>
    public void RedirectThrowingObject(BaseBullet b)
    {
        // �ж��ӵ��Ƿ��ѱ��ض����������ѱ��ض�������������ض���
        if (b.GetTagCount(StringManager.BulletRebound) > 0)
            return;
        // ����ض��������ض������ӵ�������пɹ�����λ������ĵ�λ
        {
            // ��Ѱ����Ч��λ
            BaseUnit target = FindRedirectThrowingObjectTarget(b);
            if (target != null)
            {
                // ����ȥ��
                PitcherManager.AddDefaultFlyTask(b, b.transform.position, target, true, false);
            }
            //else
            //{
            //    // ��Ҫ�ǻ�û�еĻ����Ǿ�ԭ�����ϵ�Ȼ�������
            //    PitcherManager.AddDefaultFlyTask(b, b.transform.position, b.transform.position, true, false);
            //}
            b.AddTag(StringManager.BulletRebound); // Ϊ�ӵ������ѷ����ı�ǣ���ֹ��η���
        }
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
                // �ӵ�����
                bullet.SetRotate(-bullet.GetRotate());
                bullet.AddTag(StringManager.BulletRebound); // Ϊ�ӵ������ѷ����ı�ǣ���ֹ��η���
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
