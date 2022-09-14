using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����г�ͨ��
/// </summary>
public class BaseRatTrain : BossUnit
{
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
            return (end-start).normalized;
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

        public RatTrainBodyTransferManager(RoutePoints routePoints, List<RatTrainBody> list)
        {
            this.routePoints = routePoints;
            foreach (var item in list)
            {
                ratTrainComponentQueue.Enqueue(item);
            }
            distLeft = headToBodyDist;
        }

        public void Update(float v)
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
                distLeft = length + distLeft;
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
    public static float up = MapManager.GetRowY(-1);
    public static float bottom = MapManager.GetRowY(7);

    public static float length = 1.17f; // �г�����
    public static float headToBodyDist = 1.0f; // ͷ����һ�ڳ���ľ���

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
        base.MInit();
        // ȡ���ж���û����ͼ
        CloseCollision();
        GetSpriteRenderer().enabled = false;
        
        // ����ʵ������̶���0
        transform.position = Vector3.zero;
    }

    /// <summary>
    /// ������ͷ�복��
    /// </summary>
    public void CreateHeadAndBody(int bodyCount)
    {
        // ��ͷ
        head = GameController.Instance.CreateMouseUnit(0, new BaseEnemyGroup.EnemyInfo() { type = 30, shape = mShape }).GetComponent<RatTrainHead>();
        head.SetMaster(this);
        head.SetDmgRate(1.0f, 1.0f);
        // ����
        for (int i = 0; i < bodyCount; i++)
        {
            RatTrainBody b = GameController.Instance.CreateMouseUnit(0, new BaseEnemyGroup.EnemyInfo() { type = 31, shape = mShape }).GetComponent<RatTrainBody>();
            b.SetMaster(this);
            b.SetDmgRate(1.0f, 1.0f);
            bodyList.Add(b);
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
            item.Update(GetMoveSpeed());
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

    public override void OnBurnStateEnter()
    {
        base.OnBurnStateEnter();
        foreach (var item in GetAllRatTrainComponent())
        {
            item.ExecuteBurn();
        }
    }

    /// <summary>
    /// ���µ�·������Ϣ���복����������У��Ա�������������·�ߣ�����ʱ��Ϊ��ͷ��ת�Ƶ���һ��·����󣬼������к����Ҫȡ��ǰ·����
    /// </summary>
    public void AddNextRouteToManager()
    {
        // ȡ��һ��·����
        transferQueue.Enqueue(new RatTrainBodyTransferManager(GetCurrentRoute(), GetBodyList()));
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
}
