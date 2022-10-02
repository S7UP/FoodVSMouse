using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 丁香彩虹（夜）
/// </summary>
public class Map_LilacRainbow2 : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 5*7
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[7];

        // 7行板块
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
            for (int j = 0; j < 5; j++)
            {
                listArray[i].Add(GetGrid(j, i));
            }
        }

        for (int k = 0; k < 7; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // 设置中心点坐标
            gridGroup.transform.position = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up;
            // 加入对应的格子
            foreach (var grid in listArray[k])
            {
                gridGroup.Add(grid);
            }
            // 设置并启用移动版块
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 720,
                        strandedTime = 720
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 2f * MapManager.gridWidth * Vector3.right + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 720,
                        strandedTime = 720
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (k + 1)),
                new Vector2(),
                false, false);
            gridGroup.SetCurrentMovementPercent((float)k / 6);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);
        }
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

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        // 为全图添加黑夜BUFF
        ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        GameController.Instance.AddAreaEffectExecution(e);
    }
}
