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
    private float pctAddValue; // �����˺��ٷֱ�

    public override void MInit()
    {
        mHertIndex = 0;
        pctAddValue = 0;
        base.MInit();

        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); AddPctAttackWhenHited(); });
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }



    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    public override void OnDieStateEnter()
    {
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
    /// ������������ձ����˺��ӳ�
    /// </summary>
    private void AddPctAttackWhenHited()
    {
        pctAddValue += 10;
    }

    /// <summary>
    /// ��ը����
    /// </summary>
    private void ExecuteBoom()
    {
        // ������Ǳ������ģ���������Ƴ������򲻻��б����˺��ӳ�
        // ����ת���ѱ����˺��ӳɽ�����ԭ����50%����û��
        if (mCurrentHp > 0)
        {
            if (mShape < 2)
                pctAddValue = 0;
            else
                pctAddValue /= 2;
        }

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
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            Debug.Log("��ǰ��ը������˺���" + mCurrentAttack * (1 + pctAddValue / 100));
            bombEffect.Init(this, mCurrentAttack*(1 + pctAddValue/100), GetRowIndex(), 4, 3, -0.5f, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }
}
