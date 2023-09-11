using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// Ͷ�����������ṩͶ������صľ�̬������
/// </summary>
public class PitcherManager
{
    private static List<FoodNameTypeMap> PitcherList = new List<FoodNameTypeMap>() { 
        FoodNameTypeMap.SaladPitcher, FoodNameTypeMap.ChocolatePitcher, FoodNameTypeMap.TofuPitcher, FoodNameTypeMap.EggPitcher
    };

    /// <summary>
    /// ���Ĭ�ϵ�Ͷ����������
    /// </summary>
    /// <param name="b"></param>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    public static void AddDefaultFlyTask(BaseBullet b, Vector2 startPosition, BaseUnit target, bool isNavi, bool notEndWithTakeDamage)
    {
        float d;

        // ��Ŀ��Ĺ���
        // һ���Ļ���һ�������ˣ������ܱ�֤����һ�ζ�����ԭ�ر�ը
        if (target.transform.position.x == startPosition.x)
        {
            startPosition += 0.01f * Vector2.right;
        }
        d = Mathf.Abs(target.transform.position.x - startPosition.x);

        float v = Mathf.Min(TransManager.TranToVelocity(36), d / 20); // ����ˮƽ�ٶ�ʹ��ʱ��С��20֡
        float t = Mathf.FloorToInt(d / v);
        float g = 2 * 3.0f / Mathf.Pow(9 * MapManager.gridWidth / v, 2);
        float h = Mathf.Max(0.5f, g * t * t / 2); // �߶���С������0.5��unity��λ

        // ȷ���ò���������������˶�
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, v, h, startPosition, target, isNavi, notEndWithTakeDamage));
    }

    /// <summary>
    /// ���Ĭ�ϵ�Ͷ����������
    /// </summary>
    /// <param name="b"></param>
    /// <param name="startPosition"></param>
    /// <param name="targetPosition"></param>
    public static void AddDefaultFlyTask(BaseBullet b, Vector2 startPosition, Vector2 targetPosition, bool isNavi, bool notEndWithTakeDamage)
    {
        if(targetPosition.x == startPosition.x)
        {
            startPosition += 0.01f * Vector2.right;
        }
        float d = Mathf.Abs(targetPosition.x - startPosition.x);
        float v = Mathf.Min(TransManager.TranToVelocity(36), d / 20); // ����ˮƽ�ٶ�ʹ��ʱ��С��20֡
        float t = Mathf.FloorToInt(d / v);
        float g = 2 * 3.0f / Mathf.Pow(9 * MapManager.gridWidth / v, 2);
        float h = Mathf.Max(0.5f, g * t * t / 2); // �߶���С������0.5��unity��λ

        // ȷ���ò���������������˶�
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, v, h, startPosition, targetPosition, isNavi, notEndWithTakeDamage));
    }


    /// <summary>
    /// Ͷ��Ĭ�ϵ����з�ʽ��Ѱ�ҵ�����ָ�������Ҳ�����Ŀ������ƫ��ģ�
    /// </summary>
    /// <returns></returns>
    public static BaseUnit FindTargetByPitcher(BaseUnit unit, float minX, int rowIndex)
    {
        List<BaseUnit> unitList = new List<BaseUnit>();
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            unitList.Add(u);
        }
        // Ѱ�ұ��е��ѷ�������λ��Ҳһ������
        foreach (var u in GameController.Instance.GetSpecificRowAllyList(rowIndex))
        {
            if (u.mType == (int)FoodNameTypeMap.CherryPudding && !(u is CharacterUnit))
            {
                unitList.Add(u);
            }
        }
        return FindTargetByPitcher(unit, minX, unitList, null);
    }


    /// <summary>
    /// Ͷ��Ĭ�ϵ����з�ʽ��Ѱ�ҵ�����ָ�������Ҳ�����Ŀ������ƫ��ģ�
    /// </summary>
    /// <returns></returns>
    public static BaseUnit FindTargetByPitcher(BaseUnit unit, float minX, List<BaseUnit> EnemyList, Func<BaseUnit, BaseUnit, bool> otherConditionFunc)
    {
        BaseUnit target = null;
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ҵ���ָ������Ŀ�ѡȡ��λ�������ǲ�����λ
        foreach (var item in EnemyList)
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(unit, item);
            // ���ڲ�����˵������Ҫ���ܷ�ѡΪ����Ŀ��������������ܱ�����
            if(item is CherryPuddingFoodUnit)
            {
                CherryPuddingFoodUnit pud = item as CherryPuddingFoodUnit;
                canSelect = !pud.isFrozenState;
            }

            if (item.GetHeight() == 0 && canSelect && item.transform.position.x >= minX && (otherConditionFunc==null|| otherConditionFunc(unit, item)))
                list.Add(item);
        }
        // ȥ��������С�ĵ�λ
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                if (target == null || item.transform.position.x < target.transform.position.x)
                {
                    target = item;
                }
            }
        }

        return target;
    }

    public static bool IsPitcher(FoodUnit f)
    {
        return PitcherList.Contains((FoodNameTypeMap)f.mType);
    }
}
