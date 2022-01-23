using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ͼ�йصľ�̬����
/// </summary>
public class MapManager
{
    // ��ͼ��ʼ��������
    public const float CenterX = 1;
    public const float CenterY = -0.375f;
    // ��λ���ӵĿ�ȡ��߶�
    public const float gridWidth = 0.6125f;
    public const float gridHeight = 0.655f;


    // �涨���Ͻ���һ��Ϊ(0, 0), ���½�Ϊ(8, 6)
    public static float getColumnX(int columnCount)
    {
        return CenterX + (float)(0.5 + columnCount) * gridWidth;
    }

    public static float getRowY(int rowCount)
    {
        return CenterY + (float)(0.5 + rowCount) * gridHeight;
    }

    // ��ȡĳһ�����������
    public static Vector3 GetGridPosition(int columnCount, int rowCount)
    {
        return new Vector3(getColumnX(columnCount), getRowY(rowCount), 0);
    }
}
