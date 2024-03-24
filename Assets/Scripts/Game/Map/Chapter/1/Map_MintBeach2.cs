/// <summary>
/// 薄荷海滩(夜)
/// </summary>
public class Map_MintBeach2 : ChapterMap
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
        // 布置上障碍物
        BaseBarrier b = GameController.Instance.CreateItem(6, 2, (int)ItemNameTypeMap.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
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
