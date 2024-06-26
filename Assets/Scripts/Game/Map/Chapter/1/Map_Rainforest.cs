/// <summary>
/// 炭烧雨林(日)
/// </summary>
public class Map_Rainforest : ChapterMap
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
        GetGrid(1, 0).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(1, 1).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(6, 1).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(4, 3).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(1, 5).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(6, 5).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
