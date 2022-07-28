using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 茴香竹筏（日）
/// </summary>
public class Map_FennelBambooRaft : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 最后一列
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 1; j++)
                CreateAndAddGrid(j, i);
        // 左板
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
        // 右板
        for (int i = 2; i < 7; i++)
        {
            for (int j = 5; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[2];
        listArray[0] = new List<BaseGrid>();
        // 左板
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1; j < 5; j++)
                listArray[0].Add(GetGrid(j, i));
        }
        // 右板
        listArray[1] = new List<BaseGrid>();
        for (int i = 2; i < 7; i++)
        {
            for (int j = 5; j < 9; j++)
                listArray[1].Add(GetGrid(j, i));
        }

        // 中心点坐标
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 1.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.down
        };

        // 终点坐标
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 1.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.up
        };

        for (int k = 0; k < 2; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // 设置中心点坐标
            gridGroup.transform.position = centerList[k];
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
                        targetPosition = centerList[k],
                        moveTime = 360,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 360,
                        strandedTime = 450
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + "1"),
                new Vector2(),
                false, false);
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
}
