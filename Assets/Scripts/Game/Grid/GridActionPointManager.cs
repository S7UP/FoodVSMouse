using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 格子的行动点管理器
/// </summary>
public class GridActionPointManager
{
    private Dictionary<GridActionPointType, GridActionPoint> ActionPoints { get; set; } = new Dictionary<GridActionPointType, GridActionPoint>();


    public void Initialize()
    {
        ActionPoints.Clear();

        ActionPoints.Add(GridActionPointType.BeforeSetFoodUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterSetFoodUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.BeforeRemoveFoodUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterRemoveFoodUnit, new GridActionPoint());

        ActionPoints.Add(GridActionPointType.BeforeMouseUnitEnter, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterMouseUnitEnter, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.BeforeMouseUnitExit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterMouseUnitExit, new GridActionPoint());

        ActionPoints.Add(GridActionPointType.BeforeSetItemUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterSetItemUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.BeforeRemoveItemUnit, new GridActionPoint());
        ActionPoints.Add(GridActionPointType.AfterRemoveItemUnit, new GridActionPoint());
    }

    public void AddListener(GridActionPointType actionPointType, Action<GridAction> action)
    {
        ActionPoints[actionPointType].Listeners.Add(action);
        Debug.Log("添加监听成功！");
    }

    public void RemoveListener(GridActionPointType actionPointType, Action<GridAction> action)
    {
        ActionPoints[actionPointType].Listeners.Remove(action);
        Debug.Log("移除监听成功！");
    }

    /// <summary>
    /// 触发某个行动
    /// </summary>
    /// <param name="actionPointType"></param>
    /// <param name="action"></param>
    public void TriggerActionPoint(GridActionPointType actionPointType, GridAction action)
    {
        if (ActionPoints.ContainsKey(actionPointType))
            foreach (var item in ActionPoints[actionPointType].Listeners)
            {
                item.Invoke(action);
            }
    }
}
