using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// ����г�ͨ��
/// </summary>
public class BaseRatTrain : BossUnit
{
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitedFunc = delegate { return false; };

    /// <summary>
    /// ·����
    /// ������ȷ��һ��ֱ��·��
    /// </summary>
    public struct RoutePoints
    {
        public Vector2 start;
        public Vector2 end;
        /// <summary>
        /// ��ȡ�Ƕ�
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
    /// ����λ��ת�ƹ�����
    /// </summary>
    private class RatTrainBodyTransferManager
    {
        public RoutePoints routePoints; // ת�ƺ����һ��·����
        public Queue<RatTrainComponent> ratTrainComponentQueue = new Queue<RatTrainComponent>(); // ��ͨ���ĳ����
        public float distLeft; // ��һ������ת��ʱ����Ҫ�ƶ��ľ���

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
                // ת����һ������
                if (ratTrainComponentQueue.Count > 0)
                {
                    RoutePoints r = new RoutePoints { start = routePoints.start + (-distLeft)* routePoints.GetRotate(), end = routePoints.end };
                    ratTrainComponentQueue.Dequeue().AddRoute(r);
                }
                distLeft += length * hscale;
            }
        }

        /// <summary>
        /// �Ƿ�Ϊ��
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return ratTrainComponentQueue.Count <= 0;
        }
    }

    // ȷ���ĸ��߽�
    public static float left = MapManager.GetColumnX(-1);
    public static float right = MapManager.GetColumnX(9);
    public static float up = MapManager.GetRowY(-2);
    public static float bottom = MapManager.GetRowY(8);

    public static float length = 1.17f; // �г�����
    public static float headLength = 0.83f; // ͷ����
    protected float hscale; // �г���������

    private RatTrainHead head; // ��ͷ
    private List<RatTrainBody> bodyList = new List<RatTrainBody>(); // ����
    private Queue<RatTrainBodyTransferManager> transferQueue = new Queue<RatTrainBodyTransferManager>(); // ת�ƶ���
    protected bool isMoveToDestination; // ��ǰ�г��ƶ��Ƿ�ִ��յ�

    public override void MInit()
    {
        head = null;
        bodyList.Clear();
        transferQueue.Clear();
        isMoveToDestination = false;
        hscale = 1.0f; // Ĭ��Ϊ1
        base.MInit();
        // ȡ���ж���û����ͼ
        CloseCollision();
        GetSpriteRenderer().enabled = false;
        
        // ����ʵ������̶���0
        transform.position = Vector3.zero;

        // ��һ֡ǿ���������г�������ֵ��ҽ�����ͬ������
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
    /// ������ͷ�복��
    /// </summary>
    public void CreateHeadAndBody(int bodyCount)
    {
        // ��ͷ
        head = RatTrainHead.GetInstance();
        head.SetMaster(this);
        head.SetDmgRate(1.0f, 1.0f);
        GameController.Instance.AddMouseUnit(head);
        // ����
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
    /// �������еĳ�������ɱ�ѡΪ����Ŀ��ͱ�����
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
    /// ȡ���������еĳ�������ɱ�ѡΪ����Ŀ��ͱ�����
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
        // ����ת�Ƴ���������Ķ���
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
    /// ����ʱ��������һ������
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
    /// ���µ�·������Ϣ���복����������У��Ա�������������·�ߣ�����ʱ��Ϊ��ͷ��ת�Ƶ���һ��·����󣬼������к����Ҫȡ��ǰ·����
    /// </summary>
    public void AddNextRouteToManager()
    {
        // ȡ��һ��·����
        transferQueue.Enqueue(new RatTrainBodyTransferManager(GetCurrentRoute(), GetBodyList(), hscale));
    }

    /// <summary>
    /// ���ת�ƶ���
    /// </summary>
    public void ClearTransferQueue()
    {
        transferQueue.Clear();
    }

    /// <summary>
    /// ��ȡ�г���ȫ�����
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
    /// ��ȡ��ͷ
    /// </summary>
    /// <returns></returns>
    public RatTrainHead GetHead()
    {
        return head;
    }

    /// <summary>
    /// ��ȡ�����
    /// </summary>
    /// <returns></returns>
    public List<RatTrainBody> GetBodyList()
    {
        return bodyList;
    }

    /// <summary>
    /// ��ȡ����
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
    /// ��ȡ��β
    /// </summary>
    /// <returns></returns>
    public RatTrainBody GetLastBody()
    {
        return bodyList[bodyList.Count - 1];
    }

    /// <summary>
    /// �Ƴ���β
    /// </summary>
    public void RemoveTheEndOfBody()
    {
        if (bodyList.Count > 0)
            bodyList.Remove(bodyList[bodyList.Count-1]);
    }

    /// <summary>
    /// ��ȡ��ǰ·������
    /// </summary>
    /// <returns></returns>
    public Queue<RoutePoints> GetRouteQueue()
    {
        return head.GetRouteQueue();
    }

    /// <summary>
    /// ���һ��·��
    /// </summary>
    /// <param name="route"></param>
    public void AddRoute(RoutePoints route)
    {
        head.AddRoute(route);
    }

    /// <summary>
    /// ��յ�ǰ·����
    /// </summary>
    public void ClearRouteList()
    {
        head.GetRouteQueue().Clear();
    }

    /// <summary>
    /// ͨ�����Ӻ�������ֵ���·����
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
    /// ��ȡ��ͷ��ǰ�н�·����
    /// </summary>
    /// <returns></returns>
    public RoutePoints GetCurrentRoute()
    {
        return head.GetCurrentRoute();
    }

    /// <summary>
    /// ����ѵִ��յ�
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
    /// ��ȡ��ͷ����һ�ڳ���ľ���
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
