using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseStateImplementor
{
    public void OnIdleState();
    public void OnMoveState();
    public void OnAttackState();
    public void OnStaticState();
}
