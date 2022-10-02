/// <summary>
/// 深渊岛
/// </summary>
public class Map_AbyssIsland : ChapterMap
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
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
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
        ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(6, 7, new UnityEngine.Vector2(MapManager.GetColumnX(1.5f), MapManager.GetRowY(3)));
        GameController.Instance.AddAreaEffectExecution(e);
    }
}
