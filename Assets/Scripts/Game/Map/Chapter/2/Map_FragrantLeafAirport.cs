using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 香叶空港（日）
/// </summary>
public class Map_FragrantLeafAirport : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 1357路
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
            CreateAndAddGrid(i, 6);
        }
        // 2 6同步
        for (int i = 0; i < 4; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 5);
        }
        // 4与2 6反相
        for (int i = 5; i < 9; i++)
        {
            CreateAndAddGrid(i, 3);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[3];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        
        // 2 6同步
        for (int i = 0; i < 4; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[2].Add(GetGrid(i, 5));
        }
        // 4与2 6反相
        for (int i = 5; i < 9; i++)
        {
            listArray[1].Add(GetGrid(i, 3));
        }

        // 中心点坐标
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.down,
        };

        // 终点坐标
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.down,
        };

        for (int k = 0; k < 3; k++)
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
                        moveTime = 900,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 900,
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
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 7; j+=2)
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {
        {
            // 添加云层
            for (int i = 0; i < 7; i += 2)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.5f), MapManager.GetRowY(i)), 10);
            }

            // 添加风域
            for (int i = 0; i < 7; i += 2)
            {
                WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)));
                WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 1, 450, 120, 780); // 等待时间、速度变化时间、匀速时间
                GameController.Instance.AddAreaEffectExecution(e);
            }
        }
    }
}
