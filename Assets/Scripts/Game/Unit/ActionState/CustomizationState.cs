using System;
using System.Collections.Generic;

/// <summary>
/// ×Ô¶¨Òå×´Ì¬
/// </summary>
public class CustomizationState : IBaseActionState
{
    private List<Action<CustomizationState>> OnEnterActionList = new List<Action<CustomizationState>>();
    private List<Action<CustomizationState>> OnUpdateActionList = new List<Action<CustomizationState>>();
    private List<Action<CustomizationState>> OnExitActionList = new List<Action<CustomizationState>>();
    private List<Action<CustomizationState>> OnInterruptActionList = new List<Action<CustomizationState>>();
    private List<Action<CustomizationState>> OnContinueActionList = new List<Action<CustomizationState>>();

    public void OnEnter()
    {
        foreach (var action in OnEnterActionList)
        {
            action(this);
        }
    }

    public void OnUpdate()
    {
        foreach (var action in OnUpdateActionList)
        {
            action(this);
        }
    }

    public void OnExit()
    {
        foreach (var action in OnExitActionList)
        {
            action(this);
        }
    }

    public void OnContinue()
    {
        foreach (var action in OnContinueActionList)
        {
            action(this);
        }
    }

    public void OnInterrupt()
    {
        foreach (var action in OnInterruptActionList)
        {
            action(this);
        }
    }

    public void AddOnEnterAction(Action<CustomizationState> action)
    {
        OnEnterActionList.Add(action);
    }

    public void AddOnUpdateAction(Action<CustomizationState> action)
    {
        OnUpdateActionList.Add(action);
    }

    public void AddOnExitAction(Action<CustomizationState> action)
    {
        OnExitActionList.Add(action);
    }

    public void AddOnInterruptAction(Action<CustomizationState> action)
    {
        OnInterruptActionList.Add(action);
    }

    public void AddOnContinueAction(Action<CustomizationState> action)
    {
        OnContinueActionList.Add(action);
    }

    public void RemoveOnEnterAction(Action<CustomizationState> action)
    {
        OnEnterActionList.Remove(action);
    }

    public void RemoveOnUpdateAction(Action<CustomizationState> action)
    {
        OnUpdateActionList.Remove(action);
    }

    public void RemoveOnExitAction(Action<CustomizationState> action)
    {
        OnExitActionList.Remove(action);
    }

    public void RemoveOnInterruptAction(Action<CustomizationState> action)
    {
        OnInterruptActionList.Remove(action);
    }

    public void RemoveOnContinueAction(Action<CustomizationState> action)
    {
        OnContinueActionList.Remove(action);
    }
}
