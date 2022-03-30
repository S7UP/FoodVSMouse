using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关于同一类型单位sorting layer判定的管理者
/// </summary>
public class LayerManager
{
    public enum UnitType
    {
        Ally,
        Enemy
    }

    public const int BaseAllyUnitLayer = 1250; // 友军基础权值
    public const int BaseEnemyUnitLayer = 3750; // 敌人基础权值
    public const int BaseRowLayer = 5000; // 行权重
    public const int TypeLayer = 50; // 种类权重的单位

    /// <summary>
    /// 计算目标单位的图层
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
        }
        layer += yIndex * BaseRowLayer;
        typeAndShapeValue = Mathf.Max(Mathf.Min(-25, typeAndShapeValue),24); // 取值范围为(-25, 24)
        layer += typeAndShapeValue * TypeLayer;
        layer += arrayIndex;
        return layer;
    }
}
