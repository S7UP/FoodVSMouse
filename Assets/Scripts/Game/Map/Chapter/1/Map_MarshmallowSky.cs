
using UnityEngine;
/// <summary>
/// 棉花糖天空（日）
/// </summary>
public class Map_MarshmallowSky : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 7; j++)
                CreateAndAddGrid(i, j);
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
        for (int i = 2; i < 7; i++)
            for (int j = 0; j < 7; j++)
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    
    public override void OtherProcessing()
    {
        {
            // 添加云层
            for (int i = 0; i < 7; i++)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 8);
            }

            // 添加风域
            WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(9, 7, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(3)));
            WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 420, 120, 360, true); // 等待时间、速度变化时间、匀速时间
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }
}
