using System;

using UnityEngine;
/// <summary>
/// 提供一些复合技能的常用静态方法
/// </summary>
public class CompoundSkillAbilityManager
{
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

    public static void GetMoveToTask(Transform trans, BaseUnit target, float standardVelocity, int t, out CustomizationTask task)
    {
        int time = 0;
        bool isTargetAlive = true;
        float v = TransManager.TranToVelocity(standardVelocity);
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;
        Vector2 lastPos = Vector2.zero;
        Vector2 deltaPos = Vector2.zero;
        Vector2 deltaPosLeft = Vector2.zero;
        task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            startPos = trans.position;
            endPos = target.transform.position;
            deltaPos = Vector2.zero;
            deltaPosLeft = Vector2.zero;
            lastPos = target.transform.position;
            time = 0;
        });
        task.AddTaskFunc(delegate {
            time++;
            // 吸附跟随逻辑
            if(isTargetAlive && target.IsAlive())
            {
                deltaPosLeft += (Vector2)target.transform.position - lastPos;
                lastPos = target.transform.position;
            }
            else
            {
                isTargetAlive = false;
            }
            if (deltaPosLeft.magnitude < v)
            {
                deltaPos += deltaPosLeft;
                deltaPosLeft = Vector2.zero;
            }
            else
            {
                Vector2 d_pos = deltaPosLeft.normalized * v;
                deltaPosLeft -= d_pos;
                deltaPos += d_pos;
            }
            
            float rate = Mathf.Min(1, (float)time / t);
            trans.transform.position = Vector2.Lerp(startPos, endPos, rate) + deltaPos;
            if (time >= t)
                return true;
            else
                return false;
        });
    }

    public static void GetWaitClipTask(AnimatorController animatorController, string clipName, out CustomizationTask task)
    {
        task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            animatorController.Play(clipName);
        });
        task.AddTaskFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                return true;
            return false;
        });
    }

    public static void GetWaitTimeTask(int totalTime, out CustomizationTask task)
    {
        int timeLeft = totalTime;
        task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            timeLeft--;
            if (timeLeft <= 0)
                return true;
            return false;
        });
    }

    public static void GetWaitTimeTask(int totalTime, Action<int> action, out CustomizationTask task)
    {
        int timeLeft = totalTime;
        task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            timeLeft--;
            action(timeLeft);
            if (timeLeft <= 0)
                return true;
            return false;
        });
    }
}
