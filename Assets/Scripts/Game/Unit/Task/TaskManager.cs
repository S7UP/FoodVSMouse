using UnityEngine;
/// <summary>
/// 任务管理器（提供一些静态任务）
/// </summary>
public class TaskManager
{
    public static ITask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi)
    {
        int totalTimer = 0; // 初始点到目标点用时
        int currentTimer = 0; // 当前用时
        float g = 0; // 加速度
        float velocityVertical = 0; // 垂直方向的速度

        Vector3 last_vector = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - firstPosition.x) / horizontalVelocity);
            // 计算得出重力加速度（向下）
            g = 8 * height / (totalTimer * totalTimer);
            // 为垂直方向的初速度赋值
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // 关判定
            master.CloseCollision();
        };
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            else if(currentTimer >= totalTimer - 4)
            {
                // 开判定
                master.OpenCollision();
            }
            velocityVertical -= g;
            Vector3 v = Vector3.Lerp(firstPosition, targetPosition, (float)currentTimer / totalTimer);
            Vector3 dv = v - last_vector + Vector3.up * velocityVertical;
            master.transform.position += dv;
            currentTimer++;
            last_vector = v;
            if (isNavi)
                master.transform.right = dv;
            return false;
        });
        t.OnExitFunc = delegate
        {
            master.transform.right = Vector2.right;
            master.TakeDamage(null);
        };
        master.AddTask(t);
        return t;
    }

    public static ITask AddParabolaTask(BaseUnit master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi)
    {
        int totalTimer = 0; // 初始点到目标点用时
        int currentTimer = 0; // 当前用时
        float g = 0; // 加速度
        float velocityVertical = 0; // 垂直方向的速度
        FloatModifier yPosModifier = new FloatModifier(0);

        Vector3 last_vector = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - firstPosition.x) / horizontalVelocity);
            // 计算得出重力加速度（向下）
            g = 8 * height / (totalTimer * totalTimer);
            // 为垂直方向的初速度赋值
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // 关判定
            master.CloseCollision();
        };
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            else if (currentTimer >= totalTimer - 4)
            {
                // 开判定
                master.OpenCollision();
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
        t.OnExitFunc = delegate
        {
            if(isNavi)
                master.transform.right = Vector2.right;
            master.RemoveSpriteOffsetY(yPosModifier);
        };
        master.AddTask(t);
        return t;
    }
}
