using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����г���������Ϊ��ͷ�ͳ���ĸ��ࣩ
/// </summary>
public class RatTrainComponent : MouseUnit
{
    private BaseRatTrain master; // �󶨵�BOSS����
    private float dmgRate; // �˺���������
    private float burnDmgRate; // �ҽ��˺���������
    private Queue<BaseRatTrain.RoutePoints> routeQueue = new Queue<BaseRatTrain.RoutePoints>(); // ·������
    private bool isDisappear; // �Ƿ���ʧ

    private BaseRatTrain.RoutePoints currentRoute; // ��ǰ����·��

    public override void MInit()
    {
        master = null;
        dmgRate = 0;
        burnDmgRate = 0;
        routeQueue.Clear();
        isDisappear = true;
        currentRoute = new BaseRatTrain.RoutePoints();
        base.MInit();
        // ��ӵ�ͬ��BOSS�����߻���
        BossUnit.AddBossIgnoreDebuffEffect(this);
        // ��ʼ���أ����ܻ��ж��Ҳ��ɱ�ѡȡ
        isDisappear = false;
        Hide(true);
        isBoss = true; // ����Ҳ��BOSS�ж�
    }

    /// <summary>
    /// ��Ϊ��ʧ̬
    /// </summary>
    public void Hide(bool disappear)
    {
        if (disappear)
        {
            CloseCollision();
            GetSpriteRenderer().enabled = false;
        }
        else
        {
            OpenCollision();
            GetSpriteRenderer().enabled = true;
        }
    }


    public override void OnMoveState()
    {
        base.OnMoveState(); // λ���ƶ�����
        bool flag = false;
        // Խ����
        if (currentRoute.GetRotate().Equals(Vector2.zero))
        {
            flag = true;
        }
        else if(moveRotate.x > 0)
        {
            if (transform.position.x >= currentRoute.end.x)
                flag = true;
        }
        else if(moveRotate.x < 0)
        {
            if (transform.position.x <= currentRoute.end.x)
                flag = true;
        }
        else
        {
            if(moveRotate.y>0)
                if (transform.position.y >= currentRoute.end.y)
                    flag = true;
            else
                if (transform.position.y <= currentRoute.end.y)
                    flag = true;
        }
        // ���Խ���ˣ����л�Ϊ������ʧ̬��ת��̬
        if (flag)
        {
            OnOutOfBound();
        }
    }


    public override void OnTransitionStateEnter()
    {
        if (isDisappear)
        {
            Hide(false);
            // ���������ʧ̬����Ϊ�л��ɳ���̬��ת������
            animatorController.Play("Appear");
            OnTurnToAppear();
        }
        else
        {
            // ���������ʧ̬�����״̬����Ϊ�л�����ʧ̬��ת������
            animatorController.Play("Disappear");
        }
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            // ��������һ�ζ�����
            if (isDisappear)
            {
                // ���������ʧ̬����Ϊ�л�Ϊ����̬�����ҿ�ʼ��һ���ƶ�
                isDisappear = false;
                SetActionState(new MoveState(this));
            }
            else
            {
                // ���������ʧ̬�����״̬����Ϊ�л�����ʧ̬��ת������
                if (routeQueue.Count > 0)
                {
                    Hide(true);
                    isDisappear = true;
                    currentRoute = routeQueue.Dequeue();
                    transform.position = currentRoute.start;
                    moveRotate = currentRoute.GetRotate();
                    transform.right = moveRotate*-1; // �ı䳯��
                    SetActionState(new TransitionState(this));
                }
                else
                {
                    Hide(true);
                }
            }
        }
        else
        {
            base.OnMoveState(); // λ���ƶ�����
        }
    }

    /// <summary>
    /// ת��Ϊ����̬ʱ�Ķ����¼����ⲿ���ɳ�ͷ��д��
    /// </summary>
    public virtual void OnTurnToAppear()
    {

    }


    /// <summary>
    /// ��Խ��ʱ
    /// </summary>
    public virtual void OnOutOfBound()
    {
        // Ĭ������·���������ʧЧ��
        SetActionState(new TransitionState(this));
    }

    /// <summary>
    /// ��ȡ��ǰ·������
    /// </summary>
    public Queue<BaseRatTrain.RoutePoints> GetRouteQueue()
    {
        return routeQueue;
    }

    /// <summary>
    /// ���һ��·��
    /// </summary>
    public void AddRoute(BaseRatTrain.RoutePoints route)
    {
        routeQueue.Enqueue(route);
    }

    /// <summary>
    /// ��ȡ��ǰ�н���·��
    /// </summary>
    /// <returns></returns>
    public BaseRatTrain.RoutePoints GetCurrentRoute()
    {
        return currentRoute;
    }

    /// <summary>
    /// ��ȡ��ǰ�ƶ��ٶ�
    /// </summary>
    /// <returns></returns>
    public override float GetMoveSpeed()
    {
        if(master!=null)
            return master.GetMoveSpeed();
        return 0;
    }

    public override void OnDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            master.OnDamage(dmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnDamgeIgnoreShield(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            master.OnDamgeIgnoreShield(dmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnBurnDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            master.OnBurnDamage(burnDmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnCure(float cure)
    {
        // Ȼ��ʲôҲ���ᷢ��������ζ�Ž̻ʵĻظ�Ч��������ͨ�����ᴫ����BOSS
    }

    public BaseRatTrain GetMaster()
    {
        return master;
    }

    public void SetMaster(BaseRatTrain master)
    {
        this.master = master;
    }

    public float GetDmgRate()
    {
        return dmgRate;
    }

    public void SetDmgRate(float dmgrate, float burnrate)
    {
        dmgRate = dmgrate;
        burnDmgRate = burnrate;
    }

    public override bool IsOutOfBound()
    {
        return false;
    }

    /// <summary>
    /// ����è�ж�ʱ�Ƿ��ܴ���è
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerCat()
    {
        return false;
    }

    /// <summary>
    /// ��Խ��ʧ���ж��ߺ��Ƿ�ᴥ����Ϸʧ���ж�
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }


    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(1.5f * MapManager.gridWidth, 0.75f * MapManager.gridHeight);
    }
}
