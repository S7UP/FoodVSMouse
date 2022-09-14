using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 鼠国列车部件（作为车头和车身的父类）
/// </summary>
public class RatTrainComponent : MouseUnit
{
    private BaseRatTrain master; // 绑定的BOSS对象
    private float dmgRate; // 伤害传导倍率
    private float burnDmgRate; // 灰烬伤害传导倍率
    private Queue<BaseRatTrain.RoutePoints> routeQueue = new Queue<BaseRatTrain.RoutePoints>(); // 路径队列
    private bool isDisappear; // 是否消失

    private BaseRatTrain.RoutePoints currentRoute; // 当前所在路径

    public override void MInit()
    {
        master = null;
        dmgRate = 0;
        burnDmgRate = 0;
        routeQueue.Clear();
        isDisappear = true;
        currentRoute = new BaseRatTrain.RoutePoints();
        base.MInit();
        // 添加等同于BOSS的免疫机制
        BossUnit.AddBossIgnoreDebuffEffect(this);
        // 初始隐藏，无受击判定且不可被选取
        isDisappear = false;
        Hide(true);
        isBoss = true; // 车厢也算BOSS判定
    }

    /// <summary>
    /// 设为消失态
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
        base.OnMoveState(); // 位置移动更新
        bool flag = false;
        // 越界检测
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
        // 如果越界了，则切换为进入消失态的转换态
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
            // 如果已是消失态，则为切换成出现态的转换过程
            animatorController.Play("Appear");
            OnTurnToAppear();
        }
        else
        {
            // 如果不是消失态进入此状态，则为切换成消失态的转换过程
            animatorController.Play("Disappear");
        }
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            // 当播放完一次动画后
            if (isDisappear)
            {
                // 如果已是消失态，则为切换为出现态，并且开始下一轮移动
                isDisappear = false;
                SetActionState(new MoveState(this));
            }
            else
            {
                // 如果不是消失态进入此状态，则为切换成消失态的转换过程
                if (routeQueue.Count > 0)
                {
                    Hide(true);
                    isDisappear = true;
                    currentRoute = routeQueue.Dequeue();
                    transform.position = currentRoute.start;
                    moveRotate = currentRoute.GetRotate();
                    transform.right = moveRotate*-1; // 改变朝向
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
            base.OnMoveState(); // 位置移动更新
        }
    }

    /// <summary>
    /// 转化为出现态时的额外事件（这部分由车头重写）
    /// </summary>
    public virtual void OnTurnToAppear()
    {

    }


    /// <summary>
    /// 当越界时
    /// </summary>
    public virtual void OnOutOfBound()
    {
        // 默认情况下发生传送消失效果
        SetActionState(new TransitionState(this));
    }

    /// <summary>
    /// 获取当前路径队列
    /// </summary>
    public Queue<BaseRatTrain.RoutePoints> GetRouteQueue()
    {
        return routeQueue;
    }

    /// <summary>
    /// 添加一个路径
    /// </summary>
    public void AddRoute(BaseRatTrain.RoutePoints route)
    {
        routeQueue.Enqueue(route);
    }

    /// <summary>
    /// 获取当前行进的路径
    /// </summary>
    /// <returns></returns>
    public BaseRatTrain.RoutePoints GetCurrentRoute()
    {
        return currentRoute;
    }

    /// <summary>
    /// 获取当前移动速度
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
        // boss本体取代受伤
        if (master != null)
        {
            master.OnDamage(dmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnDamgeIgnoreShield(float dmg)
    {
        // boss本体取代受伤
        if (master != null)
        {
            master.OnDamgeIgnoreShield(dmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnBurnDamage(float dmg)
    {
        // boss本体取代受伤
        if (master != null)
        {
            master.OnBurnDamage(burnDmgRate * dmg);
            master.UpdateHertMap();
        }
    }

    public override void OnCure(float cure)
    {
        // 然而什么也不会发生，这意味着教皇的回复效果并不能通过车厢传导给BOSS
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
    /// 在与猫判定时是否能触发猫
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerCat()
    {
        return false;
    }

    /// <summary>
    /// 在越过失败判定线后是否会触发游戏失败判定
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }


    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(1.5f * MapManager.gridWidth, 0.75f * MapManager.gridHeight);
    }
}
