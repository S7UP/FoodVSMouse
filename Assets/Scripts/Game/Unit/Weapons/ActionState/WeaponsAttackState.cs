using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsAttackState : WeaponsActionState
{
    public WeaponsAttackState(BaseWeapons baseWeapons) : base(baseWeapons)
    {
    }

    public override void OnEnter()
    {
        master.OnAttackStateEnter();
    }

    public override void OnUpdate()
    {
        master.OnAttackState();
    }

    public override void OnExit()
    {
        master.OnAttackStateExit();
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }
}
