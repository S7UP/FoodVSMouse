using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGridState
{
    // ���и�״̬�ĸ���
    private BaseGrid mGrid;

    public BaseGridState(BaseGrid grid)
    {
        mGrid = grid;
    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit() 
    {

    }

    public virtual void OnUpdate() 
    {
        
    }
}
