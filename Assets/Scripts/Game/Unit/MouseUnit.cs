using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{
    public float mBaseMoveSpeed; // �����ƶ��ٶ�
    public float mCurrentMoveSpeed; // ��ǰ�ƶ��ٶ�

    public MouseUnit(GameObject gameObject):base(gameObject)
    {
    }

    public override void Init()
    {
        base.Init();

        mBaseMoveSpeed = 0.5f; // �ƶ��ٶ�ֵΪ��/�룬Ĭ����Ϊ0.5��/��
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // �ƶ�״̬test
        SetActionState(new MouseMoveState(this));
    }
}
