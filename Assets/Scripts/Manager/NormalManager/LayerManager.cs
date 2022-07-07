using UnityEngine;

/// <summary>
/// ����ͬһ���͵�λsorting layer�ж��Ĺ�����
/// </summary>
public class LayerManager
{
    public enum UnitType
    {
        Ally,
        Enemy,
        Bullet
    }

    public const int BaseAllyUnitLayer = 1250; // �Ѿ�����Ȩֵ
    public const int BaseEnemyUnitLayer = 2500; // ���˻���Ȩֵ
    public const int BaseBulletLayer = 3750; // �ӵ�Ȩֵ
    public const int BaseRowLayer = 5000; // ��Ȩ��
    public const int TypeLayer = 50; // ����Ȩ�صĵ�λ

    /// <summary>
    /// ����Ŀ�굥λ��ͼ��
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="yIndex"></param>
    /// <param name="typeAndShapeValue"></param>
    /// <param name="arrayIndex"></param>
    /// <returns></returns>
    public static int CalculateSortingLayer(UnitType unitType, int yIndex, int typeAndShapeValue, int arrayIndex)
    {
        int layer = -30000;
        if(unitType == UnitType.Ally)
        {
            layer += BaseAllyUnitLayer;
        }else if(unitType == UnitType.Enemy)
        {
            layer += BaseEnemyUnitLayer;
        }else if(unitType == UnitType.Bullet)
        {
            layer += BaseBulletLayer;
        }
        layer += yIndex * BaseRowLayer;
        typeAndShapeValue = Mathf.Min(Mathf.Max(-25, typeAndShapeValue),24); // ȡֵ��ΧΪ(-25, 24)
        layer += typeAndShapeValue * TypeLayer;
        layer += arrayIndex;
        return layer;
    }
}
