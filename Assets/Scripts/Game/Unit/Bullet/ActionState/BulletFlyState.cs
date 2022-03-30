using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFlyState : BaseBulletActionState
{
    public BulletFlyState(BaseBullet baseBullet):base(baseBullet)
    {
    }

    // ������ʱ
    public override void OnEnter()
    {
        mBaseBullet.OnFlyStateEnter();
    }

    // ʵ�ֶ���״̬
    public override void OnUpdate()
    {
        mBaseBullet.OnFlyState();
    }

    // ���˳�ʱ
    public override void OnExit()
    {
        mBaseBullet.OnFlyStateExit();
    }
}
