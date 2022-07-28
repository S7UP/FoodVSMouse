/// <summary>
/// 可可岛(夜)
/// </summary>
public class Map_CocoIsland2 : ChapterMap
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
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).ChangeMainGridState(new WaterGridState(GetGrid(j, i)));
        for (int i = 5; i < 7; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).ChangeMainGridState(new WaterGridState(GetGrid(j, i)));
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
