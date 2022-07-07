using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ���ӵ��ж��������
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
        Debug.Log("��Ӽ����ɹ���");
    }

    public void RemoveListener(GridActionPointType actionPointType, Action<GridAction> action)
    {
        ActionPoints[actionPointType].Listeners.Remove(action);
        Debug.Log("�Ƴ������ɹ���");
    }

    /// <summary>
    /// ����ĳ���ж�
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
