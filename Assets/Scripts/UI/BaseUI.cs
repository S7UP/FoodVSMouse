using UnityEngine;

public class BaseUI : MonoBehaviour, IGameControllerMember
{
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
        mTaskController.Initial();
        O_Recycle();
    }

    #region 供外界调用的方法
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
    #endregion

    #region 由子类继承的方法
    /// <summary>
    /// 被回收的方法
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
