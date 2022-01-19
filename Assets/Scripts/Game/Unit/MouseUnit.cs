using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{

    public float mBaseMoveSpeed; // �����ƶ��ٶ�
    public float mCurrentMoveSpeed; // ��ǰ�ƶ��ٶ�

    public Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
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

    public override void MInit()
    {
        base.MInit();

        mBaseMoveSpeed = 0.5f; // �ƶ��ٶ�ֵΪ��/�룬Ĭ����Ϊ0.5��/��
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // �ƶ�״̬test
        SetActionState(new MouseMoveState(this));
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        
        if (collision.tag.Equals("Food"))
        {
            // ����ʳ�����ˣ�
            Debug.Log("�л�Ϊ����ģʽ��");
            MouseAttackState state = new MouseAttackState(this);
            SetActionState(state);
            state.mTargetUnit = collision.GetComponent<BaseUnit>();
        }else if (collision.tag.Equals("Bullet"))
        {
            // ���ӵ�������
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            bullet.TakeDamage(this);
        }
    }
}
