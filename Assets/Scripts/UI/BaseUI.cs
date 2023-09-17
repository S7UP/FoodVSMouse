using System;
using System.Collections.Generic;

using UnityEngine;

public class BaseUI : MonoBehaviour, IGameControllerMember
{
    private List<Action<BaseUI>> BeforeDestoryActionList = new List<Action<BaseUI>>();
    public TaskController mTaskController = new TaskController();
    private bool isHide;



    public void MInit()
    {
        O_MInit();
        isHide = true;
        Show();
        mTaskController.Initial();
    }
    public void MUpdate()
    {
        O_MUpdate();
        mTaskController.Update();
    }
    public void MPause()
    {
        O_MPause();
    }

    public void MPauseUpdate()
    {
        O_MPauseUpdate();
    }

    public void MResume()
    {
        O_MResume();
    }
    public void MDestory()
    {
        foreach (var action in BeforeDestoryActionList)
            action(this);
        BeforeDestoryActionList.Clear();
        mTaskController.Initial();
        O_Recycle();
    }

    #region �������õķ���
    public bool IsValid()
    {
        return isActiveAndEnabled;
    }

    public void Hide()
    {
        if (!isHide)
        {
            isHide = true;
            O_Hide();
        }
    }

    public void Show()
    {
        if (isHide)
        {
            isHide = false;
            O_Show();
        }
    }

    public void AddBeforeDestoryAction(Action<BaseUI> action)
    {
        BeforeDestoryActionList.Add(action);
    }

    public void RemoveBeforeDestoryAction(Action<BaseUI> action)
    {
        BeforeDestoryActionList.Remove(action);
    }
    #endregion

    #region ������̳еķ���
    /// <summary>
    /// �����յķ���
    /// </summary>
    protected virtual void O_Recycle()
    {

    }

    protected virtual void O_MInit()
    {

    }

    protected virtual void O_MUpdate()
    {

    }
    protected virtual void O_MPause()
    {

    }

    protected virtual void O_MPauseUpdate()
    {

    }

    protected virtual void O_MResume()
    {

    }

    protected virtual void O_Hide()
    {

    }

    protected virtual void O_Show()
    {

    }
    #endregion
}
