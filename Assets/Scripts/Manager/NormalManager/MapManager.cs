using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// 管理与地图有关的静态数据
/// </summary>
public class MapManager
{
    // 地图开始绘格的中心（绝对坐标）
    public const float CenterX = 1;
    public const float CenterY = -0.375f;
    // 单位格子的宽度、高度
    public const float gridWidth = 0.6125f;
    public const float gridHeight = 0.655f;


    // 规定左上角那一格为(0, 0), 右下角为(8, 6)
    public static float GetColumnX(int xIndex)
    {
        return CenterX + (float)(-(MapMaker.xColumn - 1) / 2 + xIndex) * gridWidth;
    }

    public static float GetRowY(int yIndex)
    {
        return CenterY + (float)((MapMaker.yRow - 1) / 2 - yIndex) * gridHeight;
    }

    // 反向根据坐标反推出格子下标
    public static int GetXIndex(float xPos)
    {
        return Mathf.FloorToInt((xPos - CenterX) / gridWidth + (MapMaker.xColumn - 1) / 2 + 0.5f);
    }

    public static int GetYIndex(float yPos)
    {
        return Mathf.FloorToInt(-(yPos - CenterY) / gridHeight + (MapMaker.yRow - 1) / 2 + 0.5f);
    }

    // 获取某一格的中心坐标（相对）
    public static Vector3 GetGridLocalPosition(int xIndex, int yIndex)
    {
        return new Vector3(GetColumnX(xIndex), GetRowY(yIndex), 0);
    }
}
