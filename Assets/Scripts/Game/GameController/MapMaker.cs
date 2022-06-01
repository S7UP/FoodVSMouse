using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    private bool drawLine = true;
    // 地图有关属性
    // 地图
    private Vector3 mapCenter;
    private float mapWidth; //
    private float mapHeight;
    // 格子
    [HideInInspector]
    public float gridWidth;
    [HideInInspector]
    public float gridHeight;
    // 当前关卡索引
    [HideInInspector]
    public int bigLevelID;
    [HideInInspector]
    public int levelID;
    //全部的格子对象
    public BaseGrid[,] mGridList;

    // 行列
    public const int yRow = 7;
    public const int xColumn = 9;

    private void OnEnable()
    {
        MapInit();
    }

    private void MapInit()
    {
        CalculateSize();
    }

    // 计算地图格子宽高
    private void CalculateSize()
    {
        mapCenter = new Vector3(1, -0.375f, 0);
        gridWidth = 0.6125f;
        gridHeight = 0.655f;
        mapWidth = gridWidth*xColumn;
        mapHeight = gridHeight*yRow;
    }


    // 画格子用于辅助设计
    // OnDrawGizmos()方法每当鼠标进入、点击Scene视图时就会调用一次
    private void OnDrawGizmos()
    {
        if (drawLine)
        {
            CalculateSize();
            Gizmos.color = Color.green;

            // 画行
            for (int y = 0; y <= yRow; y++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Vector3 endPos = mapCenter + new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Gizmos.DrawLine(startPos, endPos);
            }
            // 画列
            for (int x = 0; x <= xColumn; x++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
                Vector3 endPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, -mapHeight / 2);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
