using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitState : BaseBulletActionState
{
    private bool isFirstFrame; // �Ƿ�Ϊ�л�ʱ�ĵ�һ֡ 

    public BulletHitState(BaseBullet baseBullet):base(baseBullet)
    {
    }

    // ������ʱ
    public override void OnEnter()
    {
        isFirstFrame = true;
        mBaseBullet.OnHitStateEnter();
    }

    // ʵ�ֶ���״̬
    public override void OnUpdate()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }

        mBaseBullet.OnHitState();
    }

    // ���˳�ʱ
    public override void OnExit()
    {
        mBaseBullet.OnHitStateExit();
    }
}

