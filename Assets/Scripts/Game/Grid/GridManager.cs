using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 格子管理器（存放静态方法）
/// </summary>
public class GridManager
{
    /// <summary>
    /// 从一个格子表中取出随机几个格子
    /// </summary>
    /// <param name="list">格子表</param>
    /// <param name="count">取的随机数量</param>
    /// <returns></returns>
    public static List<BaseGrid> GetRandomUnitList(List<BaseGrid> list, int count)
    {
        List<BaseGrid> randList = new List<BaseGrid>();
        if (count <= 0 || list == null)
            return randList;

        // 拷贝一份List，保护原来的List不受影响
        List<BaseGrid> cp_list = new List<BaseGrid>();
        foreach (var item in list)
        {
            cp_list.Add(item);
        }

        count = Mathf.Min(count, cp_list.Count);

        for (int i = 0; i < count; i++)
        {
            int index = GameController.Instance.GetRandomInt(0, cp_list.Count);
            randList.Add(cp_list[index]);
            cp_list.Remove(cp_list[index]);
        }
        return randList;
    }

    /// <summary>
    /// 获取特定范围内的所有格子
    /// </summary>
    /// <returns></returns>
    public static List<BaseGrid> GetSpecificAreaGridList(List<BaseGrid> gridList, float left, float right, float up, float bottom)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        foreach (var g in gridList)
        {
            if (g.transform.position.x >= left && g.transform.position.x <= right && g.transform.position.y <= up && g.transform.position.y >= bottom)
                l.Add(g);
        }
        return l;
    }

    /// <summary>
    /// 获取达成特定条件 值最小 的格子组
    /// </summary>
    /// <param name="ConditionFunc"></param>
    /// <returns></returns>
    public static List<BaseGrid> GetGridListWhichHasMinCondition(List<BaseGrid> gridList, Func<BaseGrid, float> ConditionFunc)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        float min = float.MaxValue;
        foreach (var g in gridList)
        {
            float c = ConditionFunc(g);
            if (c < min)
            {
                min = c;
                l.Clear();
                l.Add(g);
            }
            else if(c == min)
            {
                l.Add(g);
            }
        }
        return l;
    }


    /// <summary>
    /// 获取达成特定条件 值最大 的格子组
    /// </summary>
    /// <param name="ConditionFunc"></param>
    /// <returns></returns>
    public static List<BaseGrid> GetGridListWhichHasMaxCondition(List<BaseGrid> gridList, Func<BaseGrid, float> ConditionFunc)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        float max = float.MinValue;
        foreach (var g in gridList)
        {
            float c = ConditionFunc(g);
            if (c > max)
            {
                max = c;
                l.Clear();
                l.Add(g);
            }
            else if (c == max)
            {
                l.Add(g);
            }
        }
        return l;
    }
}
