using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 玛奇朵港
/// </summary>
public class Map_MacchiatoHarbor : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {

    }

    /// <summary>
    /// 对格子进行加工
    /// </summary>
    public override void ProcessingGridList()
    {
        List<Vector2> list = new List<Vector2>()
        {
            new Vector2(8, 1), new Vector2(6, 2), new Vector2(8, 3), new Vector2(6, 4), new Vector2(8, 5)
        };

        foreach (var item in list)
        {
            // 生成风洞
            BaseWindCave c = (BaseWindCave)GameController.Instance.CreateItem((int)item.x, (int)item.y, (int)ItemNameTypeMap.WindCave, 0);
            c.SetStartTimeAndMaxTime(360, 480);
        }
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
