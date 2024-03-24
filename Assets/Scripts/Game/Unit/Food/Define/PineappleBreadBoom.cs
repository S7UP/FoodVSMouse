using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ���ܱ�ը���
/// </summary>
public class PineappleBreadBoom : FoodUnit
{
    private int mHertIndex; // ���˽׶� 0������ 1��С�� 2������
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };
    private float dmgRecord; // ���յ�����Ч�˺�
    private float low_burn_rate = 0; // ��ͻҽ��˺�
    private float hightest_burn_rate = 0; // ��߻ҽ��˺�

    public override void MInit()
    {
        mHertIndex = 0;
        dmgRecord = 0;
        base.MInit();

        switch (mShape)
        {
            case 1:
                low_burn_rate = 0.5f;
                hightest_burn_rate = 2.0f;
                break;
            case 2:
                low_burn_rate = 0.5f;
                hightest_burn_rate = 2.5f;
                break;
            default:
                low_burn_rate = 0;
                hightest_burn_rate = 1.5f;
                break;
        }

        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, (action)=> { 
            UpdateHertMap(); 
            if(action is DamageAction)
            {
                DamageAction d = action as DamageAction;
                if(d.Creator != null)
                    dmgRecord += d.RealCauseValue;
            }
        });
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }

    /// <summary>
    /// ը�������Ч����������
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    public override void OnDieStateEnter()
    {
        base.OnDieStateEnter(); // �Ƴ�����
        animatorController.Play("Die");
    }

    /// <summary>
    /// ���ε�һ����������������ʳ����Ŀ��
    /// </summary>
    public override void DuringDeath()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            DeathEvent();
    }

    /// <summary>
    /// ������ըЧ��
    /// </summary>
    public override void AfterDeath()
    {
        ExecuteBoom();
    }

    ////////////////////////////////////////////////////////////////////////������˽�з���/////////////////////////////////////////////////////////////


    /// <summary>
    /// �����˻��߱�����ʱ�����µ�λ��ͼ״̬
    /// </summary>
    private void UpdateHertMap()
    {
        // Ҫ�����˵Ļ������˰�
        if (isDeathState)
            return;

        // �Ƿ�Ҫ�л���������flag
        bool flag = false;
        // �ָ�����һ��������ͼ���
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // ��һ��������ͼ�ļ��
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // ���л�֪ͨʱ���л�
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
        }
    }

    /// <summary>
    /// ��ը����
    /// </summary>
    private void ExecuteBoom()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Boom");
        // ԭ�ز���һ����ը��Ч
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
        // ԭ�ز���һ����ը�˺��ж�Ч��
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*Vector3.left*MapManager.gridWidth, 4, 3, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(this, u, Mathf.Min(hightest_burn_rate, Mathf.Max(low_burn_rate, dmgRecord/500)));
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
