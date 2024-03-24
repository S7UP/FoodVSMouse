
using UnityEngine;

/// <summary>
/// 管理与地图有关的静态数据
/// </summary>
public class MapManager
{
    // 地图开始绘格的中心（绝对坐标）
    public const float CenterX = 1;
    public const float CenterY = -0.35f;
    // 单位格子的宽度、高度
    public const float gridWidth = 0.6f;
    public const float gridHeight = 0.65f;


    // 规定左上角那一格为(0, 0), 右下角为(8, 6)
    public static float GetColumnX(float xIndex)
    {
        return CenterX + (float)(-(MapController.xColumn - 1) / 2 + xIndex) * gridWidth;
    }

    public static float GetRowY(float yIndex)
    {
        return CenterY + (float)((MapController.yRow - 1) / 2 - yIndex) * gridHeight;
    }

    // 反向根据坐标反推出格子下标
    public static float GetXIndexF(float xPos)
    {
        return (xPos - CenterX) / gridWidth + (MapController.xColumn - 1) / 2;
    }

    public static float GetYIndexF(float yPos)
    {
        return -(yPos - CenterY) / gridHeight + (MapController.yRow - 1) / 2;
    }

    // 反向根据坐标反推出格子下标
    public static int GetXIndex(float xPos)
    {
         return Mathf.FloorToInt((xPos - CenterX) / gridWidth + (MapController.xColumn - 1) / 2 + 0.5f);
        //return Mathf.FloorToInt((xPos - CenterX) / gridWidth + (MapController.xColumn - 1) / 2);
    }

    public static int GetYIndex(float yPos)
    {
         return Mathf.FloorToInt(-(yPos - CenterY) / gridHeight + (MapController.yRow - 1) / 2 + 0.5f);
        //return Mathf.FloorToInt(-(yPos - CenterY) / gridHeight + (MapController.yRow - 1) / 2);
    }

    // 获取某一格的中心坐标（相对）
    public static Vector3 GetGridLocalPosition(float xIndex, float yIndex)
    {
        return new Vector3(GetColumnX(xIndex), GetRowY(yIndex), 0);
    }
}
