using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    protected bool mAttackFlag; // ������һ�ι����ܷ�������flag

    protected Animator animator;
    protected float attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���

    public override void Awake()
    {
        base.Awake();
        mAttackFlag = true;
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackPercent = 0.65f;
    }

    // �ж��ܲ��ܿ����ˣ��޶���OnAttackState()����ã�����������д
    protected virtual bool CanAttack()
    {
        // ��ȡAttack�Ķ�����Ϣ��ʹ���˺��ж��붯����ʾ������ͬ��
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // ���normalizedTime��С�����Խ��Ʊ�ʾһ���������Ž��ȵİٷֱȣ���λ������Ա�ʾ��ѭ���Ĵ�����
        int c = Mathf.FloorToInt(info.normalizedTime);

        // ���������ڵ�һ��ʱ�����˺��ж�
        float percent = info.normalizedTime - c;
        if (percent >= attackPercent && mAttackFlag)
        {
            return true;
        }
        return false;
    }

    // ����������ʱ�������ӵ�������ݣ���������Ҫ������д�Դﵽ��̬�ԡ�
    protected virtual void Attack()
    {
        // Debug.Log("��Ƭ�����ˣ�");
        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameController.Instance.CreateBullet(transform.position + Vector3.right * 0.5f * j + Vector3.up * 0.7f * i);
            }
        }
    }

    // 
    public override void MInit()
    {
        base.MInit();
        SetActionState(new IdleState(this));
        gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animator/AnimatorController/Food/7/2");
    }

    // �ڴ���״̬ʱÿ֡Ҫ������
    public override void OnIdleState()
    {
        // Ĭ��Ϊ����������Ϊ��ʱ���ܷ��𹥻�
        if (mAttackCDLeft <= 0)
        {
            SetActionState(new AttackState(this));
            mAttackCDLeft += mAttackCD;
        }
    }

    // �ڹ���״̬ʱÿ֡Ҫ������
    public override void OnAttackState()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer<=0)
        {
            return;
        }
        // ��ȡAttack�Ķ�����Ϣ��ʹ���˺��ж��붯����ʾ������ͬ��
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (CanAttack())
        {
            Attack();
            mAttackFlag = false;
        }
        else if (info.normalizedTime >= 1.0f) // ��������������һ�κ�תΪ����״̬
        {
            mAttackFlag = true;
            SetActionState(new IdleState(this));
        }
    }
}