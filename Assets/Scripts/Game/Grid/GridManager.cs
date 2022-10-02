using System.Collections.Generic;

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
}
