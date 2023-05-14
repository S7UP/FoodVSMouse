using System;
using System.Collections.Generic;

public class BaseTask : ITask
{
    private List<Action> OnEnterActionList = new List<Action>();
    private List<Action> OnExitActionList = new List<Action>();
    private bool isEnd = false;
    private bool isEnter = false;
    public TaskController taskController = new TaskController();

    public void OnEnter()
    {
        if (!isEnter)
        {
            foreach (var action in OnEnterActionList)
            {
                action();
            }
            isEnter = true;
        }
        O_OnEnter();
    }
    public void OnUpdate()
    {
        O_OnUpdate();
        taskController.Update();
    }

    public bool IsMeetingExitCondition()
    {
        return O_IsMeetingCondition();
    }
    public void OnExit()
    {
        if (!isEnd)
        {
            foreach (var action in OnExitActionList)
            {
                action();
            }
            O_OnExit();
            isEnd = true;
        }
    }

    #region ��ȡһЩ״̬��Ϣ�ķ���
    public bool IsEnd()
    {
        return isEnd;
    }
    #endregion

    #region ������̳�
    protected virtual void O_OnEnter()
    {

    }

    protected virtual void O_OnUpdate()
    {

    }

    protected virtual bool O_IsMeetingCondition()
    {
        return true;
    }

    protected virtual void O_OnExit()
    {

    }
    #endregion

    #region ������������ķ���
    public void AddOnEnterAction(Action action)
    {
        OnEnterActionList.Add(action);
    }

    public void RemoveOnEnterAction(Action action)
    {
        OnEnterActionList.Remove(action);
    }

    public void AddOnExitAction(Action action)
    {
        OnExitActionList.Add(action);
    }

    public void RemoveOnExitAction(Action action)
    {
        OnExitActionList.Remove(action);
    }
    #endregion
}
