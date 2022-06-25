using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ÎäÆ÷Ä¬ÈÏ×´Ì¬
/// </summary>
public class WeaponsIdleState : WeaponsActionState
{
    public WeaponsIdleState(BaseWeapons baseWeapons):base(baseWeapons)
    {
    }

    public override void OnEnter()
    {
        master.OnIdleStateEnter();
    }

    public override void OnUpdate()
    {
        master.OnIdleState();
    }

    public override void OnExit()
    {
        master.OnIdleStateExit();
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }
}
