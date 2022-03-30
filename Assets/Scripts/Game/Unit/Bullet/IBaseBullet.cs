using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseBullet
{
    public void TakeDamage(BaseUnit baseUnit);

    public void OnFlyStateEnter();
    public void OnFlyState();
    public void OnFlyStateExit();
    public void OnHitStateEnter();
    public void OnHitState();
    public void OnHitStateExit();

}
