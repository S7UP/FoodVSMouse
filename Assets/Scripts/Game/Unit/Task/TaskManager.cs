using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 任务管理器（提供一些静态任务）
/// </summary>
public class TaskManager
{
    #region 为子弹添加抛物线运动
    /// <summary>
    /// 为子弹添加一个无目标的抛物线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="horizontalVelocity"></param>
    /// <param name="height"></param>
    /// <param name="firstPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="isNavi"></param>
    /// <returns></returns>
    public static CustomizationTask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi, bool notEndWithTakeDamage, Func<BaseBullet, BaseUnit, bool> hitCondition)
    {
        int totalTimer = 0; // 初始点到目标点用时
        int currentTimer = 0; // 当前用时
        float g = 0; // 加速度
        float velocityVertical = 0; // 垂直方向的速度
        FloatModifier yPosModifier = new FloatModifier(0);

        Vector3 last_vector = Vector3.zero;
        Func<BaseBullet, BaseUnit, bool> noHitFunc = delegate { return false; };

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.Max(4, Mathf.CeilToInt((targetPosition - firstPosition).magnitude / horizontalVelocity));
            // 计算得出重力加速度（向下）
            g = 8 * height / (totalTimer * totalTimer);
            // 为垂直方向的初速度赋值
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            master.AddCanHitFunc(noHitFunc);
            master.AddCanHitFunc(hitCondition);
        });
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            else if (currentTimer >= totalTimer - 4)
            {
                // 开判定
                master.RemoveCanHitFunc(noHitFunc);
            }
            velocityVertical -= g;
            Vector3 v = Vector3.Lerp(firstPosition, targetPosition, (float)currentTimer / totalTimer);
            Vector3 dv = v - last_vector;
            master.RemoveSpriteOffsetY(yPosModifier);
            yPosModifier.Value += velocityVertical;
            master.AddSpriteOffsetY(yPosModifier);
            master.transform.position += dv;
            currentTimer++;
            last_vector = v;
            if (isNavi)
                master.SetSpriteRight(dv + Vector3.up * velocityVertical);
            return false;
        });
        t.AddOnExitAction(delegate
        {
            master.SetSpriteRight(Vector2.right);
            if (!notEndWithTakeDamage)
                master.TakeDamage(null);
            master.RemoveSpriteOffsetY(yPosModifier);
            master.RemoveCanHitFunc(hitCondition);
        });
        master.AddTask(t);
        return t;
    }

    /// <summary>
    /// 为子弹添加一个无目标的抛物线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="horizontalVelocity"></param>
    /// <param name="height"></param>
    /// <param name="firstPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="isNavi"></param>
    /// <returns></returns>
    public static CustomizationTask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi, bool notEndWithTakeDamage)
    {
        return AddParabolaTask(master, horizontalVelocity, height, firstPosition, targetPosition, isNavi, notEndWithTakeDamage, delegate { return true; });
    }

    public static CustomizationTask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi)
    {
        return AddParabolaTask(master, horizontalVelocity, height, firstPosition, targetPosition, isNavi, false);
    }

    /// <summary>
    /// 为子弹添加一个有目标的抛物线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="horizontalVelocity"></param>
    /// <param name="height"></param>
    /// <param name="firstPosition"></param>
    /// <param name="target"></param>
    /// <param name="isNavi"></param>
    /// <returns></returns>
    public static CustomizationTask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, BaseUnit target, bool isNavi, bool notEndWithTakeDamage)
    {
        // 添加一个锁定目标事件，如果目标不死的话子弹一定优先命中目标，目标死了才可能会命中其附近范围内的目标
        bool targetIsDie = false;
        Func<BaseBullet, BaseUnit, bool> hitCondition = (b, u) => {
            if (!targetIsDie && target != null && target.IsAlive())
            {
                if (u == target)
                    return true;
                else
                    return false;
            }
            else
            {
                targetIsDie = true;
                return true;
            }
        };

        // 先为目标添加无目标的抛物线运动
        CustomizationTask oriTask = AddParabolaTask(master, horizontalVelocity, height, firstPosition, target.transform.position, isNavi, notEndWithTakeDamage, hitCondition);
        // 然后添加一个位移补正机制
        Vector3 CurrentDelta = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            CurrentDelta = Vector2.zero;
        });
        t.AddTaskFunc(delegate {
            // 当原运动生命周期结束或者目标失效时，该任务也自动结束
            if (oriTask.IsEnd() || !target.IsAlive())
                return true;
            CurrentDelta += (Vector3)target.DeltaPosition;
            // 位移补正，每次补正的距离不超过传入的水平速度
            float v = Mathf.Min(horizontalVelocity, CurrentDelta.magnitude);
            Vector3 deltaV2 = v * CurrentDelta.normalized;
            master.transform.position += deltaV2;
            CurrentDelta -= deltaV2;
            return false;
        });
        t.AddOnExitAction(delegate
        {

        });
        master.AddTask(t);
        return t;
    }

    public static CustomizationTask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, BaseUnit target, bool isNavi)
    {
        return AddParabolaTask(master, horizontalVelocity, height, firstPosition, target, isNavi, false);
    }
    #endregion

    #region 为单位添加抛物线运动
    /// <summary>
    /// 为单位添加一个无目标的抛物线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="horizontalVelocity"></param>
    /// <param name="height"></param>
    /// <param name="firstPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="isNavi"></param>
    /// <returns></returns>
    public static CustomizationTask GetParabolaTask(BaseUnit master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi)
    {
        return GetParabolaTask(master, horizontalVelocity, height, firstPosition, targetPosition, isNavi, false);
    }

    public static CustomizationTask GetParabolaTask(BaseUnit master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi, bool isOpenCollide)
    {
        int ori_height = master.mHeight;
        int totalTimer = 0; // 初始点到目标点用时
        int currentTimer = 0; // 当前用时
        float g = 0; // 加速度
        float velocityVertical = 0; // 垂直方向的速度
        FloatModifier yPosModifier = new FloatModifier(0);
        IntModifier flyIntModifier = new IntModifier(1);

        Vector3 last_vector = Vector3.zero;
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return isOpenCollide; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return isOpenCollide; };

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            // 为目标添加一个Flying字段
            if (!master.NumericBox.IntDict.ContainsKey(StringManager.Flying))
                master.NumericBox.IntDict.Add(StringManager.Flying, new IntNumeric());
            master.NumericBox.IntDict[StringManager.Flying].AddAddModifier(flyIntModifier);

            master.transform.position = firstPosition;
            totalTimer = Mathf.Max(4, Mathf.CeilToInt((targetPosition - firstPosition).magnitude / horizontalVelocity));
            // 计算得出重力加速度（向下）
            g = 8 * height / (totalTimer * totalTimer);
            // 为垂直方向的初速度赋值
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // 关判定
            master.AddCanHitFunc(noHitFunc);
            master.AddCanBlockFunc(noBlockFunc);
        });
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            velocityVertical -= g;
            Vector3 v = Vector3.Lerp(firstPosition, targetPosition, (float)currentTimer / totalTimer);
            Vector3 dv = v - last_vector;
            master.RemoveSpriteOffsetY(yPosModifier);
            yPosModifier.Value = yPosModifier.Value + velocityVertical;
            master.AddSpriteOffsetY(yPosModifier);
            master.transform.position += dv;
            currentTimer++;
            last_vector = v;
            if (isNavi)
                master.transform.right = dv;
            return false;
        });
        t.AddOnExitAction(delegate
        {
            if (isNavi)
                master.transform.right = Vector2.right;
            master.RemoveSpriteOffsetY(yPosModifier);
            // 开判定
            master.RemoveCanHitFunc(noHitFunc);
            master.RemoveCanBlockFunc(noBlockFunc);
            // 为目标移除当前的Flying字段
            if (master.NumericBox.IntDict.ContainsKey(StringManager.Flying))
                master.NumericBox.IntDict[StringManager.Flying].RemoveAddModifier(flyIntModifier);
        });
        return t;
    }
    #endregion

    #region 追踪任务
    /// <summary>
    /// 添加跟踪能力
    /// </summary>
    /// <returns></returns>
    public static ITask AddTrackAbility(BaseBullet b, Func<BaseBullet, BaseUnit> FindTargetFunc,
        Func<BaseBullet, BaseUnit, bool> InValidFunc, Action<BaseBullet, BaseUnit> TrackAction,
        Action<BaseBullet> NoTargetAction, Func<BaseBullet, BaseUnit, bool> ExitConditionFunc)
    {
        // 默认方法的设置
        if (FindTargetFunc == null)
        {
            FindTargetFunc = delegate { return null; };
        }
        if (InValidFunc == null)
        {
            InValidFunc = (b, unit) => { return !unit.IsAlive() || !UnitManager.CanBeSelectedAsTarget(b.mMasterBaseUnit, unit); };
        }
        if (TrackAction == null)
        {
            TrackAction = (b, unit) => { b.SetRotate(unit.moveRotate); };
        }
        if (NoTargetAction == null)
        {
            NoTargetAction = delegate { };
        }
        if (ExitConditionFunc == null)
        {
            ExitConditionFunc = delegate { return false; };
        }

        CustomizationTask t = new CustomizationTask();
        bool hasTarget = false; // 是否有目标
        BaseUnit unit = null;  // 目标
        t.AddTaskFunc(delegate {
            // 如果有目标，则需要检测目标是否满足失效条件
            if (hasTarget && InValidFunc(b, unit))
            {
                hasTarget = false;
                unit = null;
            }
            // 如果没目标则需要寻找目标
            if (!hasTarget)
            {
                unit = FindTargetFunc(b);
                if (unit != null)
                    hasTarget = true;
            }

            if (hasTarget)
            {
                // 如果有目标则执行有目标的（追击）方法
                TrackAction(b, unit);
            }
            else
            {
                // 否则执行无目标的方法
                NoTargetAction(b);
            }
            // 退出的方法
            return ExitConditionFunc(b, unit);
        });
        b.AddTask(t);
        return t;
    }
    #endregion

    #region 自转任务
    /// <summary>
    /// 添加自转任务
    /// </summary>
    /// <param name="b">子弹对象引用</param>
    /// <param name="omiga">角速度</param>
    /// <returns></returns>
    public static ITask AddSpinAbility(BaseBullet b, float omiga)
    {
        float rot = 0;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            rot += omiga;
            b.SetSpriteRotate(rot);
            return false;
        });
        b.AddTask(t);
        return t;
    }
    #endregion

    #region 强制移动任务
    public static void GetMoveToTask(Transform trans, Vector2 endPos, int t, out CustomizationTask task)
    {
        int time = 0;
        Vector2 startPos = Vector2.zero;
        task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            startPos = trans.position;
            time = 0;
        });
        task.AddTaskFunc(delegate {
            time++;
            float rate = Mathf.Min(1, (float)time / t);
            trans.transform.position = Vector2.Lerp(startPos, endPos, rate);
            if (time >= t)
                return true;
            else
                return false;
        });
    }
    #endregion
}