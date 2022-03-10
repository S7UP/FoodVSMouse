using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProgressBar : MonoBehaviour, IBaseProgressBar
{
    public virtual void Hide()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsFinish()
    {
        throw new System.NotImplementedException();
    }

    public virtual void PInit()
    {
    }

    public virtual void PUpdate()
    {
    }

    public virtual void Show()
    {
        throw new System.NotImplementedException();
    }
}
