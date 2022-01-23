using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{
    protected Animator animator;
    protected float attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���

    protected bool mAttackFlag; // ������һ�ι����ܷ�������flag

    public float mBaseMoveSpeed; // �����ƶ��ٶ�
    public float mCurrentMoveSpeed; // ��ǰ�ƶ��ٶ�

    // �������
    protected bool isBlock; // �Ƿ��赲
    protected BaseUnit mBlockUnit; // �赲��

    public Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackPercent = 0.60f;
    }


    // ���ٴ󲿷�����Ӧ����һ����ֱ�ӹ������ֶ�
    public void TakeDamage(BaseUnit unit)
    {
        unit.OnDamage(mCurrentAttack);
    }


    // ע��һ��Collider2D��Ӧ��ֱ��ʹ��Transform������offset�������ƶ���������Ӧ��ʹ��Rigidbody2D���ƶ�����֮��������õ���õı��ֺ���ȷ����ײ��⡣
    // ��ˣ����������ƶ���Ӧ���������transform,�����������rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    // �ж��Ƿ��ֹ���Ŀ��
    protected virtual void SearchTarget()
    {
        // ���赲��
        if (isBlock)
        {
            // �赲�����Ƿ���Ч
            if (mBlockUnit.isGameObjectValid)
            {
                SetActionState(new AttackState(this));
            }
            else
            {
                isBlock = false;
            }
        }
    } 

    // �ж��ܲ��ܹ����ˣ��޶���OnAttackState()����ã�����������д
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

    // ����������ʱ�����ľ��������
    protected virtual void Attack()
    {
        if (mBlockUnit != null && mBlockUnit.isGameObjectValid)
        {
            TakeDamage(mBlockUnit);
        }
        Debug.Log("����˺��ж��ˣ���");
        mAttackFlag = false;
    }


    // ����Ϊ IGameControllerMember �ӿڷ�����ʵ��
    public override void MInit()
    {
        base.MInit();

        mBaseMoveSpeed = 0.5f; // �ƶ��ٶ�ֵΪ��/�룬Ĭ����Ϊ0.5��/��
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // �ƶ�״̬test
        SetActionState(new MoveState(this));
    }

    // ����Ϊ IBaseStateImplementor �ӿڵķ���ʵ��
    public override void OnIdleState()
    {
        // �����߼�
        SearchTarget();
    }

    public override void OnMoveState()
    {
        // �ƶ�����
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.005f);
        // �����߼�
        SearchTarget();
    }

    public override void OnAttackState()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
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
            SetActionState(new IdleState(this));
        }
    }



    // rigibody���

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        
        if (collision.tag.Equals("Food"))
        {
            // ����ʳ�����ˣ�
            Debug.Log("��⵽���赲��");
            //MouseAttackState state = new MouseAttackState(this);
            //SetActionState(state);
            //state.mTargetUnit = collision.GetComponent<BaseUnit>();
            isBlock = true;
            mBlockUnit = collision.GetComponent<BaseUnit>();
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // ���ӵ�������
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            bullet.TakeDamage(this);
        }
    }
}
