using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理与地图有关的静态数据
/// </summary>
public class MapManager
{
    // 地图开始绘格的中心
    public const float CenterX = 1;
    public const float CenterY = -0.375f;
    // 单位格子的宽度、高度
    public const float gridWidth = 0.6125f;
    public const float gridHeight = 0.655f;


    // 规定左上角那一格为(0, 0), 右下角为(8, 6)
    public static float getColumnX(int columnCount)
    {
        return CenterX + (float)(0.5 + columnCount) * gridWidth;
    }

    public static float getRowY(int rowCount)
    {
        return CenterY + (float)(0.5 + rowCount) * gridHeight;
    }

    // 获取某一格的中心坐标
    public static Vector3 GetGridPosition(int columnCount, int rowCount)
    {
        return new Vector3(getColumnX(columnCount), getRowY(rowCount), 0);
    }
}
