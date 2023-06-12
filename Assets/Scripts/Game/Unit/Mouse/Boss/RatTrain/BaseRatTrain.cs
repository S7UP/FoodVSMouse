using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 鼠国列车通用
/// </summary>
public class BaseRatTrain : BossUnit
{
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitedFunc = delegate { return false; };

    /// <summary>
    /// 路径点
    /// 两个点确定一条直线路径
    /// </summary>
    public struct RoutePoints
    {
        public Vector2 start;
        public Vector2 end;
        /// <summary>
        /// 获取角度
        /// </summary>
        /// <returns></returns>
        public Vector2 GetRotate()
        {
            return (end - start).normalized;
        }

        public float GetDistance()
        {
            return (end - start).magnitude;
        }
    }

    /// <summary>
    /// 车厢位置转移管理器
    /// </summary>
    private class RatTrainBodyTransferManager
    {
        public RoutePoints routePoints; // 转移后的下一个路径点
        public Queue<RatTrainComponent> ratTrainComponentQueue = new Queue<RatTrainComponent>(); // 待通过的车厢表
        public float distLeft; // 下一个车厢转移时还需要移动的距离

        public RatTrainBodyTransferManager(RoutePoints routePoints, List<RatTrainBody> list, float hscale)
        {
            this.routePoints = routePoints;
            foreach (var item in list)
            {
                ratTrainComponentQueue.Enqueue(item);
            }
            distLeft = 0.5f*headLength + 0.5f*length*hscale;
        }

        public void Update(float v, float hscale)
        {
            distLeft -= v;
            if (distLeft <= 0)
            {
                // 转移下一个车厢
                if (ratTrainComponentQueue.Count > 0)
                {
                    RoutePoints r = new RoutePoints { start = routePoints.start + (-distLeft)* routePoints.GetRotate(), end = routePoints.end };
                    ratTrainComponentQueue.Dequeue().AddRoute(r);
                }
                distLeft += length * hscale;
            }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return ratTrainComponentQueue.Count <= 0;
        }
    }

    // 确定四个边界
    public static float left = MapManager.GetColumnX(-1);
    public static float right = MapManager.GetColumnX(9);
    public static float up = MapManager.GetRowY(-2);
    public static float bottom = MapManager.GetRowY(8);

    public static float length = 1.17f; // 列车长度
    public static float headLength = 0.83f; // 头长度
    protected float hscale; // 列车横向缩放

    private RatTrainHead head; // 车头
    private List<RatTrainBody> bodyList = new List<RatTrainBody>(); // 车身
    private Queue<RatTrainBodyTransferManager> transferQueue = new Queue<RatTrainBodyTransferManager>(); // 转移队列
    protected bool isMoveToDestination; // 当前列车移动是否抵达终点

    public override void MInit()
    {
        head = null;
        bodyList.Clear();
        transferQueue.Clear();
        isMoveToDestination = false;
        hscale = 1.0f; // 默认为1
        base.MInit();
        // 取消判定且没有贴图
        CloseCollision();
        GetSpriteRenderer().enabled = false;
        
        // 自身实际坐标固定在0
        transform.position = Vector3.zero;

        // 第一帧强制设置所有车厢生命值与灰烬抗生同步自身
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            foreach (var comp in GetAllRatTrainComponent())
            {
                comp.SetGetBurnRateFunc(delegate { return mBurnRate; } );
                comp.SetMaxHpAndCurrentHp(mMaxHp);
            }
            return true;
        });
        AddTask(t);
    }

    /// <summary>
    /// 创建车头与车身
    /// </summary>
    public void CreateHeadAndBody(int bodyCount)
    {
        // 车头
        head = RatTrainHead.GetInstance();
        head.SetMaster(this);
        head.SetDmgRate(1.0f, 1.0f);
        GameController.Instance.AddMouseUnit(head);
        // 车身
        for (int i = 0; i < bodyCount; i++)
        {
            RatTrainBody b = RatTrainBody.GetInstance(i);
            b.SetMaster(this);
            b.SetDmgRate(1.0f, 1.0f);
            b.transform.localScale = new Vector2(hscale, 1);
            bodyList.Add(b);
            GameController.Instance.AddMouseUnit(b);
        }
    }

    /// <summary>
    /// 设置所有的车组件不可被选为攻击目标和被击中
    /// </summary>
    public void SetAllComponentNoBeSelectedAsTargetAndHited()
    {
        foreach (var u in GetAllRatTrainComponent())
        {
            u.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            u.AddCanHitFunc(noHitedFunc);
        }
    }

    /// <summary>
    /// 取消设置所有的车组件不可被选为攻击目标和被击中
    /// </summary>
    public void CancelSetAllComponentNoBeSelectedAsTargetAndHited()
    {
        foreach (var u in GetAllRatTrainComponent())
        {
            u.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            u.RemoveCanHitFunc(noHitedFunc);
        }
    }

    public override void OnIdleStateEnter()
    {
        //foreach (var item in GetAllRatTrainComponent())
        //{
        //    item.SetActionState(new IdleState(item));
        //}

    }

    public override void OnIdleState()
    {

    }

    public override void OnMoveStateEnter()
    {
        //foreach (var item in GetAllRatTrainComponent())
        //{
        //    item.SetActionState(new MoveState(item));
        //}
    }

    public override void OnMoveState()
    {
        // 更新转移车厢管理器的队列
        int c = 0;
        foreach (var item in transferQueue)
        {
            item.Update(GetMoveSpeed(), hscale);
            if (item.IsEmpty())
                c++;
        }
        for (int i = 0; i < c; i++)
        {
            transferQueue.Dequeue();
        }
    }

    /// <summary>
    /// 死亡时连带车厢一起死亡
    /// </summary>
    public override void OnDieStateEnter()
    {
        base.OnDieStateEnter();
        foreach (var item in GetAllRatTrainComponent())
        {
            item.ExecuteDeath();
        }
    }

    /// <summary>
    /// 将新的路径点信息存入车厢管理器队列，以便分配给后续车厢路线（触发时机为车头已转移到下一个路径点后，即出队列后，因此要取当前路径）
    /// </summary>
    public void AddNextRouteToManager()
    {
        // 取下一个路径点
        transferQueue.Enqueue(new RatTrainBodyTransferManager(GetCurrentRoute(), GetBodyList(), hscale));
    }

    /// <summary>
    /// 清空转移队列
    /// </summary>
    public void ClearTransferQueue()
    {
        transferQueue.Clear();
    }

    /// <summary>
    /// 获取列车的全部组件
    /// </summary>
    /// <returns></returns>
    public List<RatTrainComponent> GetAllRatTrainComponent()
    {
        List<RatTrainComponent> list = new List<RatTrainComponent>();
        if(head!=null)
            list.Add(head);
        foreach (var item in bodyList)
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// 获取车头
    /// </summary>
    /// <returns></returns>
    public RatTrainHead GetHead()
    {
        return head;
    }

    /// <summary>
    /// 获取车身表
    /// </summary>
    /// <returns></returns>
    public List<RatTrainBody> GetBodyList()
    {
        return bodyList;
    }

    /// <summary>
    /// 获取车身
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public RatTrainBody GetBody(int index)
    {
        if (index < bodyList.Count)
            return bodyList[index];
        return null;
    }

    /// <summary>
    /// 获取车尾
    /// </summary>
    /// <returns></returns>
    public RatTrainBody GetLastBody()
    {
        return bodyList[bodyList.Count - 1];
    }

    /// <summary>
    /// 移除车尾
    /// </summary>
    public void RemoveTheEndOfBody()
    {
        if (bodyList.Count > 0)
            bodyList.Remove(bodyList[bodyList.Count-1]);
    }

    /// <summary>
    /// 获取当前路径队列
    /// </summary>
    /// <returns></returns>
    public Queue<RoutePoints> GetRouteQueue()
    {
        return head.GetRouteQueue();
    }

    /// <summary>
    /// 添加一个路径
    /// </summary>
    /// <param name="route"></param>
    public void AddRoute(RoutePoints route)
    {
        head.AddRoute(route);
    }

    /// <summary>
    /// 清空当前路径表
    /// </summary>
    public void ClearRouteList()
    {
        head.GetRouteQueue().Clear();
    }

    /// <summary>
    /// 通过格子横纵坐标值添加路径表
    /// </summary>
    public void AddRouteListByGridIndex(List<Vector2[]> list)
    {
        foreach (var item in list)
        {
            Vector2 vstart = item[0];
            Vector2 vend = item[1];
            AddRoute(new BaseRatTrain.RoutePoints()
            {
                start = MapManager.GetGridLocalPosition(vstart.x, vstart.y),
                end = MapManager.GetGridLocalPosition(vend.x, vend.y),
            });
        }
    }

    /// <summary>
    /// 获取车头当前行进路径点
    /// </summary>
    /// <returns></returns>
    public RoutePoints GetCurrentRoute()
    {
        return head.GetCurrentRoute();
    }

    /// <summary>
    /// 标记已抵达终点
    /// </summary>
    public void MarkMoveToDestination()
    {
        isMoveToDestination = true;
    }

    public override float GetMoveSpeed()
    {
        if (mCurrentActionState is IdleState)
            return 0;
        else
            return base.GetMoveSpeed();
    }

    /// <summary>
    /// 获取车头到第一节车身的距离
    /// </summary>
    /// <returns></returns>
    public float GetHeadToBodyLength()
    {
        return 0.5f * headLength + 0.5f * length * hscale;
    }

    public float GetBodyLength()
    {
        return length * hscale;
    }
}
