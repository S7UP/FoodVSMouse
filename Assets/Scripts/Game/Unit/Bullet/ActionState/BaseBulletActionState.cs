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

    // 当进入时
    public virtual void OnEnter()
    {

    }

    // 当退出时
    public virtual void OnExit()
    {

    }

    // 实现动作状态
    public virtual void OnUpdate()
    {

    }
}
