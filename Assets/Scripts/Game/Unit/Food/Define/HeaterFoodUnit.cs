using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ����
/// </summary>
public class HeaterFoodUnit : FoodUnit
{
    private static RuntimeAnimatorController[] Bullet_RunArray;
    private static RuntimeAnimatorController SpBullet_Run;
    private float rate;

    /// <summary>
    /// ����ͨ��������ӵ����ͱ�
    /// </summary>
    public static List<BulletStyle> canThroughtBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Normal
    };

    public override void Awake()
    {
        if (Bullet_RunArray == null)
        {
            Bullet_RunArray = new RuntimeAnimatorController[4];
            for (int i = 0; i < Bullet_RunArray.Length; i++)
            {
                Bullet_RunArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Food/8/Bullet"+i);
            }
            SpBullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Food/8/SpBullet");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        CreateCheckArea();
        if(mShape >= 2)
        {
            // ��ת�³�ʱ��3���޵�
            StatusManager.AddInvincibilityBuff(this, 180);
        }
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        rate = attr.valueList[mLevel];
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
    public float GetDamageRate(float mulRate)
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
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public void OnCollision(BaseBullet b)
    {
        if (CanThrought(b)) // ����ӵ��ܷ񴩹�
        {
            float ori_dmg = b.GetDamage();
            // ǿ�ư��ӵ���ͼ��Ϊ�𵯣����ǲ��ı�ԭʼstyleֵ��
            if (mShape >= 3)
            {
                b.SetDamage(ori_dmg * GetDamageRate(rate)); // �����˺�
                b.animator.runtimeAnimatorController = SpBullet_Run;
                float dist = 2*MapManager.gridWidth;
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    dist -= b.GetVelocity();
                    if (dist <= 0)
                        return true;
                    return false;
                });
                t.AddOnExitAction(delegate {
                    // �˺�����ͼ�ָ�
                    b.SetDamage(ori_dmg * GetDamageRate(rate));
                    if(b.IsAlive())
                        b.animator.runtimeAnimatorController = Bullet_RunArray[mShape];
                });
                b.AddTask(t);
            }
            else
            {
                b.SetDamage(ori_dmg * GetDamageRate(rate)); // �����˺�
                b.animator.runtimeAnimatorController = Bullet_RunArray[mShape];
            }
            b.SetVelocity(b.GetVelocity() * 1.5f); // ����
            b.AddTag(StringManager.BulletDamgeIncreasement); // Ϊ�ӵ������������ı�ǣ���ֹ��ι���
            b.SetHitSoundEffect("FireHit"+GameManager.Instance.rand.Next(0, 2)); // ������ЧΪ�𵯻���
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
