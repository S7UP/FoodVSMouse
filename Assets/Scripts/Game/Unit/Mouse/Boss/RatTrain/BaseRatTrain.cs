using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
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
    public class RoutePoints
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

    private class ComponetPosInfo
    {
        public RatTrainComponent component;
        public float move_dist = 0;
        public bool isPlayingMoveToNextPointTask = false; // �Ƿ��ڲ���ȥ��һ��·�������ʧ����
        public bool canSkipToNextPoint = false; // ����������һ������

        public ComponetPosInfo(RatTrainComponent component, float move_dist)
        {
            this.component = component;
            this.move_dist = move_dist;
        }
    }

    private Dictionary<RoutePoints, List<ComponetPosInfo>> route_componetPosInfoDict = new Dictionary<RoutePoints, List<ComponetPosInfo>>(); // ·����-��ǰ�ڸ�·����ĳ���
    private RoutePoints lastRoutePoints; // ���һ��·��

    // ȷ���ĸ��߽�
    public static float left = MapManager.GetColumnX(-1);
    public static float right = MapManager.GetColumnX(9);
    public static float up = MapManager.GetRowY(-2);
    public static float bottom = MapManager.GetRowY(8);

    public static float length = 1.17117117f; // �г�����
    public static float headLength = 0.83f; // ͷ����
    protected float hscale; // �г���������

    private RatTrainHead head; // ��ͷ
    private List<RatTrainBody> bodyList = new List<RatTrainBody>(); // ����
    private List<ComponetPosInfo> infoList = new List<ComponetPosInfo>(); // ������Ϣ��

    public bool isMoveToDestination;

    public override void MInit()
    {
        head = null;
        bodyList.Clear();
        infoList.Clear();
        hscale = 1.03f; // Ĭ��Ϊ1.03
        route_componetPosInfoDict.Clear();
        lastRoutePoints = null;
        base.MInit();
        // ȡ���ж���û����ͼ
        CloseCollision();
        GetSpriteRenderer().enabled = false;
        
        // ����ʵ������̶���0
        transform.position = Vector3.zero;

        // ��һ֡ǿ���������г�������ֵ��ҽ�����ͬ������
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate
        {
            foreach (var comp in GetAllRatTrainComponent())
            {
                comp.SetGetBurnRateFunc(delegate { return mBurnRate; });
                comp.SetMaxHpAndCurrentHp(mMaxHp);
            }
            return true;
        });
        AddTask(t);
    }

    public override void MUpdate()
    {
        {
            List<RatTrainComponent> delList = new List<RatTrainComponent>();
            {
                List<RatTrainBody> bodyDelList = new List<RatTrainBody>();
                foreach (var u in bodyList)
                {
                    if (!u.IsAlive())
                        bodyDelList.Add(u);
                }
                foreach (var u in bodyDelList)
                {
                    bodyList.Remove(u);
                    delList.Add(u);
                }
                    
            }
            if (head != null && !head.IsAlive())
            {
                delList.Add(head);
                head = null;
            }

            List<ComponetPosInfo> delInfoList = new List<ComponetPosInfo>();
            foreach (var info in infoList)
            {
                if (delList.Contains(info.component))
                    delInfoList.Add(info);
            }
            foreach (var info in delInfoList)
            {
                infoList.Remove(info);
            }

            if(delInfoList.Count > 0)
                foreach (var infoList in route_componetPosInfoDict.Values.ToArray())
                {
                    delInfoList.Clear();
                    foreach (var info in infoList)
                    {
                        if (delList.Contains(info.component))
                            delInfoList.Add(info);
                    }
                    foreach (var info in delInfoList)
                    {
                        infoList.Remove(info);
                    }
                }
        }

        base.MUpdate();
    }

    /// <summary>
    /// ������ͷ�복��
    /// </summary>
    public void CreateHeadAndBody(int bodyCount)
    {
        float move_dist = 0;
        // ��ͷ
        if(head == null)
        {
            head = RatTrainHead.GetInstance();
            head.SetMaster(this);
            head.SetDmgRate(1.0f, 1.0f);
            head.SetGetBurnRateFunc(delegate { return mBurnRate; });
            head.SetMaxHpAndCurrentHp(mMaxHp);
            infoList.Add(new ComponetPosInfo(head, move_dist));
            GameController.Instance.AddMouseUnit(head);
        }
        move_dist -= 0.5f * headLength + 0.5f * length * hscale;

        // ����
        for (int i = 0; i < bodyCount; i++)
        {
            RatTrainBody b = RatTrainBody.GetInstance(i);
            b.SetMaster(this);
            b.SetDmgRate(1.0f, 1.0f);
            b.transform.localScale = new Vector2(hscale, 1);
            //b.SetGetBurnRateFunc(delegate { return mBurnRate; });
            //b.SetMaxHpAndCurrentHp(mMaxHp);
            bodyList.Add(b);
            infoList.Add(new ComponetPosInfo(b, move_dist));
            GameController.Instance.AddMouseUnit(b);
            move_dist -= length * hscale;
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
        // ��ÿ�ڳ���λ����Ϣ�ƶ�����
        List<ComponetPosInfo> skipList = new List<ComponetPosInfo>(); // ת�Ʊ�ת�Ƶ���һ�ݶӣ�
        foreach (var keyValuePair in route_componetPosInfoDict)
        {
            RoutePoints route = keyValuePair.Key;
            bool isLastRoute = (route == lastRoutePoints);
            List<ComponetPosInfo> infoList = keyValuePair.Value;

            // ת��
            Vector2 rot = route.GetRotate();
            foreach (var info in skipList)
            {
                info.component.SetMoveRoate(-rot);
                info.component.transform.right = -rot;
                infoList.Add(info);
            }
            skipList.Clear();

            float dist = route.GetDistance();
            foreach (var info in infoList)
            {
                if (info.canSkipToNextPoint && !isLastRoute) // ���Ϊ���һ��·����������ת��
                {
                    info.canSkipToNextPoint = false;
                    info.move_dist -= dist;
                    info.component.taskController.AddTask(GetSkipToNextRouteAppearTask(info));
                    skipList.Add(info);
                    continue;
                }

                // ʵ��λ�ø���
                {
                    float r = info.move_dist / dist;
                    if (r >= 0)
                        info.component.transform.position = Vector3.Lerp(route.start, route.end, info.move_dist / dist);
                    else
                        info.component.transform.position = MapManager.GetGridLocalPosition(15, -5); // ����Ļ��ȥ
                }

                if (info.move_dist >= dist && !info.isPlayingMoveToNextPointTask)
                {
                    info.isPlayingMoveToNextPointTask = true;
                    info.component.taskController.AddTask(GetSkipToNextRouteDisappearTask(info));
                }
            }

            // �Ƴ���Ҫת�Ƶ�info
            foreach (var info in skipList)
                infoList.Remove(info);

        }
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
        // ��ÿ�ڳ���λ����Ϣ�ƶ�����
        bool delEmptyRoute = true;
        bool isMoveToDestination = IsHeadMoveToDestination();
        List<RoutePoints> delRouteList = new List<RoutePoints>();
        List<ComponetPosInfo> skipList = new List<ComponetPosInfo>(); // ת�Ʊ�ת�Ƶ���һ�ݶӣ�
        foreach (var keyValuePair in route_componetPosInfoDict)
        {
            RoutePoints route = keyValuePair.Key;
            bool isLastRoute = (route == lastRoutePoints);
            List<ComponetPosInfo> infoList = keyValuePair.Value;

            // ת��
            Vector2 rot = route.GetRotate();
            foreach (var info in skipList)
            {
                info.component.SetMoveRoate(-rot);
                info.component.transform.right = -rot;
                infoList.Add(info);
            }
            skipList.Clear();

            float dist = route.GetDistance();
            foreach (var info in infoList)
            {
                if (info.canSkipToNextPoint && !isLastRoute) // ���Ϊ���һ��·����������ת��
                {
                    info.canSkipToNextPoint = false;
                    info.move_dist -= dist;
                    info.component.taskController.AddTask(GetSkipToNextRouteAppearTask(info));
                    skipList.Add(info);
                    continue;
                }

                // �ƶ�����
                if (!isMoveToDestination)
                {
                    info.move_dist += GetMoveSpeed();
                }
                    
                // ʵ��λ�ø���
                {
                    float r = info.move_dist / dist;
                    if(r >= 0)
                        info.component.transform.position = Vector3.Lerp(route.start, route.end, info.move_dist / dist);
                    else
                        info.component.transform.position = MapManager.GetGridLocalPosition(15, -5); // ����Ļ��ȥ
                }

                if(info.move_dist >= dist && !info.isPlayingMoveToNextPointTask)
                {
                    info.isPlayingMoveToNextPointTask = true;
                    info.component.taskController.AddTask(GetSkipToNextRouteDisappearTask(info));
                }
            }

            // �Ƴ���Ҫת�Ƶ�info
            foreach (var info in skipList)
                infoList.Remove(info);
        }
    }

    private CustomizationTask GetSkipToNextRouteDisappearTask(ComponetPosInfo info)
    {
        RatTrainComponent c = info.component;

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            c.animatorController.Play("Disappear");
        });
        task.AddTaskFunc(delegate {
            return c.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        task.AddOnExitAction(delegate {
            info.canSkipToNextPoint = true;
        });
        return task;
    }

    private CustomizationTask GetSkipToNextRouteAppearTask(ComponetPosInfo info)
    {
        RatTrainComponent c = info.component;

        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            info.isPlayingMoveToNextPointTask = true;
            c.animatorController.Play("Appear");
        });
        task.AddTaskFunc(delegate {
            return c.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        task.AddOnExitAction(delegate {
            c.animatorController.Play("Idle", true);
            info.isPlayingMoveToNextPointTask = false;
        });
        return task;
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

    /// <summary>
    /// ��ͷ�Ƿ�ִ�Ԥ�����յ�
    /// </summary>
    /// <returns></returns>
    public bool IsHeadMoveToDestination()
    {
        if (route_componetPosInfoDict.Count <= 0 || lastRoutePoints == null)
            return true;

        List<ComponetPosInfo> list = route_componetPosInfoDict[lastRoutePoints];
        if (list.Count <= 0)
            return false;
        else
        {
            return list[0].move_dist / lastRoutePoints.GetDistance() >= 1;
        }
    }

    public void SetHeadMoveDist(float dist)
    {
        ComponetPosInfo head_info = infoList[0];
        float delta = dist - head_info.move_dist;
        foreach (var info in infoList)
            info.move_dist += delta;
    }

    public void AddHeadMoveDist(float delta)
{
        foreach (var info in infoList)
            info.move_dist += delta;
    }


    public void SetHeadMoveDestination()
    {
        if(lastRoutePoints != null)
            SetHeadMoveDist(lastRoutePoints.GetDistance());
    }


    /// <summary>
    /// ���·����ͨ�����Ӻ����±�ķ�ʽ
    /// </summary>
    /// <param name="list"></param>
    public void AddRouteListByGridIndex(List<Vector2[]> list)
    {
        foreach (var v2Arr in list)
        {
            Vector2 start = v2Arr[0];
            Vector2 end = v2Arr[1];
            RoutePoints route = new RoutePoints() { start = MapManager.GetGridLocalPosition(start.x, start.y), end = MapManager.GetGridLocalPosition(end.x, end.y) };
            route_componetPosInfoDict.Add(route, new List<ComponetPosInfo>());
            lastRoutePoints = route;

            // ����ǵ�һ����ӵ�·������ô�����г�����Ϣת�ƽ���
            if(route_componetPosInfoDict.Count == 1)
            {
                Vector2 rot = route.GetRotate();

                foreach (var info in infoList)
                {
                    info.component.moveRotate = -rot;
                    info.component.transform.right = -rot;
                    route_componetPosInfoDict[route].Add(info);
                }
            }
        }
    }

    /// <summary>
    /// ������г����·��
    /// </summary>
    public void ClearRouteComponetPosInfoDict()
    {
        bodyList.Clear();
        infoList.Clear();
        route_componetPosInfoDict.Clear();
        lastRoutePoints = null;
    }
}
