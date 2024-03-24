using UnityEngine;
/// <summary>
/// 咖喱岛(夜)
/// </summary>
public class Map_CurryIsland2 : ChapterMap
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
        {
            //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
            //GameController.Instance.AddAreaEffectExecution(e);
        }


        // 添加迷雾
        // 陆地
        for (int i = 6; i > 0; i -= 4)
        {
            for (int j = 0; j <= 5; j+=5)
            {
                // 2*2矩阵
                for (int k = 0; k < 2; k++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(i+k), MapManager.GetRowY(j+l)));
                        e.SetOpen();
                        GameController.Instance.AddAreaEffectExecution(e);
                    }
                }
            }
        }
        // 水上
        for (int i = 0; i < 7; i += 4)
        {
            // 2*3矩阵
            for (int j = 0; j < 2; j ++)
            {
                for (int k = 2; k < 5; k++)
                {
                    FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(i + j), MapManager.GetRowY(k)));
                    e.SetOpen();
                    GameController.Instance.AddAreaEffectExecution(e);
                }
            }
        }
    }
}
