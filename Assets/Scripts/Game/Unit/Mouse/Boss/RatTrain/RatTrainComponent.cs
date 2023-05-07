using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����г���������Ϊ��ͷ�ͳ���ĸ��ࣩ
/// </summary>
public abstract class RatTrainComponent : MouseModel
{
    private BaseRatTrain master; // �󶨵�BOSS����
    private float dmgRate; // �˺���������
    private float burnDmgRate; // �ҽ��˺���������
    private Queue<BaseRatTrain.RoutePoints> routeQueue = new Queue<BaseRatTrain.RoutePoints>(); // ·������
    private bool isDisappear; // �Ƿ���ʧ
    private bool isHide;
    protected int bodyLayerIndex; // λ�����˵ĵڼ���

    private BaseRatTrain.RoutePoints currentRoute; // ��ǰ����·��

    public override void MInit()
    {
        bodyLayerIndex = -1;
        master = null;
        dmgRate = 0;
        burnDmgRate = 0;
        routeQueue.Clear();
        currentRoute = new BaseRatTrain.RoutePoints();
        base.MInit();
        // ��ӵ�ͬ��BOSS�����߻���
        BossUnit.AddBossIgnoreDebuffEffect(this);
        canTriggerCat = false;
        canTriggerLoseWhenEnterLoseLine = false;
        SetIsMouseUnit(false);
        // ��ʼ���أ����ܻ��ж��Ҳ��ɱ�ѡȡ
        isDisappear = false;
        isHide = false;
        Hide(true);
        isBoss = true; // ����Ҳ��BOSS�ж�
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if(master != null && master.IsAlive())
            mCurrentHp = master.mCurrentHp;
    }

    public override void LoadSkillAbility()
    {

    }

    /// <summary>
    /// ��Ϊ��ʧ̬
    /// </summary>
    public void Hide(bool disappear)
    {
        if (isHide == disappear)
            return;
        isHide = disappear;
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

    /// <summary>
    /// Ҫ�ڷ�Χ�ڲ��ܱ�ѡΪ����Ŀ��
    /// </summary>
    /// <param name="otherUnit"></param>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        //return transform.position.x >= MapManager.GetColumnX(10) && transform.position.x <= MapManager.GetColumnX(-2) && transform.position.y >= MapManager.GetRowY(7) && transform.position.y <= MapManager.GetRowY(-1) && base.CanBeSelectedAsTarget(otherUnit);
        return base.CanBeSelectedAsTarget(otherUnit);
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
                    moveRotate = currentRoute.GetRotate();
                    transform.right = moveRotate*-1; // �ı䳯��
                    if (bodyLayerIndex >= 1 && master.GetBody(bodyLayerIndex - 1) != null)
                        transform.position = (Vector2)master.GetBody(bodyLayerIndex - 1).transform.position - moveRotate * master.GetBodyLength(); // ��ʼ��Ϊ��һ�ڵ�β�ͼ�ȥ����*������
                    else if (bodyLayerIndex == 0 && master.GetHead() != null)
                        transform.position = (Vector2)master.GetHead().transform.position - moveRotate * master.GetHeadToBodyLength();
                    else
                        transform.position = currentRoute.start;
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

    public override float OnDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnDamage(dmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override float OnAoeDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnAoeDamage(dmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override float OnDamgeIgnoreShield(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnDamgeIgnoreShield(dmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override float OnRealDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnRealDamage(dmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override float OnBurnDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnBombBurnDamage(burnDmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override float OnBombBurnDamage(float dmg)
    {
        // boss����ȡ������
        if (master != null)
        {
            dmg = master.OnBombBurnDamage(burnDmgRate * dmg);
            master.UpdateHertMap();
            return dmg;
        }
        return 0;
    }

    public override void AddRecordDamage(float value)
    {
        if(master != null)
        {
            master.AddRecordDamage(dmgRate * value);
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
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// �Ƿ�����ʧ̬
    /// </summary>
    /// <returns></returns>
    public bool IsDisappear()
    {
        return isDisappear;
    }

    /// <summary>
    /// �Ƿ��Ʋأ���������
    /// </summary>
    /// <returns></returns>
    public bool IsHide()
    {
        return isHide;
    }

    /// <summary>
    /// �Ƿ���·��û����
    /// </summary>
    /// <returns></returns>
    public bool HasRoute()
    {
        return GetRouteQueue().Count > 0;
    }

    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, 7, 2, bodyLayerIndex);
    }
}
