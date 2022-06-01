using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    private bool drawLine = true;
    // ��ͼ�й�����
    // ��ͼ
    private Vector3 mapCenter;
    private float mapWidth; //
    private float mapHeight;
    // ����
    [HideInInspector]
    public float gridWidth;
    [HideInInspector]
    public float gridHeight;
    // ��ǰ�ؿ�����
    [HideInInspector]
    public int bigLevelID;
    [HideInInspector]
    public int levelID;
    //ȫ���ĸ��Ӷ���
    public BaseGrid[,] mGridList;

    // ����
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

    // �����ͼ���ӿ��
    private void CalculateSize()
    {
        mapCenter = new Vector3(1, -0.375f, 0);
        gridWidth = 0.6125f;
        gridHeight = 0.655f;
        mapWidth = gridWidth*xColumn;
        mapHeight = gridHeight*yRow;
    }


    // ���������ڸ������
    // OnDrawGizmos()����ÿ�������롢���Scene��ͼʱ�ͻ����һ��
    private void OnDrawGizmos()
    {
        if (drawLine)
        {
            CalculateSize();
            Gizmos.color = Color.green;

            // ����
            for (int y = 0; y <= yRow; y++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Vector3 endPos = mapCenter + new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Gizmos.DrawLine(startPos, endPos);
            }
            // ����
            for (int x = 0; x <= xColumn; x++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
                Vector3 endPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, -mapHeight / 2);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
