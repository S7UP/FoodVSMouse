/// <summary>
/// 芥末小屋(日)
/// </summary>
public class Map_MustardHut : ChapterMap
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
        // 铺上水格子
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        // 铺上障碍
        BaseBarrier b = GameController.Instance.CreateItem(6, 5, (int)ItemNameTypeMap.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b.mEffectController.HideEffect(true);
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
