using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 格子事件点
/// </summary>
public class GridActionPoint
{
    public List<Action<GridAction>> Listeners { get; set; } = new List<Action<GridAction>>();
}

public class GridAction
{
    public BaseUnit unit; // 涉事单位
    public BaseGrid grid; // 涉事格子

    public GridAction(BaseUnit u, BaseGrid g)
    {
        unit = u;
        grid = g;
    }
}