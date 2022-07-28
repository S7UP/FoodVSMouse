using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 孜然断桥（日）
/// </summary>
public class Map_CuminBidge : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 1、7路
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 6);
        }
        // 2、4、6路
        for (int i = 0; i < 3; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 3);
            CreateAndAddGrid(i, 5);
        }
        for (int i = 5; i < 9; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 3);
            CreateAndAddGrid(i, 5);
        }
        // 3、5路
        for (int i = 0; i < 2; i++)
        {
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
        }
        for (int i = 2; i < 6; i++)
        {
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[5];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        // 2 4 6路
        for (int i = 5; i < 9; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[2].Add(GetGrid(i, 3));
            listArray[4].Add(GetGrid(i, 5));
        }
        // 3 5路
        for (int i = 2; i < 6; i++)
        {
            listArray[1].Add(GetGrid(i, 2));
            listArray[3].Add(GetGrid(i, 4));
        }

        // 中心点坐标
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // 终点坐标
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // 移动时间
        List<int> moveTimeList = new List<int>() { 360, 540, 360, 540, 360};

        for (int k = 0; k < 5; k++)
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
                        moveTime = moveTimeList[k],
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = moveTimeList[k],
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
        // 1、7路
        for (int i = 0; i < 9; i++)
        {
            GetGrid(i, 0).ChangeMainGridState(new WaterGridState(GetGrid(i, 0)));
            GetGrid(i, 6).ChangeMainGridState(new WaterGridState(GetGrid(i, 6)));
        }
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
