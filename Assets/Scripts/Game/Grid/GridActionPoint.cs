using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �����¼���
/// </summary>
public class GridActionPoint
{
    public List<Action<GridAction>> Listeners { get; set; } = new List<Action<GridAction>>();
}

public class GridAction
{
    public BaseUnit unit; // ���µ�λ
    public BaseGrid grid; // ���¸���

    public GridAction(BaseUnit u, BaseGrid g)
    {
        unit = u;
        grid = g;
    }
}