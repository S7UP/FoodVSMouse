using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ������������ṩһЩ��̬����
/// </summary>
public class TaskManager
{
    #region Ϊ�ӵ�����������˶�
    /// <summary>
    /// Ϊ�ӵ����һ����Ŀ����������˶�
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
        int totalTimer = 0; // ��ʼ�㵽Ŀ�����ʱ
        int currentTimer = 0; // ��ǰ��ʱ
        float g = 0; // ���ٶ�
        float velocityVertical = 0; // ��ֱ������ٶ�
        FloatModifier yPosModifier = new FloatModifier(0);

        Vector3 last_vector = Vector3.zero;
        Func<BaseBullet, BaseUnit, bool> noHitFunc = delegate { return false; };

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.Max(4, Mathf.CeilToInt((targetPosition - firstPosition).magnitude / horizontalVelocity));
            // ����ó��������ٶȣ����£�
            g = 8 * height / (totalTimer * totalTimer);
            // Ϊ��ֱ����ĳ��ٶȸ�ֵ
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
                // ���ж�
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
    /// Ϊ�ӵ����һ����Ŀ����������˶�
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
    /// Ϊ�ӵ����һ����Ŀ����������˶�
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
        // ���һ������Ŀ���¼������Ŀ�겻���Ļ��ӵ�һ����������Ŀ�꣬Ŀ�����˲ſ��ܻ������丽����Χ�ڵ�Ŀ��
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

        // ��ΪĿ�������Ŀ����������˶�
        CustomizationTask oriTask = AddParabolaTask(master, horizontalVelocity, height, firstPosition, target.transform.position, isNavi, notEndWithTakeDamage, hitCondition);
        // Ȼ�����һ��λ�Ʋ�������
        Vector3 CurrentDelta = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            CurrentDelta = Vector2.zero;
        });
        t.AddTaskFunc(delegate {
            // ��ԭ�˶��������ڽ�������Ŀ��ʧЧʱ��������Ҳ�Զ�����
            if (oriTask.IsEnd() || !target.IsAlive())
                return true;
            CurrentDelta += (Vector3)target.DeltaPosition;
            // λ�Ʋ�����ÿ�β����ľ��벻���������ˮƽ�ٶ�
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

    #region Ϊ��λ����������˶�
    /// <summary>
    /// Ϊ��λ���һ����Ŀ����������˶�
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
        int totalTimer = 0; // ��ʼ�㵽Ŀ�����ʱ
        int currentTimer = 0; // ��ǰ��ʱ
        float g = 0; // ���ٶ�
        float velocityVertical = 0; // ��ֱ������ٶ�
        FloatModifier yPosModifier = new FloatModifier(0);
        IntModifier flyIntModifier = new IntModifier(1);

        Vector3 last_vector = Vector3.zero;
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return isOpenCollide; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return isOpenCollide; };

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            // ΪĿ�����һ��Flying�ֶ�
            if (!master.NumericBox.IntDict.ContainsKey(StringManager.Flying))
                master.NumericBox.IntDict.Add(StringManager.Flying, new IntNumeric());
            master.NumericBox.IntDict[StringManager.Flying].AddAddModifier(flyIntModifier);

            master.transform.position = firstPosition;
            totalTimer = Mathf.Max(4, Mathf.CeilToInt((targetPosition - firstPosition).magnitude / horizontalVelocity));
            // ����ó��������ٶȣ����£�
            g = 8 * height / (totalTimer * totalTimer);
            // Ϊ��ֱ����ĳ��ٶȸ�ֵ
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // ���ж�
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
            // ���ж�
            master.RemoveCanHitFunc(noHitFunc);
            master.RemoveCanBlockFunc(noBlockFunc);
            // ΪĿ���Ƴ���ǰ��Flying�ֶ�
            if (master.NumericBox.IntDict.ContainsKey(StringManager.Flying))
                master.NumericBox.IntDict[StringManager.Flying].RemoveAddModifier(flyIntModifier);
        });
        return t;
    }
    #endregion

    #region ׷������
    /// <summary>
    /// ��Ӹ�������
    /// </summary>
    /// <returns></returns>
    public static ITask AddTrackAbility(BaseBullet b, Func<BaseBullet, BaseUnit> FindTargetFunc,
        Func<BaseBullet, BaseUnit, bool> InValidFunc, Action<BaseBullet, BaseUnit> TrackAction,
        Action<BaseBullet> NoTargetAction, Func<BaseBullet, BaseUnit, bool> ExitConditionFunc)
    {
        // Ĭ�Ϸ���������
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
        bool hasTarget = false; // �Ƿ���Ŀ��
        BaseUnit unit = null;  // Ŀ��
        t.AddTaskFunc(delegate {
            // �����Ŀ�꣬����Ҫ���Ŀ���Ƿ�����ʧЧ����
            if (hasTarget && InValidFunc(b, unit))
            {
                hasTarget = false;
                unit = null;
            }
            // ���ûĿ������ҪѰ��Ŀ��
            if (!hasTarget)
            {
                unit = FindTargetFunc(b);
                if (unit != null)
                    hasTarget = true;
            }

            if (hasTarget)
            {
                // �����Ŀ����ִ����Ŀ��ģ�׷��������
                TrackAction(b, unit);
            }
            else
            {
                // ����ִ����Ŀ��ķ���
                NoTargetAction(b);
            }
            // �˳��ķ���
            return ExitConditionFunc(b, unit);
        });
        b.AddTask(t);
        return t;
    }
    #endregion

    #region ��ת����
    /// <summary>
    /// �����ת����
    /// </summary>
    /// <param name="b">�ӵ���������</param>
    /// <param name="omiga">���ٶ�</param>
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

    #region ǿ���ƶ�����
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