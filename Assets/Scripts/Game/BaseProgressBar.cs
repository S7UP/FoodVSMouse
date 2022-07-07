using UnityEngine;

public class BaseProgressBar : MonoBehaviour, IBaseProgressBar
{
    public float currentProgress; // 当前进度

    public virtual bool IsFinish()
    {
        return false;
    }

    public virtual void PInit()
    {
    }

    public virtual void PUpdate()
    {

    }

    public virtual void Hide()
    {

    }

    public virtual void Show()
    {

    }
}
