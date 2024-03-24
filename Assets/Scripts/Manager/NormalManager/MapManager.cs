
using UnityEngine;

/// <summary>
/// �������ͼ�йصľ�̬����
/// </summary>
public class MapManager
{
    // ��ͼ��ʼ�������ģ��������꣩
    public const float CenterX = 1;
    public const float CenterY = -0.35f;
    // ��λ���ӵĿ�ȡ��߶�
    public const float gridWidth = 0.6f;
    public const float gridHeight = 0.65f;


    // �涨���Ͻ���һ��Ϊ(0, 0), ���½�Ϊ(8, 6)
    public static float GetColumnX(float xIndex)
    {
        return CenterX + (float)(-(MapController.xColumn - 1) / 2 + xIndex) * gridWidth;
    }

    public static float GetRowY(float yIndex)
    {
        return CenterY + (float)((MapController.yRow - 1) / 2 - yIndex) * gridHeight;
    }

    // ����������귴�Ƴ������±�
    public static float GetXIndexF(float xPos)
    {
        return (xPos - CenterX) / gridWidth + (MapController.xColumn - 1) / 2;
    }

    public static float GetYIndexF(float yPos)
    {
        return -(yPos - CenterY) / gridHeight + (MapController.yRow - 1) / 2;
    }

    // ����������귴�Ƴ������±�
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

    // ��ȡĳһ����������꣨��ԣ�
    public static Vector3 GetGridLocalPosition(float xIndex, float yIndex)
    {
        return new Vector3(GetColumnX(xIndex), GetRowY(yIndex), 0);
    }
}
