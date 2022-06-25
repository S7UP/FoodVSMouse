using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ÎäÆ÷µÄ×´Ì¬»ú
/// </summary>
public class WeaponsActionState : IBaseActionState
{
    public BaseWeapons master;

    public WeaponsActionState(BaseWeapons baseWeapons)
    {
        master = baseWeapons;
    }

    public virtual void OnEnter()
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual void OnExit()
    {
        
    }

    public virtual void OnInterrupt()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnContinue()
    {
        throw new System.NotImplementedException();
    }
}
