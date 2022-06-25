using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class WeaponsDieState : WeaponsActionState
{
    public WeaponsDieState(BaseWeapons baseWeapons) : base(baseWeapons)
    {
    }

    public override void OnEnter()
    {
        master.OnDieStateEnter();
    }

    public override void OnUpdate()
    {
        master.OnDieState();
    }

    public override void OnExit()
    {
        master.OnDieStateExit();
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }
}
