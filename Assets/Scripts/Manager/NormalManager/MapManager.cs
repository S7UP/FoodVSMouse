using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// �������ͼ�йصľ�̬����
/// </summary>
public class MapManager
{
    // ��ͼ��ʼ�������ģ��������꣩
    public const float CenterX = 1;
    public const float CenterY = -0.375f;
    // ��λ���ӵĿ�ȡ��߶�
    public const float gridWidth = 0.6125f;
    public const float gridHeight = 0.655f;


    // �涨���Ͻ���һ��Ϊ(0, 0), ���½�Ϊ(8, 6)
    public static float GetColumnX(int xIndex)
    {
        return CenterX + (float)(-(MapMaker.xColumn - 1) / 2 + xIndex) * gridWidth;
    }

    public static float GetRowY(int yIndex)
    {
        return CenterY + (float)((MapMaker.yRow - 1) / 2 - yIndex) * gridHeight;
    }

    // ����������귴�Ƴ������±�
    public static int GetXIndex(float xPos)
    {
        return Mathf.FloorToInt((xPos - CenterX) / gridWidth + (MapMaker.xColumn - 1) / 2 + 0.5f);
    }

    public static int GetYIndex(float yPos)
    {
        return Mathf.FloorToInt(-(yPos - CenterY) / gridHeight + (MapMaker.yRow - 1) / 2 + 0.5f);
    }

    // ��ȡĳһ����������꣨��ԣ�
    public static Vector3 GetGridLocalPosition(int xIndex, int yIndex)
    {
        return new Vector3(GetColumnX(xIndex), GetRowY(yIndex), 0);
    }
}
