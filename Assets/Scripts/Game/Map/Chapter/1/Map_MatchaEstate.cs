/// <summary>
/// 抹茶庄园(日)
/// </summary>
public class Map_MatchaEstate : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 3-i; j <= 3+i; j++)
                CreateAndAddGrid(i, j);
        for (int i = 3; i < 6; i++)
            for (int j = 0; j < 7; j++)
                CreateAndAddGrid(i, j);
        for (int i = 6; i < 9; i++)
            for (int j = 3 - (8-i); j <= 3 + (8-i); j++)
                CreateAndAddGrid(i, j);

        //四个角
        CreateAndAddGrid(0, 0);
        CreateAndAddGrid(8, 0);
        CreateAndAddGrid(8, 6);
        CreateAndAddGrid(0, 6);
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

    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
