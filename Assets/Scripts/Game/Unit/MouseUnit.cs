using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{
    public float mBaseMoveSpeed; // 基础移动速度
    public float mCurrentMoveSpeed; // 当前移动速度

    public MouseUnit(GameObject gameObject):base(gameObject)
    {
    }

    public override void Init()
    {
        base.Init();

        mBaseMoveSpeed = 0.5f; // 移动速度值为格/秒，默认设为0.5格/秒
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // 移动状态test
        SetActionState(new MouseMoveState(this));
    }
}
