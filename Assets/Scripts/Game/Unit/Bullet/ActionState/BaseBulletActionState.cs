using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBulletActionState : IBaseActionState
{
    public BaseBullet mBaseBullet;

    public BaseBulletActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // ������ʱ
    public virtual void OnEnter()
    {

    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {

    }
}
