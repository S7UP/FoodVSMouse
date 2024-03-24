using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 果酱部落(夜)
/// </summary>
public class Map_JamTribe2 : ChapterMap
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
        // 铺上岩浆
        List<Vector2> list = new List<Vector2>()
        {
            new Vector2(5, 0), new Vector2(6, 0),
            new Vector2(6, 1),
            new Vector2(5, 2), new Vector2(6, 2),
            new Vector2(4, 3), new Vector2(5, 3),
            new Vector2(4, 4), new Vector2(5, 4),
            new Vector2(5, 5),
            new Vector2(4, 6), new Vector2(5, 6),
        };
        foreach (var v in list)
        {
            GetGrid((int)v.x, (int)v.y).AddGridType(GridType.Lava, BaseGridType.GetInstance(GridType.Lava, 0));
        }
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        // 为全图添加黑夜BUFF
        //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        //GameController.Instance.AddAreaEffectExecution(e);
    }
}
