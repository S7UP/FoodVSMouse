using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 投掷管理器（提供投掷类相关的静态方法）
/// </summary>
public class PitcherManager
{
    private static List<FoodNameTypeMap> PitcherList = new List<FoodNameTypeMap>() { 
        FoodNameTypeMap.SaladPitcher, FoodNameTypeMap.ChocolatePitcher, FoodNameTypeMap.TofuPitcher, FoodNameTypeMap.EggPitcher
    };

    /// <summary>
    /// 添加默认的投掷飞行任务
    /// </summary>
    /// <param name="b"></param>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    public static void AddDefaultFlyTask(BaseBullet b, Vector2 startPosition, BaseUnit target, bool isNavi, bool notEndWithTakeDamage)
    {
        float d;

        // 有目标的攻击
        // 一样的话加一点点就行了，这样能保证弹射一次而不是原地爆炸
        if (target.transform.position.x == startPosition.x)
        {
            startPosition += 0.01f * Vector2.right;
        }
        d = Mathf.Abs(target.transform.position.x - startPosition.x);

        float v = Mathf.Min(TransManager.TranToVelocity(36), d / 20); // 控制水平速度使耗时不小于20帧
        float t = Mathf.FloorToInt(d / v);
        float g = 2 * 3.0f / Mathf.Pow(9 * MapManager.gridWidth / v, 2);
        float h = Mathf.Max(0.5f, g * t * t / 2); // 高度最小不超过0.5个unity单位

        // 确定好参数后添加抛物线运动
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, v, h, startPosition, target, isNavi, notEndWithTakeDamage));
    }

    /// <summary>
    /// 添加默认的投掷飞行任务
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
        float v = Mathf.Min(TransManager.TranToVelocity(36), d / 20); // 控制水平速度使耗时不小于20帧
        float t = Mathf.FloorToInt(d / v);
        float g = 2 * 3.0f / Mathf.Pow(9 * MapManager.gridWidth / v, 2);
        float h = Mathf.Max(0.5f, g * t * t / 2); // 高度最小不超过0.5个unity单位

        // 确定好参数后添加抛物线运动
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, v, h, startPosition, targetPosition, isNavi, notEndWithTakeDamage));
    }


    /// <summary>
    /// 投手默认的索敌方式（寻找单行中指点坐标右侧所有目标中最偏左的）
    /// </summary>
    /// <returns></returns>
    public static BaseUnit FindTargetByPitcher(BaseUnit unit, float minX, int rowIndex)
    {
        List<BaseUnit> unitList = new List<BaseUnit>();
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(rowIndex))
        {
            unitList.Add(u);
        }
        // 寻找本行的友方布丁单位，也一并加入
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
    /// 投手默认的索敌方式（寻找单行中指点坐标右侧所有目标中最偏左的）
    /// </summary>
    /// <returns></returns>
    public static BaseUnit FindTargetByPitcher(BaseUnit unit, float minX, List<BaseUnit> EnemyList, Func<BaseUnit, BaseUnit, bool> otherConditionFunc)
    {
        BaseUnit target = null;
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0且大于指定坐标的可选取单位，或者是布丁单位
        foreach (var item in EnemyList)
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(unit, item);
            // 对于布丁来说，不需要它能否被选为攻击目标的条件，但不能被控制
            if(item is CherryPuddingFoodUnit)
            {
                CherryPuddingFoodUnit pud = item as CherryPuddingFoodUnit;
                canSelect = !pud.isFrozenState;
            }

            if (item.GetHeight() == 0 && canSelect && item.transform.position.x >= minX && (otherConditionFunc==null|| otherConditionFunc(unit, item)))
                list.Add(item);
        }
        // 去找坐标最小的单位
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
