using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitState : BaseBulletActionState
{
    private bool isFirstFrame; // 是否为切换时的第一帧 

    public BulletHitState(BaseBullet baseBullet):base(baseBullet)
    {
    }

    // 当进入时
    public override void OnEnter()
    {
        isFirstFrame = true;
        mBaseBullet.OnHitStateEnter();
    }

    // 实现动作状态
    public override void OnUpdate()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }

        mBaseBullet.OnHitState();
    }

    // 当退出时
    public override void OnExit()
    {
        mBaseBullet.OnHitStateExit();
    }
}

