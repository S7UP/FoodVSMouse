using UnityEngine;
/// <summary>
/// ������������ṩһЩ��̬����
/// </summary>
public class TaskManager
{
    public static ITask AddParabolaTask(BaseBullet master, float horizontalVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool isNavi)
    {
        int totalTimer = 0; // ��ʼ�㵽Ŀ�����ʱ
        int currentTimer = 0; // ��ǰ��ʱ
        float g = 0; // ���ٶ�
        float velocityVertical = 0; // ��ֱ������ٶ�

        Vector3 last_vector = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - firstPosition.x) / horizontalVelocity);
            // ����ó��������ٶȣ����£�
            g = 8 * height / (totalTimer * totalTimer);
            // Ϊ��ֱ����ĳ��ٶȸ�ֵ
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // ���ж�
            master.CloseCollision();
        };
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            else if(currentTimer >= totalTimer - 4)
            {
                // ���ж�
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
        int totalTimer = 0; // ��ʼ�㵽Ŀ�����ʱ
        int currentTimer = 0; // ��ǰ��ʱ
        float g = 0; // ���ٶ�
        float velocityVertical = 0; // ��ֱ������ٶ�
        FloatModifier yPosModifier = new FloatModifier(0);

        Vector3 last_vector = Vector3.zero;

        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate {
            master.transform.position = firstPosition;
            totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - firstPosition.x) / horizontalVelocity);
            // ����ó��������ٶȣ����£�
            g = 8 * height / (totalTimer * totalTimer);
            // Ϊ��ֱ����ĳ��ٶȸ�ֵ
            velocityVertical = g * totalTimer / 2;
            last_vector = master.transform.position;
            // ���ж�
            master.CloseCollision();
        };
        t.AddTaskFunc(delegate {
            if (currentTimer >= totalTimer)
            {
                return true;
            }
            else if (currentTimer >= totalTimer - 4)
            {
                // ���ж�
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
